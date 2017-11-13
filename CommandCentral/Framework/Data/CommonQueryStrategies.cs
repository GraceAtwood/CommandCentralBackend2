using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Utilities;
using CommandCentral.Utilities.Types;
using LinqKit;
using System;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Framework.Data
{
    public static class CommonQueryStrategies
    {
        public static Expression<Func<Person, bool>> IsPersonInChainOfCommandExpression(Person person)
        {
            var divisionLevelGroups = PermissionsCache.PermissionGroupsCache.Values
                .Where(x => x.AccessLevels.Any(y => y.Value == ChainOfCommandLevels.Division))
                .Select(x => x.Name)
                .ToList();
            var departmentLevelGroups = PermissionsCache.PermissionGroupsCache.Values
                .Where(x => x.AccessLevels.Any(y => y.Value == ChainOfCommandLevels.Division))
                .Select(x => x.Name)
                .ToList();
            var commandLevelGroups = PermissionsCache.PermissionGroupsCache.Values
                .Where(x => x.AccessLevels.Any(y => y.Value == ChainOfCommandLevels.Division))
                .Select(x => x.Name)
                .ToList();

            return x => x.PermissionGroups.Any(y => commandLevelGroups.Contains(y.Name)) &&
                        x.Division.Department.Command == person.Division.Department.Command ||
                        x.PermissionGroups.Any(y => departmentLevelGroups.Contains(y.Name)) &&
                        x.Division.Department == person.Division.Department ||
                        x.PermissionGroups.Any(y => divisionLevelGroups.Contains(y.Name)) &&
                        x.Division == person.Division;
        }

        public static Expression<Func<Person, bool>> GetPersonsSubscribedToEventForPersonExpression(
            SubscribableEvents subscribableEvent, Person person)
        {
            return x => x.SubscribedEvents.ContainsKey(subscribableEvent) &&
                        (x.SubscribedEvents[subscribableEvent] == ChainOfCommandLevels.Command &&
                         x.Division.Department.Command == person.Division.Department.Command ||
                         x.SubscribedEvents[subscribableEvent] == ChainOfCommandLevels.Department &&
                         x.Division.Department == person.Division.Department ||
                         x.SubscribedEvents[subscribableEvent] == ChainOfCommandLevels.Division &&
                         x.Division == person.Division);
        }

        public static Expression<Func<Person, bool>> GetPersonsSubscribedToEventAtLevelExpression(
            SubscribableEvents subscribableEvent, ChainOfCommandLevels level)
        {
            return x => x.SubscribedEvents.ContainsKey(subscribableEvent) &&
                        x.SubscribedEvents[subscribableEvent] == level;
        }

        public static IQueryable<T> NullSafeWhere<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return predicate == null
                ? query
                : query.Where(predicate);
        }

        public static Expression<Func<T, bool>> AddStringQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, string>> selector, string searchValue)
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            var predicate = searchValue
                .SplitByOr()
                .Select(phrase =>
                    phrase.SplitByAnd()
                        .Aggregate<string, Expression<Func<T, bool>>>(null, (current, term) =>
                            current.NullSafeAnd(x => selector.Invoke(x).Contains(term))))
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddIntQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, int>> selector, string searchValue)
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            Expression<Func<T, bool>> predicate = null;

            foreach (var term in searchValue.SplitByOr())
            {
                if (Int32.TryParse(term, out var number))
                {
                    predicate = predicate.NullSafeOr(x => selector.Invoke(x) == number);
                }
            }

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddReferenceListQueryExpression<T, TProperty>(
            this Expression<Func<T, bool>> initial, Expression<Func<T, TProperty>> selector, string searchValue)
            where TProperty : ReferenceListItemBase
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            var predicate = searchValue.SplitByOr()
                .Select(phrase =>
                {
                    if (Guid.TryParse(phrase, out var id))
                        return ((Expression<Func<T, bool>>) null).And(x => selector.Invoke(x).Id == id);

                    return phrase.SplitByAnd()
                        .Aggregate((Expression<Func<T, bool>>) null,
                            (current, term) => current.And(x => selector.Invoke(x).Value.Contains(term)));
                })
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddPersonQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, Person>> selector, string searchValue)
        {
            var expression = GetPersonQueryExpression(selector, searchValue);
            return expression == null
                ? initial
                : initial.NullSafeAnd(expression);
        }

        public static Expression<Func<T, bool>> GetPersonQueryExpression<T>(Expression<Func<T, Person>> selector,
            string searchValue)
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return null;

            return searchValue.SplitByOr()
                .Select(phrase =>
                {
                    if (Guid.TryParse(phrase, out var id))
                        return ((Expression<Func<T, bool>>) null).NullSafeAnd(x => selector.Invoke(x).Id == id);

                    return phrase.SplitByAnd()
                        .Aggregate((Expression<Func<T, bool>>) null,
                            (current, term) => current.NullSafeAnd(x =>
                                selector.Invoke(x).FirstName.Contains(term) ||
                                selector.Invoke(x).LastName.Contains(term) ||
                                selector.Invoke(x).MiddleName.Contains(term) ||
                                selector.Invoke(x).Division.Name.Contains(term) ||
                                selector.Invoke(x).Division.Department.Name.Contains(term) ||
                                EnumUtilities.GetPartialValueMatches<Paygrades>(term)
                                    .Contains(selector.Invoke(x).Paygrade) ||
                                selector.Invoke(x).UIC.Value.Contains(term) ||
                                selector.Invoke(x).Designation.Value.Contains(term)));
                })
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));
        }

        public static Expression<Func<T, bool>> AddDateTimeQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, DateTime?>> selector, DTOs.DateTimeRangeQuery range)
        {
            if (range == null || range.HasNeither())
                return initial;

            initial = initial.NullSafeAnd(x => selector.Invoke(x) != null);

            if (range.HasFromNotTo())
                return initial.NullSafeAnd(x => selector.Invoke(x) >= range.From);

            return range.HasToNotFrom()
                ? initial.NullSafeAnd(x => selector.Invoke(x) <= range.To)
                : initial.NullSafeAnd(x => selector.Invoke(x) <= range.To && selector.Invoke(x) >= range.From);
        }

        public static Expression<Func<T, bool>> AddDateTimeQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, DateTime>> selector, DTOs.DateTimeRangeQuery range)
        {
            if (range == null || range.HasNeither())
                return initial;

            if (range.HasFromNotTo())
                return initial.NullSafeAnd(x => selector.Invoke(x) >= range.From);

            return range.HasToNotFrom()
                ? initial.NullSafeAnd(x => selector.Invoke(x) <= range.To)
                : initial.NullSafeAnd(x => selector.Invoke(x) <= range.To && selector.Invoke(x) >= range.From);
        }

        public static Expression<Func<T, bool>> AddTimeRangeQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, TimeRange>> selector, DTOs.DateTimeRangeQuery range)
        {
            var expression = GetTimeRangeQueryExpression(selector, range);
            return expression == null
                ? initial
                : initial.NullSafeAnd(expression);
        }

        public static Expression<Func<T, bool>> GetTimeRangeQueryExpression<T>(Expression<Func<T, TimeRange>> selector,
            DTOs.DateTimeRangeQuery range)
        {
            if (range == null || range.HasNeither())
                return null;

            if (range.HasFromNotTo())
                return x => selector.Invoke(x).Start >= range.From || selector.Invoke(x).End >= range.From;

            if (range.HasToNotFrom())
                return x => selector.Invoke(x).Start <= range.To || selector.Invoke(x).End <= range.To;

            return x => selector.Invoke(x).Start <= range.To && selector.Invoke(x).End >= range.From;
        }

        public static Expression<Func<T, bool>> AddNullableBoolQueryExpression<T>(
            this Expression<Func<T, bool>> initial, Expression<Func<T, bool?>> selector, bool? value)
        {
            return !value.HasValue
                ? initial
                : initial.NullSafeAnd(x => selector.Invoke(x) == value);
        }

        public static Expression<Func<T, bool>> AddCommandQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, Command>> selector, string searchValue)
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            var predicate = searchValue.SplitByOr()
                .Select(phrase =>
                {
                    if (Guid.TryParse(phrase, out var id))
                        return ((Expression<Func<T, bool>>) null).And(x => selector.Invoke(x).Id == id);

                    return phrase.SplitByAnd()
                        .Aggregate((Expression<Func<T, bool>>) null,
                            (current, term) => current.And(x => selector.Invoke(x).Name.Contains(term)));
                })
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddDepartmentQueryExpression<T>(this Expression<Func<T, bool>> initial,
            Expression<Func<T, Department>> selector, string searchValue)
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            var predicate = searchValue.SplitByOr()
                .Select(phrase =>
                {
                    if (Guid.TryParse(phrase, out var id))
                        return ((Expression<Func<T, bool>>) null).And(x => selector.Invoke(x).Id == id);

                    return phrase.SplitByAnd()
                        .Aggregate((Expression<Func<T, bool>>) null,
                            (current, term) => current.And(x => selector.Invoke(x).Name.Contains(term)));
                })
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));

            return initial.NullSafeAnd(predicate);
        }

        /// <summary>
        /// Adds a query expression for the given enum type to this initial expression.  
        /// Null safe.  If the initial expression is null, this resulting expression is returned.  
        /// If the search value is null, the initial expression is returned.  
        /// The query is first split by the Or operator (<seealso cref="StringUtilities.SplitByOr"/>) then 
        /// the query is split by the And operator (<seealso cref="StringUtilities.SplitByAnd"/>).  
        /// The query terms are parsed to the correspondeing Enum using a case insensitive parse.  
        /// Any values that fail parsing are simply thrown out of the query by not being added to the resulting expression.
        /// </summary>
        /// <param name="initial">The starting, "seed" expression.  Can be null.</param>
        /// <param name="selector">A selector for the property to search on.</param>
        /// <param name="searchValue">A string containing the search query.</param>
        /// <typeparam name="T">The initial, starting type of the query.</typeparam>
        /// <typeparam name="TEnum">An enumerated type.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if <seealso cref="TEnum"/> is not an enumerated type.</exception>
        public static Expression<Func<T, bool>> AddExactEnumQueryExpression<T, TEnum>(
            this Expression<Func<T, bool>> initial, Expression<Func<T, TEnum>> selector, string searchValue)
            where TEnum : struct, IConvertible
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException($"{nameof(TEnum)} must be an enumerated type.", nameof(TEnum));

            var predicate = searchValue.SplitByOr()
                .Select(phrase =>
                {
                    Expression<Func<T, bool>> subPredicate = null;
                    foreach (var term in phrase.SplitByAnd())
                    {
                        if (Enum.TryParse(term, true, out TEnum value))
                        {
                            subPredicate = subPredicate.NullSafeAnd(x => selector.Invoke(x).Equals(value));
                        }
                    }
                    return subPredicate;
                })
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddPartialEnumQueryExpression<T, TEnum>(
            this Expression<Func<T, bool>> initial, Expression<Func<T, TEnum>> selector, string searchValue)
            where TEnum : struct, IConvertible
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;
            
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException($"{nameof(TEnum)} must be an enumerated type.", nameof(TEnum));

            var predicate = searchValue.SplitByOr()
                .Select(phrase =>
                {
                    Expression<Func<T, bool>> subPredicate = null;
                    foreach (var term in phrase.SplitByAnd())
                    {
                        var possibleValues = EnumUtilities.GetPartialValueMatches<TEnum>(term).ToList();
                        if (possibleValues.Any())
                            subPredicate = subPredicate.NullSafeAnd(x => possibleValues.Contains(selector.Invoke(x)));
                    }
                    return subPredicate;
                })
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current1, subPredicate) => current1.NullSafeOr(subPredicate));

            return initial.NullSafeAnd(predicate);
        }
    }
}