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
using LinqKit;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ReferenceListsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.ReferenceList.GetList>))]
        public IActionResult Get([FromQuery] string value, [FromQuery] string description, [FromQuery] string type)
        {

            Expression<Func<ReferenceListItemBase, bool>> predicate = null;

            predicate = predicate
                .AddStringQueryExpression(x => x.Value, value)
                .AddStringQueryExpression(x => x.Description, description);

            var queries = new List<IQueryable<ReferenceListItemBase>>();

            if (!String.IsNullOrWhiteSpace(type))
            {
                foreach (var item in type.SplitByOr())
                {
                    if (!ReferenceListHelper.ReferenceListNamesToType.TryGetValue(item, out Type listType))
                        return BadRequest($"One or more reference list types supplied by your parameter '{nameof(type)}' do not exist. Allowed, case-insensitive values are: {String.Join(", ", ReferenceListHelper.ReferenceListNamesToType.Keys)}");

                    queries.Add(DBSession.Query<ReferenceListItemBase>(listType.Name));
                }
            }
            else
            {
                queries.Add(DBSession.Query<ReferenceListItemBase>());
            }

            var results = queries
                .SelectMany(x => x.AsExpandable().NullSafeWhere(predicate).ToFuture())
                .GroupBy(x => x.GetEntityType(DBSession.GetSessionImplementation().PersistenceContext))
                .Select(x => new DTOs.ReferenceList.GetList(x, x.Key))
                .ToList();

            return Ok(results);
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
