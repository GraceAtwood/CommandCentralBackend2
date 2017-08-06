using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.Muster;
using NHibernate.Criterion;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class StatusPeriodController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get([FromQuery] Guid? person, [FromQuery] Guid? submittedBy, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] Guid? reason, [FromQuery] bool? exemptsFromWatch, [FromQuery] int limit = 1000)
        {
            var query = DBSession.QueryOver<StatusPeriod>();

            if (person.HasValue)
                query = query.Where(x => x.Person.Id == person);

            if (submittedBy.HasValue)
                query = query.Where(x => x.SubmittedBy.Id == submittedBy);

            if (start.HasValue && !end.HasValue)
                query = query.Where(x => x.Range.Start >= start || x.Range.End >= start);
            else if (end.HasValue && !start.HasValue)
                query = query.Where(x => x.Range.Start <= end || x.Range.End <= end);
            else if (end.HasValue && end.HasValue)
                query = query.Where(x => x.Range.Start <= end && x.Range.End >= start);

            if (reason.HasValue)
                query = query.Where(x => x.Reason.Id == reason);

            if (exemptsFromWatch.HasValue)
                query = query.Where(x => x.ExemptsFromWatch == exemptsFromWatch);

            var result = query.OrderBy(x => x.Range.Start).Desc.Take(limit)
                .Future()
                .Where(statusPeriod => User.GetFieldPermissions<Person>(statusPeriod.Person).CanReturn(x => x.StatusPeriods))
                .Select(statusPeriod =>
                    new DTOs.StatusPeriod.Get
                    {
                        DateSubmitted = statusPeriod.DateSubmitted,
                        ExemptsFromWatch = statusPeriod.ExemptsFromWatch,
                        Id = statusPeriod.Id,
                        Person = statusPeriod.Person.Id,
                        Range = statusPeriod.Range,
                        Reason = statusPeriod.Reason.Id,
                        SubmittedBy = statusPeriod.SubmittedBy.Id,
                        DateLastModified = statusPeriod.DateLastModified,
                        LastModifiedBy = statusPeriod.LastModifiedBy.Id
                    });

            return Ok(result);
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFound();

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.StatusPeriods))
                return Forbid();

            return Ok(new DTOs.StatusPeriod.Get
            {
                DateSubmitted = item.DateSubmitted,
                ExemptsFromWatch = item.ExemptsFromWatch,
                Id = item.Id,
                Person = item.Person.Id,
                Range = item.Range,
                Reason = item.Reason.Id,
                SubmittedBy = item.SubmittedBy.Id,
                DateLastModified = item.DateLastModified,
                LastModifiedBy = item.LastModifiedBy.Id
            });
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]DTOs.StatusPeriod.Post dto)
        {
            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFound($"Unable to find object referenced by parameter: {nameof(dto.Person)}.");

            if (dto.ExemptsFromWatch && !User.IsInChainOfCommand(person, ChainsOfCommand.QuarterdeckWatchbill))
            {
                return Forbid("Must be in the Watchbill chain of command to exempt a person from watch.");
            }

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.StatusPeriods))
                return Forbid("Can not submit a status period for this person.");

            var reason = ReferenceListHelper<StatusPeriodReason>.Get(dto.Reason);
            if (reason == null)
                return NotFound($"Unable to find object referenced by parameter: {nameof(dto.Reason)}.");

            var item = new StatusPeriod
            {
                DateSubmitted = CallTime,
                ExemptsFromWatch = dto.ExemptsFromWatch,
                Id = Guid.NewGuid(),
                Person = person,
                Range = dto.Range,
                Reason = reason,
                SubmittedBy = User,
                DateLastModified = CallTime,
                LastModifiedBy = User
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.StatusPeriod.Get
                {
                    DateSubmitted = item.DateSubmitted,
                    ExemptsFromWatch = item.ExemptsFromWatch,
                    Id = item.Id,
                    Person = item.Person.Id,
                    Range = item.Range,
                    Reason = item.Reason.Id,
                    SubmittedBy = item.SubmittedBy.Id,
                    DateLastModified = item.DateLastModified,
                    LastModifiedBy = item.LastModifiedBy.Id
                });
            }
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.StatusPeriod.Put dto)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFound();

            if (item.ExemptsFromWatch && !User.IsInChainOfCommand(item.Person, ChainsOfCommand.QuarterdeckWatchbill))
            {
                return Forbid("Must be in the Watchbill chain of command to modify a status period that exempts a person from watch.");
            }

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.StatusPeriods))
            {
                return Forbid();
            }

            var reason = ReferenceListHelper<StatusPeriodReason>.Get(dto.Reason);
            if (reason == null)
                return NotFound($"Unable to find object referenced by parameter: {nameof(dto.Reason)}.");

            using (var transaction = DBSession.BeginTransaction())
            {
                item.ExemptsFromWatch = dto.ExemptsFromWatch;
                item.Range = dto.Range;
                item.Reason = reason;
                item.LastModifiedBy = User;
                item.DateLastModified = CallTime;

                var result = item.Validate();
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.StatusPeriod.Get
                {
                    DateSubmitted = item.DateSubmitted,
                    ExemptsFromWatch = item.ExemptsFromWatch,
                    Id = item.Id,
                    Person = item.Person.Id,
                    Range = item.Range,
                    Reason = item.Reason.Id,
                    SubmittedBy = item.SubmittedBy.Id,
                    DateLastModified = item.DateLastModified,
                    LastModifiedBy = item.LastModifiedBy.Id
                });
            }
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFound();

            if (item.ExemptsFromWatch && !User.IsInChainOfCommand(item.Person, ChainsOfCommand.QuarterdeckWatchbill))
            {
                return Forbid("Must be in the Watchbill chain of command to delete a status period that exempts a person from watch.");
            }

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.StatusPeriods))
            {
                return Forbid();
            }

            if (item.Range.Start <= DateTime.UtcNow)
                return Conflict("You may not delete a status period whose time range has already started.  You may only modify its ending time.");

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);

                transaction.Commit();
            }

            return Ok();
        }
    }
}
