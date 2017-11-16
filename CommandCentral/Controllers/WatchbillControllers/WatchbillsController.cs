using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.WatchbillControllers
{
    public class WatchbillsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Watchbill.Get>))]
        public IActionResult Get()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Watchbill.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Watchbill>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Watchbill.Get(item));
        }

        /// <summary>
        /// Creates a new watchbill.  Client must be in the watchbill chain of command.  Only one watchbill may be 
        /// created for a given command, month, and year combination.
        /// </summary>
        /// <param name="dto">A dto containing all of the required information to create a watchbill.</param>
        /// <returns></returns>
        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Watchbill.Get))]
        public IActionResult Post([FromBody] DTOs.Watchbill.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                ChainOfCommandLevels.Command)
                return Forbid("You must be in the command level of the watchbill chain of command.");

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFoundParameter(dto.Command, nameof(dto.Command));

            if (DBSession.Query<Watchbill>()
                    .Count(x => x.Command == command && x.Month == dto.Month && x.Year == dto.Year) != 0)
                return Conflict("A watchbill already exists for the given command, month, and year combination.  " +
                                "Please considering using that one or deleting it.");

            var watchbill = new Watchbill
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Year = dto.Year,
                Month = dto.Month,
                Command = command,
                Phase = WatchbillPhases.Initial,
                CreatedBy = User
            };

            var result = watchbill.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(watchbill);
            CommitChanges();

            return CreatedAtAction(nameof(Get),
                new {id = watchbill.Id},
                new DTOs.Watchbill.Get(watchbill));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Watchbill.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.Watchbill.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                ChainOfCommandLevels.Command)
                return Forbid("You must be in the command level of the watchbill chain of command.");

            var watchbill = DBSession.Get<Watchbill>(id);
            if (watchbill == null)
                return NotFoundParameter(id, nameof(id));

            watchbill.Phase = dto.Phase;
            watchbill.Title = dto.Title;

            var result = watchbill.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = watchbill.Id}, new DTOs.Watchbill.Get(watchbill));

            // TODO: Handle Phase change events for watchbills
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                ChainOfCommandLevels.Command)
                return Forbid("You must be in the command level of the watchbill chain of command.");

            var watchbill = DBSession.Get<Watchbill>(id);
            if (watchbill == null)
                return NotFoundParameter(id, nameof(id));
            
            var tempDate = new DateTime(watchbill.Year, watchbill.Month, 1);
            if (tempDate < DateTime.UtcNow)
                return BadRequest("You may not delete a watchbill whose first day has already passed.");

            DBSession.Delete(watchbill);
            CommitChanges();

            return NoContent();
        }
    }
}