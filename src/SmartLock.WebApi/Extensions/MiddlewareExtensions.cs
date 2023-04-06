using SmartLock.WebApi.Middlewares;

namespace SmartLock.WebApi.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ApiExceptionHandlingMiddleware>();
}