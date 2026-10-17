namespace RaceDay.Api.Dtos;

public record RegisterDto(string Name, string Email, string Password, string Role);
public record LoginDto(string Email, string Password);