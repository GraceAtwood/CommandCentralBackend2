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
    /// <summary>
    /// Provides access to the email addresses resource.
    /// </summary>
    public class EmailAddressesController : CommandCentralController
    {
        /// <summary>
        /// Provides querying of the email addresses collection.  
        /// If your client is not in a person's chain of command and the email address is not marked as releasable outside the CoC, 
        /// the address property will be replaced with 'REDACTED'.
        /// </summary>
        /// <param name="person">A person query for the owner of the email address.</param>
        /// <param name="address">A string query for the address itself.</param>
        /// <param name="isReleasableOutsideCoC">A bool query for if the owner of the email address 
        /// has indicated their email address can be released outside their chain of command.</param>
        /// <param name="isPreferred">A bool query for if the client would prefer to be contacted at this email address.</param>
        /// <param name="limit">[Default: 1000] [Optional] Instructs the service to return no more than this number of results.  Because this 
        /// endpoint does not support an order by parameter, the result set will be arbitrarily ordered and truncated.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DTOs.EmailAddress.Get))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string address,
            [FromQuery] bool? isReleasableOutsideCoC, [FromQuery] bool? isPreferred, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<EmailAddress, bool>>) null)
                .AddStringQueryExpression(x => x.Address, address)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.IsPreferred, isPreferred)
                .AddNullableBoolQueryExpression(x => x.IsReleasableOutsideCoC, isReleasableOutsideCoC);

            var results = DBSession.Query<EmailAddress>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .Take(limit)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.EmailAddress.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves the email address with the given id.  
        /// Replaces the address text with REDACTED if the client can't see the email address.
        /// </summary>
        /// <param name="id">The id of the email address to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.EmailAddress.Get))]
        public IActionResult Get(Guid id)
        {
            var address = DBSession.Get<EmailAddress>(id);
            if (address == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(address))
                return Forbid("You may not return this email address.");

            return Ok(new DTOs.EmailAddress.Get(address));
        }

        /// <summary>
        /// Creates a new email address.
        /// </summary>
        /// <param name="dto">A dto containing all of the information needed to create a new email address.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.EmailAddress.Post))]
        public IActionResult Post([FromBody] DTOs.EmailAddress.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (!User.CanEdit(person, x => x.EmailAddresses))
                return Forbid("You may not modify the email addresses collection for this person.");

            var emailAddress = new EmailAddress
            {
                Address = dto.Address,
                Id = Guid.NewGuid(),
                IsPreferred = dto.IsPreferred,
                IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                Person = person
            };
            
            if (emailAddress.IsPreferred)
            {
                foreach (var address in emailAddress.Person.EmailAddresses)
                {
                    address.IsPreferred = false;
                }
            }

            var results = emailAddress.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(emailAddress);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = emailAddress.Id}, new DTOs.EmailAddress.Get(emailAddress));
        }

        /// <summary>
        /// Modifies an email address.
        /// </summary>
        /// <param name="id">The id of the email address to modify.</param>
        /// <param name="dto">A dto containing all of the inforamtion needed to make a new email address.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.EmailAddress.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.EmailAddress.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var emailAddress = DBSession.Get<EmailAddress>(id);
            if (emailAddress == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(emailAddress))
                return Forbid("You may not modify the email addresses collection for this person");

            emailAddress.Address = dto.Address;
            emailAddress.IsPreferred = dto.IsPreferred;
            emailAddress.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;
            
            if (emailAddress.IsPreferred)
            {
                foreach (var address in emailAddress.Person.EmailAddresses)
                {
                    address.IsPreferred = false;
                }
            }
            
            var results = emailAddress.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = emailAddress.Id}, new DTOs.EmailAddress.Get(emailAddress));
        }

        /// <summary>
        /// Deletes an email address.
        /// </summary>
        /// <param name="id">The id of the email address to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var emailAddress = DBSession.Get<EmailAddress>(id);
            if (emailAddress == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(emailAddress))
                return Forbid("You may not modify the email addresses collection for this person");

            DBSession.Delete(emailAddress);
            CommitChanges();

            return NoContent();
        }
    }
}