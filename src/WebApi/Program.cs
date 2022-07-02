using Microsoft.Extensions.Internal;
using MonkeyCage.MonkeyBusiness;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services
    .AddSingleton<ISystemClock, SystemClock>()
    .AddTransient<MonkeyFactory>()
    .AddTransient<MonkeyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Run();
