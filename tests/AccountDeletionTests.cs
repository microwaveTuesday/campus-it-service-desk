using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CampusIT.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace CampusIT.Tests;

public class AccountDeletionTests
{
    const string Password="Synthetic!Test123";
    static async Task<HttpResponseMessage> DeleteSelf(HttpClient client,string password=Password) => await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,"/api/account") {Content=JsonContent.Create(new{password})});
    static async Task<JsonElement> Me(HttpClient client) => await client.GetFromJsonAsync<JsonElement>("/api/account/me");
    static async Task<int> PendingId(HttpClient admin) => (await admin.GetFromJsonAsync<JsonElement>("/api/account/deletion-requests"))[0].GetProperty("id").GetInt32();

    [Fact] public async Task Last_admin_cannot_delete_even_with_correct_password()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var admin=await TestAccounts.Client(database.Factory,true);
        var me=await Me(admin);Assert.False(me.GetProperty("canDeleteAdmin").GetBoolean());
        Assert.Equal(HttpStatusCode.Conflict,(await DeleteSelf(admin)).StatusCode);
        (await admin.GetAsync("/api/requests")).EnsureSuccessStatusCode();
    }
    [Fact] public async Task Admin_deletion_preserves_tickets_and_invalidates_old_token()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var first=await TestAccounts.Client(database.Factory,true);using var second=await TestAccounts.Client(database.Factory,true);
        var categories=await first.GetFromJsonAsync<JsonElement>("/api/categories");
        var created=await first.PostAsJsonAsync("/api/requests",new{requesterName="Synthetic Admin",issue="Keep this support history",location="Lab",categoryId=categories[0].GetProperty("id").GetInt32()});created.EnsureSuccessStatusCode();var id=(await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.True((await Me(first)).GetProperty("canDeleteAdmin").GetBoolean());
        Assert.Equal(HttpStatusCode.NoContent,(await DeleteSelf(first)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await first.GetAsync("/api/account/me")).StatusCode);
        (await second.GetAsync($"/api/requests/{id}")).EnsureSuccessStatusCode();
        using var scope=database.Factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();Assert.Null((await db.ServiceRequests.FindAsync(id))!.OwnerId);
        Assert.Equal(HttpStatusCode.Conflict,(await DeleteSelf(second)).StatusCode);
    }
    [Fact] public async Task Concurrent_admin_deletions_leave_one_active_admin()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var first=await TestAccounts.Client(database.Factory,true);using var second=await TestAccounts.Client(database.Factory,true);
        var results=await Task.WhenAll(DeleteSelf(first),DeleteSelf(second));
        Assert.Single(results,x=>x.StatusCode==HttpStatusCode.NoContent);Assert.Single(results,x=>x.StatusCode==HttpStatusCode.Conflict);
        using var scope=database.Factory.Services.CreateScope();Assert.Single(scope.ServiceProvider.GetRequiredService<AppDbContext>().Users);
    }
    [Fact] public async Task Admin_deletion_requires_current_password()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var first=await TestAccounts.Client(database.Factory,true);using var second=await TestAccounts.Client(database.Factory,true);
        Assert.Equal(HttpStatusCode.BadRequest,(await DeleteSelf(first,"Wrong!Password123")).StatusCode);
        (await first.GetAsync("/api/account/me")).EnsureSuccessStatusCode();
    }
    [Fact] public async Task Staff_request_requires_password_and_cannot_duplicate_pending_request()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var staff=await TestAccounts.Client(database.Factory);
        Assert.Equal(HttpStatusCode.Forbidden,(await DeleteSelf(staff)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await staff.PostAsJsonAsync("/api/account/deletion-request",new{password="Wrong!Password123"})).StatusCode);
        (await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password,reason="Synthetic request"})).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict,(await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password})).StatusCode);
        (await staff.GetAsync("/api/requests")).EnsureSuccessStatusCode();
        Assert.Equal("Pending",(await Me(staff)).GetProperty("deletionRequest").GetProperty("status").GetString());
    }
    [Fact] public async Task Admin_approval_deletes_staff_and_revokes_its_token()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var admin=await TestAccounts.Client(database.Factory,true);using var staff=await TestAccounts.Client(database.Factory);
        var staffId=(await Me(staff)).GetProperty("id").GetString();
        (await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password})).EnsureSuccessStatusCode();var id=await PendingId(admin);
        Assert.Equal(HttpStatusCode.Forbidden,(await staff.PostAsJsonAsync($"/api/account/deletion-requests/{id}/review",new{approve=true})).StatusCode);
        (await admin.PostAsJsonAsync($"/api/account/deletion-requests/{id}/review",new{approve=true})).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized,(await staff.GetAsync("/api/requests")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync($"/api/account/deletion-requests/{id}/review",new{approve=true})).StatusCode);
        using var scope=database.Factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();Assert.Null(await db.Users.FindAsync(staffId));var record=await db.AccountDeletionRequests.FindAsync(id);Assert.Equal("Approved",record!.Status);Assert.Null(record.UserId);
    }
    [Fact] public async Task Staff_can_cancel_only_own_request_and_resubmit_after_decline()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var admin=await TestAccounts.Client(database.Factory,true);using var staff=await TestAccounts.Client(database.Factory);using var other=await TestAccounts.Client(database.Factory);
        (await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password})).EnsureSuccessStatusCode();var id=await PendingId(admin);
        Assert.Equal(HttpStatusCode.NotFound,(await other.DeleteAsync("/api/account/deletion-request")).StatusCode);
        (await staff.DeleteAsync("/api/account/deletion-request")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync($"/api/account/deletion-requests/{id}/review",new{approve=true})).StatusCode);
        (await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password})).EnsureSuccessStatusCode();id=await PendingId(admin);
        (await admin.PostAsJsonAsync($"/api/account/deletion-requests/{id}/review",new{approve=false})).EnsureSuccessStatusCode();
        Assert.Equal("Declined",(await Me(staff)).GetProperty("deletionRequest").GetProperty("status").GetString());
        (await staff.GetAsync("/api/requests")).EnsureSuccessStatusCode();(await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password})).EnsureSuccessStatusCode();
    }
    [Fact] public async Task Promoted_staff_request_cannot_be_used_to_delete_an_admin()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var admin=await TestAccounts.Client(database.Factory,true);using var staff=await TestAccounts.Client(database.Factory);
        var me=await Me(staff);(await staff.PostAsJsonAsync("/api/account/deletion-request",new{password=Password})).EnsureSuccessStatusCode();var id=await PendingId(admin);
        using(var scope=database.Factory.Services.CreateScope()){
            var users=scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<CampusIT.Api.Models.AppUser>>();var user=await users.FindByIdAsync(me.GetProperty("id").GetString()!);Assert.True((await users.AddToRoleAsync(user!,"Admin")).Succeeded);
        }
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync($"/api/account/deletion-requests/{id}/review",new{approve=true})).StatusCode);
    }
    [Theory][InlineData("GET","/api/account/me")][InlineData("DELETE","/api/account")][InlineData("POST","/api/account/deletion-request")][InlineData("GET","/api/account/deletion-requests")][InlineData("POST","/api/account/deletion-requests/1/review")]
    public async Task Account_endpoints_require_sign_in(string method,string path)
    {
        await using var database=await IsolatedAccountDatabase.Create();using var client=database.Factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.SendAsync(new HttpRequestMessage(new HttpMethod(method),path){Content=JsonContent.Create(new{password=Password,approve=true})})).StatusCode);
    }
}
