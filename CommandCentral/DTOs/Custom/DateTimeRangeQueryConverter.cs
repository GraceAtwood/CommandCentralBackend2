using System;
using System.ComponentModel;
using System.Globalization;

namespace CommandCentral.DTOs.Custom
{
    /// <summary>
    /// Provides conversions for the date time range query dto.
    /// </summary>
    public class DateTimeRangeQueryConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str && DateTimeRangeQuery.TryParse(str, out var dateTimeRangeQuery))
                return dateTimeRangeQuery;
            
            return base.ConvertFrom(context, culture, value);
        }
    }
}