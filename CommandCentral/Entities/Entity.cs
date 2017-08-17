using CommandCentral.Authorization.Rules;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Domain objects should implement this class.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// The id of this entity.  This is the primary key.
        /// </summary>
        [CanNeverEdit]
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Returns the type of this object without needing to worry about NHibernate proxies.
        /// </summary>
        /// <returns></returns>
        public virtual Type GetTypeUnproxied()
        {
            return GetType();
        }

        /// <summary>
        /// Performs Id based equality on two objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Entity entity && entity.Id == this.Id;
        }

        /// <summary>
        /// Returns the hash code of the id.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Performs Id based quality on the two objects.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(Entity x, Entity y)
        {
            if (object.ReferenceEquals(null, x))
                return object.ReferenceEquals(null, y);

            return x.Equals(y);
        }

        /// <summary>
        /// Performs Id based quality on the two objects.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(Entity x, Entity y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Returns the Id.ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Id.ToString();
        }

        /// <summary>
        /// Must be implemented by the implementing class to enable validation of domain objects.
        /// </summary>
        /// <returns></returns>
        public abstract ValidationResult Validate();
    }
}
