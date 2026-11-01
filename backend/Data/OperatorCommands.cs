using CampusIT.Api.Contracts;
using CampusIT.Api.Services;
namespace CampusIT.Api.Data;
public static class OperatorCommands
{
    public static async Task ProvisionAdmin(IServiceProvider services, IConfiguration config)
    {
        var email = config["ADMIN_EMAIL"] ?? throw new InvalidOperationException("Set ADMIN_EMAIL");
        var password = config["ADMIN_PASSWORD"] ?? throw new InvalidOperationException("Set ADMIN_PASSWORD");
        var accounts = services.GetRequiredService<AccountService>();
        var result = await accounts.RegisterAdmin(new RegistrationInput(email, password, "Campus Administrator"), promoteExisting: true);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors));
        Console.WriteLine("Admin provisioned. Existing account passwords are unchanged. Maximum: two admins.");
    }
}
