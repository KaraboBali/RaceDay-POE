namespace RaceDay.Api.Dtos;

public record ResultDto(TimeSpan FinishTime, int Position);
public record ProfileDto(string Name, string Email);
