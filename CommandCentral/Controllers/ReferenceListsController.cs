using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Controllers
{
    public class ReferenceListsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.ReferenceList.GetList>))]
        public IActionResult Get([FromQuery] string value, [FromQuery] string description, [FromQuery] string type)
        {
            /*Expression<Func<ReferenceListItemBase, bool>> predicate = null;

            predicate = predicate
                .AddStringQueryExpression(x => x.Value, value)
                .AddStringQueryExpression(x => x.Description, description);

            IQueryable<ReferenceListItemBase> query = ;

            if (!String.IsNullOrWhiteSpace(type))
            {
                if (!ReferenceListHelper.ReferenceListNamesToType.TryGetValue(type, out Type listType))
                    return BadRequest($"The reference list type supplied by your parameter '{nameof(type)}' do not exist. Allowed, case-insensitive values are: {String.Join(", ", ReferenceListHelper.ReferenceListNamesToType.Keys)}");

                query = DBSession.Query<ReferenceListItemBase>(listType.Name);
            }
            else
            {
                query = DBSession.Query<ReferenceListItemBase>();
            }
            
            var results = query
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .GroupBy(x => x.GetEntityType(DBSession.GetSessionImplementation().PersistenceContext))
                .Select(x => new DTOs.ReferenceList.GetList(x, x.Key))
                .ToList();
            
            return Ok(results);*/

            throw new NotImplementedException();
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

            DBSession.Save(item);

            CommitChanges();

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
            
            CommitChanges();

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
            
            DBSession.Delete(item);
            
            CommitChanges();

            return NoContent();
        }
    }
}