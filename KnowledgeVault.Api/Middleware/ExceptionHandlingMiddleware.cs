using KnowledgeVault.Api.Contracts.Responses;

namespace KnowledgeVault.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse
                {
                    Code = "business_rule_violation",
                    Message = ex.Message
                });
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse
                {
                    Code = "internal_error",
                    Message = "An unexpected error occurred."
                });
        }
    }
}