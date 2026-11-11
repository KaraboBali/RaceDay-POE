namespace RaceDay.MVC.Services;

public static class DisplayHelpers
{
    // 1 -> 1st, 2 -> 2nd, 47 -> 47th
    public static string Ordinal(int n)
    {
        var lastTwo = n % 100;
        string suffix;
        if (lastTwo == 11 || lastTwo == 12 || lastTwo == 13) suffix = "th";
        else suffix = (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        return n + suffix;
    }

    // 01:23:45 or 23:45
    public static string Time(TimeSpan t)
    {
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}";
        return $"{t.Minutes:00}:{t.Seconds:00}";
    }
}