using CommandCentral.Authorization.Rules;
using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class PropertyPermissionsCollection
    {

        public PropertyInfo Property { get; private set; }
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> LevelsRequiredToEditForChainOfCommand { get; private set; }
            = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.Command);
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> LevelsRequiredToReturnForChainOfCommand { get; private set; }
            = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.Command);

        public bool CanReturnIfSelf { get; private set; }
        public bool CanEditIfSelf { get; private set; }

        public bool HiddenFromPermission { get; private set; }

        public bool CanNeverEdit { get; private set; }

        public PropertyPermissionsCollection(Type type, string propertyName) :
            this(type.GetProperty(propertyName))
        {
        }

        public PropertyPermissionsCollection(PropertyInfo property)
        {
            Property = property;
            var canReturnIfInChainOfCommand = property.GetCustomAttributes<CanReturnIfInChainOfCommandAttribute>();
            var canEditIfInChainOfCommand = property.GetCustomAttributes<CanEditIfInChainOfCommandAttribute>();

            if (!canReturnIfInChainOfCommand.Any())
            {
                LevelsRequiredToReturnForChainOfCommand = LevelsRequiredToReturnForChainOfCommand.ToDictionary(x => x.Key, x => ChainOfCommandLevels.None);
            }
            else
            {
                foreach (var value in canReturnIfInChainOfCommand)
                {
                    if (value.Level < LevelsRequiredToReturnForChainOfCommand[value.ChainOfCommand])
                        LevelsRequiredToReturnForChainOfCommand[value.ChainOfCommand] = value.Level;
                }
            }
            
            foreach (var value in canEditIfInChainOfCommand)
            {
                if (value.Level < LevelsRequiredToEditForChainOfCommand[value.ChainOfCommand])
                LevelsRequiredToEditForChainOfCommand[value.ChainOfCommand] = value.Level;
            }

            CanEditIfSelf = property.GetCustomAttribute<CanEditIfSelfAttribute>() != null;
            CanReturnIfSelf = property.GetCustomAttribute<CanReturnIfSelfAttribute>() != null;

            HiddenFromPermission = property.GetCustomAttribute<HiddenFromPermissionsAttribute>() != null;

            CanNeverEdit = property.GetCustomAttribute<CanNeverEditAttribute>() != null;
        }

        public override int GetHashCode()
        {
            return Property.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyPermissionsCollection other)
            {
                return other.Property == this.Property;
            }

            return false;
        }

        public override string ToString()
        {
            return Property.ToString();
        }

    }
}
