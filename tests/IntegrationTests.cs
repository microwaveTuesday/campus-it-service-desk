using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
namespace CampusIT.Tests;
// These tests exercise the complete HTTP middleware and routing pipeline against a real PostgreSQL test database.
// Configure ConnectionStrings__Default and Jwt__Key before executing; CI provisions PostgreSQL automatically.
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>> {
 readonly HttpClient client;
 public IntegrationTests(WebApplicationFactory<Program> factory)=>client=factory.CreateClient();
 [Theory][InlineData("/api/requests")][InlineData("/api/requests/1")][InlineData("/api/categories")][InlineData("/api/categories/1")][InlineData("/api/dashboard")][InlineData("/api/weather")][InlineData("/api/requests/1/attachments")]
 public async Task Protected_get_requires_authentication(string url){var result=await client.GetAsync(url);Assert.Equal(HttpStatusCode.Unauthorized,result.StatusCode);}
 [Fact]public async Task Request_create_requires_authentication(){var x=await client.PostAsJsonAsync("/api/requests",new{requesterName="T",issue="Test",location="Lab",categoryId=1,status="Pending"});Assert.Equal(HttpStatusCode.Unauthorized,x.StatusCode);}
 [Fact]public async Task Category_create_requires_authentication(){var x=await client.PostAsJsonAsync("/api/categories",new{name="Test"});Assert.Equal(HttpStatusCode.Unauthorized,x.StatusCode);}
 [Fact]public async Task Request_delete_requires_authentication(){var x=await client.DeleteAsync("/api/requests/1");Assert.Equal(HttpStatusCode.Unauthorized,x.StatusCode);}
 [Fact]public async Task Health_is_public(){var x=await client.GetAsync("/health");Assert.Equal(HttpStatusCode.OK,x.StatusCode);}
 [Fact]public async Task Wrong_password_returns_401(){var x=await client.PostAsJsonAsync("/api/auth/login",new{email="nobody@example.test",password="Incorrect!123"});Assert.Equal(HttpStatusCode.Unauthorized,x.StatusCode);}
}
