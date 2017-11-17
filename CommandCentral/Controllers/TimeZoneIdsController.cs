using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers
{
    public class TimeZoneIdsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<TimeZoneInfo>))]
        public IActionResult Get()
        {
            return Ok(TimeZoneInfo.GetSystemTimeZones());
        }
    }
}