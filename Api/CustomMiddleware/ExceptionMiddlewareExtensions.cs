
using Newtonsoft.Json;
using System.Net;
using System;
using Api.Helpers;
using static Api.Helpers.IdentityException;

namespace Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void ConfigureCustomMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }


    public class abc
    {
        private readonly RequestDelegate _next;
        public abc(RequestDelegate rd)
        {
            _next = rd;
        }

        public async Task InvokeAsync(HttpContext httpContext)//, ILogExceptionService logExceptionService)
        {
            await Task.Delay(100);
        }
    }


    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        //private ILogExceptionService _logExceptionService;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            //_logger = logger;
            _next = next;
            //_logExceptionService = logExceptionService;
            _logger = loggerFactory.CreateLogger(typeof(ExceptionMiddleware));

        }

        public async Task InvokeAsync(HttpContext httpContext)//, ILogExceptionService logExceptionService)
        {
            //_logExceptionService = logExceptionService;

            try
            {
                await _next(httpContext);
            }
            catch (IdentityException ex)
            {
                await ReturnExceptionAsResponseAsync(httpContext, new Exception(string.Join(",", ex.Errors)), HttpStatusCode.InternalServerError);
            }
            catch (Unauthorized ex)
            {
                await ReturnExceptionAsResponseAsync(httpContext, ex, HttpStatusCode.Unauthorized, string.Concat("-token-expired|", ex.InformativeMsg));
            }
            catch (InformativeException ex)
            {
                await ReturnExceptionAsResponseAsync(httpContext, ex, HttpStatusCode.NotFound, ex.InformativeMsg);
            }
            catch (ArgumentException ex)
            {
                await ReturnExceptionAsResponseAsync(httpContext, ex, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                SaveExceptionIntoDB(ex);
                await ReturnExceptionAsResponseAsync(httpContext, ex, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private Task ReturnExceptionAsResponseAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string message = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            Exception _exception = null;

            if (statusCode != HttpStatusCode.Unauthorized && statusCode != HttpStatusCode.NotFound)
            {
                _exception = GetInnerException(exception);
            }


            if (message == null)
                message = _exception.Message;


            ErrorDetail error = new ErrorDetail
            {
                statusCode = context.Response.StatusCode,
                message = message,
                //Exception = statusCode == HttpStatusCode.Unauthorized ? null : _exception.Message
                Exception = _exception == null ? null : _exception.Message
            };

            var stringAsJson = JsonConvert.SerializeObject(error);

            return context.Response.WriteAsync(stringAsJson);
        }

        private void SaveExceptionIntoDB(Exception ex)
        {
            _logger.LogInformation($"Something went wrong: {ex}");

            Exception _innerException = GetInnerException(ex);

            string stackTrace = ex.StackTrace.ToString();

            //await _logExceptionService.CreateAsync(new LogException
            //{
            //    ExceptionType = ExceptionType.General,
            //    StackTrace = stackTrace.Length > 4000 ? stackTrace.Substring(0, 3999) : ex.StackTrace.ToString(),
            //    Message = _innerException.Message.ToString(),
            //});
        }

        private Exception GetInnerException(Exception ex)
        {
            Exception _exception = ex;

            while (_exception.InnerException != null)
            {
                _exception = _exception.InnerException;
            }

            return _exception;
        }

    }


    public class ErrorDetail
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public object Exception { get; set; }
    }
}
