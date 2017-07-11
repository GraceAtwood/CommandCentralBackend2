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
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> LevelsRequiredToEdit { get; private set; }
            = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.None);
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> LevelsRequiredToReturn { get; private set; }
            = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.None);

        public PropertyPermissionsCollection(Type type, string propertyName) :
            this(type.GetProperty(propertyName))
        {
        }

        public PropertyPermissionsCollection(PropertyInfo property)
        {
            Property = Property;
            var canReturn = property.GetCustomAttributes<CanReturnAttribute>();
            var canEdit = property.GetCustomAttributes<CanEditAttribute>();

            if (!canReturn.Any())
            {
                LevelsRequiredToReturn = LevelsRequiredToReturn.ToDictionary(x => x.Key, x => ChainOfCommandLevels.Command);
            }
            else
            {
                foreach (var value in canReturn)
                {
                    LevelsRequiredToReturn[value.ChainOfCommand] = value.Level;
                }
            }
            
            foreach (var value in canEdit)
            {
                LevelsRequiredToEdit[value.ChainOfCommand] = value.Level;
            }
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
