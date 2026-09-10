using Application.Common.Interfaces;
using Domain.Aggregates.Cart;
using Domain.Interfaces;

namespace Application.Common.Extensions;

public static class LocalizationExtensions
{
    public static string Localize(this Language lang, string en, string ar) =>
        lang == Language.Ar ? (string.IsNullOrWhiteSpace(ar) ? en : ar) : en;

    public static string? LocalizeNullable(this Language lang, string? en, string? ar) =>
        lang == Language.Ar ? (ar ?? en) : en;

   
}