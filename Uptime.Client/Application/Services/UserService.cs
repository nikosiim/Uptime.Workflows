namespace Uptime.Client.Application.Services;

public record User(string Name, string Email, bool IsAdmin)
{
    public static User Default => new("John Doe", "", false);
    public static User System => new("System", "", true);
    public static string GetNameOrDefault(User? user) => user?.Name ?? Default.Name;
    public static string GetNameOrSystemAccount(User? user) => user?.Name ?? System.Name;

    public override string ToString()
    {
        return Name;
    }
}

public class UserService : IUserService
{
    public static readonly List<User> AvailableUsers =
    [
        new("Klient Üks", "klient1@example.com", false),
        new("Klient Kaks", "klient2@example.com", false),
        new("Klient Kolm", "klient3@example.com", false),
        new("Klient Neli", "klient4@example.com", false),
        new("Klient Viis", "klient5@example.com", false),

        new("Marika Oja", "marika.oja@example.com", false),
        new("Jana Pärn", "jana.parn@example.com", false),
        new("Piia Saar", "piia.saar@example.com", false),
        new("Urve Oja", "urve.oja@example.com", false),
        new("Peeter Sepp", "peeter.sepp@example.com", false),
        new("Markus Lepik", "markus.lepik@example.com", false),
        new("Marta Laine", "marta.laine@example.com", false),
        new("Anton Rebane", "anton.rebane@example.com", false),
        new("Signe Kask", "signe.kask@example.com", false),
        
        new("Riin Koppel", "riin.koppel@example.com", true),
        new("Lauri Saar", "lauri.saar@example.com", true),
        new("Viljar Laine", "viljar.laine@example.com", true),
        new("Kristina Kroon", "kristina.kroon@example.com", true)
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