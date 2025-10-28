using LibraryApp.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace LibraryApp.Api.Middlewares
{
    internal sealed class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }catch(Exception ex)
            {
                HttpStatusCode statusCode = ex switch
                {
                    RequestException => HttpStatusCode.BadRequest,
                    UnauthorizedException => HttpStatusCode.Unauthorized,
                    NotFoundException => HttpStatusCode.NotFound,
                    UnexpectedErrorException => HttpStatusCode.InternalServerError,
                    _ => HttpStatusCode.InternalServerError
                };

                var message = "Error inesperado ocorreu no sistema";
                if (ex is BaseException be)
                    message = be.GetMessages();

                context.Response.StatusCode = (int)statusCode;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = (int)statusCode,
                    Title = "Um error ocorreu",
                    Type = ex.GetType().Name,
                    Detail = message
                });
            }
        }
    }
}
