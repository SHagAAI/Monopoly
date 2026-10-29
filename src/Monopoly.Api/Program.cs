using Monopoly.Api.Hubs;
using Monopoly.Api.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller & SignalR

//  change it because C# cant automatically detect "Red" as an enum. 
// by default accept integer as enum.
// builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ADD SINGLETON GAMESERVICE
builder.Services.AddSingleton<GameService>();

// 2. Cors for React Frontend + SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// LOGGER
// SERILOG CONFIGURATION
Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                // .WriteTo.File(new JsonFormatter(), "logs/monopolyLog.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
     .WriteTo.File(new JsonFormatter(), "logs/monopolyLog.log", rollingInterval: RollingInterval.Day));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS must be used before MapHub
app.UseCors("AllowReactApp");
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");


try
{
    app.Run();
}
catch (System.Exception)
{

    Log.CloseAndFlush();
}