namespace Api.Filter
{
	using Microsoft.AspNetCore.Mvc.Filters;
	using Transactions.Application.DTOs;

	public class TenantInjectionFilter : IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (context.HttpContext.Items.TryGetValue("TenantId", out var tenantObj)
				&& tenantObj is int tenantId)
			{
				foreach (var arg in context.ActionArguments.Values)
				{
					if (arg is CreateTransactionRequest request)
					{
						request.TenantId = tenantId;
					}
				}
			}
		}

		public void OnActionExecuted(ActionExecutedContext context) { }
	}
}
