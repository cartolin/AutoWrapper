namespace AutoWrapper
{
    /// <summary>
    /// Defines how AutoWrapper exposes error responses.
    /// </summary>
    public enum ErrorOutputMode
    {
        /// <summary>
        /// Preserves the original AutoWrapper error response schema using responseException.
        /// This is the default mode for backward compatibility.
        /// </summary>
        Legacy = 0,

        /// <summary>
        /// Exposes errors through the main response envelope using message, result, and errors.
        /// This mode is optional and does not change the default AutoWrapper behavior.
        /// </summary>
        Unified = 1
    }
}
