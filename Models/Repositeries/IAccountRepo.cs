
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using FirebaseAdmin.Auth.Hash;
using Microsoft.IdentityModel.Tokens;

public interface IAccountRepo{
    Task<Response> Register(User user);
    Task<LoginResponse> Login(LoginDto loginDto);
}

public class AccountRepo : IAccountRepo
{
    private readonly IConfiguration _config;
    public AccountRepo(IConfiguration config)
    {
        _config = config;
    }

    public async Task<LoginResponse> Login(LoginDto loginDto)
    {
        var user = await Users.GetUserByEmail(loginDto.Email);
        if(user != null)
        {
            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password,user.Password); 
            if (!verifyPassword)
            {
                return new LoginResponse(false,null!,"Invalid Password");
            }
            string token = GenerateToke(user);
            return new LoginResponse(true,token,null!);
        }
        return new LoginResponse(false,null!,"Invalid Email");
    }
    public async Task<Response> Register(User user)
    {
        var _user =await Users.GetUserByEmail(user.Email!);
        if(_user!=null)
        {
            return new Response(false, "Email already exists");
        }
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password!);
        await Users.AddUser(user);
        return new Response(true, "User registered successfully");
    }
    private string GenerateToke(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new []
        {
            new Claim("Fullname", user.Name!),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Email, user.Email!),
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
        
    }
}