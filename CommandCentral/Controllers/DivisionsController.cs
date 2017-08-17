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

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class DivisionsController : CommandCentralController
    {
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Division>(id);
            if (item == null)
                return NotFound();

            return Ok(new DTOs.Division.Get
            {
                Department = item.Department.Id,
                Description = item.Description,
                Id = item.Id,
                Name = item.Name
            });
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody] DTOs.Division.Update dto)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var department = DBSession.Get<Department>(dto.Department);
            if (department == null)
                return NotFound($"The paramater {nameof(dto.Department)} could not be found.");

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = new Division
                {
                    Department = department,
                    Description = dto.Description,
                    Id = Guid.NewGuid(),
                    Name = dto.Name
                };

                var result = item.Validate();
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.Division.Get
                {
                    Id = item.Id,
                    Department = item.Department.Id,
                    Description = item.Description,
                    Name = item.Name
                });
            }
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.Division.Update dto)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<Division>(id);
            if (item == null)
                return NotFound();

            var department = DBSession.Get<Department>(dto.Department);
            if (department == null)
                return NotFound($"The paramater {nameof(dto.Department)} could not be found.");

            using (var transaction = DBSession.BeginTransaction())
            {
                if (item.Department != department)
                {
                    item.Department.Divisions.Remove(item);
                    department.Divisions.Add(item);
                    item.Department = department;
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

                return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.Division.Get
                {
                    Id = item.Id,
                    Name = item.Name,
                    Department = item.Department.Id,
                    Description = item.Description
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
                var item = DBSession.Get<Division>(id);

                if (item == null)
                    return NotFound();

                if (DBSession.QueryOver<Person>().Where(x => x.Division.Id == item.Id).RowCount() != 0)
                    return Conflict();

                DBSession.Delete(item);
                transaction.Commit();
                return Ok();
            }
        }
    }
}
