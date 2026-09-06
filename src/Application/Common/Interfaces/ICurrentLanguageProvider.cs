namespace Application.Common.Interfaces;

public enum Language
{
    En,
    Ar
}

public interface ICurrentLanguageProvider
{
    Language Language { get; }
}