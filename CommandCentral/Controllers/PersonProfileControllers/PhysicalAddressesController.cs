using System;
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
    public class PhysicalAddressesController : CommandCentralController
    {
        /// <summary>
        /// Queries  the physical addresses collection.
        /// </summary>
        /// <param name="person">A person query for the person who owns a physical address.</param>
        /// <param name="isReleasableOutsideCoC">A boolean query for if a physical address is releasable outside the person's chain of command.</param>
        /// <param name="limit">[Default: 1000] [Optional] Instructs the service to return no more than this number of results.  Because this 
        /// endpoint does not support an order by parameter, the result set will be arbitrarily ordered and truncated.</param>
        /// <param name="isHomeAddress">A boolean query for if the address is the client's home address.</param>
        /// <param name="zipCode">A string query for the zip code.</param>
        /// <param name="city">A string query for the city.</param>
        /// <param name="state">A string query for the state.</param>
        /// <param name="country">A string query for the country.</param>
        /// <param name="address">A string query for the address.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DTOs.PhoneNumber.Get))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string address, [FromQuery] string city,
            [FromQuery] string state, [FromQuery] string country, [FromQuery] bool? isHomeAddress,
            [FromQuery] string zipCode, [FromQuery] bool? isReleasableOutsideCoC, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<PhysicalAddress, bool>>) null)
                .AddStringQueryExpression(x => x.Address, address)
                .AddStringQueryExpression(x => x.State, state)
                .AddStringQueryExpression(x => x.ZipCode, zipCode)
                .AddStringQueryExpression(x => x.Country, country)
                .AddStringQueryExpression(x => x.City, city)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.IsHomeAddress, isHomeAddress)
                .AddNullableBoolQueryExpression(x => x.IsReleasableOutsideCoC, isReleasableOutsideCoC);

            var results = DBSession.Query<PhysicalAddress>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .Take(limit)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.PhysicalAddress.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves the physical address with the given id.  
        /// Replaces the address text with REDACTED if the client can't see the physical address.
        /// </summary>
        /// <param name="id">The id of the physical address to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.PhysicalAddress.Get))]
        public IActionResult Get(Guid id)
        {
            var physicalAddress = DBSession.Get<PhysicalAddress>(id);
            if (physicalAddress == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(physicalAddress))
                return Forbid("You can not request this physical address.");
            
            return Ok(new DTOs.PhysicalAddress.Get(physicalAddress));
        }

        /// <summary>
        /// Creates a new physical address.
        /// </summary>
        /// <param name="dto">A dto containing all of the information needed to make a new physical address.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.PhysicalAddress.Get))]
        public IActionResult Post([FromBody] DTOs.PhysicalAddress.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (!User.CanEdit(person, x => x.PhysicalAddresses))
                return Forbid("You may not modify the physical addresses collection for this person.");

            var physicalAddress = new PhysicalAddress
            {
                Id = Guid.NewGuid(),
                IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                Person = person,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                IsHomeAddress = dto.IsHomeAddress,
                State = dto.State,
                ZipCode = dto.ZipCode
            };

            if (physicalAddress.IsHomeAddress)
            {
                foreach (var address in physicalAddress.Person.PhysicalAddresses)
                {
                    address.IsHomeAddress = false;
                }
            }

            var results = physicalAddress.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(physicalAddress);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = physicalAddress.Id},
                new DTOs.PhysicalAddress.Get(physicalAddress));
        }

        /// <summary>
        /// Modifies a physical address.
        /// </summary>
        /// <param name="id">The id of the physical address to modify.</param>
        /// <param name="dto">A dto containing all of the inforamtion needed to make a new physical address.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.PhysicalAddress.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.PhysicalAddress.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var physicalAddress = DBSession.Get<PhysicalAddress>(id);
            if (physicalAddress == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(physicalAddress))
                return Forbid("You may not modify the physical addresses collection for this person");

            physicalAddress.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;
            physicalAddress.Address = dto.Address;
            physicalAddress.City = dto.City;
            physicalAddress.Country = dto.Country;
            physicalAddress.IsHomeAddress = dto.IsHomeAddress;
            physicalAddress.State = dto.State;
            physicalAddress.ZipCode = dto.ZipCode;

            if (physicalAddress.IsHomeAddress)
            {
                foreach (var address in physicalAddress.Person.PhysicalAddresses)
                {
                    address.IsHomeAddress = false;
                }
            }

            var results = physicalAddress.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = physicalAddress.Id},
                new DTOs.PhysicalAddress.Get(physicalAddress));
        }
        
        /// <summary>
        /// Deletes a physical address.
        /// </summary>
        /// <param name="id">The id of the physical address to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var physicalAddress = DBSession.Get<PhysicalAddress>(id);
            if (physicalAddress == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(physicalAddress))
                return Forbid("You may not modify the physical addresses collection for this person");

            Delete(physicalAddress);
            CommitChanges();

            return NoContent();
        }
    }
}