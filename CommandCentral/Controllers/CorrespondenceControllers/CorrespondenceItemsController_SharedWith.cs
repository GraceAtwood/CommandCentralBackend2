using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;
using CommandCentral.Enums;
using CommandCentral.Events;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Dialect.Schema;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CorrespondenceControllers
{
    public partial class CorrespondenceItemsController
    {
        /// <summary>
        /// Gets the persons the given correspondence item is shared with.
        /// </summary>
        /// <param name="correspondenceItemId">Id of the correspondence item for which you want to know who it is shared with.</param>
        /// <returns></returns>
        [HttpGet("{correspondenceItemId}/SharedWith")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Person.Get>))]
        public IActionResult GetSharedWith(Guid correspondenceItemId)
        {
            var item = DBSession.Get<CorrespondenceItem>(correspondenceItemId);
            if (item == null)
                return NotFoundParameter(correspondenceItemId, nameof(correspondenceItemId));

            if (!item.CanPersonViewItem(User))
                return Forbid();

            var result = item.SharedWith.Select(person =>
                    new DTOs.Person.Get(person, User.GetFieldPermissions<Person>(person)))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Modified the persons a corr item is shared with by setting the collection to the value given.  Additions and deletions will be figured out for you.
        /// </summary>
        /// <param name="correspondenceItemId">Id of the correspondence item for which to modify the shared with collection.</param>
        /// <param name="personIds">A list of Ids representing the persons to share this corr item with.</param>
        /// <returns></returns>
        [HttpPut("{correspondenceItemId}/SharedWith")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(List<DTOs.Person.Get>))]
        public IActionResult PutSharedWith(Guid correspondenceItemId, [FromBody] List<Guid> personIds)
        {
            if (personIds == null)
                return BadRequestDTONull();

            var item = DBSession.Get<CorrespondenceItem>(correspondenceItemId);
            if (item == null)
                return NotFoundParameter(correspondenceItemId, nameof(correspondenceItemId));

            if (!item.CanPersonEditItem(User))
                return Forbid();

            var personsFromClient = new HashSet<Person>(DBSession.Query<Person>()
                .Where(x => personIds.Contains(x.Id))
                .ToList());

            if (personsFromClient.Count != personIds.Count)
                return BadRequest("One or more of the ids you provided were not valid.");

            var personsFromDB = new HashSet<Person>(item.SharedWith);

            var removed = personsFromDB.Where(x => !personsFromClient.Contains(x));
            var added = personsFromClient.Where(x => !personsFromDB.Contains(x));

            //Do this here instead of reassigning the collection so that NHibernate doesn't try to just delete everything.
            item.SharedWith.Clear();
            foreach (var person in personsFromClient)
            {
                item.SharedWith.Add(person);
            }

            CommitChanges();

            EventManager.OnCorrespondenceShared(new CorrespondenceItemSharedEventArgs
            {
                Item = item,
                Added = added,
                Removed = removed
            }, this);

            return CreatedAtAction(nameof(GetSharedWith), new {correspondenceItemId = item.Id},
                item.SharedWith.Select(person =>
                {
                    var permissions = User.GetFieldPermissions<Person>(person);
                    return new DTOs.Person.Get(person, permissions);
                }).ToList());
        }
    }
}