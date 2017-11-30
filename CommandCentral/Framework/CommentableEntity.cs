using CommandCentral.Entities;
using System.Collections.Generic;

namespace CommandCentral.Framework
{
    public abstract class CommentableEntity : Entity
    {
        public virtual IList<Comment> Comments { get; set; }
    }
}
