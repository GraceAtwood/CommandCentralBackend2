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
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PersonController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Person.Get>))]
        public IActionResult Get([FromQuery] string command, [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(Person.LastName))
        {
            if (limit <= 0)
                return BadRequest($"The value '{limit}' for the property '{nameof(limit)}' was invalid.  It must be greater than zero.");

            var query = DBSession.Query<Person>();

            if (!String.IsNullOrWhiteSpace(command))
            {
                Expression<Func<Person, bool>> predicate = null;

                foreach (var phrase in command.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.Division.Department.Command.Id == id);
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<Person, bool>> subPredicate = null;

                        foreach (var term in terms)
                        {
                            subPredicate = subPredicate.NullSafeOr(x => x.Division.Department.Command.Name.Contains(term));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (String.Equals(orderBy, nameof(Person.LastName), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.LastName);
            else
                return BadRequest($"Your requested value '{orderBy}' for the parameter '{nameof(orderBy)}' is not supported.  The supported values are '{nameof(Person.LastName)}' (this is the default) and nothing else yet.");

            var result = query
                .Take(limit)
                .ToList()
                .Select(item =>
                {
                    var perms = User.GetFieldPermissions<Person>(item);

                    return new DTOs.Person.Get(item, perms);
                });

            return Ok(result.ToList());
        }

        /// <summary>
        /// Retrieves the person identified by the given Id.
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Person.Get))]
        public IActionResult GetMe()
        {
            var perms = User.GetFieldPermissions<Person>(User);

            return Ok(new DTOs.Person.Get(User, perms));
        }

        /// <summary>
        /// Retrieves the person identified by the given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Person.Get))]
        public IActionResult Get(Guid id)
        {
            var person = DBSession.Get<Person>(id);
            if (person == null)
                return NotFound();

            var perms = User.GetFieldPermissions<Person>(person);

            return Ok(new DTOs.Person.Get(person, perms));
        }

        /// <summary>
        /// Creates a new person.  Client must have access to the "CreatePerson" submodule.
        /// </summary>
        /// <param name="dto">The dto containing all of the information needed to create a person.</param>
        /// <returns></returns>
        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Person.Get))]
        public IActionResult Post([FromBody] DTOs.Person.Post dto)
        {
            if (dto == null)
                return BadRequest();

            if (!User.CanAccessSubmodules(SubModules.CreatePerson))
                return Forbid();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                DateOfArrival = dto.DateOfArrival,
                DateOfBirth = dto.DateOfBirth,
                UIC = DBSession.Get<UIC>(dto.UIC),
                Designation = DBSession.Get<Designation>(dto.Designation),
                Paygrade = DBSession.Get<Paygrade>(dto.Paygrade),
                Division = DBSession.Get<Division>(dto.Division),
                DoDId = dto.DoDId,
                SSN = dto.SSN,
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                Sex = DBSession.Get<Sex>(dto.Sex),
                DutyStatus = DBSession.Get<DutyStatus>(dto.DutyStatus)
            };

            var result = person.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(person);
                transaction.Commit();
            }

            Events.EventManager.OnPersonCreated(new Events.Args.PersonCreatedEventArgs
            {
                CreatedBy = User,
                Person = person
            }, this);

            return CreatedAtAction(nameof(Get), new { id = person.Id }, new DTOs.Person.Get(person, User.GetFieldPermissions<Person>(person)));
        }

        [HttpPatch("{id}")]
        [RequireAuthentication]
        public IActionResult Patch(Guid id, [FromBody]JsonPatchDocument<Person> personPatchDocument)
        {
            throw new NotImplementedException();
        }
    }
}

