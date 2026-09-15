using System.Text.Json;
using System.Text.Json.Serialization;
using Instagram.Api.Data;
using Instagram.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// The Angular dev server runs on its own origin, so it needs an explicit grant.
const string AngularCors = "angular-dev";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200", "https://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCors, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase is the default, but the enum converter is worth being explicit about.
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// One store for the whole process: likes, comments and follows made through
// the API stick around until the app restarts.
builder.Services.AddSingleton<DummyDataStore>();
builder.Services.AddScoped<DtoMapper>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Instagram Dummy API",
        Version = "v1",
        Description = "A dummy Instagram-style API backed by in-memory seed data.",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Instagram Dummy API v1");
        options.DocumentTitle = "Instagram Dummy API";
    });
}
else
{
    // Only redirect in non-development: over plain `dotnet run` the HTTPS port
    // may not be listening, and redirecting there would break the dev proxy.
    app.UseHttpsRedirection();
}

app.UseCors(AngularCors);

app.MapControllers();

// A tiny liveness probe, handy for checking the API is up before the UI is.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "instagram-dummy-api" }));

// Land on Swagger rather than a blank 404.
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
