using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs
{
    public class ProfileLockQueryDTO
    {
        public Guid? Id { get; set; }
        public Guid? Owner { get; set; }
        public Guid? LockedPerson { get; set; }
        public DateTime? RangeStart { get; set; }
        public DateTime? RangeEnd { get; set; }
    }
}
