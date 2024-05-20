using Log.WebAPI.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSingleton<LogFilterAttribute>();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = "Serhan",
        ValidAudience = "Ozge",
        IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("my secret key my secret key my secret key my secret key my secret key"))
    };
});

builder.Services.AddAuthorizationBuilder();

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    //.WriteTo.Console()
    //.WriteTo.File("./log.txt",LogEventLevel.Information,rollingInterval: RollingInterval.Minute)
    .WriteTo.MSSqlServer(
    connectionString:builder.Configuration.GetConnectionString("SqlServer"),
    sinkOptions: new()
    {
        TableName = "Logs",
        AutoCreateSqlTable = true
    },
    restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.Seq("http://localhost:5341",LogEventLevel.Information)
    .CreateLogger();

builder.Services.AddDbContext<ApplicationDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Host.UseSerilog();//Serilog'un otomatik olarak çalýþtýðý yapý ekliyoruz. Ve uygulamanýn her adýmýnýn logluyor.

//var log = new LoggerConfiguration()
//    .CreateLogger();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** yourt JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
}
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        object errorLog = new
        {
            Error = ex,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException,
            Message = ex.Message,
            Path = context.Request.Path
        };

        context.Response.StatusCode = 500;
        object data = new
        {
            Message = ex.Message
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(data));
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers().RequireAuthorization(options =>
{
    options.RequireClaim(ClaimTypes.NameIdentifier);
    options.AddAuthenticationSchemes("Bearer");
});

app.Run();
