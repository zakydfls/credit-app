using CreditApplication.Api.Middleware;
using CreditApplication.Api.Responses;
using CreditApplication.Api.Authentication;
using CreditApplication.Api.OpenApi;
using CreditApplication.Application.CreditApplications;
using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Auth;
using CreditApplication.Application.Installments;
using CreditApplication.Infrastructure;
using CreditApplication.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

// Add Jwt authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ICreditApplicationService, CreditApplicationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IInstallmentService, InstallmentService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Masukkan access token JWT."
    });
    options.OperationFilter<BearerAuthorizationOperationFilter>();
});

// Add Serilog
builder.Services.AddProblemDetails();
builder.Services.AddSerilog((_, logger) => logger
    .Enrich.FromLogContext()
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("CreditDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'CreditDatabase' was not configured.");

builder.Services.AddInfrastructure(connectionString, builder.Configuration);

// Add exception handler and validation response factory
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory =
        ApiValidationResponse.Create;
});


var app = builder.Build();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("SeedDemoUser"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DemoUserSeeder>().SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, exception) =>
        exception is not null ||
        httpContext.Response.StatusCode >= 500
            ? Serilog.Events.LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400
                ? Serilog.Events.LogEventLevel.Warning
                : Serilog.Events.LogEventLevel.Information;

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
    };
});

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
