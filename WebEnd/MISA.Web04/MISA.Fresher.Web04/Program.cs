using MISA.Fresher.Infrastructer.Data;
using MISA.Fresher.Infrastructer.Interfaces;
using MISA.Fresher.Infrastructer.Services;
using MISA.Fresher04.Infrastructer.Repositories;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI: Connection Factory
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new MySqlConnectionFactory(builder.Configuration.GetConnectionString("Default")!));

// DI: Repository + Service
builder.Services.AddScoped<IFixedAssetRepository, FixedAssetRepository>();
builder.Services.AddScoped<IFixedAssetService, FixedAssetService>();

var app = builder.Build();

Console.WriteLine("CS=" + builder.Configuration.GetConnectionString("Default"));

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
