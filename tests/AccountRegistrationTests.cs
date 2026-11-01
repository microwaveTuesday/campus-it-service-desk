using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace CampusIT.Tests;

public class AccountRegistrationTests
{
    static object Input(string? email = null) => new { email = email ?? $"admin-{Guid.NewGuid():N}@example.test", password = "Synthetic!Admin123", displayName = "Synthetic Admin" };
    [Fact]
    public async Task First_two_admins_can_register_and_login_but_third_is_rejected()
    {
        await using var database = await IsolatedAccountDatabase.Create(); using var client = database.Factory.CreateClient();
        var initial = await client.GetFromJsonAsync<JsonElement>("/api/auth/registration-status");
        Assert.True(initial.GetProperty("canRegister").GetBoolean()); Assert.Equal(0, initial.GetProperty("registeredAdmins").GetInt32());
        for (var i=0;i<2;i++)
        {
            var email = $"admin{i}@example.test";
            (await client.PostAsJsonAsync("/api/auth/register",Input(email))).EnsureSuccessStatusCode();
            var login = await client.PostAsJsonAsync("/api/auth/login",Input(email)); login.EnsureSuccessStatusCode();
            Assert.Contains("Admin",(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("roles").EnumerateArray().Select(x=>x.GetString()));
        }
        var status = await client.GetFromJsonAsync<JsonElement>("/api/auth/registration-status");
        Assert.False(status.GetProperty("canRegister").GetBoolean()); Assert.Equal(2,status.GetProperty("registeredAdmins").GetInt32());
        Assert.Equal(HttpStatusCode.Conflict,(await client.PostAsJsonAsync("/api/auth/register",Input())).StatusCode);
        using var scope=database.Factory.Services.CreateScope(); var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(2,db.Users.Count());
    }
    [Fact]
    public async Task Concurrent_registrations_cannot_exceed_two_admins()
    {
        await using var database=await IsolatedAccountDatabase.Create(); using var client=database.Factory.CreateClient();
        var results=await Task.WhenAll(Enumerable.Range(0,6).Select(_=>client.PostAsJsonAsync("/api/auth/register",Input())));
        Assert.Equal(2,results.Count(x=>x.IsSuccessStatusCode)); Assert.Equal(4,results.Count(x=>x.StatusCode==HttpStatusCode.Conflict));
        var status=await client.GetFromJsonAsync<JsonElement>("/api/auth/registration-status"); Assert.Equal(2,status.GetProperty("registeredAdmins").GetInt32());
    }
    [Fact]
    public async Task Failed_registration_does_not_consume_an_admin_slot()
    {
        await using var database=await IsolatedAccountDatabase.Create(); using var client=database.Factory.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/auth/register",new{email="invalid",password="weak",displayName=""})).StatusCode);
        (await client.PostAsJsonAsync("/api/auth/register",Input("one@example.test"))).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/auth/register",Input("one@example.test"))).StatusCode);
        var status=await client.GetFromJsonAsync<JsonElement>("/api/auth/registration-status"); Assert.Equal(1,status.GetProperty("registeredAdmins").GetInt32());
    }
    [Fact]
    public async Task Admin_creates_edits_disables_and_reenables_staff_without_exposing_passwords()
    {
        await using var database=await IsolatedAccountDatabase.Create(); using var admin=await TestAccounts.Client(database.Factory,true);
        const string email="managed-staff@example.test";
        (await admin.PostAsJsonAsync("/api/staff",Input(email))).EnsureSuccessStatusCode();
        var listed=await admin.GetFromJsonAsync<JsonElement>("/api/staff"); var staff=listed.EnumerateArray().Single();var id=staff.GetProperty("id").GetString();
        Assert.False(staff.TryGetProperty("passwordHash",out _));
        using var client=database.Factory.CreateClient();var login=await client.PostAsJsonAsync("/api/auth/login",Input(email));login.EnsureSuccessStatusCode();
        var loggedIn=await login.Content.ReadFromJsonAsync<JsonElement>();Assert.DoesNotContain("Admin",loggedIn.GetProperty("roles").EnumerateArray().Select(x=>x.GetString()));
        client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",loggedIn.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/staff")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/staff",Input())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PutAsJsonAsync($"/api/staff/{id}",new{email,displayName="Unauthorized",isActive=false})).StatusCode);
        (await admin.PutAsJsonAsync($"/api/staff/{id}",new{email,displayName="Updated staff",isActive=false})).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/requests")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",Input(email))).StatusCode);
        (await admin.PutAsJsonAsync($"/api/staff/{id}",new{email,displayName="Updated staff",isActive=true})).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/api/auth/login",Input(email))).EnsureSuccessStatusCode();
    }
    [Theory][InlineData("GET")][InlineData("POST")][InlineData("PUT")]
    public async Task Anonymous_cannot_manage_staff(string method)
    {
        await using var database=await IsolatedAccountDatabase.Create();using var client=database.Factory.CreateClient();
        var request=new HttpRequestMessage(new HttpMethod(method),method=="PUT"?"/api/staff/unknown":"/api/staff"){Content=JsonContent.Create(Input())};
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.SendAsync(request)).StatusCode);
    }
    [Fact]
    public async Task Staff_routes_cannot_edit_admin_accounts_or_accept_invalid_profiles()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var admin=await TestAccounts.Client(database.Factory,true);
        using var scope=database.Factory.Services.CreateScope();var users=scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();var person=(await users.GetUsersInRoleAsync("Admin")).Single();
        Assert.Equal(HttpStatusCode.NotFound,(await admin.PutAsJsonAsync($"/api/staff/{person.Id}",new{email=person.Email,displayName="Unchanged",isActive=false})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.PostAsJsonAsync("/api/staff",new{email="bad",password="weak",displayName=""})).StatusCode);
        Assert.Empty((await admin.GetFromJsonAsync<JsonElement>("/api/staff")).EnumerateArray());
    }
}
