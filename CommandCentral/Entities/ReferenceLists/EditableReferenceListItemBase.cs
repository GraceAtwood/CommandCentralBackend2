using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    public abstract class EditableReferenceListItemBase : ReferenceListItemBase, IValidatable
    {
        public abstract ValidationResult Validate();
    }
}
