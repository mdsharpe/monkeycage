using MonkeyCage.MonkeyBusiness;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddTransient<TypewriterService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Run();