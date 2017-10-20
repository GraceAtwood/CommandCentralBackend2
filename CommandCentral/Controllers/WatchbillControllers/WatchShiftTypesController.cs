using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.WatchbillControllers
{
    public class WatchShiftTypesController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.WatchShiftType.Get>))]
        public IActionResult Get()
        {
            var items = DBSession.Query<WatchShiftType>().ToList();

            return Ok(items.Select(x => new DTOs.WatchShiftType.Get(x)));
        }
        
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.WatchShiftType.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<WatchShiftType>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.WatchShiftType.Get(item));
        }
    }
}