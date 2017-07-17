using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Entities
{
    public abstract class CommentableEntity : Entity
    {

        public virtual IList<Comment> Comments { get; set; }

        public abstract bool CanPersonAccessComments(Person Person);

    }
}
