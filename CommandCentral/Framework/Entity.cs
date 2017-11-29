using System;
using FluentValidation.Results;
using NHibernate.Id.Insert;

namespace CommandCentral.Framework
{
    /// <summary>
    /// Domain objects should implement this class.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// The id of this entity.  This is the primary key.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Returns the type of this entity.
        /// </summary>
        public virtual Type GetTypeUnproxied() => GetType();

        /// <summary>
        /// Performs Id based equality on two objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Entity entity && entity.Id == Id;
        }

        /// <summary>
        /// Returns the hash code of the id.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Performs Id based quality on the two objects.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(Entity x, Entity y)
        {
            if (ReferenceEquals(null, x))
                return ReferenceEquals(null, y);

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
            return Id.ToString();
        }

        /// <summary>
        /// Must be implemented by the implementing class to enable validation of domain objects.
        /// </summary>
        /// <returns></returns>
        public abstract ValidationResult Validate();
    }
}
