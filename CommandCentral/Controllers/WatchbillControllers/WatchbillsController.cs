using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
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
            var items = DBSession.Query<Watchbill>().ToList();

            return Ok(items.Select(x => new DTOs.Watchbill.Get(x)));
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

//        [HttpPost]
//        [RequireAuthentication]
//        [ProducesResponseType(200, Type = typeof(DTOs.Watchbill.Get))]
//        public IActionResult Post([FromBody] DTOs.Watchbill.Post dto)
//        {
//            if (dto == null)
//                return BadRequest();
//
//            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
//                Enums.ChainOfCommandLevels.Command)
//                return Forbid();
//
//            var watchbill = new Watchbill
//            {
//                Title = dto.Title,
//                Year = dto.Year,
//                Month = dto.Month,
//                Command = dto.Command,
//                Phase = Enums.WatchbillPhases.Initial
//            };
//
//            var result = watchbill.Validate();
//            if (!result.IsValid)
//                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
//
//            DBSession.Save(watchbill);
//
//            CommitChanges();
//
//            return CreatedAtAction(nameof(Get),
//                new {id = watchbill.Id},
//                new DTOs.Watchbill.Get(watchbill));
//        }

//        [HttpDelete("{id}")]
//        [RequireAuthentication]
//        [ProducesResponseType(200)]
//        public IActionResult Delete(Guid id)
//        {
//            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
//                Enums.ChainOfCommandLevels.Command)
//                return Forbid();
//
//            var watchbill = DBSession.Get<Watchbill>(id);
//
//            if (watchbill == null)
//                return NotFound();
//
//            DBSession.Delete(watchbill);
//            
//            CommitChanges();
//
//            return NoContent();
//        }
    }
}