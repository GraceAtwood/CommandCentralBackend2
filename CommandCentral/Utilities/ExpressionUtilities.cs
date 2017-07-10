using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class ExpressionUtilities
    {

        /// <summary>
        /// For the given property of the given type, returns the name of that property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(this Expression<Func<T, object>> expression)
        {
            return GetProperty(expression).Name;
        }

        /// <summary>
        /// For the given property of a given type, returned the member info of that property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberInfo GetProperty<T>(this Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression memberExp)
                return memberExp.Member;

            // for unary types like datetime or guid
            if (expression.Body is UnaryExpression unaryExp)
            {
                memberExp = unaryExp.Operand as MemberExpression;
                if (memberExp != null)
                    return memberExp.Member;
            }

            throw new ArgumentException("'expression' should be a member expression or a method call expression.", "expression");
        }

    }
}
