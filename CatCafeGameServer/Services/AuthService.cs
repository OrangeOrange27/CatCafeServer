using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CatCafeGameServer.Models;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary;

namespace CatCafeGameServer.Services;

public class AuthService : IAuthService
{
    private readonly Settings _settings;
    private readonly GameDbContext _dbContext;

    public AuthService(Settings settings, GameDbContext dbContext)
    {
        _settings = settings;
        _dbContext = dbContext;
    }

    public (bool success, string content) Register(string username, string password)
    {
        if (_dbContext.Users.Any(u => u.Username == username)) return (false, "Username not available");

        var user = new User { Username = username };

        user.ProvideSaltAndHash(password);

        _dbContext.Add(user);
        _dbContext.SaveChanges();

        return (true, "");
    }

    public (bool success, string token) Login(string username, string password)
    {
        var user = _dbContext.Users.SingleOrDefault(u => u.Username == username);
        if (user == null)
            return (false, "Invalid username");

        return user.PasswordHash != AuthenticationHelper.ComputeHash(password, user.Salt)
            ? (false, "Invalid password")
            : (true, GenerateJwtToken(AssembleClaimsIdentity(user)));
    }

    private static ClaimsIdentity AssembleClaimsIdentity(User user)
    {
        var subject = new ClaimsIdentity(new[]
        {
            new Claim("id", user.Id.ToString()),
        });

        return subject;
    }

    private string GenerateJwtToken(ClaimsIdentity subject)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_settings.BearerKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.Now.AddYears(10),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public interface IAuthService
{
    (bool success, string content) Register(string username, string password);
    (bool success, string token) Login(string username, string password);
}

public static class AuthenticationHelper
{
    public static void ProvideSaltAndHash(this User user, string password)
    {
        var salt = GenerateSalt();
        user.Salt = Convert.ToBase64String(salt);
        user.PasswordHash = ComputeHash(password, user.Salt);
    }

    public static string ComputeHash(string password, string saltString)
    {
        var salt = Convert.FromBase64String(saltString);

        using var hashGenerator = new Rfc2898DeriveBytes(password, salt);
        hashGenerator.IterationCount = 10101;
        var bytes = hashGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes);
    }

    private static byte[] GenerateSalt()
    {
        var rng = RandomNumberGenerator.Create();
        var salt = new byte[24];
        rng.GetBytes(salt);
        return salt;
    }
}