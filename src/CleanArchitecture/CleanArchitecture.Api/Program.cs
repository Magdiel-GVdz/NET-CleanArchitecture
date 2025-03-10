using Asp.Versioning;
using Asp.Versioning.Builder;
using CleanArchitecture.Api.Controllers.Alquileres;
using CleanArchitecture.Api.Documentation;
using CleanArchitecture.Api.Extensions;
using CleanArchitecture.Api.OptionsSetup;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Abstractions.Authentication;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Authentication;
using CleanArchitecture.Infrastructure.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
configuration.ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer();


QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;


builder.Services.ConfigureOptions<JwtOptionsSetup>();

builder.Services
.Configure<GmailSettings>(builder.Configuration.GetSection("GmailSettings"));

builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

builder.Services.AddTransient<IJwtProvider, JwtProvider>();

builder.Services.AddAuthorization();
builder.Services
.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services
.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.ToString());
});


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();


app.MapGet("/", () => "Hola Clean Architecture en .NET University");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();

        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }


    });
}

await app.ApplyMigration();
//app.SeedData();
//app.SeedDataAuthentication();



app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseCustomExceptionHandler();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

ApiVersionSet apiVersion = app.NewApiVersionSet()
      .HasApiVersion(new ApiVersion(1))
      .ReportApiVersions()
      .Build();

var routeGroupBuilder = app
.MapGroup("api/v{version:apiVersion}")
.WithApiVersionSet(apiVersion);

routeGroupBuilder.MapAlquilerEndpoints();


app.Run();

public partial class Program;


