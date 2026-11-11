using System.Globalization;

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

    // Accepts h:mm:ss or mm:ss
    public static bool TryParseTime(string? text, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Trim().Split(':');
        if (parts.Length < 2 || parts.Length > 3) return false;

        var nums = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out nums[i]))
                return false;
        }

        int h = 0, m, s;
        if (parts.Length == 3) { h = nums[0]; m = nums[1]; s = nums[2]; }
        else { m = nums[0]; s = nums[1]; }

        if (s > 59 || (parts.Length == 3 && m > 59)) return false;

        time = new TimeSpan(h, m, s);
        return time > TimeSpan.Zero;
    }
}