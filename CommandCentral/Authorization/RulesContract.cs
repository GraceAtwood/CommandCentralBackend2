using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities;
using CommandCentral.Utilities;

namespace CommandCentral.Authorization
{
    public abstract class RulesContract<T> : BaseRulesContract where T : Entities.Entity
    {
        public List<PropertyGroup<T>> PropertyGroups { get; set; } = new List<PropertyGroup<T>>();

        public PropertyGroup<T> RulesFor(params Expression<Func<T, object>>[] propertySelectors)
        {
            if (propertySelectors != null && propertySelectors.Any() && propertySelectors.Any(expression =>
                    PropertyGroups.Any(group => group.Properties.Contains(expression.GetProperty()))))
                throw new Exception("There is already a rule with this property.");

            PropertyGroups.Add(new PropertyGroup<T>(propertySelectors));
            return PropertyGroups.Last();
        }

        public bool CanEditAnyProperty(Person editor, T subject)
        {
            var group = PropertyGroups.FirstOrDefault() ??
                        throw new Exception("Unable to find a group.");

            return group.CanEditRule(editor, subject);
        }
        
        public bool CanEditProperty(Person editor, T subject, Expression<Func<T, object>> propertySelector)
        {
            var group = PropertyGroups.SingleOrDefault(x => x.Properties.Contains(propertySelector.GetProperty())) ??
                        throw new Exception("Unable to find that property!");

            return group.CanEditRule(editor, subject);
        }
        
        public bool CanReturnAnyProperty(Person editor, T subject)
        {
            var group = PropertyGroups.FirstOrDefault() ??
                        throw new Exception("Unable to find a group.");

            return group.CanReturnRule(editor, subject);
        }

        public bool CanReturnProperty(Person editor, T subject, Expression<Func<T, object>> propertySelector)
        {
            var group = PropertyGroups.SingleOrDefault(x => x.Properties.Contains(propertySelector.GetProperty())) ??
                        throw new Exception("Unable to find that property!");

            return group.CanReturnRule(editor, subject);
        }
    }
}