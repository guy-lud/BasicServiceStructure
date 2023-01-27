namespace Company.Product.Infrastructure.Middleware;

public static class FailingMiddlewareAppBuilderExtensions
{
    public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder, Action<FailingOptions>? action = null)
    {
        var options = new FailingOptions();
        action?.Invoke(options);
        builder.UseMiddleware<FailingMiddleware>(options);
        return builder;
    }
}