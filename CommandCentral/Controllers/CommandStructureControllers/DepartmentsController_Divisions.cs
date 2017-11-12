using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CommandStructureControllers
{
    public partial class DepartmentsController
    {
        /// <summary>
        /// Retrieves the divisions belonging to a department.
        /// </summary>
        /// <param name="departmentId">The id of the department for which to retrieve divisions.</param>
        /// <returns></returns>
        [HttpGet("{departmentId}/Divisions")]
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

        /// <summary>
        /// Retrieves a single division belonging to a department.
        /// </summary>
        /// <param name="departmentId">The id of the department to which your division belongs.</param>
        /// <param name="divisionId">The id of the division to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{departmentId}/Divisions/{divisionId}")]
        [ProducesResponseType(200, Type = typeof(DTOs.Division.Get))]
        public IActionResult GetDivision(Guid departmentId, Guid divisionId)
        {
            var division = DBSession.Query<Division>()
                .SingleOrDefault(x => x.Id == divisionId && x.Department.Id == departmentId);

            if (division == null)
                return NotFoundChildParameter(departmentId, nameof(departmentId), divisionId, nameof(divisionId));

            return Ok(new DTOs.Division.Get(division));
        }

        /// <summary>
        /// Creates a new division.
        /// </summary>
        /// <param name="departmentId">The id of the department in which you want to create the new division.</param>
        /// <param name="dto">A dto containing the information needed to create a new division.</param>
        /// <returns></returns>
        [HttpPost("{departmentId}/Divisions")]
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

            DBSession.Save(division);
            
            CommitChanges();

            return CreatedAtAction(nameof(GetDivision), new { departmentId = division.Department.Id, divisionId = division.Id }, new DTOs.Division.Get(division));
        }

        /// <summary>
        /// Modifies a division.
        /// </summary>
        /// <param name="departmentId">The id of the department containing the division you want to modify.</param>
        /// <param name="divisionId">The id of the division to modify.</param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{departmentId}/Divisions/{divisionId}")]
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

            CommitChanges();

            return CreatedAtAction(nameof(GetDivision), new { departmentId = division.Department.Id, divisionId = division.Id }, new DTOs.Division.Get(division));
        }
    }
}
