using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentLanguageProvider(IHttpContextAccessor httpContextAccessor) : ICurrentLanguageProvider
{
    public Language Language
    {
        get
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx?.Items.TryGetValue("Language", out var value) == true && value is Language lang)
                return lang;
            return Language.En;
        }
    }
}