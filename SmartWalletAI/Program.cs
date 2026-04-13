using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartWalletAI.Application;
using SmartWalletAI.Infrastructure; // Artýk tüm Infra burada
using SmartWalletAI.WebAPI.Middlewares;
using SmartWalletAI.WebAPI.Workers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Katman Servislerini Kaydet
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration); // Manager, Repository ve Client'lar burada çözülüyor

builder.Services.AddMemoryCache();

// 2. Rate Limiting (Ýstek Sýnýrlandýrma)
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

// 3. Authentication & JWT Ayarlarý
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
            Console.WriteLine($"\n\n=== TOKEN REDDEDÝLDÝ! ===\nSEBEP: {context.Exception.Message}\n==========================\n\n");
            return Task.CompletedTask;
        }
    };
});

// 4. Background Services (Worker)
// Not: Worker içindeki IMarketPriceManager kullanýmý için CreateScope() mantýðýný unutma!
builder.Services.AddHostedService<MarketDataWorker>();

// 5. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Tokeninizi 'Bearer {token}' þeklinde girin",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

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

// Özel Hata Yönetimi
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();