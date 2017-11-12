using System;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public class ProfileLocksController : CommandCentralController
    {
        [HttpGet("me")]
        [ProducesResponseType(200, Type = typeof(DTOs.ProfileLock.Get))]
        public IActionResult GetMe()
        {
            var profileLock = DBSession.Query<ProfileLock>()
                .SingleOrDefault(x => x.Owner == User);

            if (profileLock == null)
                return NotFound("The client owns no profile lock.");

            return Ok(new DTOs.ProfileLock.Get(profileLock));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.ProfileLock.Get))]
        public IActionResult Get(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var profileLock = DBSession.Get<ProfileLock>(id);
            if (profileLock == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.ProfileLock.Get(profileLock));
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.ProfileLock.Get))]
        public IActionResult Post([FromBody] DTOs.ProfileLock.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var lockedPerson = DBSession.Get<Person>(dto.LockedPerson);

            if (lockedPerson == null)
                return NotFoundParameter(dto.LockedPerson, nameof(dto.LockedPerson));

            //First, we'll ask if the given person is already locked.
            var existingProfileLock = DBSession.Query<ProfileLock>()
                .SingleOrDefault(x => x.LockedPerson.Id == lockedPerson.Id);

            if (existingProfileLock != null)
            {
                if (existingProfileLock.Owner.Id != User.Id)
                    return Forbid();

                existingProfileLock.SubmitTime = CallTime;

                CommitChanges();

                return Ok(new DTOs.ProfileLock.Get(existingProfileLock));
            }

            //Ok there is no existing lock.  We also need to ask if the client already owns a lock elsewhere. 
            //If the client does own a lock elsewhere, we will release it and give them this lock.
            var ownedProfileLock = DBSession.Query<ProfileLock>()
                .SingleOrDefault(x => x.Owner == User);

            var profileLock = new ProfileLock
            {
                Id = Guid.NewGuid(),
                LockedPerson = lockedPerson,
                Owner = User,
                SubmitTime = CallTime
            };

            if (ownedProfileLock != null)
            {
                DBSession.Delete(ownedProfileLock);
            }

            DBSession.Save(profileLock);

            CommitChanges();

            return CreatedAtAction(nameof(GetMe), new DTOs.ProfileLock.Get(profileLock));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Load<ProfileLock>(id);

            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (item.Owner.Id != User.Id)
                return Forbid();

            DBSession.Delete(item);

            CommitChanges();

            return NoContent();
        }
    }
}