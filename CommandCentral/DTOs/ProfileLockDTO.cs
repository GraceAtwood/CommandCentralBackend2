using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace CommandCentral.DTOs
{
    public class ProfileLockDTO
    {
        public Guid Id { get; set; }
        public Guid Owner { get; set; }
        public Guid LockedPerson { get; set; }
        public DateTime SubmitTime { get; set; }
        public TimeSpan MaxAge { get; set; }
    }
}
