using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using System;

namespace AutoWrapper.Contexts
{
    /// <summary>
    /// Provides contextual data used to customize exception responses.
    /// </summary>
    public sealed class ExceptionErrorContext
    {
        public HttpContext HttpContext { get; set; }

        public Exception Exception { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; }

        public object Errors { get; set; }

        public ApiError ApiError { get; set; }
    }
}
