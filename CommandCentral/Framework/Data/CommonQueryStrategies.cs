using CommandCentral.Entities.Muster;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Utilities;
using CommandCentral.Utilities.Types;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Framework.Data
{
    public static class CommonQueryStrategies
    {
        public static IQueryable<T> NullSafeWhere<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                return query;

            return query.Where(predicate);
        }

        public static Expression<Func<T, bool>> AddStringQueryExpression<T>(this Expression<Func<T, bool>> initial, Expression<Func<T, string>> selector, string searchValue)
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            Expression<Func<T, bool>> predicate = null;

            foreach (var phrase in searchValue.SplitByOr())
            {
                Expression<Func<T, bool>> subPredicate = null;

                foreach (var term in phrase.SplitByAnd())
                {
                    subPredicate = subPredicate.NullSafeOr(x => selector.Invoke(x).Contains(term));
                }

                predicate = predicate.NullSafeAnd(subPredicate);
            }

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddReferenceListQueryExpression<T, TProperty>(this Expression<Func<T, bool>> initial, Expression<Func<T, TProperty>> selector, string searchValue) where TProperty : ReferenceListItemBase
        {
            if (String.IsNullOrWhiteSpace(searchValue))
                return initial;

            Expression<Func<T, bool>> predicate = null;

            foreach (var phrase in searchValue.SplitByOr())
            {
                Expression<Func<T, bool>> subPredicate = null;

                if (Guid.TryParse(phrase, out Guid id))
                {
                    subPredicate = subPredicate.NullSafeOr(x => selector.Invoke(x).Id == id);
                }
                else
                {
                    foreach (var term in phrase.SplitByAnd())
                    {
                        subPredicate = subPredicate.NullSafeOr(x => selector.Invoke(x).Value.Contains(term));
                    }
                }

                predicate = predicate.NullSafeAnd(subPredicate);
            }

            return initial.NullSafeAnd(predicate);
        }

        public static Expression<Func<T, bool>> AddDateTimeQueryExpression<T>(this Expression<Func<T, bool>> initial, Expression<Func<T, DateTime?>> selector, DTOs.DateTimeRangeQuery range)
        {
            if (range == null || range.HasNeither())
                return initial;

            initial = initial.NullSafeAnd(x => selector.Invoke(x) != null);

            if (range.HasFromNotTo())
                return initial.NullSafeAnd(x => selector.Invoke(x) >= range.From);
            else if (range.HasToNotFrom())
                return initial.NullSafeAnd(x => selector.Invoke(x) <= range.To);
            else
                return initial.NullSafeAnd(x => selector.Invoke(x) <= range.To && selector.Invoke(x) >= range.From);
        }

        public static Expression<Func<T, bool>> AddDateTimeQueryExpression<T>(this Expression<Func<T, bool>> initial, Expression<Func<T, DateTime>> selector, DTOs.DateTimeRangeQuery range)
        {
            if (range == null || range.HasNeither())
                return initial;

            if (range.HasFromNotTo())
                return initial.NullSafeAnd(x => selector.Invoke(x) >= range.From);
            else if (range.HasToNotFrom())
                return initial.NullSafeAnd(x => selector.Invoke(x) <= range.To);
            else
                return initial.NullSafeAnd(x => selector.Invoke(x) <= range.To && selector.Invoke(x) >= range.From);
        }

        public static Expression<Func<T, bool>> GetTimeRangeQueryExpression<T>(Expression<Func<T, TimeRange>> selector, DTOs.DateTimeRangeQuery range)
        {
            if (range == null || range.HasNeither())
                return null;

            if (range.HasFromNotTo())
                return x => selector.Invoke(x).Start >= range.From || selector.Invoke(x).End >= range.From;
            else if (range.HasToNotFrom())
                return x => selector.Invoke(x).Start <= range.To || selector.Invoke(x).End <= range.To;
            else
                return x => selector.Invoke(x).Start <= range.To && selector.Invoke(x).End >= range.From;
        }
    }
}
