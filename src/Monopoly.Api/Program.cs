/// dotnet new webapi -n Monopoly.Api -o . --use-controllers -f net8.0
/// cd ..
/// dotnet add Monopoly.Api/Monopoly.Api.csproj reference Monopoly.Core/Monopoly.Core.csproj
/// 
 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Controller
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Cors for Future FE



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
