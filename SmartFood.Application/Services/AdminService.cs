
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartFood.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IFileStorageService _FileUpload;
        private readonly IGenericRepository<Organization, int> _RepoOrganization;
        private readonly IGenericRepository<Restaurant, int> _RepoRestaurant;
        private readonly IGenericRepository<Admin, int> _RepoAdmin;
        private readonly IGenericRepository<Tokens, int> _TokenRepo;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly IConfiguration _Configuration;

        public AdminService(
            IGenericRepository<Organization, int> repoOrganization,
            IGenericRepository<Restaurant, int> repoRestaurant,
            IGenericRepository<Admin, int> repoAdmin,
            IGenericRepository<Tokens, int> tokenRepo,
            ISpecificationBuilderFactory factory,
            IFileStorageService fileStorageService,
            IConfiguration configuration)
        {
            _RepoOrganization = repoOrganization;
            _RepoRestaurant = repoRestaurant;
            _RepoAdmin = repoAdmin;
            _TokenRepo = tokenRepo;
            _Factory = factory;
            _Configuration = configuration;
            _FileUpload = fileStorageService;
        }

        // ==========================================
        // ADMIN LOGIN
        // ==========================================

        public async Task<bool> RegisterAdminAsync(RegisterAdminDto dto)
        {
            var existingAdmin = await _RepoAdmin.GetAsync(
                _Factory.CreateBuilder<Admin>()
                .AddCriteria(x => x.Email == dto.Email)
            );

            if (existingAdmin != null)
                return false;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var admin = new Admin(
                dto.Name,
                dto.Email,
                passwordHash
            );

            await _RepoAdmin.AddAsync(admin);

            return true;
        }
        public async Task<AdminResponceTokenDto> LoginAdmin(
            LoginAdminDto loginData)
        {
            var specification = _Factory.CreateBuilder<Admin>()
                .AddCriteria(x => x.Email == loginData.Email);

            var admin = await _RepoAdmin.GetAsync(specification);

            if (admin == null ||
                !BCrypt.Net.BCrypt.Verify(
                    loginData.Password,
                    admin.Password))
            {
                throw new Exception("Invalid Email Or Password");
            }

            return await GenerateTokensAsync(admin);
        }

        // ==========================================
        // GET PENDING ORGANIZATIONS
        // ==========================================

        public async Task<IReadOnlyList<ReturnOrganizationDto>>
    OrganizationDetail()
        {
            var specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Status == RegisterStatus.Pending)
                .AddInclude(x => x.VerificationDocument);

            var organizations =
                await _RepoOrganization.ListAsync(specification);

            return organizations.Select(x =>
                new ReturnOrganizationDto(
                    x.Id,
                    x.OrganizationName,
                    x.OwnerName,
                    x.RegistrationNumber,
                    x.Address,
                    x.Status,
                    x.VerificationDocument == null
                        ? null
                        : new VerificationDocumentDto(
                            x.VerificationDocument.Id,
                            x.VerificationDocument.FilePath
                        )
                )).ToList();
        }

        // ==========================================
        // GET PENDING RESTAURANTS
        // ==========================================

        public async Task<IReadOnlyList<ReturnRestaurantDto>>
            RestaurantDetail()
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Status == RegisterStatus.Pending)
                .AddInclude(x => x.VerificationDocument);

            var restaurants =
                await _RepoRestaurant.ListAsync(specification);

            return restaurants.Select(x =>
                new ReturnRestaurantDto(
                    x.Id,
                    x.RestaurantName,
                    x.OwnerName,
                    x.BusinessRegistrationNumber,
                    x.Address,
                    x.Status,
                    x.VerificationDocument == null ? null : new VerificationDocumentDto(
                        x.VerificationDocument.Id,
                        x.VerificationDocument.FilePath
                    )
                )).ToList();
        }

        // ==========================================
        // APPROVE ORGANIZATION
        // ==========================================

        public async Task<bool> OrganizationUpdateData(
            UpdateOrganizationNotApprovedDto update)
        {
            var specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x =>
                    x.Id == update.OrganizationId &&
                    x.Status == RegisterStatus.Pending);

            var organization =
                await _RepoOrganization.GetAsync(specification);

            if (organization == null)
                return false;

            organization.Status = RegisterStatus.Approved;

            await _RepoOrganization.UpdateAsync(organization);

            return true;
        }

        // ==========================================
        // APPROVE RESTAURANT
        // ==========================================

        public async Task<bool> RestaurantUpdateData(
            UpdateRestaurantNotApprovedDto update)
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x =>
                    x.Id == update.RestaurantId &&
                    x.Status == RegisterStatus.Pending);

            var restaurant =
                await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return false;

            restaurant.Status = RegisterStatus.Approved;

            await _RepoRestaurant.UpdateAsync(restaurant);

            return true;
        }

        // ==========================================
        // REMOVE PENDING ORGANIZATION
        // ==========================================

        public async Task<bool> RemoveOrganization(
            RemoveOrganizationDto removeData)
        {
            var specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x =>
                    x.Id == removeData.OrganizationId &&
                    x.Status == RegisterStatus.Pending &&
                    x.CreatedAt <= DateTime.UtcNow.AddDays(-1)
                );

            var organization =
                await _RepoOrganization.GetAsync(specification);

            if (organization == null)
                return false;

            if (organization.VerificationId != null)
            {
                await _FileUpload.DeleteFolderOrg(organization.OrganizationName);
            }
            
            await _RepoOrganization.DeleteAsync(organization.Id);
            return true;
        }

        // ==========================================
        // REMOVE PENDING RESTAURANT
        // ==========================================

        public async Task<bool> RemoveRestaurant(
            RemoveRestaurantDto removeData)
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x =>
                    x.Id == removeData.RestaurantId &&
                    x.Status == RegisterStatus.Pending &&
                    x.CreatedAt <= DateTime.UtcNow.AddDays(-1)
                );

            var restaurant =
                await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return false;

            if(restaurant.VerificationId != null)
            {
                await _FileUpload.DeleteFolderRes(restaurant.RestaurantName);
            }
            await _RepoRestaurant.DeleteAsync(restaurant.Id);

            return true;
        }

        // ==========================================
        // LIST ALL ORGANIZATIONS
        // ==========================================

        public async Task<IReadOnlyList<ListOrganizationDto>>
            ListOrganizationAsync()
        {
            var specification =
                _Factory.CreateBuilder<Organization>();

            var organizations =
                await _RepoOrganization.ListAsync(specification);

            return organizations.Select(x =>
                new ListOrganizationDto(
                    x.Id,
                    x.OrganizationName
                )).ToList();
        }

        // ==========================================
        // LIST ALL RESTAURANTS
        // ==========================================

        public async Task<IReadOnlyList<ListRestaurantDto>>
            ListRestaurantAsync()
        {
            var specification =
                _Factory.CreateBuilder<Restaurant>();

            var restaurants =
                await _RepoRestaurant.ListAsync(specification);

            return restaurants.Select(x =>
                new ListRestaurantDto(
                    x.Id,
                    x.RestaurantName
                )).ToList();
        }

        // ==========================================
        // GENERATE JWT + REFRESH TOKEN
        // ==========================================

        private async Task<AdminResponceTokenDto> GenerateTokensAsync(
            Admin admin)
        {
            string key = _Configuration["JWT:Key"]
                ?? throw new Exception("JWT key is missing.");

            string issuer = _Configuration["JWT:Issuer"]
                ?? throw new Exception("JWT issuer is missing.");

            string audience = _Configuration["JWT:Audience"]
                ?? throw new Exception("JWT audience is missing.");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    admin.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    admin.Email),

                new Claim(
                    ClaimTypes.Role,
                    "Admin")
            };

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            string accessToken =
                new JwtSecurityTokenHandler().WriteToken(jwt);

            // Generate refresh token
            string refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            // Hash refresh token before storing
            string hashedRefreshToken =
                HashRefreshToken(refreshToken);

            // Store refresh token in Tokens table
            var tokenEntity = new Tokens(
                null,
                null,
                hashedRefreshToken,
                DateTime.UtcNow.AddDays(7),
                null,
                admin.Id
            );

            await _TokenRepo.AddAsync(tokenEntity);

            return new AdminResponceTokenDto(
                accessToken,
                refreshToken
            );
        }

        // ==========================================
        // HASH REFRESH TOKEN
        // ==========================================

        private static string HashRefreshToken(string token)
        {
            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(bytes);
        }

        // ==========================================
        // REFRESH ADMIN TOKEN
        // ==========================================

        public async Task<AdminResponceTokenDto> RefreshTokenAsync(
            AdminRefreshTokenDto dto)
        {
            var adminSpecification = _Factory.CreateBuilder<Admin>()
                .AddCriteria(x => x.Email == dto.Email);

            var admin =
                await _RepoAdmin.GetAsync(adminSpecification);

            if (admin == null)
                throw new Exception("Admin not found.");

            string hashedRefreshToken =
                HashRefreshToken(dto.RefreshToken);

            var tokenSpecification = _Factory.CreateBuilder<Tokens>()
                .AddCriteria(x =>
                    x.AdminId == admin.Id &&
                    x.RefreshToken == hashedRefreshToken);

            var storedToken =
                await _TokenRepo.GetAsync(tokenSpecification);

            if (storedToken == null)
                throw new Exception("Invalid refresh token.");

            if (storedToken.RefreshTokenRevokedAt != null)
                throw new Exception("Refresh token has been revoked.");

            if (storedToken.RefreshTokenExpir <= DateTime.UtcNow)
                throw new Exception("Refresh token has expired.");

            // Revoke old refresh token
            storedToken.RefreshTokenRevokedAt = DateTime.UtcNow;

            await _TokenRepo.UpdateAsync(storedToken);

            // Generate new access and refresh tokens
            return await GenerateTokensAsync(admin);
        }

        // ==========================================
        // ADMIN LOGOUT
        // ==========================================

        public async Task LogoutAsync(AdminRefreshTokenDto dto)
        {
            var adminSpecification = _Factory.CreateBuilder<Admin>()
                .AddCriteria(x => x.Email == dto.Email);

            var admin =
                await _RepoAdmin.GetAsync(adminSpecification);

            if (admin == null)
                throw new Exception("Admin not found.");

            string hashedRefreshToken =
                HashRefreshToken(dto.RefreshToken);

            var tokenSpecification = _Factory.CreateBuilder<Tokens>()
                .AddCriteria(x =>
                    x.AdminId == admin.Id &&
                    x.RefreshToken == hashedRefreshToken);

            var storedToken =
                await _TokenRepo.GetAsync(tokenSpecification);

            if (storedToken == null)
                throw new Exception("Invalid refresh token.");

            if (storedToken.RefreshTokenRevokedAt == null)
            {
                storedToken.RefreshTokenRevokedAt = DateTime.UtcNow;

                await _TokenRepo.UpdateAsync(storedToken);
            }
        }
    }
}
