using LibraryApp.Api.Middlewares;
using LibraryApp.Application;
using LibraryApp.Infrastructure;
using LibraryApp.Infrastructure.Context;
using LibraryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);
});
builder.Services.AddRouting(d => d.LowercaseUrls = true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.Migrate();
}


//Redirect automatically http requests to https requests
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseCors("allowAny");

app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandler>();

app.MapControllers();

app.Run();