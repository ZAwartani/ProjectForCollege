using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartFood.Application.Services
{
    public class AuthenticationRestaurantService
        : IAuthenticationServiceRestaurant
    {
        private readonly IGenericRepository<Restaurant, int>
            _RepoRestaurant;

        private readonly IGenericRepository<Tokens, int>
            _TokenRepo;

        private readonly IGenericRepository<CodeOTP, int>
            _CodeRepo;

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

        // ==========================================
        // REGISTER RESTAURANT
        // ==========================================

        public async Task RegisterNewRestaurantAsync(
            RestaurantRegisterDto dto)
        {
            var existingRestaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (existingRestaurant != null)
                throw new Exception("Email already exists.");

            var restaurant = new Restaurant(
                null,
                dto.Email,
                BCrypt.Net.BCrypt.HashPassword(dto.Password),
                dto.RestaurantName,
                dto.OwnerName,
                dto.BusinessRegistrationNumber,
                dto.Address,
                null,
                null
            );

            await _RepoRestaurant.AddAsync(restaurant);
        }

        // ==========================================
        // LOGIN: EMAIL + PASSWORD -> SEND OTP
        // ==========================================

        public async Task RestaurantLoginAsync(
            RestaurantLoginDto dto)
        {
            var restaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (restaurant == null ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    restaurant.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }

            string otp = GenerateOTP();

            var oldCodes = await _CodeRepo.ListAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.RestaurantId == restaurant.Id));

            if (oldCodes.Any())
                await _CodeRepo.RemoveRangeAsync(oldCodes);

            
            // Create new OTP
            var code = new CodeOTP(
                null,
                restaurant.Id,
                otp,
                DateTime.UtcNow.AddMinutes(5)
            );

            await _CodeRepo.AddAsync(code);

            try
            {
                await SendOtpEmailAsync(
                    restaurant.Email,
                    otp);
            }
            catch
            {
                await _CodeRepo.DeleteAsync(code.Id);
                throw;
            }
        }

        // ==========================================
        // VERIFY LOGIN OTP -> GENERATE TOKENS
        // ==========================================

        public async Task<RestaurantTokenResponceDto>
            RestaurantVerifyOTPAsync(
                RestaurantVerifyOTPDto dto)
        {
            var restaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var code = await _CodeRepo.GetAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.RestaurantId == restaurant.Id &&
                        x.Code == dto.OTP));

            if (code == null)
                throw new Exception("Invalid OTP.");

            if (code.CodeExpirDate <= DateTime.UtcNow)
            {
                await _CodeRepo.DeleteAsync(code.Id);
                throw new Exception("OTP expired.");
            }

            // OTP can only be used once
            await _CodeRepo.DeleteAsync(code.Id);

            return await GenerateTokensAsync(restaurant);
        }

        // ==========================================
        // FORGOT PASSWORD: EMAIL -> SEND OTP
        // ==========================================

        public async Task RestaurantForgotPasswordAsync(
            RestaurantSendOTPDto dto)
        {
            var restaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            string otp = GenerateOTP();

            // Delete old OTPs for this restaurant
            var oldCodes = await _CodeRepo.ListAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.RestaurantId == restaurant.Id));

            if (oldCodes.Any())
                await _CodeRepo.RemoveRangeAsync(oldCodes);

            // Delete all old OTP records globally
            var allOldCodes = await _CodeRepo.ListAsync(
                _Factory.CreateBuilder<CodeOTP>()
            );

            if (allOldCodes.Any())
                await _CodeRepo.RemoveRangeAsync(allOldCodes);

            // Create new OTP
            var code = new CodeOTP(
                null,
                restaurant.Id,
                otp,
                DateTime.UtcNow.AddMinutes(5)
            );

            await _CodeRepo.AddAsync(code);

            try
            {
                await SendOtpEmailAsync(
                    restaurant.Email,
                    otp);
            }
            catch
            {
                await _CodeRepo.DeleteAsync(code.Id);
                throw;
            }
        }

        // ==========================================
        // RESET PASSWORD: EMAIL + OTP + NEW PASSWORD
        // ==========================================

        public async Task<string?> RestaurantChangePasswordAsync(
            RestaurantChangePasswordDto dto)
        {
            var restaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var code = await _CodeRepo.GetAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.RestaurantId == restaurant.Id &&
                        x.Code == dto.OTP));

            if (code == null)
                throw new Exception("Invalid OTP.");

            if (code.CodeExpirDate <= DateTime.UtcNow)
            {
                await _CodeRepo.DeleteAsync(code.Id);
                throw new Exception("OTP expired.");
            }

            // Consume OTP
            await _CodeRepo.DeleteAsync(code.Id);

            // Hash new password
            restaurant.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _RepoRestaurant.UpdateAsync(restaurant);

            return "Password changed successfully.";
        }

        // ==========================================
        // REFRESH TOKEN
        // ==========================================

        public async Task<RestaurantTokenResponceDto>
            RestaurantRefreshTokenAsync(
                RestaurantRefreshTokenDto dto)
        {
            var restaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var storedToken = await _TokenRepo.GetAsync(
                _Factory.CreateBuilder<Tokens>()
                    .AddCriteria(x =>
                        x.RestaurantId == restaurant.Id &&
                        x.RefreshToken == dto.RefreshToken));

            if (storedToken == null)
                throw new Exception("Invalid refresh token.");

            if (storedToken.RefreshTokenRevokedAt != null ||
                storedToken.RefreshTokenExpir <= DateTime.UtcNow)
            {
                throw new Exception(
                    "Refresh token is invalid or expired.");
            }

            // Revoke old refresh token
            storedToken.RefreshTokenRevokedAt = DateTime.UtcNow;

            await _TokenRepo.UpdateAsync(storedToken);

            // Generate new token pair
            return await GenerateTokensAsync(restaurant);
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        public async Task RestaurantLogoutAsync(
            RestaurantLogoutDto dto)
        {
            var restaurant =
                await _RepoRestaurant.GetAsync(
                    _Factory.CreateBuilder<Restaurant>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var token = await _TokenRepo.GetAsync(
                _Factory.CreateBuilder<Tokens>()
                    .AddCriteria(x =>
                        x.RestaurantId == restaurant.Id &&
                        x.RefreshToken == dto.RefreshToken));

            if (token == null)
                throw new Exception("Invalid refresh token.");

            if (token.RefreshTokenRevokedAt != null)
                throw new Exception("Token already revoked.");

            token.RefreshTokenRevokedAt = DateTime.UtcNow;

            await _TokenRepo.UpdateAsync(token);
        }

        // ==========================================
        // GENERATE JWT + REFRESH TOKEN
        // ==========================================

        private async Task<RestaurantTokenResponceDto>
            GenerateTokensAsync(Restaurant restaurant)
        {
            string key = _Configuration["JWT:Key"]
                ?? throw new Exception("JWT key is missing.");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
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

            var jwt = new JwtSecurityToken(
                issuer: _Configuration["JWT:Issuer"],
                audience: _Configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            string accessToken =
                new JwtSecurityTokenHandler().WriteToken(jwt);

            string refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            var tokenEntity = new Tokens(
                null,
                restaurant.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(7),
                null
            );

            await _TokenRepo.AddAsync(tokenEntity);

            return new RestaurantTokenResponceDto(
                accessToken,
                refreshToken
            );
        }

        // ==========================================
        // GENERATE OTP
        // ==========================================

        private string GenerateOTP()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();
        }

        // ==========================================
        // SEND OTP EMAIL
        // ==========================================

        private async Task SendOtpEmailAsync(
            string email,
            string otp)
        {
            string senderEmail = "fastfoodunizu@gmail.com";

            // Keep your Gmail App Password here.
            string appPassword = "uleqvzpsjurmiuvx";

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "SmartFood",
                    senderEmail));

            message.To.Add(
                new MailboxAddress(
                    "Restaurant",
                    email));

            message.Subject = "SmartFood - Verification Code";

            message.Body = new TextPart("plain")
            {
                Text =
                    $"Your SmartFood verification code is: {otp}\n\n" +
                    "This code will expire in 5 minutes."
            };

            using var client = new SmtpClient();

            try
            {
                var host = _Configuration["Smtp:Host"]
                    ?? throw new InvalidOperationException("SMTP host is missing.");

                var port = _Configuration.GetValue<int>("Smtp:Port");

                var security = Enum.Parse<MailKit.Security.SecureSocketOptions>(
                    _Configuration["Smtp:Security"]
                        ?? throw new InvalidOperationException(
                            "SMTP security is missing."
                        )
                );

                using var connectionTimeout =
                    new CancellationTokenSource(TimeSpan.FromSeconds(30));

                await client.ConnectAsync(
                    host,
                    port,
                    security,
                    connectionTimeout.Token);

                await client.AuthenticateAsync(
                    senderEmail,
                    appPassword);

                await client.SendAsync(message);
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
        }
    }
}