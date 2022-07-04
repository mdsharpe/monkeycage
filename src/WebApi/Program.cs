using MonkeyCage.MonkeyBusiness;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddDebug()
    .AddApplicationInsights()
    .AddAzureWebAppDiagnostics();

// Add services to the container.

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

builder.Services.AddControllers();

builder.Services
    .AddHttpClient()
    .AddTransient<MonkeyFactory>()
    .AddTransient<MonkeyService>()
    .AddTransient<ResultPersistenceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
