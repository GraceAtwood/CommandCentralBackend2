using System;
using System.ComponentModel;
using System.Globalization;

namespace CommandCentral.DTOs.Custom
{
    /// <summary>
    /// A wrapper around two date time values that allow for the querying for a time range.  
    /// Time ranges can be expressed as negative to positive infinity, or with a moment on either side or both sides to declare a range.
    /// </summary>
    [TypeConverter(typeof(DateTimeRangeQueryConverter))]
    public class DateTimeRangeQuery
    {
        /// <summary>
        /// The date/time at which the range starts.  Null = negative infinity.
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// The date/time at which the range ends.  Null = positive infinity.
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// Creates a new date time range query.  If both values are given, from must be before to. 
        /// </summary>
        /// <param name="from">The datetime at which the range starts.</param>
        /// <param name="to">The datetime at which the range ends.</param>
        /// <exception cref="ArgumentOutOfRangeException">If from is after to.</exception>
        public DateTimeRangeQuery(DateTime? from, DateTime? to)
        {
            From = from;
            To = to;

            if (HasBoth() && From.Value > To.Value)
                throw new ArgumentOutOfRangeException(nameof(from));
        }

        /// <summary>
        /// From has a value but to doesn't.  (From a certain point in time to the end of time)
        /// </summary>
        /// <returns></returns>
        public bool HasFromNotTo()
        {
            return From.HasValue && !To.HasValue;
        }

        /// <summary>
        /// To has a value but from doesn't.  (From the beginning of time to a certain point in time)
        /// </summary>
        /// <returns></returns>
        public bool HasToNotFrom()
        {
            return To.HasValue && !From.HasValue;
        }

        /// <summary>
        /// Has a from and a to.  Defines a discrete range of time.
        /// </summary>
        /// <returns></returns>
        public bool HasBoth()
        {
            return To.HasValue && From.HasValue;
        }

        /// <summary>
        /// Has neither, meaning the beginning of time to the end of time.
        /// </summary>
        /// <returns></returns>
        public bool HasNeither()
        {
            return !To.HasValue && !From.HasValue;
        }

        /// <summary>
        /// Attempts to parse the given string to a date time range query.  Possible formats are: 
        /// <para />
        /// "," OR "" OR "datetime," OR ",datetime" OR "datetime,datetime"
        /// </summary>
        /// <param name="str">The string to attempt to parse.</param>
        /// <param name="result">The resulting date time range query if parsing succeeds.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool TryParse(string str, out DateTimeRangeQuery result)
        {
            result = null;

            if (String.IsNullOrWhiteSpace(str) || str.Trim() == ",")
            {
                result = new DateTimeRangeQuery(null, null);
                return true;
            }

            var parts = str.Split(new[] {','}, StringSplitOptions.None);

            if (parts.Length > 2)
                return false;

            if (parts.Length == 1)
            {
                if (!DateTime.TryParseExact(parts[0], Framework.Startup.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dateTime))
                    return false;

                result = new DateTimeRangeQuery(dateTime, dateTime);
                return true;
            }

            DateTime? from;
            if (String.IsNullOrWhiteSpace(parts[0]))
                from = null;
            else
            {
                if (!DateTime.TryParseExact(parts[0], Framework.Startup.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var temp))
                    return false;

                from = temp;
            }

            DateTime? to;
            if (String.IsNullOrWhiteSpace(parts[1]))
                to = null;
            else
            {
                if (!DateTime.TryParseExact(parts[1], Framework.Startup.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var temp))
                    return false;

                to = temp;
            }

            if (!from.HasValue && !to.HasValue)
                throw new Exception("How did you get here cotton eyed joe?");

            if (from.HasValue && to.HasValue && from > to)
            {
                var temp = from;
                from = to;
                to = temp;
            }

            result = new DateTimeRangeQuery(from, to);
            return true;
        }
    }
}