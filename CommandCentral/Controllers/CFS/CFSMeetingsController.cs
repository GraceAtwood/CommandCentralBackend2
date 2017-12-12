using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs.Custom;
using CommandCentral.Entities;
using CommandCentral.Entities.CFS;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.CFS
{
    public class CFSMeetingsController : CommandCentralController
    {
        /// <summary>
        /// Provides querying functionality of the CFS meetings.  You may not query for anything but yourself in the "person" 
        /// field if you are not in the CFS chain of command.  Additionally, if you are not in the CFS chain of command, all other 
        /// query terms will be ignored besides "person."  Finally, if you are in the CFS chain of command, a chain of command 
        /// query qualification will be automatically added to your query to limit results to your permission level.
        /// </summary>
        /// <param name="range">A date range query for when the meeting took place.</param>
        /// <param name="person">A person query for the person who the meeting was held for.</param>
        /// <param name="advisor">A person query for the person who held the meeting.</param>
        /// <param name="notes">A string query for the text of the notes.</param>
        /// <param name="request">A query for the request to which a meeting is tied to.  You may query either by the id of a request or the request type (by text).</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CFSMeeting.Get>))]
        public IActionResult Get([FromQuery] DateTimeRangeQuery range, [FromQuery] string person,
            [FromQuery] string advisor, [FromQuery] string notes, [FromQuery] string request)
        {
            //If the client isn't in the CFS chain of command, they can only query for themselves by Id.
            if (User.GetHighestAccessLevels()[ChainsOfCommand.CommandFinancialSpecialist] == ChainOfCommandLevels.None)
            {
                if (String.IsNullOrWhiteSpace(person) || !Guid.TryParse(person, out var id) || User.Id != id)
                {
                    return Forbid("You may not query CFS meetings for anyone " +
                                  "but yourself if you're not in the CFS chain of command.");
                }

                return Ok(DBSession.Query<Meeting>().Where(x => x.Person.Id == id).ToList()
                    .Select(x => new DTOs.CFSMeeting.Get(x)));
            }

            //Alright!  The client is in the CFS chain of command.  Here we go!
            var expression = ((Expression<Func<Meeting, bool>>) null)
                .AddTimeRangeQueryExpression(x => x.Range, range)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddPersonQueryExpression(x => x.Advisor, advisor)
                .AddStringQueryExpression(x => x.Notes, notes);

            //We need to add a custom request query here.  It's not in the common query strats because I don't think it'll be used elsewhere.
            if (!String.IsNullOrWhiteSpace(request))
            {
                var subExpression = request.SplitByOr()
                    .Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<Meeting, bool>>) null).NullSafeAnd(x => x.Request.Id == id);

                        return phrase.SplitByAnd()
                            .Aggregate((Expression<Func<Meeting, bool>>) null,
                                (current, term) =>
                                    current.NullSafeAnd(x => x.Request.RequestType.Value.Contains(term)));
                    })
                    .Aggregate<Expression<Func<Meeting, bool>>, Expression<Func<Meeting, bool>>>(null,
                        (current1, subPredicate) => current1.NullSafeOr(subPredicate));

                expression = expression.NullSafeAnd(subExpression);
            }

            //Finally, add the chain of command expression to limit the visible set to sailors in the user's chain of command.
            expression = expression.AddIsPersonInChainOfCommandExpression(x => x.Person, User,
                ChainsOfCommand.CommandFinancialSpecialist);

            var results = DBSession.Query<Meeting>()
                .AsExpandable()
                .NullSafeWhere(expression)
                .ToList()
                .Select(x => new DTOs.CFSMeeting.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a single meeting by its Id.
        /// </summary>
        /// <param name="id">The id of the meeting to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.CFSMeeting.Get))]
        public IActionResult Get(Guid id)
        {
            var meeting = DBSession.Get<Meeting>(id);

            if (!User.IsInChainOfCommand(meeting.Person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You may not view this CFS meeing.  " +
                              "You are not in the chain of command of the person for whom the meeting was held.");

            return Ok(new DTOs.CFSMeeting.Get(meeting));
        }

        /// <summary>
        /// Creates a new CFS meeting.  
        /// Meetings are meant to be created after they have happened as a way of "catalogging" their occurence.
        /// </summary>
        /// <param name="dto">A dto containing all of the information needed to create a new meeting.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.CFSMeeting.Get))]
        public IActionResult Post([FromBody] DTOs.CFSMeeting.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (User.GetHighestAccessLevels()[ChainsOfCommand.CommandFinancialSpecialist] == ChainOfCommandLevels.None)
                return Forbid("You may not create a meeting if you're not in the CFS chain of command.");

            var advisor = DBSession.Get<Person>(dto.Advisor);
            if (advisor == null)
                return NotFoundParameter(dto.Advisor, nameof(dto.Advisor));

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            var request = DBSession.Get<Request>(dto.Request);
            if (request == null)
                return NotFoundParameter(dto.Request, nameof(dto.Request));

            if (!User.IsInChainOfCommand(person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You may not create meetings for a person for whom you are not in the chain of command.");

            var meeting = new Meeting
            {
                Advisor = advisor,
                Id = Guid.NewGuid(),
                Notes = dto.Notes,
                Person = person,
                Range = dto.Range,
                Request = request
            };

            var results = meeting.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(meeting);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = meeting.Id}, new DTOs.CFSMeeting.Get(meeting));
        }

        /// <summary>
        /// Modifies the notes of a meeting, allowing CFS members to change the notes later.
        /// </summary>
        /// <param name="id">The id of the meeting for which you want to modify the notes.</param>
        /// <param name="dto">A dto containing all of the inforamtion needed to modify a meeting.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.CFSMeeting.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CFSMeeting.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var meeting = DBSession.Get<Meeting>(id);
            if (meeting == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.IsInChainOfCommand(meeting.Person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You may not modify a meeting that was held for a Sailor not in your chain of command.");

            meeting.Notes = dto.Notes;
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = meeting.Id}, new DTOs.CFSMeeting.Get(meeting));
        }
    }
}