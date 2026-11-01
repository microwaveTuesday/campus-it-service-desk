using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace CampusIT.Tests;

public class WorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> factory;
    public WorkflowTests(WebApplicationFactory<Program> factory) => this.factory = factory;
    Task<HttpClient> Account(bool admin = false) => TestAccounts.Client(factory, admin);
    [Theory]
    [InlineData("POST", "/api/categories")]
    [InlineData("PUT", "/api/categories/1")]
    [InlineData("DELETE", "/api/categories/1")]
    [InlineData("DELETE", "/api/requests/1")]
    public async Task Staff_cannot_use_admin_operations(string method, string url)
    {
        using var client = await Account();
        var request = new HttpRequestMessage(new HttpMethod(method), url) { Content = JsonContent.Create(new { name = "Synthetic category" }) };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(request)).StatusCode);
    }
    [Fact]
    public async Task Both_entities_complete_crud_and_dashboard_persists()
    {
        using var admin = await Account(true);
        var category = await admin.PostAsJsonAsync("/api/categories", new { name = $"Test-{Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, category.StatusCode);
        var cid = (await category.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        (await admin.GetAsync($"/api/categories/{cid}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsJsonAsync($"/api/categories/{cid}", new { name = $"Updated-{Guid.NewGuid():N}" })).StatusCode);
        var input = new { requesterName = "Synthetic User", issue = "Test network outage", location = "Lab 1", categoryId = cid, status = "Pending" };
        var created = await admin.PostAsJsonAsync("/api/requests", input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        using var staff = await Account();
        var fetched = await staff.GetFromJsonAsync<JsonElement>($"/api/requests/{id}");
        Assert.Equal(input.issue, fetched.GetProperty("issue").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/api/categories/{cid}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await staff.PutAsJsonAsync($"/api/requests/{id}", input)).StatusCode);
        var dashboard = await staff.GetFromJsonAsync<JsonElement>("/api/dashboard");
        Assert.True(dashboard.GetProperty("total").GetInt32() > 0);
        using var upload = new MultipartFormDataContent();
        var bytes = new ByteArrayContent("%PDF-1.7\nsynthetic test"u8.ToArray());
        bytes.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        upload.Add(bytes, "file", "test.pdf");
        var uploaded = await staff.PostAsync($"/api/requests/{id}/attachments", upload);
        uploaded.EnsureSuccessStatusCode();
        var aid = (await uploaded.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var download = await staff.GetByteArrayAsync($"/api/requests/{id}/attachments/{aid}");
        Assert.Equal("%PDF-1.7\nsynthetic test"u8.ToArray(), download);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/requests/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await staff.GetAsync($"/api/requests/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/categories/{cid}")).StatusCode);
    }
    [Fact]
    public async Task Invalid_request_is_rejected()
    {
        using var client = await Account();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/requests", new { requesterName = "", issue = "", location = "", categoryId = 0 })).StatusCode);
    }
    [Theory]
    [InlineData("fake.pdf", "application/pdf", 20)]
    [InlineData("bad.exe", "application/octet-stream", 20)]
    [InlineData("empty.pdf", "application/pdf", 0)]
    [InlineData("big.pdf", "application/pdf", 5242881)]
    public async Task Invalid_upload_is_rejected(string name, string mime, int size)
    {
        using var client = await Account(true);
        var categories = await client.GetFromJsonAsync<JsonElement>("/api/categories");
        var cid = categories[0].GetProperty("id").GetInt32();
        var created = await client.PostAsJsonAsync("/api/requests", new { requesterName = "Synthetic", issue = "Upload validation", location = "Lab", categoryId = cid });
        created.EnsureSuccessStatusCode();
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        try
        {
            using var upload = new MultipartFormDataContent();
            var bytes = new ByteArrayContent(new byte[size]);
            bytes.Headers.ContentType = new MediaTypeHeaderValue(mime);
            upload.Add(bytes, "file", name);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/api/requests/{id}/attachments", upload)).StatusCode);
        }
        finally { (await client.DeleteAsync($"/api/requests/{id}")).EnsureSuccessStatusCode(); }
    }
}

