using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace delivery_rental_system.Presentation.Filters;

public sealed class ModelStateToBadRequestFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        if (!ctx.ModelState.IsValid)
        {
            ctx.Result = new BadRequestObjectResult(new { mensagem = "Dados inválidos" });
            return;
        }

        await next();
    }
}
