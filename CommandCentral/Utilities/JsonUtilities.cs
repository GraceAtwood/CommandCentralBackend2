using CommandCentral.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class JsonUtilities
    {
        /// <summary>
        /// The settings themselves.
        /// </summary>
        public static readonly JsonSerializerSettings StandardSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
            ContractResolver = new CustomContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        /// <summary>
        /// Determines whether or not a given string starts and ends with either {} or [].
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool IsValidJson(string json)
        {
            json = json.Trim();

            return (json.StartsWith("{") && json.EndsWith("}")) || (json.StartsWith("[") && json.EndsWith("]"));
        }

        public static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, StandardSettings);
        }

        public static T Deserialize<T>(this string json)
        {
            return (string.IsNullOrWhiteSpace(json) || !IsValidJson(json)) ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        public static JObject DeserializeToJObject(this string json)
        {
            return JObject.Parse(json);
        }

        /// <summary>
        /// Casts the object (which is expected to be a JObject) into the requested class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CastJToken<T>(this object value) where T : class, new()
        {
            if (value == null)
                return null;

            if (!(value is JToken))
                throw new Exception("The value could not be cast to a JToken.");


            return ((JToken)value).ToObject<T>();
        }

        /// <summary>
        /// Casts the object (which is expected to be a JObject) into the requested class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JToken CastJToken(this object value)
        {
            if (!(value is JToken))
                throw new Exception("The value could not be cast to a JToken.");


            return ((JToken)value);
        }

    }
}
