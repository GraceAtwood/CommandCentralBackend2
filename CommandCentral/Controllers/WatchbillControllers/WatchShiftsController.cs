using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs.Custom;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    /// <summary>
    /// Provides access to the watch shifts of a watchbill.
    /// </summary>
    public class WatchShiftsController : CommandCentralController
    {
        /// <summary>
        /// Queries the watch shifts collection.  Results are automatically order by Range.Start in descending order.
        /// </summary>
        /// <param name="watchbill">A query for the watchbill of a watch shift.</param>
        /// <param name="title">A string query for the title.</param>
        /// <param name="range">A date range query for the range.</param>
        /// <param name="hasWatchAssignment">A boolean query for if the watch assignment if set to null or not.</param>
        /// <param name="shiftType">A query for the watch shift type.</param>
        /// <param name="divisionAssignedTo">A division query for the division.</param>
        /// <param name="limit">[Optional][Default = 1000] Instructs the service to return no more than this many results.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.WatchShift.Get>))]
        public IActionResult Get([FromQuery] string watchbill, [FromQuery] string title,
            [FromQuery] DateTimeRangeQuery range, [FromQuery] bool? hasWatchAssignment,
            [FromQuery] string shiftType, [FromQuery] string divisionAssignedTo, [FromQuery] int limit = 1000)
        {
            var predicate = ((Expression<Func<WatchShift, bool>>) null)
                .AddStringQueryExpression(x => x.Title, title)
                .AddTimeRangeQueryExpression(x => x.Range, range)
                .AddDivisionQueryExpression(x => x.DivisionAssignedTo, divisionAssignedTo);

            if (hasWatchAssignment.HasValue)
                predicate = hasWatchAssignment.Value
                    ? predicate.NullSafeAnd(x => x.WatchAssignment != null)
                    : predicate.NullSafeAnd(x => x.WatchAssignment == null);

            if (!String.IsNullOrWhiteSpace(shiftType))
            {
                predicate = predicate.NullSafeAnd(shiftType.SplitByOr()
                    .Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<WatchShift, bool>>) null).And(x => x.ShiftType.Id == id);


                        return phrase.SplitByAnd()
                            .Aggregate((Expression<Func<WatchShift, bool>>) null,
                                (current, term) => current.And(x => x.ShiftType.Name.Contains(term)));
                    })
                    .Aggregate<Expression<Func<WatchShift, bool>>, Expression<Func<WatchShift, bool>>>(null,
                        (current1, subPredicate) => current1.NullSafeOr(subPredicate)));
            }

            if (!String.IsNullOrWhiteSpace(watchbill))
            {
                predicate = predicate.NullSafeAnd(watchbill.SplitByOr()
                    .Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<WatchShift, bool>>) null).And(x => x.Watchbill.Id == id);


                        return phrase.SplitByAnd()
                            .Aggregate((Expression<Func<WatchShift, bool>>) null,
                                (current, term) => current.And(x => x.Watchbill.Title.Contains(term)));
                    })
                    .Aggregate<Expression<Func<WatchShift, bool>>, Expression<Func<WatchShift, bool>>>(null,
                        (current1, subPredicate) => current1.NullSafeOr(subPredicate)));
            }

            var results = DBSession.Query<WatchShift>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.Range.Start)
                .Take(limit)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.WatchShift.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a watch shift.
        /// </summary>
        /// <param name="id">The id of the object to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DTOs.WatchShift.Get), 200)]
        public IActionResult Get(Guid id)
        {
            if (!TryGet(id, out WatchShift shift))
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(shift))
                return Forbid("You can't view this item.");

            return Ok(new DTOs.WatchShift.Get(shift));
        }

        /// <summary>
        /// Creates a new watch shift.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to create a new watch shift.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(DTOs.WatchShift.Get), 201)]
        public IActionResult Post([FromBody] DTOs.WatchShift.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!TryGet(dto.Watchbill, out Watchbill watchbill))
                return NotFoundParameter(dto.Watchbill, nameof(dto.Watchbill));

            if (!TryGet(dto.ShiftType, out WatchShiftType shiftType))
                return NotFoundParameter(dto.ShiftType, nameof(dto.ShiftType));

            var shift = new WatchShift
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                ShiftType = shiftType,
                Range = dto.Range,
                Watchbill = watchbill
            };

            var result = shift.Validate();
            if (!result.IsValid)
                return BadRequestWithValidationErrors(result);

            if (!User.CanEdit(shift))
                return Forbid("You can't create a new watch shift.");

            Save(shift);
            LogEntityCreation(shift);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = shift.Id}, new DTOs.WatchShift.Get(shift));
        }

        /// <summary>
        /// Modifies a watch shift.
        /// </summary>
        /// <param name="id">The id of the watch shift.</param>
        /// <param name="dto">A dto containing the information needed to modify a watch shift.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchShift.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.WatchShift.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!TryGet(id, out WatchShift shift))
                return NotFoundParameter(id, nameof(id));

            if (!TryGet(dto.ShiftType, out WatchShiftType shiftType))
                return NotFoundParameter(dto.ShiftType, nameof(dto.ShiftType));

            shift.Title = dto.Title;
            shift.Range = dto.Range;
            shift.ShiftType = shiftType;

            var result = shift.Validate();
            if (!result.IsValid)
                return BadRequestWithValidationErrors(result);

            if (!User.CanEdit(shift))
                return Forbid("You can't edit the shift.");

            LogEntityModification(shift);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = shift.Id}, new DTOs.WatchShift.Get(shift));
        }

        /// <summary>
        /// Deletes a watch shift.
        /// </summary>
        /// <param name="id">The id of the watch shift to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!TryGet(id, out WatchShift shift))
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(shift))
                return Forbid("You can't delete this shift.");

            Delete(shift);
            LogEntityDeletion(shift);
            CommitChanges();

            return NoContent();
        }
    }
}