using Application.Common.Interfaces;

namespace Infrastructure.Middleware;

public class LanguageMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Items["Language"] = ResolveLanguage(context);
        await next(context);
    }

  private static Language ResolveLanguage(HttpContext context)
{
    var header = context.Request.Headers.AcceptLanguage.ToString();

    return header.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
        ? Language.Ar
        : Language.En;
}
  
  }