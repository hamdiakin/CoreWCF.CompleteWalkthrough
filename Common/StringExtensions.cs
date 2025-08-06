namespace Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// Safely converts a value to its string representation, returning an empty string if conversion fails or value is null.
        /// </summary>
        public static string SafeToString<T>(this T value)
        {
            try
            {
                return value?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}