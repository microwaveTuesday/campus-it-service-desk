using CampusIT.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace CampusIT.Tests;
public class CleanStartupTests
{
    [Fact]
    public async Task Normal_startup_leaves_workspaces_empty_and_demo_seed_is_explicit_and_idempotent()
    {
        await using var database = await IsolatedAccountDatabase.Create();
        using var client = database.Factory.CreateClient();
        (await client.GetAsync("/health")).EnsureSuccessStatusCode();
        using var scope = database.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Categories.ToListAsync());
        Assert.Empty(await db.ServiceRequests.ToListAsync());
        Assert.Empty(await db.Users.ToListAsync());
        Assert.Equal(2,await db.Roles.CountAsync());
        await SeedDemo.SeedAsync(db);
        await SeedDemo.SeedAsync(db);
        Assert.Equal(3,await db.Categories.CountAsync());
        Assert.Equal(2,await db.ServiceRequests.CountAsync());
        Assert.Empty(await db.Users.ToListAsync());
    }
}
