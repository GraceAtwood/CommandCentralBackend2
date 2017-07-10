using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.DTOs
{
    public class LoginRequestDTO : IValidatable
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        public class Validator : AbstractValidator<LoginRequestDTO>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty().MinimumLength(8);
                RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            }
        }
    }
}
