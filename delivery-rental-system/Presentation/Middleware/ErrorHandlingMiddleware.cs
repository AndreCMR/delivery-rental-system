using FluentValidation;
using System.Net.Mime;

namespace delivery_rental_system.Presentation.Middleware;

    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;

                context.Response.StatusCode = ex switch
                {
                    ValidationException => StatusCodes.Status400BadRequest,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                await context.Response.WriteAsJsonAsync(new { mensagem = "Dados inválidos" });
            }
        }
    }
