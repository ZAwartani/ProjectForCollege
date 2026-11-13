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
    public class AuthenticationOrgnizationService
        : IAuthenticationServiceOrganization
    {
        private readonly IGenericRepository<Organization, int>
            _RepoOrganization;

        private readonly IGenericRepository<Tokens, int>
            _TokenRepo;

        private readonly IGenericRepository<CodeOTP, int>
            _CodeRepo;

        private readonly ISpecificationBuilderFactory _Factory;

        private readonly IConfiguration _Configuration;
        private readonly ICurrentUser _CurrentUser;

        public AuthenticationOrgnizationService(
            IGenericRepository<Organization, int> repoOrganization,
            IGenericRepository<Tokens, int> tokenRepo,
            IGenericRepository<CodeOTP, int> codeRepo,
            ISpecificationBuilderFactory factory,
            ICurrentUser currentUser,
            IConfiguration configuration)
        {
            _RepoOrganization = repoOrganization;
            _TokenRepo = tokenRepo;
            _CodeRepo = codeRepo;
            _Factory = factory;
            _Configuration = configuration;
            _CurrentUser = currentUser;
        }
        private readonly string Email;
        // ==========================================
        // REGISTER ORGANIZATION
        // ==========================================

        public async Task RegisterNewOrgnaizationAsync(
            OrganizationRegisterDto dto)
        {
            var existingOrganization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (existingOrganization != null)
                throw new Exception("Email already exists.");

            var organization = new Organization(
                null,
                dto.OrganizationName,
                dto.Email,
                BCrypt.Net.BCrypt.HashPassword(dto.Password),
                dto.OwnerName,
                dto.RegistrationNumber,
                dto.Address,
                null,
                null,
                null
            );

            await _RepoOrganization.AddAsync(organization);
        }

        // ==========================================
        // LOGIN: EMAIL + PASSWORD -> SEND OTP
        // ==========================================

        public async Task OrganizationLoginAsync(
            OrganizationLoginDto dto)
        {
            var organization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (organization == null ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    organization.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }

            string otp = GenerateOTP();

            // Delete old OTPs for this organization
            var oldCodes = await _CodeRepo.ListAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.OrganizationId == organization.Id));

            if (oldCodes.Any())
                await _CodeRepo.RemoveRangeAsync(oldCodes);

            // Create new OTP
            var code = new CodeOTP(
                organization.Id,
                null,
                otp,
                DateTime.UtcNow.AddMinutes(5)
            );

            await _CodeRepo.AddAsync(code);
            try
            {
                await SendOtpEmailAsync(
                    organization.Email,
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
            OrganizationVerifyOTPAsync(
                OrganizationVerifyCodeOtp dto)
        {
            var organization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (organization == null)
                throw new Exception("Organization not found.");

            var code = await _CodeRepo.GetAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.OrganizationId == organization.Id &&
                        x.Code == dto.CodeOTP));

            if (code == null)
                throw new Exception("Invalid OTP.");

            if (code.CodeExpirDate <= DateTime.UtcNow)
            {
                await _CodeRepo.DeleteAsync(code.Id);
                throw new Exception("OTP expired.");
            }

            // OTP can only be used once
            await _CodeRepo.DeleteAsync(code.Id);

            return await GenerateTokensAsync(organization);
        }

        // ==========================================
        // FORGOT PASSWORD: EMAIL -> SEND OTP
        // ==========================================

        public async Task OrganizationForgotPasswordAsync(
            OrganizationSendOTPDto dto)
        {
            var organization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (organization == null)
                throw new Exception("Organization not found.");

            string otp = GenerateOTP();

            // Delete old OTPs for this organization
            var oldCodes = await _CodeRepo.ListAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.OrganizationId == organization.Id));

            if (oldCodes.Any())
                await _CodeRepo.RemoveRangeAsync(oldCodes);


            // Create new OTP
            var code = new CodeOTP(
                organization.Id,
                null,
                otp,
                DateTime.UtcNow.AddMinutes(5)
            );

            await _CodeRepo.AddAsync(code);

            try
            {
                await SendOtpEmailAsync(
                    organization.Email,
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

        public async Task<string?> OrganizationResetPasswordAsync(
            OrganizationChangePasswordDto dto)
        {
            var organization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (organization == null)
                throw new Exception("Organization not found.");

            var code = await _CodeRepo.GetAsync(
                _Factory.CreateBuilder<CodeOTP>()
                    .AddCriteria(x =>
                        x.OrganizationId == organization.Id &&
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
            organization.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _RepoOrganization.UpdateAsync(organization);

            return "Password changed successfully.";
        }

        // ==========================================
        // REFRESH TOKEN
        // ==========================================

        public async Task<RestaurantTokenResponceDto>
            OrganizationRefreshTokenAsync(
                OrganizationRefreshTokenDto dto)
        {
            var organization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (organization == null)
                throw new Exception("Organization not found.");

            var storedToken = await _TokenRepo.GetAsync(
                _Factory.CreateBuilder<Tokens>()
                    .AddCriteria(x =>
                        x.OrganizationId == organization.Id &&
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
            return await GenerateTokensAsync(organization);
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        public async Task OragnizationLogoutAsync(
            OrganizationLogoutDto dto)
        {
            var organization =
                await _RepoOrganization.GetAsync(
                    _Factory.CreateBuilder<Organization>()
                        .AddCriteria(x => x.Email == dto.Email));

            if (organization == null)
                throw new Exception("Organization not found.");

            var token = await _TokenRepo.GetAsync(
                _Factory.CreateBuilder<Tokens>()
                    .AddCriteria(x =>
                        x.OrganizationId == organization.Id &&
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
            GenerateTokensAsync(Organization organization)
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
                    organization.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    organization.Email),

                new Claim(
                    ClaimTypes.Role,
                    "Organization")
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
                organization.Id,
                null,
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
                    "Organization",
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