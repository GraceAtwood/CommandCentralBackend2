using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.CFS
{
    public class CFSMeetingsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof())]
        public IActionResult Get()
        {
            
        }
    }
}