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
    public class EmailAddressController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult GetByPerson([FromQuery]Guid Person)
        {
            if (Person == Guid.Empty)
                return BadRequest("Query string with a person id is missing or malformed. Loading all email addresses is not allowed.");

            var items = DBSession.QueryOver<EmailAddress>().Where(x => x.Person.Id == Person).List();

            if (!items.Any())
                return NotFound();

            var isInChainOfCommand = new ResolvedPermissions(User, items.First().Person).IsInChainOfCommand.Any(x => x.Value);

            IEnumerable<EmailAddress> result;
            if (!isInChainOfCommand)
            {
                result = items.Where(x => x.IsReleasableOutsideCoC);
            }
            else
            {
                result = items;
            }

            return Ok(result.Select(x =>
                new EmailAddressDTO
                {
                    Address = x.Address,
                    IsReleasableOutsideCoC = x.IsReleasableOutsideCoC,
                    Id = x.Id,
                    IsPreferred = x.IsPreferred,
                    Person = x.Person.Id
                })
            );
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<EmailAddress>(id);
            if (item == null)
                return NotFound();

            var v = new ResolvedPermissions(User, item.Person);

            var canSeeEmail = false;
            if (User.Id == item.Person.Id)
            {
                canSeeEmail = true;
            }
            else
            {
                foreach (var l in v.HighestLevels)
                {
                    switch (l.Value)
                    {
                        case Enums.ChainOfCommandLevels.Command:
                            if (User.IsInSameCommandAs(item.Person))
                                canSeeEmail = true;
                            break;
                        case Enums.ChainOfCommandLevels.Department:
                            if (User.IsInSameDepartmentAs(item.Person))
                                canSeeEmail = true;
                            break;
                        case Enums.ChainOfCommandLevels.Division:
                            if (User.IsInSameDivisionAs(item.Person))
                                canSeeEmail = true;
                            break;
                        case Enums.ChainOfCommandLevels.None:
                            break;
                        default:
                            throw new NotImplementedException("Switch statement in EmailAddress Post fell to default.");
    
                    }
                }
            }

            if (item.IsReleasableOutsideCoC || canSeeEmail)
            {
                return Ok(new EmailAddressDTO
                {
                    Id = item.Id,
                    Address = item.Address,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]EmailAddressPostDTO dto)
        {
            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
            {
                return BadRequest("No person exists for that Id");
            }

            if (!new ResolvedPermissions(User, person).FieldPermissions[typeof(Person)][nameof(Person.EmailAddresses)].CanEdit)
            {
                return Unauthorized("You do not have permission to add email addresses for this user.");
            }

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = new EmailAddress
                {
                    Id = Guid.NewGuid(),
                    Address = dto.Address,
                    IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                    Person = person,
                    IsPreferred = dto.IsPreferred
                };

                var result = new EmailAddress.EmailAddressValidator().Validate(item);
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new EmailAddressDTO
                {
                    Id = item.Id,
                    Address = item.Address,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
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
