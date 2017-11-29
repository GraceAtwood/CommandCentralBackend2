using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommandCentral.Entities;
using CommandCentral.Utilities;

namespace CommandCentral.Authorization
{
    public class PropertyGroup<T>
    {
        public HashSet<PropertyInfo> Properties { get; set; }

        public Func<Person, T, bool> CanEditRule { get; set; }

        public Func<Person, T, bool> CanReturnRule { get; set; }

        public PropertyGroup(Expression<Func<T, object>>[] propertySelectors)
        {
            if (propertySelectors == null || !propertySelectors.Any())
            {
                Properties = new HashSet<PropertyInfo>(typeof(T).GetProperties());
            }
            else
            {
                Properties = new HashSet<PropertyInfo>(propertySelectors.Select(x => (PropertyInfo)x.GetProperty()));
            }
        }
        
        public PropertyGroup<T> CanEdit(Func<Person, T, bool> canEditRule)
        {
            if (CanEditRule != null)
                throw new Exception("You can't set the can edit rule again.");
            CanEditRule = canEditRule;
            return this;
        }

        public PropertyGroup<T> CanReturn(Func<Person, T, bool> canReturnRule)
        {
            if (CanReturnRule != null)
                throw new Exception("You can't set the can return rule again.");
            CanReturnRule = canReturnRule;
            return this;
        }
    }
}