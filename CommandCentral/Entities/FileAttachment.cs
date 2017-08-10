using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    public class FileAttachment : CommentableEntity
    {
        public override bool CanPersonAccessComments(Person person)
        {
            throw new NotImplementedException();
        }

        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
