using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;

namespace AutoWrapper.Contexts
{
    /// <summary>
    /// Provides contextual data used to customize controlled HTTP error responses.
    /// </summary>
    public sealed class ControlledErrorContext
    {
        public HttpContext HttpContext { get; set; }

        public int StatusCode { get; set; }

        public object Body { get; set; }

        public string Message { get; set; }

        public object Errors { get; set; }

        public ApiError ApiError { get; set; }
    }
}
