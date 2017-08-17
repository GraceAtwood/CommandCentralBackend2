using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Utilities;
using CommandCentral.Framework.Data;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using NHibernate.Linq;
using System.Linq.Expressions;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReferenceListsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.ReferenceList.GetList>))]
        public IActionResult Get([FromQuery] string value, [FromQuery] string description, [FromQuery] string type)
        {
            IQueryable<ReferenceListItemBase> query;

            if (!String.IsNullOrWhiteSpace(type))
            {
                if (!ReferenceListHelper.ReferenceListNamesToType.TryGetValue(type, out Type listType))
                    return BadRequest($"The reference list type '{type}' supplied by your parameter '{nameof(type)}' does not exist. Allowed, case-insensitive values are: {String.Join(", ", ReferenceListHelper.ReferenceListNamesToType.Keys)}");

                query = DBSession.Query<ReferenceListItemBase>(SessionManager.ClassMetaData[listType].EntityName);
            }
            else
            {
                query = DBSession.Query<ReferenceListItemBase>();
            }

            if (!String.IsNullOrWhiteSpace(value))
            {
                Expression<Func<ReferenceListItemBase, bool>> predicate = null;

                foreach (var term in value.Split(',').Select(x => x.Trim()))
                {
                    predicate = predicate.NullSafeOr(x => x.Value.Contains(term));
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(description))
            {
                Expression<Func<ReferenceListItemBase, bool>> predicate = null;

                foreach (var term in description.Split(',').Select(x => x.Trim()))
                {
                    predicate = predicate.NullSafeOr(x => x.Description.Contains(term));
                }

                query = query.Where(predicate);
            }

            var test = query.GroupBy(x => x.GetEntityType(DBSession.GetSessionImplementation().PersistenceContext)).ToList();

            var result = test
                .Select(x => new DTOs.ReferenceList.GetList(x, x.Key))
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFound();

            return Ok(new DTOs.ReferenceList.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Post([FromBody]DTOs.ReferenceList.Post dto)
        {
            if (dto == null)
                return BadRequest();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            if (!ReferenceListHelper.ReferenceListNamesToType.TryGetValue(dto.Type, out Type type))
                return BadRequest($"The reference list type identified by your parameter '{nameof(dto.Type)}' does not exist.");

            var item = (ReferenceListItemBase)Activator.CreateInstance(type);
            item.Value = dto.Value;
            item.Description = dto.Description;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.ReferenceList.Get(item));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.ReferenceList.Put dto)
        {
            if (dto == null)
                return BadRequest();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFound();

            item.Value = dto.Value;
            item.Description = dto.Description;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.ReferenceList.Get(item));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFound();

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}
