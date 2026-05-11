using AutoWrapper.Base;
using AutoWrapper.Contexts;
using AutoWrapper.Wrappers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AutoWrapper
{
    public class AutoWrapperOptions : OptionBase
    {
        public bool UseCustomSchema { get; set; } = false;
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; } = ReferenceLoopHandling.Ignore;
        public bool UseCustomExceptionFormat { get; set; } = false;
        public bool UseApiProblemDetailsException { get; set; } = false;
        public bool LogRequestDataOnException { get; set; } = true;
        public bool IgnoreWrapForOkRequests { get; set; } = false;
        public bool ShouldLogRequestData { get; set; } = true;

        public string SwaggerPath { get; set; } = "/swagger";

        public IEnumerable<AutoWrapperExcludePath>  ExcludePaths { get; set; } = null;

        /// <summary>
        /// Controls how error responses are exposed by AutoWrapper.
        /// Legacy preserves the original responseException schema.
        /// Unified exposes errors through the main response envelope.
        /// </summary>
        public ErrorOutputMode ErrorOutputMode { get; set; } = ErrorOutputMode.Legacy;

        /// <summary>
        /// Allows customizing controlled HTTP error responses such as BadRequest, NotFound,
        /// Conflict, Unauthorized, Forbidden, and similar non-success HTTP responses.
        /// </summary>
        public Func<ControlledErrorContext, ApiResponse> ControlledErrorResponseFactory { get; set; }

        /// <summary>
        /// Allows customizing exception responses generated from ApiException,
        /// UnauthorizedAccessException, or unhandled exceptions.
        /// </summary>
        public Func<ExceptionErrorContext, ApiResponse> ExceptionResponseFactory { get; set; }

        /// <summary>
        /// Allows customizing the object assigned to the errors property when validation errors are detected.
        /// </summary>
        public Func<ValidationErrorContext, object> ValidationErrorsFactory { get; set; }

        /// <summary>
        /// Default message used for validation errors when ErrorOutputMode is set to Unified.
        /// </summary>
        public string ValidationErrorMessage { get; set; } = Helpers.ResponseMessage.ValidationError;

        /// <summary>
        /// Default message used for unhandled exceptions when ErrorOutputMode is set to Unified
        /// and IsDebug is false.
        /// </summary>
        public string UnhandledErrorMessage { get; set; } = Helpers.ResponseMessage.Unhandled;
    }
}
