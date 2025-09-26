using LibraryApp.Api.Middlewares;
using LibraryApp.Application;
using LibraryApp.Infrastructure;
using LibraryApp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("services:smtp"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });

    app.UseReDoc(opt => opt.SpecUrl("/openapi/v1.json"));

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionHandler>();

app.UseAuthorization();

app.MapControllers();

app.Run();
