using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartWalletAI.Application;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Infrastructure;
using SmartWalletAI.Infrastructure.Services;
using SmartWalletAI.WebAPI.Middlewares;
using SmartWalletAI.WebAPI.Workers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

builder.Services.AddMemoryCache();

builder.Services.AddInfrastructureServices(builder.Configuration);



builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(30); 
        opt.PermitLimit = 5;                   
        opt.QueueLimit = 0;                    

       
        opt.SegmentsPerWindow = 3;
      
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
        
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"\n\n=== TOKEN REDDEDÝLDÝ! ===");
            Console.WriteLine($"SEBEP: {context.Exception.Message}");
            Console.WriteLine($"=====================================================\n\n");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHostedService<MarketDataWorker>();
builder.Services.AddHttpClient<IMarketDataService, CollectApiMarketService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Tokeni buraya yapýþtýr",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, 
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Bu kilidin tüm endpointler için geçerli olduðunu söylüyoruz
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();