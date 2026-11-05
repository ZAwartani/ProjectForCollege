using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using System.IdentityModel.Tokens.Jwt;
using MailKit.Net.Smtp;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net.WebSockets;

namespace SmartFood.Application.Services
{
    public class AuthenticationRestaurantService : IAuthenticationServiceRestaurant
    {
        private readonly IGenericRepository<Restaurant, int> _RepoRestaurant;
        private readonly IGenericRepository<Tokens, int> _TokenRepo;
        private readonly IGenericRepository<CodeOTP, int> _CodeRepo;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly IConfiguration _Configuration;

        public AuthenticationRestaurantService(
            IGenericRepository<Restaurant, int> repoRestaurant,
            IGenericRepository<Tokens, int> tokenRepo,
            IGenericRepository<CodeOTP, int> codeRepo,
            ISpecificationBuilderFactory factory,
            IConfiguration configuration)
        {
            _RepoRestaurant = repoRestaurant;
            _TokenRepo = tokenRepo;
            _CodeRepo = codeRepo;
            _Factory = factory;
            _Configuration = configuration;
        }

        public async Task RegisterNewRestaurantAsync(RestaurantRegisterDto dto)
        {
            var existingRestaurant = await _RepoRestaurant.GetAsync(
                _Factory.CreateBuilder<Restaurant>()
                    .AddCriteria(r => r.Email == dto.Email));

            if (existingRestaurant != null)
                throw new Exception("Email already exists");

            var restaurant = new Restaurant(
                dto.VerificationDocumentId,
                dto.Email,
                BCrypt.Net.BCrypt.HashPassword(dto.Password),
                dto.RestaurantName,
                dto.OwnerName,
                dto.BusinessRegistrationNumber,
                dto.Address);

            await _RepoRestaurant.AddAsync(restaurant);
        }

        public async Task<string?> RestaurantChangePasswordAsync(
            RestaurantChangePasswordDto dto)
        {
            var restaurant = await _RepoRestaurant.GetAsync(
                _Factory.CreateBuilder<Restaurant>()
                    .AddCriteria(r => r.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found");

            var otp = await _CodeRepo.GetAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(c => c.IdRestaurant == restaurant.Id)
                    .AddCriteria(c => c.Code == dto.OTP));

            if (otp == null)
                throw new Exception("Invalid verification code");

            if (otp.CodeExpirDate < DateTime.UtcNow)
                throw new Exception("Verification code has expired");

            restaurant.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _RepoRestaurant.UpdateAsync(restaurant);
            await _CodeRepo.DeleteAsync(otp.Id);

            return "Password Changed Successfully";
        }

        public async Task RestaurantLoginAsync(RestaurantLoginDto dto)
        {
            var restaurant = await _RepoRestaurant.GetAsync(
                _Factory.CreateBuilder<Restaurant>()
                    .AddCriteria(r => r.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Invalid email or password");

            var passwordValid = BCrypt.Net.BCrypt.Verify(
                dto.Password,
                restaurant.PasswordHash);

            if (!passwordValid)
                throw new Exception("Invalid email or password");

            await RestaurantSendOTPAsync(
                new RestaurantSendOTPDto(dto.Email));
        }

        public async Task<RestaurantTokenResponceDto> RestaurantVerifyOTPAsync(
    RestaurantVerifyOTPDto dto)
        {
            var restaurant = await _RepoRestaurant.GetAsync(
                _Factory.CreateBuilder<Restaurant>()
                    .AddCriteria(r => r.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found");

            var otp = await _CodeRepo.GetAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(c => c.IdRestaurant == restaurant.Id)
                    .AddCriteria(c => c.Code == dto.OTP));

            if (otp == null)
                throw new Exception("Invalid verification code");

            if (otp.CodeExpirDate < DateTime.UtcNow)
                throw new Exception("Verification code has expired");

            await _CodeRepo.DeleteAsync(otp.Id);

            return await GenerateAndSaveTokensAsync(restaurant);
        }

        public async Task RestaurantLogoutAsync(RestaurantLogoutDto dto)
        {
            var restaurant = await _RepoRestaurant.GetAsync(
                _Factory.CreateBuilder<Restaurant>()
                    .AddCriteria(r => r.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found");

            var token = await _TokenRepo.GetAsync(
                _Factory.CreateBuilder<Tokens>()
                    .AddCriteria(t => t.IdRestaurant == restaurant.Id)
                    .AddCriteria(t => t.RefreshToken == dto.RefreshToken)
                    .AddCriteria(t => !t.RefreshTokenRevokedAt.HasValue));

            if (token == null)
                throw new Exception("Invalid refresh token");

            token.RefreshTokenRevokedAt = DateTime.UtcNow;

            await _TokenRepo.UpdateAsync(token);
        }

        public async Task<RestaurantTokenResponceDto> RestaurantRefreshTokenAsync(
            RestaurantRefreshTokenDto dto)
        {
            var token = await _TokenRepo.GetAsync(
                _Factory.CreateBuilder<Tokens>()
                    .AddCriteria(t => t.Restaurant.Email == dto.Email)
                    .AddCriteria(t => t.RefreshToken == dto.RefreshToken)
                    .AddInclude(t => t.Restaurant));

            if (token == null)
                throw new Exception("Invalid refresh token");

            if (token.RefreshTokenRevokedAt != null)
                throw new Exception("Refresh token has been revoked");

            if (token.RefreshTokenExpir < DateTime.UtcNow)
                throw new Exception("Refresh token has expired");

            var restaurant = token.Restaurant;

            token.RefreshTokenRevokedAt = DateTime.UtcNow;
            await _TokenRepo.UpdateAsync(token);

            return await GenerateAndSaveTokensAsync(restaurant);
        }

        public async Task RestaurantSendOTPAsync(RestaurantSendOTPDto SendOTPDto)
        {
            var restaurant = await _RepoRestaurant.GetAsync(
      _Factory
          .CreateBuilder<Restaurant>()
          .AddCriteria(r => r.Email == SendOTPDto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found");

            string codeOTPToEmail =
                RandomNumberGenerator.GetInt32(10000, 99999).ToString();

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "SmartFood",
                    "zakaryawork005@gmail.com"));

            message.To.Add(new MailboxAddress(
                "Restaurant",
                SendOTPDto.Email));

            message.Subject =
                "SmartFood Website Send To You Code";

            message.Body = new TextPart("plain")
            {
                Text =
                    $"This is your verification code: {codeOTPToEmail}. It will expire in 5 minutes."
            };

            await _CodeRepo.RemoveRangeAsync(
                await _CodeRepo.ListAsync(
                    _Factory
                        .CreateBuilder<CodeOTP>()
                        .AddCriteria(c => c.IdRestaurant == restaurant.Id)));

            var newOtp = new CodeOTP(
                null,
                restaurant.Id,
                codeOTPToEmail,
                DateTime.UtcNow.AddMinutes(5));

            await _CodeRepo.AddAsync(newOtp);

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(
                    "smtp.gmail.com",
                    587,
                    MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    "zakaryawork005@gmail.com",
                    "YOUR_GMAIL_APP_PASSWORD");

                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error sending email: {ex.Message}");

                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        private async Task<RestaurantTokenResponceDto>
            GenerateAndSaveTokensAsync(Restaurant restaurant)
        {
            var utcNow = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    restaurant.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    restaurant.Email),

                new Claim(
                    ClaimTypes.Role,
                    "Restaurant")
            };

            var jwtKey = _Configuration["JWT:Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is not configured");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _Configuration["JWT:Issuer"],
                audience: _Configuration["JWT:Audience"],
                claims: claims,
                expires: utcNow.AddMinutes(15),
                signingCredentials: credentials);

            var accessToken =
                new JwtSecurityTokenHandler().WriteToken(jwt);

            var refreshToken = GenerateRefreshToken();

            var token = new Tokens(
                null,
                restaurant.Id,
                refreshToken,
                utcNow.AddDays(7),
                null);

            await _TokenRepo.AddAsync(token);

            return new RestaurantTokenResponceDto(
                accessToken,
                refreshToken);
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
