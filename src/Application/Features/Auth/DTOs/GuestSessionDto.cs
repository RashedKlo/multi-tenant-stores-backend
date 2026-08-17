namespace Application.Auth.DTOs;

public record GuestSessionDto(string GuestToken, DateTimeOffset ExpiresAt);
