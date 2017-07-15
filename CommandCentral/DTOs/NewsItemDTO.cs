using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentValidation;

namespace CommandCentral.DTOs
{
    public class NewsItemDTO : IValidatable
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Guid? Creator { get; set; }
        public DateTime? CreationTime { get; set; }

        public ValidationResult Validate()
        {
            return new NewsItemDTOValidator().Validate(this);
        }

        private class NewsItemDTOValidator : AbstractValidator<NewsItemDTO>
        {
            public NewsItemDTOValidator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(3, 50);
                RuleFor(x => x.Body).Length(10, 3500);
            }
        }
    }
}
