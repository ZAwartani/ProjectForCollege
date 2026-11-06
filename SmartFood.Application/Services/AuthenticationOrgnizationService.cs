
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
        private readonly IGenericRepository<Organization, int> _RepoOrganization;
        private readonly IGenericRepository<Tokens, int> _TokenRepo;
        private readonly IGenericRepository<CodeOTP, int> _CodeRepo;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly IConfiguration _Configuration;

        public AuthenticationOrgnizationService(
            IGenericRepository<Organization, int> repoOrganization,
            IGenericRepository<Tokens, int> tokenRepo,
            IGenericRepository<CodeOTP, int> codeRepo,
            ISpecificationBuilderFactory factory,
            IConfiguration configuration)
        {
            _RepoOrganization = repoOrganization;
            _TokenRepo = tokenRepo;
            _CodeRepo = codeRepo;
            _Factory = factory;
            _Configuration = configuration;
        }

        // REGISTER
        public async Task RegisterNewOrgnaizationAsync(
            OrganizationRegisterDto dto)
        {
            var existingOrganization = await _RepoOrganization.GetAsync(
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
                null
                );

            await _RepoOrganization.AddAsync(organization);
        }

        // LOGIN: EMAIL + PASSWORD -> SEND OTP
        public async Task OrganizationLoginAsync(
            OrganizationLoginDto dto)
        {
            var organization = await _RepoOrganization.GetAsync(
                _Factory.CreateBuilder<Organization>()
                    .AddCriteria(x => x.Email == dto.Email));

            if (organization == null ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    organization.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }

            // Generate OTP
            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000).ToString();

            // Save OTP
            var code = new CodeOTP(
                organization.Id,
                null,
                otp,
                DateTime.UtcNow.AddMinutes(5));

            await _CodeRepo.AddAsync(code);

            // Send OTP
            await SendOtpEmailAsync(organization.Email, otp);
        }

        // VERIFY OTP -> GENERATE JWT + REFRESH TOKEN
        public async Task<RestaurantTokenResponceDto>
            OrganizationVerifyOTPAsync(
                OrganizationVerifyCodeOtp dto)
        {
            // Find organization
            var organization = await _RepoOrganization.GetAsync(
                _Factory.CreateBuilder<Organization>()
                    .AddCriteria(x => x.Email == dto.Email));

            if (organization == null)
                throw new Exception("Organization not found.");

            // Find OTP
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

            // OTP is single-use
            await _CodeRepo.DeleteAsync(code.Id);

            return await GenerateTokensAsync(organization);
        }

        // REFRESH TOKEN
        public async Task<RestaurantTokenResponceDto>
            OrganizationRefreshTokenAsync(
                OrganizationRefreshTokenDto dto)
        {
            var organization = await _RepoOrganization.GetAsync(
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
                throw new Exception("Refresh token is invalid or expired.");
            }

            // Revoke old token
            storedToken.RefreshTokenRevokedAt = DateTime.UtcNow;

            await _TokenRepo.UpdateAsync(storedToken);

            return await GenerateTokensAsync(organization);
        }

        // LOGOUT
        public async Task OragnizationLogoutAsync(
            OrganizationLogoutDto dto)
        {
            var organization = await _RepoOrganization.GetAsync(
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

        // CHANGE PASSWORD USING OTP
        public async Task<string?>
            OrganizationChangePasswordAsync(
                OrganizationChangePasswordDto dto)
        {
            var organization = await _RepoOrganization.GetAsync(
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

            await _CodeRepo.DeleteAsync(code.Id);

            organization.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _RepoOrganization.UpdateAsync(organization);

            return "Password changed successfully.";
        }

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
                signingCredentials: credentials);

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
                refreshToken);
        }

        private async Task SendOtpEmailAsync(
            string email,
            string otp)
        {
            var orgnaization = await _RepoOrganization.GetAsync(
            _Factory
            .CreateBuilder<Organization>()
            .AddCriteria(r => r.Email == email));

            if (orgnaization == null)
                throw new Exception("Restaurant not found");

            string codeOTPToEmail =
                RandomNumberGenerator.GetInt32(10000, 99999).ToString();

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "SmartFood",
                    "fastfoodunizu@gmail.com"));

            message.To.Add(new MailboxAddress(
                "Restaurant",
                    email));

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
                        .AddCriteria(c => c.RestaurantId == orgnaization.Id)));

            var newOtp = new CodeOTP(
                null,
                orgnaization.Id,
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
                    "fastfoodunizu@gmail.com",
                    "uleqvzpsjurmiuvx");

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
    }
}
