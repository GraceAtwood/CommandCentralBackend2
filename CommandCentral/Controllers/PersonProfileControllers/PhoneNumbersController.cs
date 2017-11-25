using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public class PhoneNumbersController : CommandCentralController
    {
        /// <summary>
        /// Queries  the phone numbers collection.
        /// </summary>
        /// <param name="person">A person query for the person who owns a phone number.</param>
        /// <param name="number">A string query for a phone number.</param>
        /// <param name="isReleasableOutsideCoC">A boolean query for if a phone number is releasable outside the person's chain of command.</param>
        /// <param name="isPreferred">A boolean query for if a person would prefer to be contacted at this phone number.</param>
        /// <param name="phoneType">An exact enum query for the type of a phone number.</param>
        /// <param name="limit">[Default: 1000] [Optional] Instructs the service to return no more than this number of results.  Because this 
        /// endpoint does not support an order by parameter, the result set will be arbitrarily ordered and truncated.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DTOs.PhoneNumber.Get))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string number,
            [FromQuery] bool? isReleasableOutsideCoC, [FromQuery] bool? isPreferred, [FromQuery] string phoneType,
            [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<PhoneNumber, bool>>) null)
                .AddStringQueryExpression(x => x.Number, number)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.IsPreferred, isPreferred)
                .AddNullableBoolQueryExpression(x => x.IsReleasableOutsideCoC, isReleasableOutsideCoC)
                .AddExactEnumQueryExpression(x => x.PhoneType, phoneType);

            var phoneNumbers = DBSession.Query<PhoneNumber>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .Take(limit)
                .ToList();

            var checkedPersons = new Dictionary<Person, bool>();
            foreach (var phoneNumber in phoneNumbers.Where(x => !x.IsReleasableOutsideCoC))
            {
                if (!checkedPersons.TryGetValue(phoneNumber.Person, out var canView))
                {
                    checkedPersons[phoneNumber.Person] = User.IsInChainOfCommand(phoneNumber.Person);
                    canView = checkedPersons[phoneNumber.Person];
                }

                if (!canView)
                    phoneNumber.Number = "REDACTED";
            }

            //Now that we've scrubbed out all the values we don't want to expose to the client, 
            //we're ready to send the results out.
            var results = phoneNumbers.Select(x => new DTOs.PhoneNumber.Get(x)).ToList();
            return Ok(results);
        }
        
        /// <summary>
        /// Retrieves the phone number with the given id.  
        /// Replaces the number text with REDACTED if the client can't see the phone number.
        /// </summary>
        /// <param name="id">The id of the phone number to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.PhoneNumber.Get))]
        public IActionResult Get(Guid id)
        {
            var phoneNumber = DBSession.Get<PhoneNumber>(id);
            if (phoneNumber == null)
                return NotFoundParameter(id, nameof(id));

            if (!phoneNumber.IsReleasableOutsideCoC && !User.IsInChainOfCommand(phoneNumber.Person))
                phoneNumber.Number = "REDACTED";

            return Ok(new DTOs.PhoneNumber.Get(phoneNumber));
        }
        
        /// <summary>
        /// Creates a new phone number.
        /// </summary>
        /// <param name="dto">A dto containing all of the information needed to make a new phone number.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.PhoneNumber.Post))]
        public IActionResult Post([FromBody] DTOs.PhoneNumber.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.PhoneNumbers))
                return Forbid("You may not modify the phone numbers collection for this person.");

            var phoneNumber = new PhoneNumber
            {
                Number = dto.Number,
                Id = Guid.NewGuid(),
                IsPreferred = dto.IsPreferred,
                IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                Person = person,
                PhoneType = dto.PhoneType
            };
            
            if (phoneNumber.IsPreferred)
            {
                foreach (var address in phoneNumber.Person.PhoneNumbers)
                {
                    address.IsPreferred = false;
                }
            }

            var results = phoneNumber.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(phoneNumber);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = phoneNumber.Id}, new DTOs.PhoneNumber.Get(phoneNumber));
        }
        
        /// <summary>
        /// Modifies a phone number.
        /// </summary>
        /// <param name="id">The id of the phone number to modify.</param>
        /// <param name="dto">A dto containing all of the inforamtion needed to make a new phone number.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.PhoneNumber.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.PhoneNumber.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var phoneNumber = DBSession.Get<PhoneNumber>(id);
            if (phoneNumber == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.GetFieldPermissions<Person>(phoneNumber.Person).CanEdit(x => x.PhoneNumbers))
                return Forbid("You may not modify the phone numbers collection for this person");

            phoneNumber.Number = dto.Number;
            phoneNumber.IsPreferred = dto.IsPreferred;
            phoneNumber.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;
            phoneNumber.PhoneType = dto.PhoneType;
            
            if (phoneNumber.IsPreferred)
            {
                foreach (var address in phoneNumber.Person.PhoneNumbers)
                {
                    address.IsPreferred = false;
                }
            }
            
            var results = phoneNumber.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = phoneNumber.Id}, new DTOs.PhoneNumber.Get(phoneNumber));
        }
        
        /// <summary>
        /// Deletes a phone number.
        /// </summary>
        /// <param name="id">The id of the phone number to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var phoneNumber = DBSession.Get<PhoneNumber>(id);
            if (phoneNumber == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.GetFieldPermissions<Person>(phoneNumber.Person).CanEdit(x => x.PhoneNumbers))
                return Forbid("You may not modify the phone numbers collection for this person");

            DBSession.Delete(phoneNumber);
            CommitChanges();

            return NoContent();
        }
    }
}