namespace Uptime.Client.Application.Services;

public record User(string Sid, string Name, string Email, bool IsAdmin)
{
    private static User System => new("S-1-5-21-1", "System", "", true);

    public static User Default => new("S-1-5-21-10000", "John Doe", "", false);

    /// <summary>
    /// Returns the given user if not null; otherwise, returns the system account.
    /// </summary>
    public static User OrSystemAccount(User? user) => user ?? System;

    public override string ToString()
    {
        return Name;
    }
}

public class UserService : IUserService
{
    private static readonly List<User> AvailableUsers =
    [
        new("S-1-5-21-10001", "Klient Üks", "klient1@example.com", false),
        new("S-1-5-21-10002", "Klient Kaks", "klient2@example.com", false),
        new("S-1-5-21-10003", "Klient Kolm", "klient3@example.com", false),
        new("S-1-5-21-10004", "Klient Neli", "klient4@example.com", false),
        new("S-1-5-21-10005", "Klient Viis", "klient5@example.com", false),

        new("S-1-5-21-10006", "Marika Oja", "marika.oja@example.com", false),
        new("S-1-5-21-10007", "Jana Pärn", "jana.parn@example.com", false),
        new("S-1-5-21-10008", "Piia Saar", "piia.saar@example.com", false),
        new("S-1-5-21-10009", "Urve Oja", "urve.oja@example.com", false),
        new("S-1-5-21-10010", "Peeter Sepp", "peeter.sepp@example.com", false),
        new("S-1-5-21-10011", "Markus Lepik", "markus.lepik@example.com", false),
        new("S-1-5-21-10012", "Marta Laine", "marta.laine@example.com", false),
        new("S-1-5-21-10013", "Anton Rebane", "anton.rebane@example.com", false),
        new("S-1-5-21-10014", "Signe Kask", "signe.kask@example.com", false),

        new("S-1-5-21-10015", "Riin Koppel", "riin.koppel@example.com", true),
        new("S-1-5-21-10016", "Lauri Saar", "lauri.saar@example.com", true),
        new("S-1-5-21-10017", "Viljar Laine", "viljar.laine@example.com", true),
        new("S-1-5-21-10018", "Kristina Kroon", "kristina.kroon@example.com", true)
    ];

    public List<User> GetUsers()
    {
        return AvailableUsers;
    }
}

public static class UserExtensions
{
    public static string UserType(this User user)
    {
        return user.IsAdmin ? "Admin" : "User";
    }
}