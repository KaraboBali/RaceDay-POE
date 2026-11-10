namespace RaceDay.MVC.Services;

// Small helpers for reading who is logged in from the session.
public static class UserSessionExtensions
{
    public static bool IsLoggedIn(this ISession s) => s.GetInt32("UserId") != null;
    public static int CurrentUserId(this ISession s) => s.GetInt32("UserId") ?? 0;
    public static string CurrentRole(this ISession s) => s.GetString("Role") ?? string.Empty;
    public static string CurrentName(this ISession s) => s.GetString("UserName") ?? string.Empty;

    public static void SignIn(this ISession s, int id, string name, string role)
    {
        s.SetInt32("UserId", id);
        s.SetString("UserName", name);
        s.SetString("Role", role);
    }
}