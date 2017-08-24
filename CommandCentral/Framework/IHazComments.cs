using CommandCentral.Entities;
using System;
using System.Collections.Generic;

namespace CommandCentral.Framework
{
    public interface IHazComments
    {
        Guid Id { get; set; }

        bool CanPersonAccessComments(Person person);

        IList<Comment> Comments { get; set; }
    }
}
