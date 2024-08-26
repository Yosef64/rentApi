using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAccountRepo, AccountRepo>();
builder.Services.AddScoped<IAccountService, AccountService>();

// Configure FirestoreDb
string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

// Configure JWT authentication

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{

    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:key"] ?? string.Empty);
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true
    };
});

builder.Services.AddSwaggerGen(swagger=>{
    swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "ASP .NET 8 Web API", Version = "v1",Description ="Authentication" });
    swagger.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme(){
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Name="Authorization"
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },Array.Empty<string>()
        }    
    }
    );
    
});

builder.Services.AddAuthorization();
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

// Users Endpoints
app.MapGet("/users", async () => await Users.GetUsersAsync()).RequireAuthorization();
app.MapGet("/users/{email}", async (string email) => await Users.GetUserByEmail(email)).RequireAuthorization();
app.MapPost("/users", async (User user) => await Users.AddUser(user)).RequireAuthorization();

// Authentication Endpoints
app.MapPost("/register", async (User user,IAccountService account) =>
{
   return Results.Ok(await account.Register(user));
}).AllowAnonymous();
app.MapPost("/login", async (LoginDto loginDto,IAccountService account) =>
{
   return Results.Ok(await account.Login(loginDto));
}).AllowAnonymous();


// Posts Endpoints
app.MapGet("/posts", async () => await Posts.GetPostsAsync()).RequireAuthorization();
app.MapPost("/posts", async (Post post) => await Posts.AddPostAsync(post)).RequireAuthorization();
app.MapDelete("/posts/{id}", async (string id) => await Posts.DeletePostAsync(id)).RequireAuthorization();
app.MapPut("/posts", async (RatePut post) => await Posts.UpdatePostRatingAsync(post)).RequireAuthorization();

app.Run();
