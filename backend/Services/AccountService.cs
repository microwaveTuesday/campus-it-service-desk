using CampusIT.Api.Contracts;
using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Services;

public record AccountCreationResult(bool Succeeded, bool LimitReached, string[] Errors);

public class AccountService(AppDbContext db, UserManager<AppUser> users)
{
    public const int MaximumAdmins = 2;
    public const string AdminLimitMessage = "Maximum of 2 admins are already registered. You can no longer register more.";

    public async Task<int> AdminCount() => (await users.GetUsersInRoleAsync("Admin")).Count;

    public async Task<AccountCreationResult> RegisterAdmin(RegistrationInput input, bool promoteExisting = false)
    {
        // The same PostgreSQL transaction lock is used by public signup and operator provisioning.
        // Counting and assigning the role are atomic even across concurrent API instances.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(741938205)");
        var user = promoteExisting ? await users.FindByEmailAsync(input.Email) : null;
        if (user is not null && await users.IsInRoleAsync(user, "Admin"))
            return new(true, false, []);
        if (await AdminCount() >= MaximumAdmins)
            return new(false, true, [AdminLimitMessage]);
        if (user is null)
        {
            user = new AppUser { UserName = input.Email, Email = input.Email, DisplayName = input.DisplayName.Trim() };
            var created = await users.CreateAsync(user, input.Password);
            if (!created.Succeeded) return Failure(created);
        }
        var assigned = await users.AddToRoleAsync(user, "Admin");
        if (!assigned.Succeeded) return Failure(assigned);
        await transaction.CommitAsync();
        return new(true, false, []);
    }

    public async Task<AccountCreationResult> CreateStaff(RegistrationInput input)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new AppUser { UserName = input.Email, Email = input.Email, DisplayName = input.DisplayName.Trim() };
        var created = await users.CreateAsync(user, input.Password);
        if (!created.Succeeded) return Failure(created);
        var assigned = await users.AddToRoleAsync(user, "Staff");
        if (!assigned.Succeeded) return Failure(assigned);
        await transaction.CommitAsync();
        return new(true, false, []);
    }

    static AccountCreationResult Failure(IdentityResult result) => new(false, false, result.Errors.Select(x => x.Description).ToArray());
}
