using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.DTOs;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class ProfileLockController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get([FromQuery] ProfileLockQueryDTO dto)
        {
            return Ok();
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var profileLock = DBSession.Get<ProfileLock>(id);
            if (profileLock == null)
                return NotFound();

            return Ok(new ProfileLockDTO
            {
                Id = profileLock.Id,
                LockedPerson = profileLock.LockedPerson.Id,
                MaxAge = ProfileLock.MaxAge,
                Owner = profileLock.Owner.Id,
                SubmitTime = profileLock.SubmitTime
            });
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]ProfileLockPostDTO dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {

                var lockedPerson = DBSession.Load<Person>(dto.LockedPerson);

                if (lockedPerson == null)
                    return NotFound();

                //First, we'll ask if the given person is already locked.
                var existingProfileLock = DBSession.QueryOver<ProfileLock>().Where(x => x.LockedPerson.Id == lockedPerson.Id).SingleOrDefault();

                if (existingProfileLock != null)
                {
                    //If the client owns the existing lock on the person, then just update the submit time and return it.
                    if (existingProfileLock.Owner.Id == User.Id)
                    {
                        existingProfileLock.SubmitTime = CallTime;
                        DBSession.Update(existingProfileLock);
                        return Ok(new ProfileLockDTO
                        {
                            Id = existingProfileLock.Id,
                            LockedPerson = existingProfileLock.LockedPerson.Id,
                            MaxAge = ProfileLock.MaxAge,
                            Owner = existingProfileLock.Owner.Id,
                        });
                    }
                    else
                    {
                        //If the client does not own the existing lock, return a forbidden response.
                        return Forbid(new ProfileLockDTO
                        {
                            Id = existingProfileLock.Id,
                            LockedPerson = existingProfileLock.LockedPerson.Id,
                            MaxAge = ProfileLock.MaxAge,
                            Owner = existingProfileLock.Owner.Id,
                            SubmitTime = existingProfileLock.SubmitTime
                        });
                    }
                }

                //Ok there is no existing lock.  We also need to ask if the client already owns a lock elsewhere. 
                //If the client does own a lock elsewhere, we will release it and give them this lock.
                var ownedProfileLock = DBSession.QueryOver<ProfileLock>().Where(x => x.Owner.Id == User.Id).SingleOrDefault();

                if (ownedProfileLock != null)
                {
                    DBSession.Delete(ownedProfileLock);
                }

                var profileLock = new ProfileLock
                {
                    Id = Guid.NewGuid(),
                    LockedPerson = lockedPerson,
                    Owner = User,
                    SubmitTime = CallTime
                };

                DBSession.Save(profileLock);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = profileLock.Id }, new ProfileLockDTO
                {
                    Id = profileLock.Id,
                    SubmitTime = profileLock.SubmitTime,
                    Owner = profileLock.Owner.Id,
                    LockedPerson = profileLock.LockedPerson.Id,
                    MaxAge = ProfileLock.MaxAge
                });
            }
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
