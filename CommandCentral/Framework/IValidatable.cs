using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Framework
{
    interface IValidatable
    {
        ValidationResult Validate();
    }
}
