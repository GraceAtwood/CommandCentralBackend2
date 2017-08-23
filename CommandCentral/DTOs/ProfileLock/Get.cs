using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.ProfileLock
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public Guid Owner { get; set; }
        public DateTime SubmitTime { get; set; }
        public TimeSpan MaxAge { get; set; }

        public Get(Entities.ProfileLock profileLock)
        {
            Id = profileLock.Id;
            MaxAge = Entities.ProfileLock.MaxAge;
            Owner = profileLock.Owner.Id;
            SubmitTime = profileLock.SubmitTime;
            LockedPerson = profileLock.LockedPerson.Id;
        }
    }
}
