using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.DTOs;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class PhysicalAddressController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult GetByPerson([FromQuery]Guid Person)
        {
            if (Person == Guid.Empty)
                return BadRequest("Query string with a person id is missing or malformed. Loading all physical addresses is not allowed.");

            var items = DBSession.QueryOver<PhysicalAddress>().Where(x => x.Person.Id == Person).List();

            if (!items.Any())
                return NotFound();

            IEnumerable<PhysicalAddress> result;
            if (!User.IsInChainOfCommand(items.First().Person))
            {
                result = items.Where(x => x.IsReleasableOutsideCoC);
            }
            else
            {
                result = items;
            }

            return Ok(result.Select(x =>
                new DTOs.PhysicalAddress.Get
                {
                    Id = x.Id,
                    Address = x.Address,
                    City = x.City,
                    Country = x.Country,
                    IsHomeAddress = x.IsHomeAddress,
                    IsReleaseableOutsideCoC = x.IsReleasableOutsideCoC,
                    Person = x.Person.Id,
                    State = x.State,
                    ZipCode = x.ZipCode
                })
            );
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<PhysicalAddress>(id);
            if (item == null)
                return NotFound();

            if (item.IsReleasableOutsideCoC || User.IsInChainOfCommand(item.Person))
            {
                return Ok(new DTOs.PhysicalAddress.Get
                {
                    Id = item.Id,
                    Address = item.Address,
                    City = item.City,
                    Country = item.Country,
                    IsHomeAddress = item.IsHomeAddress,
                    IsReleaseableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    State = item.State,
                    ZipCode = item.ZipCode
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]DTOs.PhysicalAddress.Update dto)
        {
            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFound();

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.PhysicalAddresses))
                return PermissionDenied();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = new PhysicalAddress
                {
                    Id = Guid.NewGuid(),
                    Address = dto.Address,
                    City = dto.City,
                    Country = dto.Country,
                    IsHomeAddress = dto.IsHomeAddress,
                    IsReleasableOutsideCoC = dto.IsReleaseableOutsideCoC,
                    Person = person,
                    State = dto.State,
                    ZipCode = dto.ZipCode
                };

                var result = item.Validate();
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.PhysicalAddress.Get
                {
                    Id = item.Id,
                    Address = item.Address,
                    City = item.City,
                    Country = item.Country,
                    IsHomeAddress = item.IsHomeAddress,
                    IsReleaseableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    State = item.State,
                    ZipCode = item.ZipCode
                });
            }
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.PhysicalAddress.Update dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<PhysicalAddress>(id);
                if (item == null)
                    return NotFound();

                if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhysicalAddresses))
                    return PermissionDenied();

                item.Address = dto.Address;
                item.City = dto.City;
                item.Country = dto.Country;
                item.IsHomeAddress = dto.IsHomeAddress;
                item.IsReleasableOutsideCoC = dto.IsReleaseableOutsideCoC;
                item.State = dto.State;
                item.ZipCode = dto.ZipCode;

                var result = item.Validate();
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.PhysicalAddress.Get
                {
                    Id = item.Id,
                    Address = item.Address,
                    City = item.City,
                    Country = item.Country,
                    IsHomeAddress = item.IsHomeAddress,
                    IsReleaseableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    State = item.State,
                    ZipCode = item.ZipCode
                });
            }
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<PhysicalAddress>(id);

                if (item == null)
                    return NotFound();

                if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhysicalAddresses))
                    return PermissionDenied();

                DBSession.Delete(item);
                transaction.Commit();
                return Ok();
            }
        }
    }
}
