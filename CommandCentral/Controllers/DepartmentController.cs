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

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class DepartmentController : CommandCentralController
    {
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Department>(id);
            if (item == null)
                return NotFound();

            return Ok(new DTOs.Department.Get
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Command = item.Command.Id,
                Divisions = item.Divisions.Select(x => x.Id).ToList()
            });
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody] DTOs.Department.Update dto)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFound($"The paramater {nameof(dto.Command)} could not be found.");

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = new Department
                {
                    Id = Guid.NewGuid(),
                    Command = command,
                    Description = dto.Description,
                    Name = dto.Name
                };

                var result = item.Validate();
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.Department.Get
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Command = item.Command.Id,
                    Divisions = item.Divisions.Select(x => x.Id).ToList()
                });
            }
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.Department.Update dto)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<Department>(id);
            if (item == null)
                return NotFound();

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFound($"The paramater {nameof(dto.Command)} could not be found.");

            using (var transaction = DBSession.BeginTransaction())
            {
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
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.Department.Get
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Command = item.Command.Id,
                    Divisions = item.Divisions.Select(x => x.Id).ToList()
                });
            }
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<Department>(id);

                if (item == null)
                    return NotFound();

                if (DBSession.Query<Person>().Where(x => x.Division.Department.Id == item.Id).Count() != 0)
                    return Conflict();

                DBSession.Delete(item);
                transaction.Commit();
                return Ok();
            }
        }
    }
}
