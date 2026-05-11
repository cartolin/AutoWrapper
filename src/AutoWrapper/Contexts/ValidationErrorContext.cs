using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AutoWrapper.Contexts
{
    /// <summary>
    /// Provides contextual data used to customize validation error responses.
    /// </summary>
    public sealed class ValidationErrorContext
    {
        public HttpContext HttpContext { get; set; }

        public int StatusCode { get; set; }

        public IEnumerable<ValidationError> ValidationErrors { get; set; }
    }
}
