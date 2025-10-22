using LibraryApp.Api.Middlewares;
using LibraryApp.Application;
using LibraryApp.Infrastructure;
using LibraryApp.Infrastructure.Context;
using LibraryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRouting(d => d.LowercaseUrls = true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("services:smtp"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
    var @params = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("services:jwt:signKey")!)),
        ValidateLifetime = true
    };
    if (!builder.Environment.IsDevelopment())
    {
        @params.ValidateIssuer = true;
        @params.ValidIssuer = builder.Configuration.GetValue<string>("services:jwt:issuer");
        @params.ValidateAudience = true;
        @params.ValidAudience = builder.Configuration.GetValue<string>("services:jwt:audience");
    }

    jwtOptions.TokenValidationParameters = @params;
});

builder.Services.AddCors(d => d.AddPolicy("allowAny", d => d.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
}).AddCookie()
.AddGoogle(cfg =>
{
    cfg.ClientSecret = builder.Configuration.GetValue<string>("services:oauth:google:clientSecret")!;
    cfg.ClientId = builder.Configuration.GetValue<string>("services:oauth:google:clientId")!;
    cfg.CallbackPath = "/api/login/signin-google";
});

builder.Services.AddHttpContextAccessor();

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

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dbContext.Database.Migrate();
    }
}

//Redirect automatically http requests to https requests
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("allowAny");

app.UseMiddleware<GlobalExceptionHandler>();

app.MapControllers();

app.Run();
