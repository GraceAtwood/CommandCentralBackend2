using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public partial class PersonsController
    {
        /// <summary>
        /// Retrieves all events to which the identified person is subscribed.
        /// </summary>
        /// <param name="personId">The person for whom to retrieve subscribed events.</param>
        /// <returns></returns>
        [HttpGet("{personId}/SubscribedEvents")]
        [ProducesResponseType(200, Type = typeof(List<DTOs.SubscribedEvents.Generic>))]
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

            return Ok(person.SubscribedEvents.Concat(additionalEvents)
                .Select(x => new DTOs.SubscribedEvents.Generic(x)));
        }

        /// <summary>
        /// Modifies the subscribed events collection for the identified person.  
        /// This endpoint simply replaces all of the subscribed events on the person with the given collection. 
        /// Clients may only modify a given person's subscribed events if they have access to that person's SubScribedEvents property.
        /// </summary>
        /// <param name="personId">The person for whom to modify subscribed events.</param>
        /// <param name="dto">A dto containing a collection of subscribed events and levels to subscribe the client to.</param>
        /// <returns></returns>
        [HttpPut("{personId}/SubscribedEvents")]
        [ProducesResponseType(200, Type = typeof(List<DTOs.SubscribedEvents.Generic>))]
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

            return Ok(person.SubscribedEvents.Concat(additionalEvents)
                .Select(x => new DTOs.SubscribedEvents.Generic(x)));
        }
    }
}