using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public partial class PersonsController
    {
        [HttpGet("{personId}/SubscribedEvents")]
        [RequireAuthentication]
        public IActionResult GetSubscribedEvents(Guid personId)
        {
            var person = DBSession.Get<Person>(personId);
            if (person == null)
                return NotFoundParameter(personId, nameof(personId));

            if (!User.GetFieldPermissions<Person>(person).CanReturn(x => x.SubscribedEvents))
                return Forbid();

            var additionalEvents = new Dictionary<SubscribableEvents, ChainOfCommandLevels>();

            foreach (var subscribableEvent in (SubscribableEvents[]) Enum.GetValues(typeof(SubscribableEvents)))
            {
                if (!person.SubscribedEvents.ContainsKey(subscribableEvent))
                    additionalEvents.Add(subscribableEvent, ChainOfCommandLevels.None);
            }

            return Ok(person.SubscribedEvents.Concat(additionalEvents));
        }

        [HttpPut("{personId}/SubscribedEvents")]
        [RequireAuthentication]
        public IActionResult PutSubscribedEvent(Guid personId,
            [FromBody] List<KeyValuePair<SubscribableEvents, ChainOfCommandLevels>> dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(personId);
            if (person == null)
                return NotFoundParameter(personId, nameof(personId));

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.SubscribedEvents))
                return Forbid();

            person.SubscribedEvents.Clear();
            foreach (var pair in dto.Where(x => x.Value != ChainOfCommandLevels.None))
            {
                person.SubscribedEvents[pair.Key] = pair.Value;
            }

            CommitChanges();
            
            var additionalEvents = new Dictionary<SubscribableEvents, ChainOfCommandLevels>();

            foreach (var subscribableEvent in (SubscribableEvents[]) Enum.GetValues(typeof(SubscribableEvents)))
            {
                if (!person.SubscribedEvents.ContainsKey(subscribableEvent))
                    additionalEvents.Add(subscribableEvent, ChainOfCommandLevels.None);
            }

            return Ok(person.SubscribedEvents.Concat(additionalEvents));
        }
    }
}