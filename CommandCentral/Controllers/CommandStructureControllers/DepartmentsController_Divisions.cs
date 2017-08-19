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
    public partial class DepartmentsController : CommandCentralController
    {
        [HttpGet("{departmentId}/Divisions")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Division.Get>))]
        public IActionResult GetDivisions(Guid departmentId)
        {
            if (!DBSession.Query<Department>().Any(x => x.Id == departmentId))
                return NotFoundParameter(departmentId, nameof(departmentId));

            var results = DBSession.Query<Division>()
                .Where(x => x.Department.Id == departmentId)
                .Select(x => new DTOs.Division.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{departmentId}/Divisions/{divisionId}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Division.Get))]
        public IActionResult GetDivision(Guid departmentId, Guid divisionId)
        {
            var division = DBSession.Query<Division>()
                .SingleOrDefault(x => x.Id == divisionId && x.Department.Id == departmentId);

            if (division == null)
                return NotFoundChildParameter(departmentId, nameof(departmentId), divisionId, nameof(divisionId));

            return Ok(new DTOs.Division.Get(division));
        }

        [HttpPost("{departmentId}/Divisions")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Division.Get))]
        public IActionResult PostDivision(Guid departmentId, [FromBody] DTOs.Division.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var department = DBSession.Get<Department>(departmentId);
            if (department == null)
                return NotFoundParameter(departmentId, nameof(departmentId));

            var division = new Division
            {
                Department = department,
                Description = dto.Description,
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            var result = division.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(division);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetDivision), new { departmentId = division.Department.Id, divisionId = division.Id }, new DTOs.Division.Get(division));
        }

        [HttpPut("{departmentId}/Divisions/{divisionId}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Division.Get))]
        public IActionResult PutDivision(Guid departmentId, Guid divisionId, [FromBody]DTOs.Division.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var division = DBSession.Query<Division>()
                .SingleOrDefault(x => x.Id == divisionId && x.Department.Id == departmentId);

            if (division == null)
                return NotFoundChildParameter(departmentId, nameof(departmentId), divisionId, nameof(divisionId));

            if (division.Department.Id != dto.Department)
            {
                var newDepartment = DBSession.Get<Department>(dto.Department);
                if (newDepartment == null)
                    return NotFoundParameter(dto.Department, nameof(dto.Department));

                division.Department = newDepartment;
            }

            division.Description = dto.Description;
            division.Name = dto.Name;

            var result = division.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(division);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetDivision), new { departmentId = division.Department.Id, divisionId = division.Id }, new DTOs.Division.Get(division));
        }
    }
}
