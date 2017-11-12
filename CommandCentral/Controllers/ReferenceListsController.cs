using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using CommandCentral.Utilities;
using NHibernate.Criterion;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Provides access to the reference lists and their individual items.  Reference lists are used to provide (sometimes editable) options to different fields throughout the application.
    /// In addition to requiring access to admin tools to modify a list, a list itself must also be editable.
    /// Information on the editability of a list can be obtained from the GET endpoint.
    /// </summary>
    public class ReferenceListsController : CommandCentralController
    {
        /// <summary>
        /// Contains a mapping of all reference list names to their matching types.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> _referenceListNamesToType;
        static ReferenceListsController()
        {
            _referenceListNamesToType = new ConcurrentDictionary<string, Type>(
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => typeof(ReferenceListItemBase).IsAssignableFrom(x))
                    .ToDictionary(x => x.Name, x => x), StringComparer.CurrentCultureIgnoreCase);
        }
        
        /// <summary>
        /// Retrieves reference lists.
        /// We highly recommend using the 'types' filter if possible in order to load only the data you need and avoid wasting data.
        /// </summary>
        /// <param name="types">An OR-combined string representing a query/filter for the types of a reference list.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.ReferenceList.GetList>))]
        public IActionResult Get([FromQuery] string types)
        {
            // I'm using Query Over here because the Query (Linq to SQL) provider doesn't support multiple
            // type/class queries.  Instead, I needed to use query over in order to use the disjunction stuff.
            // The use of queryover here is ok because we're not doing any joins, so we don't have to deal
            // with the ass syntax of joins that queryover makes us use.  String queries would usually use 
            // the string query from the CommonStrategies class which use Query (Linq to SQL)-specific syntax
            // that QueryOver can't understand.  For this reason, I am also not providing the ability to query
            // the value or description of a reference list as it would require QueryOver-specific methods.

            var query = DBSession.QueryOver<ReferenceListItemBase>();

            var queriedTypes = new List<Type>();

            if (!String.IsNullOrWhiteSpace(types))
            {
                var disjunction = Restrictions.Disjunction();

                foreach (var typeName in types.SplitByOr())
                {
                    if (!_referenceListNamesToType.TryGetValue(typeName, out var type))
                        return BadRequest(
                            $"One or more reference list types supplied in your '{nameof(types)}'" +
                            " parameter were not actual reference list types.");

                    // The "class" property is a special/magic property provided by NHibernate to allow queries against
                    // the type of a class.  This will get translated to a WHERE `clazz` = # query in the post union query.
                    disjunction.Add(Restrictions.Eq("class", type));
                    queriedTypes.Add(type);
                }

                query.Where(disjunction);
            }

            var results = query
                .List()
                .GroupBy(x => x.GetEntityType(DBSession.GetSessionImplementation().PersistenceContext))
                .Select(x => new DTOs.ReferenceList.GetList(x, x.Key))
                .ToList();
            //Insert an empty list for all queries types that returned no results.
            foreach (var type in queriedTypes)
            {
                if (results.All(x => x.Type != type.Name))
                {
                    results.Add(new DTOs.ReferenceList.GetList(new List<ReferenceListItemBase>(), type));
                }
            }

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a single reference list item.
        /// </summary>
        /// <param name="id">The id of the reference list item to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.ReferenceList.Get(item));
        }

        /// <summary>
        /// Creates a new reference list item.  Requires access to admin tools.
        /// </summary>
        /// <param name="dto">A dto containg the necessary information to create a new reference list item.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Post([FromBody] DTOs.ReferenceList.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            if (!_referenceListNamesToType.TryGetValue(dto.Type, out var type))
                return BadRequest(
                    $"The reference list type identified by your parameter '{nameof(dto.Type)}' does not exist.");

            var item = (ReferenceListItemBase) Activator.CreateInstance(type);
            item.Id = Guid.NewGuid();
            item.Value = dto.Value;
            item.Description = dto.Description;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(item);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.ReferenceList.Get(item));
        }

        /// <summary>
        /// Modifies a reference list item.  Requires access to admin tools.
        /// </summary>
        /// <param name="id">The id of the reference list item to modify.</param>
        /// <param name="dto">A dto containing the information to modify the reference list item.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.ReferenceList.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.ReferenceList.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            item.Value = dto.Value;
            item.Description = dto.Description;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
            
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.ReferenceList.Get(item));
        }

        /// <summary>
        /// Removes a reference list item.  Requires access to admin tools.
        /// </summary>
        /// <param name="id">The id of the reference list item to remove.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<ReferenceListItemBase>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            DBSession.Delete(item);
            
            CommitChanges();

            return NoContent();
        }
    }
}