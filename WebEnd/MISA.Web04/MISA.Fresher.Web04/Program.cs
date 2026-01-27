using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Core.Interface.Service;
using MISA.Fresher.Core.Service;
using MISA.Fresher.Infrastructer.Repository;
using MISA.Fresher04.Infrastructer.Repositories;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//Add service to the container
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// DI: Repository + Service
builder.Services.AddScoped<IFixedAssetRepo, FixedAssetRepo>();
builder.Services.AddScoped<IFixedAssetService, FixedAssetService>();

builder.Services.AddScoped<IFixedAssetDepartmentRepo, FixedAssetDepartmentRepo>();
builder.Services.AddScoped<IFixedAssetDepartmentService, FixedAssetDepartmentService>();

builder.Services.AddScoped<IFixedAssetTypeRepo, FixedAssetTypeRepo>();
builder.Services.AddScoped<IFixedAssetTypeService, FixedAssetTypeService>();

// Mô tả: Cấu hình CORS cho phép Frontend (Vite) gọi API
// Ngày tạo: 2026-01-15
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVite", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // nếu sau này dùng cookie/jwt
    });
});

var app = builder.Build();
app.UseCors("AllowVite");

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
