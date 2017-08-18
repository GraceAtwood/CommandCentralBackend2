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
    public class DepartmentsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Department.Get>))]
        public IActionResult Get([FromQuery] string name, [FromQuery] string description, [FromQuery] string command)
        {
            Expression<Func<Department, bool>> predicate = null;

            predicate = predicate
                .AddStringQueryExpression(x => x.Name, name)
                .AddStringQueryExpression(x => x.Description, description)
                .AddCommandQueryExpression(x => x.Command, command);

            var result = DBSession.Query<Department>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToFuture()
                .Select(item => new DTOs.Department.Get(item))
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Department.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Department>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Department.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Department.Get))]
        public IActionResult Post([FromBody] DTOs.Department.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFoundParameter(dto.Command, nameof(dto.Command));

            var item = new Department
            {
                Id = Guid.NewGuid(),
                Command = command,
                Description = dto.Description,
                Name = dto.Name
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.Department.Get(item));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Department.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.Department.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<Department>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFoundParameter(dto.Command, nameof(dto.Command));

            if (item.Command != command)
            {
                item.Command.Departments.Remove(item);
                command.Departments.Add(item);
                item.Command = command;
            }

            item.Description = dto.Description;
            item.Name = dto.Name;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.Department.Get(item));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<Department>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (DBSession.Query<Person>().Where(x => x.Division.Department.Id == item.Id).Count() != 0)
                return Conflict();

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}
