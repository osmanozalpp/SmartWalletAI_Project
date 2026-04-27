using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Domain.Exceptions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SmartWalletAI.WebAPI.Middlewares
{


    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

       
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            ErrorResponse response;

            switch (exception)
            {
                case FluentValidation.ValidationException valEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = new ErrorResponse
                    {
                        Message = "Doğrulama hataları oluştu.",
                        Errors = valEx.Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                                             .ToDictionary(g => g.Key, g => g.ToArray())
                    };
                    break;

               
                case UnauthorizedException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    response = new ErrorResponse { Message = exception.Message };
                    break;

                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    response = new ErrorResponse { Message = exception.Message };
                    break;

                case BusinessException:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = new ErrorResponse { Message = exception.Message };
                    break;

                case DbUpdateException:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = new ErrorResponse
                    {
                        Message = "Kayıt sırasında bir çakışma oluştu.",
                        Detail = "E-posta veya IBAN zaten kayıtlı olabilir."
                    };
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    response = new ErrorResponse
                    {
                        Message = "Sunucu tarafında beklenmeyen bir hata oluştu.",
                        Detail = exception.Message
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}