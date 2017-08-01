using CommandCentral.Authorization.Rules;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Entities
{
    public abstract class Entity
    {
        [CanNeverEdit]
        public virtual Guid Id { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Entity entity && entity.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public static bool operator ==(Entity x, Entity y)
        {
            if (object.ReferenceEquals(null, x))
                return object.ReferenceEquals(null, y);

            return x.Equals(y);
        }

        public static bool operator !=(Entity x, Entity y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        public abstract ValidationResult Validate();
    }
}
