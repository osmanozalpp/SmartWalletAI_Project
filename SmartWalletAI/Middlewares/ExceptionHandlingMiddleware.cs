using Microsoft.EntityFrameworkCore; // DbUpdateException için şart
using System.Text.Json;

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
            object? response = null;

           
            if (exception is FluentValidation.ValidationException validationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var errors = validationException.Errors
                   .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                   .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

                response = new { Message = "Doğrulama hataları oluştu.", Errors = errors };
            }
            
            else if (exception is DbUpdateException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new
                {
                    Message = "Kayıt sırasında bir çakışma oluştu.",
                    Detail = "Bu E-posta veya IBAN adresi zaten sistemde kayıtlı olabilir."
                };
            }
            
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new
                {
                    Message = "Sunucu tarafında beklenmeyen bir hata oluştu.",
                    Detail = exception.Message
                };
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}