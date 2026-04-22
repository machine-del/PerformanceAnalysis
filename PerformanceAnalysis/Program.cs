using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using PerformanceAnalysis.Application.Auth;
using PerformanceAnalysis.Application.Auth.Dtos;
using PerformanceAnalysis.Application.Auth.Services;
using PerformanceAnalysis.Application.Reports;
using PerformanceAnalysis.Infrastructure.Reports;
using System.Data;
using System.Text;

namespace PerformanceAnalysis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PerformanceAnalysis API",
                    Version = "v1"
                });

                // Добавляем поддержку JWT Bearer в Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Введите JWT токен в формате: Bearer {your_token}"
                });

                // Добавляем требования безопасности (можно использовать или Bearer, или Cookie)
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                        {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
                },
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                        {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Cookie"
                    }
                },
                    Array.Empty<string>()
                }
                });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "PerformanceAnalysis.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.LoginPath = "/api/auth/login-with-cookie";
                options.LogoutPath = "/api/auth/logout";
            });

            builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(builder.Configuration.GetConnectionString("Default")));
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IDapperExecutor, DapperExecutor>();
            builder.Services.AddDbContext<AuthDbContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
            builder.Services.AddScoped<IAuthServices, AuthService>();
            builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();
            builder.Services.AddScoped<ICookieAuthService, CookieAuthService>();
            builder.Services.AddScoped<IAuthServices, AuthService>();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
