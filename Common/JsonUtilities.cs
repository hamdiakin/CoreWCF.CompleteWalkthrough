#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace Common
{
    /// <summary>
    /// Centralized JSON utilities for consistent serialization across the application
    /// </summary>
    public static class JsonUtilities
    {
        private static JsonSerializerSettings CreateSettings(Formatting formatting = Formatting.None)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() },
                Formatting = formatting,
                TypeNameHandling = TypeNameHandling.All
            };
        }

        /// <summary>
        /// Standard JSON serialization settings used throughout the application
        /// </summary>
        public static readonly JsonSerializerSettings StandardSettings = CreateSettings();

        /// <summary>
        /// Pretty-printed JSON settings for debugging and logging
        /// </summary>
        public static readonly JsonSerializerSettings PrettySettings = CreateSettings(Formatting.Indented);

        /// <summary>
        /// Safely serializes an object to JSON string
        /// </summary>
        public static string SafeSerialize<T>(T obj, bool pretty = false)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, pretty ? PrettySettings : StandardSettings);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Safely deserializes JSON string to specified type with polymorphic support.
        /// Returns the fallback value if deserialization fails or input is invalid.
        /// For reference types, fallbackValue must be provided explicitly.
        /// </summary>
        public static T SafeDeserialize<T>(string json, T fallbackValue)
        {
            if (string.IsNullOrWhiteSpace(json))
                return fallbackValue;
            try
            {
                return JsonConvert.DeserializeObject<T>(json, StandardSettings) ?? fallbackValue;
            }
            catch (Exception)
            {
                return fallbackValue;
            }
        }
        // Overload for value types
        public static T SafeDeserialize<T>(string json) where T : struct
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;
            try
            {
                return JsonConvert.DeserializeObject<T>(json, StandardSettings);
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Safely deserializes JSON string to specified type with new instance fallback
        /// </summary>
        public static T SafeDeserializeWithNew<T>(string json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json))
                return new T();
            try
            {
                return JsonConvert.DeserializeObject<T>(json, StandardSettings) ?? new T();
            }
            catch (Exception)
            {
                return new T();
            }
        }

        /// <summary>
        /// Safely extracts a property value from a JSON object. For reference types, fallbackValue must be provided explicitly.
        /// </summary>
        public static T SafeGetProperty<T>(string json, string propertyName, T fallbackValue)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
                return fallbackValue;
            try
            {
                var jsonObject = JsonConvert.DeserializeObject<JObject>(json, StandardSettings);
                if (jsonObject != null && jsonObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out var value))
                {
                    return value.ToObject<T>() ?? fallbackValue;
                }
                return fallbackValue;
            }
            catch (Exception)
            {
                return fallbackValue;
            }
        }
        // Overload for value types
        public static T SafeGetProperty<T>(string json, string propertyName) where T : struct
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
                return default;
            try
            {
                var jsonObject = JsonConvert.DeserializeObject<JObject>(json, StandardSettings);
                if (jsonObject != null && jsonObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out var value))
                {
                    return value.ToObject<T>();
                }
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Validates if a string is valid JSON
        /// </summary>
        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;
            try
            {
                JsonConvert.DeserializeObject(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}