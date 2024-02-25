using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QMUL.DiabetesBackend.Controllers.Tests")]
namespace QMUL.DiabetesBackend.Controllers.Middlewares;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Sets the application culture from the Accept-Language header. The default culture is neutral 'es'
/// </summary>
public class CultureMiddleware
{
    internal const string DefaultCulture = "en";

    private readonly RequestDelegate next;
    private readonly List<string> supportedCultures = new() { DefaultCulture, "es" };

    public CultureMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cultures = context.Request.Headers.AcceptLanguage;
        var culture = cultures.ToString()?.Split(',').FirstOrDefault() ?? DefaultCulture;
        var cultureInfo = new CultureInfo(culture);

        if (cultureInfo.IsNeutralCulture && supportedCultures.Contains(cultureInfo.Name))
        {
            ApplyCulture(context, cultureInfo);
        }
        else if (supportedCultures.Contains(cultureInfo.Parent.Name))
        {
            ApplyCulture(context, cultureInfo.Parent);
        }
        else
        {
            ApplyCulture(context, new CultureInfo(DefaultCulture));
        }

        await next(context);
    }

    private void ApplyCulture(HttpContext context, CultureInfo cultureInfo)
    {
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        context.Response.Headers.AcceptLanguage = cultureInfo.Name;
    }
}

public static class CultureMiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="CultureMiddleware"/> to set the application culture
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/></param>
    public static void UseRequestCulture(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<CultureMiddleware>();
    }
}
