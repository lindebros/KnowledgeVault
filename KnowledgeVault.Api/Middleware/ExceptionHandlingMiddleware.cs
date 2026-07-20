using KnowledgeVault.Api.Contracts.Responses;

namespace KnowledgeVault.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
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
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception occurred while processing request");
            context.Response.StatusCode = 500;

            var message = env != null && env.IsEnvironment("Testing") ? ex.ToString() : "An unexpected error occurred.";

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse
                {
                    Code = "internal_error",
                    Message = message
                });
        }
    }
}