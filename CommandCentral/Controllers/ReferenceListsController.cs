using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NHibernate.Linq;

namespace CommandCentral.Controllers
{
    public class ReferenceListsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.ReferenceList.GetList>))]
        public IActionResult Get([FromQuery] string value, [FromQuery] string description, [FromQuery] string types)
        {
            var predicate = ((Expression<Func<ReferenceListItemBase, bool>>) null)
                .AddStringQueryExpression(x => x.Value, value)
                .AddStringQueryExpression(x => x.Description, description);

            if (!String.IsNullOrWhiteSpace(types))
            {
                var subPredicateTypeQuery = (Expression<Func<ReferenceListItemBase, bool>>) null;

                foreach (var typeName in types.SplitByOr())
                {
                    if (!ReferenceListHelper.ReferenceListNamesToType.TryGetValue(typeName, out Type type))
                        return BadRequest(
                            $"One or more reference list types supplied in your '{nameof(types)}'" +
                            " parameter were not actual reference list types.");

                    //TODO: figure out how to do type querying in the Linq to SQL provider.  Might have to move to QueryOver for this one.
                    subPredicateTypeQuery = subPredicateTypeQuery.NullSafeOr(x => x is Paygrade);
                }

                predicate = predicate.NullSafeAnd(subPredicateTypeQuery);
            }

            var results = DBSession.Query<ReferenceListItemBase>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
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
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.ReferenceList.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Post([FromBody] DTOs.ReferenceList.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            if (!ReferenceListHelper.ReferenceListNamesToType.TryGetValue(dto.Type, out Type type))
                return BadRequest(
                    $"The reference list type identified by your parameter '{nameof(dto.Type)}' does not exist.");

            var item = (ReferenceListItemBase) Activator.CreateInstance(type);
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

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.ReferenceList.Get(item));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.ReferenceList.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

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

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.ReferenceList.Get(item));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}