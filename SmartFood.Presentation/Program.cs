using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFood.Application.Concrete.Factories;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Application.Services;
using SmartFood.Presentation.Persistence;
using SmartFood.Presentation.Repositories;
using System.Text;
using TodoApp.Presentation.Service;

namespace SmartFood.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();


            // ============================
            // Dependency Injection
            // ============================
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Approved", policy =>
                {
                    policy.Requirements.Add(new ApprovedRequirement());
                });
            });

            builder.Services.AddScoped<IAuthorizationHandler, ApprovedHandler>();

            // Generic Repository
            builder.Services.AddScoped(
                typeof(IGenericRepository<,>),
                typeof(GenericRepository<,>)
            );

            builder.Services.AddScoped<
                IRestaurantService,
                RestaurantService
                >();
            // Current User
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            // Specification Builder Factory
            builder.Services.AddSingleton<
                ISpecificationBuilderFactory,
                SpeificationBuilderFactory
            >();

            // Restaurant Authentication
            builder.Services.AddScoped<
                IAuthenticationServiceRestaurant,
                AuthenticationRestaurantService
            >();

            // Organization Authentication
            builder.Services.AddScoped<
                IAuthenticationServiceOrganization,
                AuthenticationOrgnizationService
            >();

            // File Storage
            builder.Services.AddScoped<
                IFileStorageService,
                LocalFileStorageService
            >();

            // File Upload
            builder.Services.AddScoped<
                IFileService,
                FileUploadService
            >();

            builder.Services.AddScoped<
                IAdminService,
                AdminService
            >();

            // ============================
            // Database
            // ============================
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("MyConnection")
                );
            });

            // ============================
            // CORS
            // ============================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SmartFood", policy =>
                {
                    policy
                        .WithOrigins(
                            "https://localhost:7259",
                            "http://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // ============================
            // JWT Authentication
            // ============================
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine(
                                "JWT ERROR : " +
                                context.Exception.Message
                            );

                            return Task.CompletedTask;
                        },

                        OnForbidden = context =>
                        {
                            Console.WriteLine("JWT FORBIDDEN");

                            return Task.CompletedTask;
                        }
                    };

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ClockSkew = TimeSpan.Zero,

                            ValidIssuer =
                                builder.Configuration["JWT:Issuer"],

                            ValidAudience =
                                builder.Configuration["JWT:Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        builder.Configuration["JWT:Key"]!
                                    )
                                )
                        };
                });

            // ============================
            // Swagger + JWT
            // ============================
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,

                        Description =
                            "Enter: Bearer {your JWT token}"
                    }
                );

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
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
                        }
                    }
                );
            });

            var app = builder.Build();

            // ============================
            // Middleware Pipeline
            // ============================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("SmartFood");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}