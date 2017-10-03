using System.Collections.Generic;
using CommandCentral.Enums;

namespace CommandCentral.Utilities
{
    /// <summary>
    /// Provides methods for dealing with paygrades.
    /// </summary>
    public static class PaygradeUtilities
    {
        /// <summary>
        /// Contains the set of all officer paygrades.
        /// </summary>
        public static readonly HashSet<Paygrades> OfficerPaygrades = new HashSet<Paygrades>
        {
            Paygrades.CWO2,
            Paygrades.CWO3,
            Paygrades.CWO4,
            Paygrades.CWO5,
            Paygrades.O1,
            Paygrades.O1E,
            Paygrades.O2,
            Paygrades.O2E,
            Paygrades.O3,
            Paygrades.O3E,
            Paygrades.O4,
            Paygrades.O5,
            Paygrades.O6,
            Paygrades.O7,
            Paygrades.O8,
            Paygrades.O9,
            Paygrades.O10
        };

        /// <summary>
        /// Contains the set of all enlisted paygrades.
        /// </summary>
        public static readonly HashSet<Paygrades> EnlistedPaygrades = new HashSet<Paygrades>
        {
            Paygrades.E1,
            Paygrades.E2,
            Paygrades.E3,
            Paygrades.E4,
            Paygrades.E5,
            Paygrades.E6,
            Paygrades.E7,
            Paygrades.E8,
            Paygrades.E9
        };

        /// <summary>
        /// Contains the set of all civilian paygrades.
        /// </summary>
        public static readonly HashSet<Paygrades> CivilianPaygrades = new HashSet<Paygrades>
        {
            Paygrades.GG1,
            Paygrades.GG2,
            Paygrades.GG3,
            Paygrades.GG4,
            Paygrades.GG5,
            Paygrades.GG6,
            Paygrades.GG7,
            Paygrades.GG8,
            Paygrades.GG9,
            Paygrades.GG10,
            Paygrades.GG11,
            Paygrades.GG12,
            Paygrades.GG13,
            Paygrades.GG14,
            Paygrades.GG15,
            Paygrades.CON
        };

        /// <summary>
        /// Returns a boolean indicating if this paygrade is that of an officer or not.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsOfficerPaygrade(this Paygrades paygrade)
        {
            return OfficerPaygrades.Contains(paygrade);
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a civilian paygrade or not.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsCivilianPaygrade(this Paygrades paygrade)
        {
            return CivilianPaygrades.Contains(paygrade);
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is an enlisted paygrade or not.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsEnlistedPaygrade(this Paygrades paygrade)
        {
            return EnlistedPaygrades.Contains(paygrade);
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a chief paygrade.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsChief(this Paygrades paygrade)
        {
            return paygrade == Paygrades.E7 || paygrade == Paygrades.E8 || paygrade == Paygrades.E9;
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a petty officer.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsPettyOfficer(this Paygrades paygrade)
        {
            return paygrade == Paygrades.E4 || paygrade == Paygrades.E5 || paygrade == Paygrades.E6;
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a seaman.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsSeaman(this Paygrades paygrade)
        {
            return paygrade == Paygrades.E1 || paygrade == Paygrades.E2 || paygrade == Paygrades.E3;
        }
    }
}