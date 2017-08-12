using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Enums
{
    public enum AccountHistoryTypes
    {
        Created,
        Login,
        Logout,
        FailedLogin,
        RegistrationStarted,
        RegistrationCompleted,
        PasswordResetStarted,
        PasswordResetCompleted,
        PasswordChanged,
        UsernameForgotten
    }
}
