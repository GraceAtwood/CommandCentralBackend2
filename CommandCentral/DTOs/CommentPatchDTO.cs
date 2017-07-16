using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentValidation;

namespace CommandCentral.DTOs
{
    public class CommentPatchDTO : IValidatable
    {
        public string Body { get; set; }

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        private class Validator : AbstractValidator<CommentPatchDTO>
        {
            public Validator()
            {
                RuleFor(x => x.Body).NotEmpty().Length(1, 1000);
            }
        }
    }
}
