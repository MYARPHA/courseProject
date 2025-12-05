using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using courseProject.Data;
using courseProject.Models;

namespace courseProject.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string? Authenticate(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return GenerateJwtToken(user);
        }

        public async Task<User> Register(UserRegistrationRequest request)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = HashPassword(request.Password),
                Role = request.Role ?? "accountant",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private string GenerateJwtToken(User user)
        {
            // Safe read of JWT configuration with environment fallback and defaults
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings["SecretKey"]
                         ?? _configuration["Jwt:SecretKey"]
                         ?? Environment.GetEnvironmentVariable("JWT_SECRET")
                         ?? "dev_secret_change_in_production";

            var issuer = jwtSettings["Issuer"] ?? _configuration["Jwt:Issuer"] ?? "courseProjectIssuer";
            var audience = jwtSettings["Audience"] ?? _configuration["Jwt:Audience"] ?? "courseProjectAudience";
            var expStr = jwtSettings["ExpirationMinutes"] ?? _configuration["Jwt:ExpirationMinutes"] ?? "60";
            if (!int.TryParse(expStr, out var minutes)) minutes = 60;

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim("id", user.UserId.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role ?? "user")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        // Change user's password if current password matches. Returns true when changed.
        public bool ChangePassword(User user, string currentPassword, string newPassword)
        {
            if (user == null) return false;
            if (!VerifyPassword(currentPassword, user.PasswordHash)) return false;
            user.PasswordHash = HashPassword(newPassword);
            _context.SaveChanges();
            return true;
        }
    }

    public class UserRegistrationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
