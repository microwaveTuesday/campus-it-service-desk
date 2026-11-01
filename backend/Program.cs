using CampusIT.Api.Data;
using CampusIT.Api.Models;
using CampusIT.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder=WebApplication.CreateBuilder(args);
var connection=builder.Configuration.GetConnectionString("Default")??throw new InvalidOperationException("ConnectionStrings:Default is required");
var jwtKey=builder.Configuration["Jwt:Key"]??throw new InvalidOperationException("Jwt:Key is required");
if(Encoding.UTF8.GetByteCount(jwtKey)<32)throw new InvalidOperationException("JWT key must contain at least 32 bytes");
builder.Services.AddDbContext<AppDbContext>(o=>o.UseNpgsql(connection));
builder.Services.AddIdentityCore<AppUser>(o=>{o.Password.RequiredLength=10;o.Password.RequireNonAlphanumeric=true;o.User.RequireUniqueEmail=true;}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddSignInManager();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>{o.TokenValidationParameters=new(){ValidateIssuer=true,ValidIssuer=builder.Configuration["Jwt:Issuer"],ValidateAudience=true,ValidAudience=builder.Configuration["Jwt:Audience"],ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),ValidateLifetime=true,ClockSkew=TimeSpan.FromMinutes(1)};o.Events=new JwtBearerEvents{OnTokenValidated=async context=>{var id=context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;var manager=context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();var user=id is null?null:await manager.FindByIdAsync(id);if(user is null||!user.IsActive)context.Fail("Account unavailable");}};});
builder.Services.AddAuthorization(o=>o.AddPolicy("CanManageRequests",p=>p.RequireRole("Admin","Staff")));
builder.Services.AddScoped<AccountService>();builder.Services.AddScoped<ServiceRequestService>();builder.Services.AddHttpClient<WeatherService>(c=>c.Timeout=TimeSpan.FromSeconds(4));
builder.Services.AddControllers();builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen(o=>{o.AddSecurityDefinition("Bearer",new(){Name="Authorization",Type=Microsoft.OpenApi.Models.SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT",In=Microsoft.OpenApi.Models.ParameterLocation.Header});o.AddSecurityRequirement(new(){[new Microsoft.OpenApi.Models.OpenApiSecurityScheme{Reference=new(){Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,Id="Bearer"}}]=Array.Empty<string>()});});
var origins=builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()??["http://localhost:3000"];
builder.Services.AddCors(o=>o.AddPolicy("frontend",p=>p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();
var app=builder.Build();app.UseExceptionHandler();app.UseSwagger();app.UseSwaggerUI();app.UseCors("frontend");app.UseAuthentication();app.UseAuthorization();app.MapControllers();app.MapGet("/health",()=>Results.Ok(new{status="ok"}));
using(var scope=app.Services.CreateScope()){
 var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();await db.Database.MigrateAsync();
 var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();foreach(var role in new[]{"Admin","Staff"})if(!await roles.RoleExistsAsync(role))await roles.CreateAsync(new IdentityRole(role));
 if(args.Contains("--provision-admin"))await OperatorCommands.ProvisionAdmin(scope.ServiceProvider,builder.Configuration);
 if(args.Contains("--seed-demo"))await SeedDemo.SeedAsync(db);
}
if(args.Contains("--provision-admin")||args.Contains("--seed-demo"))return;
app.Run();
public partial class Program {}
