using System;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    public class PasswordReset : Entity
    {
        
        
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public class Validator : AbstractValidator<PasswordReset>
        {
            public Validator()
            {
                throw new NotImplementedException();
            }
        }
    }
}