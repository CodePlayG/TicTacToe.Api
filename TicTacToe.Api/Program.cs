using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerUI; 
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<GameSessionStore>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Game API",
        Version = "v1",
        Description = "API for managing game sessions and scoreboard."
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline
app.UseCors();
//app.UseSwagger();
//app.UseSwaggerUI(options =>
//{
//    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Game API v1");
//    options.RoutePrefix = string.Empty; // Swagger at root
//});

app.MapControllers();

app.Run();
