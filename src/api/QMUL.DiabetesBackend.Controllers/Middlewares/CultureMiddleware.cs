namespace QMUL.DiabetesBackend.Controllers.Middlewares;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class CultureMiddleware
{
    private readonly RequestDelegate next;
    private const string DefaultCulture = "en";
    private readonly List<string> supportedCultures = new() { DefaultCulture, "es" };

    public CultureMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cultures = context.Request.Headers.AcceptLanguage;
        var culture = cultures.ToString()?.Split(',').FirstOrDefault() ?? DefaultCulture;
        if (!supportedCultures.Contains(culture))
        {
            culture = DefaultCulture;
        }

        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await next(context);
    }
}

public static class CultureMiddlewareExtensions
{
    public static void UseRequestCulture(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<CultureMiddleware>();
    }
}
