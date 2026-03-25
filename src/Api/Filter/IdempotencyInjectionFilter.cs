namespace Api.Filter;

using Microsoft.AspNetCore.Mvc.Filters;
using Transactions.Application.DTOs;

public class IdempotencyInjectionFilter : IActionFilter
{
    private const string IdempotenceHeaderName = "X-Idempotence-Key";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(IdempotenceHeaderName, out var headerValue) ||
            string.IsNullOrWhiteSpace(headerValue))
        {
            // Sem header, não faz nada; o use case ainda pode gerar uma key nova
            return;
        }

        var idempotenceKey = headerValue.ToString();

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is CreateTransactionRequest request &&
                string.IsNullOrWhiteSpace(request.IdempotenceKey))
            {
                request.IdempotenceKey = idempotenceKey;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
