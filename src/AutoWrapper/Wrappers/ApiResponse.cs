using Newtonsoft.Json;

namespace AutoWrapper.Wrappers
{
    public class ApiResponse
    {
        public string Version { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StatusCode { get; set; }

        public string Message { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? IsError { get; set; }

        /// <summary>
        /// Original AutoWrapper error envelope.
        /// Kept for Legacy mode and backwards compatibility.
        /// Must be ignored when null so Unified mode never exposes:
        /// "responseException": null
        /// even when IgnoreNullValue = false.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ResponseException { get; set; }

        public object Result { get; set; }

        /// <summary>
        /// Unified error details.
        /// This property is intentionally flexible so applications can expose a string,
        /// object, array, validation error list, or any custom error structure.
        /// </summary>
        public object Errors { get; set; }

        [JsonConstructor]
        public ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
            Version = apiVersion;
        }
        public ApiResponse(object result, int statusCode = 200)
        {
            StatusCode = statusCode;
            Result = result;
        }

        /// <summary>
        /// Legacy constructor.
        /// Preserves original AutoWrapper responseException behavior.
        /// </summary>
        public ApiResponse(int statusCode, object apiError)
        {
            StatusCode = statusCode;
            ResponseException = apiError;
            IsError = true;
        }

        public ApiResponse() { }

        /// <summary>
        /// Creates an error response using the unified response envelope.
        /// </summary>
        public static ApiResponse Error(int statusCode, string message, object errors = null, string apiVersion = null)
        {
            return new ApiResponse
            {
                StatusCode = statusCode,
                IsError = true,
                Message = message,
                Result = null,
                Errors = errors,
                Version = apiVersion
            };
        }
    }
}


