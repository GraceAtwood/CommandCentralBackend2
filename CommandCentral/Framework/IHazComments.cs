using CommandCentral.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Framework
{
    public interface IHazComments
    {
        Guid Id { get; set; }

        bool CanPersonAccessComments(Person person);

        IList<Comment> Comments { get; set; }
    }
}
