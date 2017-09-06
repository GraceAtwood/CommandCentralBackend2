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
    public partial class CommandsController
    {
        /// <summary>
        /// Retrieves all departments of this command.
        /// </summary>
        /// <param name="commandId">The id of the command for which to retrieve departments.</param>
        /// <returns></returns>
        [HttpGet("{commandId}/Departments")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Department.Get>))]
        public IActionResult GetDepartments(Guid commandId)
        {
            if (!DBSession.Query<Command>().Any(x => x.Id == commandId))
                return NotFoundParameter(commandId, nameof(commandId));

            var results = DBSession.Query<Department>()
                .Where(x => x.Command.Id == commandId)
                .Select(x => new DTOs.Department.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a single department for the given command.
        /// </summary>
        /// <param name="commandId">The id of the command for which to retrieve a department.</param>
        /// <param name="departmentId">The of the department to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{commandId}/Department/{departmentId}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Department.Get))]
        public IActionResult GetDepartment(Guid commandId, Guid departmentId)
        {
            var department = DBSession.Query<Department>()
                .SingleOrDefault(x => x.Id == departmentId && x.Command.Id == commandId);

            if (department == null)
                return NotFoundChildParameter(commandId, nameof(commandId), departmentId, nameof(departmentId));

            return Ok(new DTOs.Department.Get(department));
        }

        /// <summary>
        /// Creates a new department.
        /// </summary>
        /// <param name="commandId">The id of the command to which to add a new department.</param>
        /// <param name="dto">A dto containing all of the information needed to make a new department.</param>
        /// <returns></returns>
        [HttpPost("{commandId}/Department")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Department.Get))]
        public IActionResult PostDepartment(Guid commandId, [FromBody] DTOs.Department.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var command = DBSession.Get<Command>(commandId);
            if (command == null)
                return NotFoundParameter(commandId, nameof(commandId));

            var department = new Department
            {
                Command = command,
                Description = dto.Description,
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            var result = department.Validate();
            if (!result.IsValid)
            {
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
            }

            DBSession.Save(department);

            CommitChanges();

            return CreatedAtAction(nameof(GetDepartment),
                new {commandId = department.Command.Id, departmentId = department.Id},
                new DTOs.Department.Get(department));
        }

        /// <summary>
        /// Modifies a department belonging to the given command.
        /// </summary>
        /// <param name="commandId">The id of the command to which the department you want to modify belongs.</param>
        /// <param name="departmentId">The id of the department to modify.</param>
        /// <param name="dto">A dto containing the information needed to modify a department.</param>
        /// <returns></returns>
        [HttpPut("{commandId}/Department/{departmentId}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Department.Get))]
        public IActionResult PutDepartment(Guid commandId, Guid departmentId, [FromBody] DTOs.Department.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var department = DBSession.Query<Department>()
                .SingleOrDefault(x => x.Id == departmentId && x.Command.Id == commandId);

            if (department == null)
                return NotFoundChildParameter(commandId, nameof(commandId), departmentId, nameof(departmentId));

            if (department.Command.Id != dto.Command)
            {
                var newCommand = DBSession.Get<Command>(dto.Command);
                if (newCommand == null)
                    return NotFoundParameter(dto.Command, nameof(dto.Command));

                department.Command = newCommand;
            }

            department.Description = dto.Description;
            department.Name = dto.Name;

            var result = department.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            return CreatedAtAction(nameof(GetDepartment),
                new {commandId = department.Command.Id, departmentId = department.Id},
                new DTOs.Department.Get(department));
        }
    }
}