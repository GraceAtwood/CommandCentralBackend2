﻿using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentValidation;

namespace CommandCentral.DTOs
{
    public class CommentDTO : IValidatable
    {
        public Guid Id { get; set; }
        public Guid Creator { get; set; }
        public Guid OwningEntity { get; set; }
        public string Body { get; set; }
        public DateTime TimeCreated { get; set; }

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        private class Validator : AbstractValidator<CommentDTO>
        {
            public Validator()
            {
                RuleFor(x => x.OwningEntity).NotEmpty();
                RuleFor(x => x.Body).NotEmpty().Length(1, 1000);
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.TimeCreated).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
            }
        }
    }
}