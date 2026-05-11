using AutoWrapper.Helpers;
using AutoWrapper.Test.Helper;
using AutoWrapper.Test.Models;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AutoWrapper.Test
{
    public class AutoWrapperMiddlewareTests
    {

        [Fact(DisplayName = "DefaultTemplateNotResultData")]
        public async Task AutoWrapperDefaultTemplateNotResultData_Test()
        {
            var builder = new WebHostBuilder()
                 .ConfigureServices(services => { services.AddMvcCore(); })
                 .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => Task.FromResult(0));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            var json = JsonHelper.ToJson(new ApiResponse("GET Request successful.", "", 0, null), null);
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            content.ShouldBe(json);
        }



        [Fact(DisplayName = "DefaultTemplateWithResultData")]
        public async Task AutoWrapperDefaultTemplateWithResultData_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => context.Response.WriteAsync("HueiFeng"));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var json = JsonHelper.ToJson(new ApiResponse("GET Request successful.", "HueiFeng", 0, null), null);
            content.ShouldBe(json);
        }
        [Fact(DisplayName = "CustomMessage")]
        public async Task AutoWrapperCustomMessage_Test()
        {
            var builder = new WebHostBuilder()
            .ConfigureServices(services => { services.AddMvcCore(); })
            .Configure(app =>
            {
                app.UseApiResponseAndExceptionWrapper();
                app.Run(context => context.Response.WriteAsync(
                    new ApiResponse("customMessage.", "Test", 200).ToJson()));
            });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var json = JsonHelper.ToJson(new ApiResponse("customMessage.", "Test", 0, null), null);
            content.ShouldBe(json);
        }

        [Fact(DisplayName = "CustomMessageWithStatusCode")]
        public async Task AutoWrapperCustomMessageWithStatusCode_Test()
        {
            var builder = new WebHostBuilder()
            .ConfigureServices(services => { services.AddMvcCore(); })
            .Configure(app =>
            {
                app.UseApiResponseAndExceptionWrapper(options: new AutoWrapperOptions()
                {
                    ShowStatusCode = true
                });
                app.Run(context => context.Response.WriteAsync(
                    new { firstName = "Test", lastName = "User", statusCode = 202 }.ToJson()));
            });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var json = JsonHelper.ToJson(new ApiResponse("GET Request successful.", new { firstName = "Test", lastName = "User", statusCode = 202 }, 200, null), null);
            content.ShouldBe(json);
        }

        [Fact(DisplayName = "CapturingModelStateApiException")]
        public async Task AutoWrapperCapturingModelState_ApiException_Test()
        {
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("name", "some error");
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();

                    app.Run(context => throw new ApiException(dictionary["name"]));
                });
            Exception ex;
            try
            {
                throw new ApiException(dictionary["name"]);
            }
            catch (Exception e)
            {
                ex = e;
            }
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(400);
            var ex1 = ex as ApiException;
            var json = JsonHelper.ToJson(new ApiResponse(0, ex1.CustomError), null);
            content.ShouldBe(json);
        }
        [Fact(DisplayName = "CapturingModelStateApiProblemDetailsException")]
        public async Task AutoWrapperCapturingModelState_ApiProblemDetailsException_Test()
        {
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("name", "some error");
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
                    app.Run(context => throw new ApiProblemDetailsException(dictionary));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(422);
            var str = "{\"isError\":true,\"errors\":null,\"validationErrors\":[{\"name\":\"name\",\"reason\":\"some error\"}],\"details\":null,\"type\":\"https://httpstatuses.com/422\",\"title\":\"Unprocessable Entity\",\"status\":422,\"detail\":\"Your request parameters didn't validate.\",\"instance\":\"/\"}";
            str.ShouldBe(content);
        }

        [Fact(DisplayName = "ThrowingExceptionMessageApiException")]
        public async Task AutoWrapperThrowingExceptionMessage_ApiException_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => throw new ApiException("does not exist.", 404));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            var ex1 = new ApiException("does not exist.", 404);
            var json = JsonHelper.ToJson(
                new ApiResponse(0, new ApiError(ex1.Message) { ReferenceErrorCode = ex1.ReferenceErrorCode, ReferenceDocumentLink = ex1.ReferenceDocumentLink })
                , null);
            content.ShouldBe(json);

        }

        [Fact(DisplayName = "ThrowingExceptionMessageApiProblemDetailsException")]
        public async Task AutoWrapperThrowingExceptionMessage_ApiProblemDetailsException_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
                    app.Run(context => throw new ApiProblemDetailsException("does not exist.", 404));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            var str = "{\"isError\":true,\"errors\":null,\"validationErrors\":null,\"details\":null,\"type\":\"https://httpstatuses.com/404\",\"title\":\"does not exist.\",\"status\":404,\"detail\":null,\"instance\":\"/\"}";
            str.ShouldBe(content);
        }

        [Fact(DisplayName = "ModelValidations")]
        public async Task AutoWrapperModelValidations_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<ApiBehaviorOptions>(options =>
                    {
                        options.SuppressModelStateInvalidFilter = true;
                    });
                    services.AddMvcCore();
                })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper<MapResponseObject>();
                    app.Run(context => context.Response.WriteAsync(
                        new ApiResponse("customMessage.", "Test", 200).ToJson()));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var options = new AutoWrapperOptions();
            var jsonSettings = JSONHelper.GetJSONSettings<MapResponseObject>(options.IgnoreNullValue, options.ReferenceLoopHandling, options.UseCamelCaseNamingStrategy);
            var json = JsonHelper.ToJson(new ApiResponse("customMessage.", "Test", 0, null), jsonSettings.Settings);
            content.ShouldBe(json);
        }

        [Fact(DisplayName = "CustomErrorObject")]
        public async Task AutoWrapperCustomErrorObject_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMvcCore();
                })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper<MapResponseCustomErrorObject>();
                    app.Run(context =>
                 throw new ApiException(
                        new Error("An error blah.", "InvalidRange",
                            new InnerError("12345678", "2020-03-20")
                        )));
                }); 
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(400);
            Exception ex;
            try
            {
                throw new ApiException(
                    new Error("An error blah.", "InvalidRange",
                        new InnerError("12345678", "2020-03-20")
                    ));
            }
            catch (Exception e)
            {
                ex = e;
            }
            var ex1 = ex as ApiException;
            var options = new AutoWrapperOptions();
            var jsonSettings = JSONHelper.GetJSONSettings<MapResponseCustomErrorObject>(options.IgnoreNullValue, options.ReferenceLoopHandling, options.UseCamelCaseNamingStrategy);
            var json = JsonHelper.ToJson(new ApiResponse(0, ex1.CustomError), jsonSettings.Settings);
            json.ToJson().ShouldBe(content.ToJson());
        }


        [Fact(DisplayName = "CustomResponse")]
        public async Task AutoWrapperCustomResponse_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMvcCore();
                })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseCustomSchema = true });
                    app.Run(context => context.Response.WriteAsync(new MyCustomApiResponse("Mr.A").ToJson()));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var str = "{\"Code\":200,\"Payload\":\"Mr.A\",\"SentDate\":\"0001-01-01 00:00:00\"}";
            str.ShouldBe(content);
        }

        // Unified error output mode tests

        [Fact(DisplayName = "Unified_NotFound_String_Should_Use_Message_And_No_ResponseException")]
        public async Task Unified_NotFound_String_Should_Use_Message_And_No_ResponseException()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false
                    });

                    app.Run(context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return context.Response.WriteAsync("Category not found.");
                    });
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            json["statusCode"]!.Value<int>().ShouldBe(404);
            json["isError"]!.Value<bool>().ShouldBe(true);
            json["message"]!.Value<string>().ShouldBe("Category not found.");
            json["result"]!.Type.ShouldBe(JTokenType.Null);
            json["errors"]!.Type.ShouldBe(JTokenType.Null);
            json.ContainsKey("responseException").ShouldBe(false);
        }

        [Fact(DisplayName = "Unified_ApiException_Should_Use_Message_And_No_ResponseException")]
        public async Task Unified_ApiException_Should_Use_Message_And_No_ResponseException()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false
                    });

                    app.Run(context => throw new ApiException("does not exist.", 404));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            json["statusCode"]!.Value<int>().ShouldBe(404);
            json["isError"]!.Value<bool>().ShouldBe(true);
            json["message"]!.Value<string>().ShouldBe("does not exist.");
            json["result"]!.Type.ShouldBe(JTokenType.Null);
            json["errors"]!.Type.ShouldBe(JTokenType.Null);
            json.ContainsKey("responseException").ShouldBe(false);
        }

        [Fact(DisplayName = "Unified_Validation_Should_Move_ValidationErrors_To_Errors")]
        public async Task Unified_Validation_Should_Move_ValidationErrors_To_Errors()
        {
            var validationErrors = new[] {
                new ValidationError("LastName", "'Last Name' must not be empty."),
                new ValidationError("FirstName", "'First Name' must not be empty.")
            };

            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false
                    });

                    app.Run(context => throw new ApiException(validationErrors));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);
            var errors = (JArray)json["errors"]!;

            Convert.ToInt32(rep.StatusCode).ShouldBe(400);
            json["statusCode"]!.Value<int>().ShouldBe(400);
            json["isError"]!.Value<bool>().ShouldBe(true);
            json["message"]!.Value<string>().ShouldBe("Request responded with one or more validation errors.");
            json["result"]!.Type.ShouldBe(JTokenType.Null);

            errors.Count.ShouldBe(2);
            errors[0]["name"]!.Value<string>().ShouldBe("LastName");
            errors[0]["reason"]!.Value<string>().ShouldBe("'Last Name' must not be empty.");
            errors[0]["code"].ShouldBeNull();

            json.ContainsKey("responseException").ShouldBe(false);
        }

        [Fact(DisplayName = "Unified_Validation_CustomFactory_Should_Allow_Code_Optional")]
        public async Task Unified_Validation_CustomFactory_Should_Allow_Code_Optional()
        {
            var validationErrors = new[]
                    {
                new ValidationError("LastName", "'Last Name' must not be empty.")
            };

            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false,
                        ValidationErrorMessage = "The request contains validation errors.",
                        ValidationErrorsFactory = ctx =>
                            ctx.ValidationErrors.Select(x => new
                            {
                                field = x.Name,
                                message = x.Reason,
                                code = (string)null
                            }).ToList()
                    });

                    app.Run(context => throw new ApiException(validationErrors));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);
            var errors = (JArray)json["errors"]!;

            Convert.ToInt32(rep.StatusCode).ShouldBe(400);
            json["statusCode"]!.Value<int>().ShouldBe(400);
            json["message"]!.Value<string>().ShouldBe("The request contains validation errors.");
            errors.Count.ShouldBe(1);

            errors[0]["field"]!.Value<string>().ShouldBe("LastName");
            errors[0]["message"]!.Value<string>().ShouldBe("'Last Name' must not be empty.");
            errors[0]["code"]!.Type.ShouldBe(JTokenType.Null);

            json.ContainsKey("responseException").ShouldBe(false);
        }

        [Fact(DisplayName = "Unified_Conflict_Object_Should_Extract_Message_And_Move_Rest_To_Errors")]
        public async Task Unified_Conflict_Object_Should_Extract_Message_And_Move_Rest_To_Errors()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false
                    });

                    app.Run(context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        context.Response.ContentType = "application/json";

                        return context.Response.WriteAsync(new
                        {
                            message = "Category already exists.",
                            code = "Category.Duplicate"
                        }.ToJson());
                    });
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(409);
            json["statusCode"]!.Value<int>().ShouldBe(409);
            json["isError"]!.Value<bool>().ShouldBe(true);
            json["message"]!.Value<string>().ShouldBe("Category already exists.");
            json["result"]!.Type.ShouldBe(JTokenType.Null);
            json["errors"]!["code"]!.Value<string>().ShouldBe("Category.Duplicate");
            json.ContainsKey("responseException").ShouldBe(false);
        }

        [Fact(DisplayName = "Legacy_Should_Keep_ResponseException")]
        public async Task Legacy_Should_Keep_ResponseException()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Legacy,
                        ShowStatusCode = true,
                        IgnoreNullValue = false
                    });

                    app.Run(context => throw new ApiException("does not exist.", 404));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            json["statusCode"]!.Value<int>().ShouldBe(404);
            json["isError"]!.Value<bool>().ShouldBe(true);
            json.ContainsKey("responseException").ShouldBe(true);
            json["responseException"]!["exceptionMessage"]!.Value<string>().ShouldBe("does not exist.");
        }

        [Fact(DisplayName = "IsError_False_Should_Not_Be_Forced_To_True")]
        public async Task IsError_False_Should_Not_Be_Forced_To_True()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false
                    });

                    app.Run(context => context.Response.WriteAsync("OK"));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            json["statusCode"]!.Value<int>().ShouldBe(200);
            json["isError"]!.Value<bool>().ShouldBe(false);
            json["message"]!.Value<string>().ShouldBe("GET Request successful.");
        }

        [Fact(DisplayName = "Unified_UnhandledException_Should_Not_Expose_Internal_Details_When_IsDebug_False")]
        public async Task Unified_UnhandledException_Should_Not_Expose_Internal_Details_When_IsDebug_False()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false,
                        IsDebug = false
                    });

                    app.Run(context => throw new InvalidOperationException("Sensitive database connection failed."));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(500);
            json["statusCode"]!.Value<int>().ShouldBe(500);
            json["isError"]!.Value<bool>().ShouldBe(true);

            json["message"]!.Value<string>().ShouldBe("Unhandled Exception occurred. Unable to process the request.");
            json["result"]!.Type.ShouldBe(JTokenType.Null);
            json["errors"]!.Type.ShouldBe(JTokenType.Null);

            content.ShouldNotContain("Sensitive database connection failed.");
            content.ShouldNotContain("InvalidOperationException");
            content.ShouldNotContain("StackTrace");
            json.ContainsKey("responseException").ShouldBe(false);
        }

        [Fact(DisplayName = "Unified_ExceptionResponseFactory_Should_Customize_Exception_Output")]
        public async Task Unified_ExceptionResponseFactory_Should_Customize_Exception_Output()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
                    {
                        ErrorOutputMode = ErrorOutputMode.Unified,
                        ShowStatusCode = true,
                        ShowIsErrorFlagForSuccessfulResponse = true,
                        IgnoreNullValue = false,

                        ExceptionResponseFactory = ctx => ApiResponse.Error(
                            statusCode: ctx.StatusCode,
                            message: "Custom exception response.",
                            errors: new
                            {
                                type = ctx.Exception.GetType().Name
                            })
                    });

                    app.Run(context => throw new InvalidOperationException("Internal failure."));
                });

            var server = new TestServer(builder);
            var rep = await server.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, ""));
            var content = await rep.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            Convert.ToInt32(rep.StatusCode).ShouldBe(500);
            json["statusCode"]!.Value<int>().ShouldBe(500);
            json["isError"]!.Value<bool>().ShouldBe(true);
            json["message"]!.Value<string>().ShouldBe("Custom exception response.");
            json["result"]!.Type.ShouldBe(JTokenType.Null);
            json["errors"]!["type"]!.Value<string>().ShouldBe("InvalidOperationException");
            json.ContainsKey("responseException").ShouldBe(false);
        }
    }
}
