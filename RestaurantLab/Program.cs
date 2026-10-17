using Microsoft.EntityFrameworkCore;
using RestaurantLab.Data;
using RestaurantLab.UoW;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestaurantDb")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
