namespace Application.Auth.DTOs;

public record AuthTokensDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);
