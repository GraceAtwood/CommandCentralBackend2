using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities;
using CommandCentral.Framework;
using CommandCentral.Utilities;

namespace CommandCentral.Authorization
{
    public abstract class RulesContract<T> : BaseRulesContract where T : Entity
    {
        public List<PropertyGroup<T>> PropertyGroups { get; set; } = new List<PropertyGroup<T>>();

        /// <summary>
        /// Overrides all other rules in this contract relating to the ability of a person to edit this object and any property in it.
        /// </summary>
        public Func<Person, T, bool> CanEditRuleOverride { get; set; }

        /// <summary>
        /// Overrides all other rules in this contract relating to the ability of a person to return this object and any property in it.
        /// </summary>
        public Func<Person, T, bool> CanReturnRuleOverride { get; set; }

        /// <summary>
        /// Determines if a person can delete this object.
        /// </summary>
        public Func<Person, T, bool> CanDeleteRule { get; set; }

        /// <summary>
        /// Determines if a person can create this object (persist it in the database).
        /// </summary>
        public Func<Person, T, bool> CanCreateRule { get; set; }

        /// <summary>
        /// Declares rules for properties.
        /// </summary>
        /// <param name="propertySelectors"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public PropertyGroup<T> RulesFor(params Expression<Func<T, object>>[] propertySelectors)
        {
            if (propertySelectors == null || !propertySelectors.Any())
                throw new ArgumentException($"You must give at least one property in rule for type {typeof(T).Name}.",
                    nameof(propertySelectors));

            if (propertySelectors != null && propertySelectors.Any() && propertySelectors.Any(expression =>
                    PropertyGroups.Any(group => group.Properties.Contains(expression.GetProperty()))))
                throw new Exception("There is already a rule with this property.");

            PropertyGroups.Add(new PropertyGroup<T>(propertySelectors));
            return PropertyGroups.Last();
        }

        public bool CanEdit(Person editor, T subject)
        {
            return CanEditRuleOverride(editor, subject);
        }

        public bool CanEditProperty(Person editor, T subject, Expression<Func<T, object>> propertySelector)
        {
            if (CanEditRuleOverride != null)
                return CanEditRuleOverride(editor, subject);

            var group = PropertyGroups.SingleOrDefault(x => x.Properties.Contains(propertySelector.GetProperty())) ??
                        throw new Exception("Unable to find that property!");

            return group.CanEditRule(editor, subject);
        }

        public bool CanEditProperty(Person editor, T subject, string propertyName)
        {
            if (CanEditRuleOverride != null)
                return CanEditRuleOverride(editor, subject);

            var group = PropertyGroups.SingleOrDefault(propGroup =>
                            propGroup.Properties.Any(property => property.Name == propertyName)) ??
                        throw new Exception("Unable to find that property!");

            return group.CanEditRule(editor, subject);
        }

        public bool CanReturn(Person editor, T subject)
        {
            return CanReturnRuleOverride(editor, subject);
        }

        public bool CanReturnProperty(Person editor, T subject, Expression<Func<T, object>> propertySelector)
        {
            if (CanReturnRuleOverride != null)
                return CanReturnRuleOverride(editor, subject);

            var group = PropertyGroups.SingleOrDefault(x => x.Properties.Contains(propertySelector.GetProperty())) ??
                        throw new Exception("Unable to find that property!");

            return group.CanReturnRule(editor, subject);
        }

        public bool CanReturnProperty(Person editor, T subject, string propertyName)
        {
            if (CanReturnRuleOverride != null)
                return CanReturnRuleOverride(editor, subject);

            var group = PropertyGroups.SingleOrDefault(propGroup =>
                            propGroup.Properties.Any(property => property.Name == propertyName)) ??
                        throw new Exception("Unable to find that property!");

            return group.CanReturnRule(editor, subject);
        }
    }
}