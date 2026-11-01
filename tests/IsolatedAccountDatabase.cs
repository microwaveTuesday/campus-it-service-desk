using CampusIT.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace CampusIT.Tests;

internal sealed class IsolatedAccountDatabase : IAsyncDisposable
{
    readonly string connection;
    readonly string name;
    public WebApplicationFactory<Program> Factory { get; }
    IsolatedAccountDatabase(string connection, string name, string isolatedConnection)
    {
        this.connection = connection; this.name = name;
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(isolatedConnection));
            }));
    }
    public static async Task<IsolatedAccountDatabase> Create()
    {
        var source = Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? throw new InvalidOperationException("Disposable PostgreSQL configuration required");
        var name = "campus_accounts_" + Guid.NewGuid().ToString("N");
        await using var connection = new NpgsqlConnection(source); await connection.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection); await create.ExecuteNonQueryAsync();
        var isolated = new NpgsqlConnectionStringBuilder(source) { Database = name }.ConnectionString;
        return new(source, name, isolated);
    }
    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await using var database = new NpgsqlConnection(connection); await database.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE \"{name}\" WITH (FORCE)", database); await drop.ExecuteNonQueryAsync();
    }
}
