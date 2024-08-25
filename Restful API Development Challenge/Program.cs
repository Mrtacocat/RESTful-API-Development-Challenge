using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Restful_API_Development_Challenge.Data;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


// I Really tried making a authentication with JWT bearer but i ran into issues but this is close to what its suppose to be.
// To implement this auth in a CRUD operation you just have to type [Authorize] above the operation & using Microsoft.AspNetCore.Authorization;
// the "Token" is in the 'appsettings.json' file
// Configured the Database context
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LibraryDb")));

// Configured JWT Authentication
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options =>
     {
         var jwtSection = builder.Configuration.GetSection("JWT");
         var key = jwtSection["Key"];

         if (string.IsNullOrEmpty(key))
         {
             throw new InvalidOperationException("JWT Key is not configured.");
         }

         var keyBytes = Encoding.UTF8.GetBytes(key);

         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
             ValidIssuer = jwtSection["Issuer"],
             ValidAudiences = jwtSection.GetSection("Audience").Get<string[]>()
         };
     });

// Configured Swagger with JWT Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Restful API Development Challenge",
        Description = "An ASP.NET Core Web API for managing a small library",
    });

    // Fix so I can use xml
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization : `Bearer Generated-JWT-Token`",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type= ReferenceType.SecurityScheme,
                Id= JwtBearerDefaults.AuthenticationScheme
            }
        }, new string[]{ }
        }
    });
   
});

var app = builder.Build();

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
