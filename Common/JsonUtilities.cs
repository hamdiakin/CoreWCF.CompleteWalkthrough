using System.Text.Json;

namespace Common
{
    /// <summary>
    /// Centralized JSON utilities for consistent serialization across the application
    /// </summary>
    public static class JsonUtilities
    {
        /// <summary>
        /// Standard JSON serialization options used throughout the application
        /// </summary>
        public static readonly JsonSerializerOptions StandardOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Pretty-printed JSON options for debugging and logging
        /// </summary>
        public static readonly JsonSerializerOptions PrettyOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Safely serializes an object to JSON string
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="pretty">Whether to use pretty formatting</param>
        /// <returns>JSON string or empty string if serialization fails</returns>
        public static string SafeSerialize<T>(T obj, bool pretty = false)
        {
            try
            {
                var options = pretty ? PrettyOptions : StandardOptions;
                return JsonSerializer.Serialize(obj, options);
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Safely deserializes JSON string to specified type
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="fallbackValue">Value to return if deserialization fails</param>
        /// <returns>Deserialized object or fallback value</returns>
        public static T SafeDeserialize<T>(string json, T fallbackValue = default!)
        {
            if (string.IsNullOrWhiteSpace(json))
                return fallbackValue;

            try
            {
                return JsonSerializer.Deserialize<T>(json, StandardOptions) ?? fallbackValue;
            }
            catch (JsonException)
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Safely deserializes JSON string to specified type with new instance fallback
        /// </summary>
        /// <typeparam name="T">Type to deserialize to (must have parameterless constructor)</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>Deserialized object or new instance</returns>
        public static T SafeDeserializeWithNew<T>(string json) where T : new()
        {
            return SafeDeserialize(json, new T());
        }

        /// <summary>
        /// Validates if a string is valid JSON
        /// </summary>
        /// <param name="json">String to validate</param>
        /// <returns>True if valid JSON, false otherwise</returns>
        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Safely extracts a property value from JsonElement
        /// </summary>
        /// <typeparam name="T">Type of property to extract</typeparam>
        /// <param name="element">JsonElement to extract from</param>
        /// <param name="propertyName">Name of property to extract</param>
        /// <param name="fallbackValue">Value to return if extraction fails</param>
        /// <returns>Property value or fallback value</returns>
        public static T SafeGetProperty<T>(JsonElement element, string propertyName, T fallbackValue = default!)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return fallbackValue;

            try
            {
                return typeof(T) switch
                {
                    var t when t == typeof(string) => (T)(object)(property.GetString() ?? string.Empty),
                    var t when t == typeof(bool) => (T)(object)property.GetBoolean(),
                    var t when t == typeof(int) => (T)(object)property.GetInt32(),
                    var t when t == typeof(long) => (T)(object)property.GetInt64(),
                    var t when t == typeof(double) => (T)(object)property.GetDouble(),
                    var t when t == typeof(decimal) => (T)(object)property.GetDecimal(),
                    var t when t == typeof(DateTime) => (T)(object)property.GetDateTime(),
                    var t when t == typeof(Guid) => (T)(object)property.GetGuid(),
                    _ => JsonSerializer.Deserialize<T>(property.GetRawText(), StandardOptions) ?? fallbackValue
                };
            }
            catch (JsonException)
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Converts any value to string safely
        /// </summary>
        /// <typeparam name="T">Type of value to convert</typeparam>
        /// <param name="value">Value to convert</param>
        /// <returns>String representation or empty string</returns>
        public static string SafeToString<T>(T value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
} 