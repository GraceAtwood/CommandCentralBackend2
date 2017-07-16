using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentValidation;
using CommandCentral.Entities;

namespace CommandCentral.DTOs
{
    public class CommentPostDTO : IValidatable
    {
        public Guid OwningEntity { get; set; }
        public string Body { get; set; }

        public ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        private class Validator : AbstractValidator<CommentPostDTO>
        {
            public Validator()
            {
                RuleFor(x => x.OwningEntity).NotEmpty();
                RuleFor(x => x.Body).NotEmpty().Length(1, 1000);
            }
        }
    }
}
