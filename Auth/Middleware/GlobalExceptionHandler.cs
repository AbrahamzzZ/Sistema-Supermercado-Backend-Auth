using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Utilities.Shared;

namespace Auth.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString() ?? httpContext.TraceIdentifier;
            using var logUsuario = Serilog.Context.LogContext.PushProperty("IdUsuario", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            using var logMetodo = Serilog.Context.LogContext.PushProperty("RequestMethod", httpContext.Request.Method);
            _logger.LogError(exception, "Error no controlado en {Metodo} {Endpoint}",
                httpContext.Request.Method, httpContext.Request.Path.Value);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>
            {
                IsSuccess = false,
                Message = $"Ocurrió un error interno. Código de seguimiento: {correlationId}"
            }, cancellationToken);

            return true;
        }
    }
}
