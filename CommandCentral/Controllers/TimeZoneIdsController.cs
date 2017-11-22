using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Exposes all the system time zones.
    /// </summary>
    public class TimeZoneIdsController : CommandCentralController
    {
        /// <summary>
        /// Exposes all the system time zones.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<TimeZoneInfo>))]
        public IActionResult Get()
        {
            return Ok(TimeZoneInfo.GetSystemTimeZones());
        }
    }
}