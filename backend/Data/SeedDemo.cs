using CampusIT.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Data;
public static class SeedDemo
{
    // Optional operator command only. Normal startup leaves categories and requests empty.
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(new Category { Name = "Network" }, new Category { Name = "Hardware" }, new Category { Name = "Software" });
            await db.SaveChangesAsync();
        }
        if (await db.ServiceRequests.AnyAsync()) return;
        var categories = await db.Categories.OrderBy(x => x.Id).ToListAsync();
        db.ServiceRequests.AddRange(
            new ServiceRequest { RequesterName = "Anna Reyes (sample)", Issue = "Cannot connect to campus Wi-Fi", Location = "Computer Laboratory 1", Status = "Pending", CategoryId = categories[0].Id },
            new ServiceRequest { RequesterName = "Carlo Mendoza (sample)", Issue = "Projector has no display", Location = "Room 302", Status = "In Progress", CategoryId = categories[Math.Min(1, categories.Count - 1)].Id });
        await db.SaveChangesAsync();
    }
}
