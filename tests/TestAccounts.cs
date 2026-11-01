using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CampusIT.Api.Models;
using CampusIT.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
namespace CampusIT.Tests;

internal static class TestAccounts
{
    // Fixtures seed identities directly so ordinary CRUD tests do not consume public Admin signup slots.
    public static async Task<HttpClient> Client(WebApplicationFactory<Program> factory, bool admin = false)
    {
        var email = $"fixture-{Guid.NewGuid():N}@example.test";
        const string password = "Synthetic!Test123";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!await db.Categories.AnyAsync())
            {
                db.Categories.Add(new Category { Name = "Synthetic support" });
                await db.SaveChangesAsync();
            }
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { UserName = email, Email = email, DisplayName = "Synthetic fixture" };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded) throw new InvalidOperationException(string.Join("; ", created.Errors.Select(x=>x.Description)));
            var assigned = await users.AddToRoleAsync(user, admin ? "Admin" : "Staff");
            if (!assigned.Succeeded) throw new InvalidOperationException("Fixture role assignment failed");
        }
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());
        return client;
    }
}
