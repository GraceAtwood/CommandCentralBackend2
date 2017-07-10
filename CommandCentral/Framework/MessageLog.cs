using FluentNHibernate.Mapping;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Framework
{
    public class MessageLog
    {

        #region Properties

        public virtual Guid Id { get; set; }

        public virtual Authentication.APIKey APIKey { get; set; }

        public virtual Authentication.AuthenticationSession AuthenticationSession { get; set; }

        public virtual DateTime CallTime { get; set; } = DateTime.UtcNow;

        public virtual string Action { get; set; }

        public virtual int ProcessingTime { get; set; } = 0;

        public virtual string HostAddress { get; set; }

        #endregion

        public class MessageLogMapping : ClassMap<MessageLog>
        {
            public MessageLogMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.APIKey).Not.Nullable();
                References(x => x.AuthenticationSession);

                Map(x => x.CallTime).CustomType<UtcDateTimeType>().Not.Nullable();
                Map(x => x.Action).Not.Nullable();
                Map(x => x.ProcessingTime).Not.Nullable();
                Map(x => x.HostAddress).Not.Nullable();
            }
        }
    }
}
