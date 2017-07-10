using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    public class FatalErrorEmailModel
    {
        /// <summary>
        /// The original message.
        /// </summary>
        public string OriginalMessage { get; set; }

        /// <summary>
        /// The exception to send.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The token...
        /// </summary>
        public MessageLog MessageLog { get; set; }

        /// <summary>
        /// Returns a comma delineated list of the permissions the client has.
        /// </summary>
        public string ClientPermissionNames
        {
            get
            {
                return null;
                //TODO
                /*if (Token.AuthenticationSession == null)
                    return null;

                return String.Join(", ", Token.AuthenticationSession.Person.PermissionGroupNames);*/
            }
        }
    }
}
