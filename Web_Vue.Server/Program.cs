using Autofac;
using Autofac.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Web_Vue.Server;
using Web_Vue.Server._Authorization;
using Web_Vue.Server._Middleware;
using Web_Vue.Server.Interfaces;
using Web_Vue.Server.Models;
using Web_Vue.Server.Tools;
using Web_Vue.Server.ViewModels.Base;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container.

// 停用 camelCase，讓 JSON 屬性名稱與 C# 類別一致（PascalCase）
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 多語系：從 Resources/SharedResource.{locale}.resx 讀取
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 註冊 FluentValidation，自動掃描當前組件中所有 IValidator 實作
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── EF Core + PostgreSQL ──────────────────────────────────────────────────────
builder.Services.AddDbContext<DbEntityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 記憶體快取 ────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ── 當前使用者服務 ──────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── AppSettings（統一設定入口，後續以 IOptions<AppSettings> 注入） ────────────────
builder.Services.Configure<AppSettings>(builder.Configuration);

var appSettings = builder.Configuration.Get<AppSettings>()
    ?? throw new InvalidOperationException("AppSettings 讀取失敗");

var jwtSettings = appSettings.JwtSettings;

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JwtSettings:SecretKey 未設定");
}

// ── JWT 身份驗證 ───────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew                = TimeSpan.Zero,
        };
        // 從 HttpOnly Cookie 讀取 JWT（取代 Authorization Header）
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Cookies["jwt"];
                if (!string.IsNullOrWhiteSpace(token))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── 權限驗證 ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "請輸入 JWT Token，例如：Bearer eyJhbGci...",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer",
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS：從 appsettings 讀取允許來源清單 ────────────────────────────────────────
var allowedOrigins = appSettings.Cors.AllowedOrigins;
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins([.. allowedOrigins])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Autofac：自動掃描 Services / Repositories 並以 Scoped 生命週期註冊 ──────────
builder.Host.ConfigureContainer<ContainerBuilder>(c => c.RegisterModule<AppModule>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();

// 啟用多語系中介軟體，依 Accept-Language header 切換語系
var supportedCultures = new[] { "zh-TW", "en-US" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("zh-TW")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseAuthentication();
app.UseMiddleware<CsrfValidationMiddleware>();
app.UseAuthorization();

// ── 安全 HTTP Headers（API 回應） ─────────────────────────────────────────────
// HTML 頁面的 CSP 由 Nuxt server/middleware/security.ts 管理
// 此處針對 /api/** JSON 回應補上必要的安全 headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"]  = "nosniff";
    context.Response.Headers["X-Frame-Options"]         = "DENY";
    context.Response.Headers["Referrer-Policy"]         = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"]        = "0";
    context.Response.Headers["Permissions-Policy"]      =
        "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("Server");

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] =
            "max-age=63072000; includeSubDomains; preload";
    }

    await next();
});

app.MapControllers();
app.MapFallbackToFile("/index.html");

// 初始化種子資料
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbEntityContext>();
    await SeedData.InitializeAsync(db);
}

app.Run();

