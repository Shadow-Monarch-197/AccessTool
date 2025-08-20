using System.IdentityModel.Tokens.Jwt;                    // NEW
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;                 // NEW
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using quizTool.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ExcelDataReader requirement
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// ---------- Controllers ----------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------- DB ----------
builder.Services.AddDbContext<QuizTool_Dbcontext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("APIConnection")));

// ---------- AuthN/Z ----------
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // NEW: don't remap claim types

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false; // dev
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("underworldmeinjigrayaarakenaamka")),
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ClockSkew = TimeSpan.Zero,
//        // read the same claim names we will emit in UserController
//        NameClaimType = JwtRegisteredClaimNames.UniqueName,   // NEW
//        RoleClaimType = "role"                                // NEW
//    };
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("underworldmeinjigrayaarakenaamka")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = System.Security.Claims.ClaimTypes.Role, // NEW
        NameClaimType = System.Security.Claims.ClaimTypes.Name  // NEW
    };
});

//// Require auth by default (Login/Register still marked [AllowAnonymous])
//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();                                            // NEW
//});

builder.Services.AddAuthorization();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "quizTool API", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName);

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT token **only** (no 'Bearer ' prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

app.UseCors("allowCors");

// ensure wwwroot/uploads exists
var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "quizTool API v1"); });

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();   // must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// Run migrations + seed (don't crash if DB is down)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<QuizTool_Dbcontext>();
        db.Database.Migrate();
        db.SeedUsers();
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB init failed: " + ex.Message);
    }
}

app.Run();
