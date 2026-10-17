namespace RaceDay.Api.Dtos;

public record EventDto(string Name, DateTime Date, string Location, string? Description, string? EventType);