# SmartFood Backend API (derived from source code)

Everything below was read from the backend source (Controllers, Services, DTOs, Program.cs). Nothing here was run against a live backend.

## Global facts

| Topic | Fact | Source |
|---|---|---|
| Base URLs | `http://localhost:5288` (profile `http`), `https://localhost:7270;http://localhost:5288` (profile `https`) | launchSettings.json |
| Route prefix | `api/[controller]` -> the controller name without "Controller" | all controllers |
| JSON naming | ASP.NET Core defaults: request/response properties are camelCase, request matching is case-insensitive | Program.cs (no custom JSON options) |
| Enums | Serialized as numbers: `RegisterStatus` 0 = Pending, 1 = Approved, 2 = Rejected | Program.cs (no string enum converter) |
| Auth scheme | JWT Bearer, header `Authorization: Bearer <accessToken>` | Program.cs |
| Access token | Lifetime 15 minutes, `ClockSkew = 0`. Claims: `ClaimTypes.NameIdentifier` (id), `ClaimTypes.Email`, `ClaimTypes.Role` (`Restaurant`, `Organization` or `Admin`) | *Service.GenerateTokensAsync |
| Claim names in the token | The token is built with `new JwtSecurityToken(claims: ...)`, so claim types are expected to be the long URIs (`.../claims/role`, `.../claims/nameidentifier`, `.../claims/emailaddress`). **Must be confirmed with a real token.** | *Service.GenerateTokensAsync |
| Refresh token | 64 random bytes, base64, lifetime 7 days. Rotated on every refresh (old one is revoked). Refresh requires **email + refreshToken** | *Service |
| CORS | Policy `SmartFood`: origins `http://localhost:5173` and `https://localhost:7259`, any header, any method, no credentials | Program.cs |
| HTTPS redirect | `UseHttpsRedirection()` is unconditional and runs **before** CORS. With the `http` profile there is no HTTPS port, so no redirect. With the `https` profile, calls to `http://localhost:5288` get redirected and browser preflight (OPTIONS) fails. | Program.cs |
| Error handling | There is **no** global exception handler or `ProblemDetails`. Services throw plain `Exception` for business errors (wrong password, wrong OTP...), which becomes **HTTP 500**, not 400/401. In Development the body is the developer exception text (plain text), not JSON. | services + Program.cs |
| Validation | DTOs are records without validation attributes. The backend does not validate email format, password length, etc. | DTOs |
| Static files | `UseStaticFiles()`: files saved under `wwwroot` are served at `<base>/<relative path>` | Program.cs |
| Authorization failures | No token or invalid token: 401. Wrong role or failed policy: 403 | ASP.NET Core |
| Approval policies | `RestaurantApproved` / `OrganizationApproved`: account Status must be Approved (1) **and** Latitude and Longitude must both be set | ApprovedHandler* |

Body binding: parameters of record type with `[ApiController]` are inferred as JSON bodies; `[FromForm]` means multipart/form-data.

## 1. AuthenticationRestaurantController — prefix `/api/AuthenticationRestaurant`
All endpoints: no authentication.

| Method | URL | Request (JSON) | Success response | Notes |
|---|---|---|---|---|
| POST | `/restaurant/register` | `RestaurantRegisterDto`: `email`, `password`, `restaurantName`, `ownerName`, `businessRegistrationNumber`, `address` | 200 text: "Restaurant registered successfully." | Duplicate email throws -> 500. New account Status = Pending |
| POST | `/restaurant/login` | `RestaurantLoginDto`: `email`, `password` | 200 text: "Verification code sent to your email." | Sends a 6-digit OTP (valid 5 min) by SMTP. Bad credentials throw "Invalid email or password." -> 500. Pending accounts can still log in |
| POST | `/restaurant/verify-otp` | `RestaurantVerifyOTPDto`: `email`, `otp` | 200 JSON `{ accessToken, refreshToken }` | "Invalid OTP." / "OTP expired." / "Restaurant not found." -> 500 |
| POST | `/restaurant/refresh-token` | `RestaurantRefreshTokenDto`: `email`, `refreshToken` | 200 JSON `{ accessToken, refreshToken }` | Old refresh token is revoked |
| POST | `/restaurant/send-otp` | `RestaurantSendOTPDto`: `email` | 200 text: "Verification code sent to your email." | Forgot password step 1 |
| POST | `/restaurant/reset-password` | `RestaurantChangePasswordDto`: `email`, `password`, `otp` | 200 text: "Password changed successfully." | `password` is the NEW password |
| POST | `/restaurant/logout` | `RestaurantLogoutDto`: `email`, `refreshToken` | 200 text: "Logged out successfully." | No Authorization header needed |

## 2. AuthenticationOrganizationController — prefix `/api/AuthenticationOrganization`
All endpoints: no authentication. Property names differ from the restaurant ones.

| Method | URL | Request (JSON) | Success response | Notes |
|---|---|---|---|---|
| POST | `/register` | `OrganizationRegisterDto`: `email`, `password`, `organizationName`, `ownerName`, `registrationNumber`, `address` | 200 JSON `{ message }` | Status = Pending |
| POST | `/login` | `OrganizationLoginDto`: `email`, `password` | 200 JSON `{ message }` | Sends OTP |
| POST | `/verify-otp` | `OrganizationVerifyCodeOtp`: `email`, **`codeOTP`** | 200 JSON `{ accessToken, refreshToken }` | OTP field is `codeOTP`, not `otp` |
| POST | `/forgot-password` | `OrganizationSendOTPDto`: `email` | 200 JSON `{ message }` | |
| POST | `/reset-password` | `OrganizationChangePasswordDto`: `email`, `password`, `otp` | 200 JSON `{ message }` | `password` is the NEW password |
| POST | `/refresh-token` | `OrganizationRefreshTokenDto`: `email`, `refreshToken` | 200 JSON `{ accessToken, refreshToken }` | |
| POST | `/logout` | `OrganizationLogoutDto`: `email`, `refreshToken` | 200 JSON `{ message }` | |

## 3. AdminController — prefix `/api/Admin`

| Method | URL | Auth | Request | Success response |
|---|---|---|---|---|
| POST | `/register` | **none (open to anyone — security issue)** | JSON `RegisterAdminDto`: `name`, `email`, `password` | 200 text; 409 text "Admin email already exists." |
| POST | `/login` | none | JSON `LoginAdminDto`: `email`, `password` | 200 `{ accessToken, refreshToken }` (no OTP). Refresh token is stored hashed (SHA-256) |
| POST | `/refresh-token` | none | JSON `AdminRefreshTokenDto`: `email`, `refreshToken` | 200 `{ accessToken, refreshToken }` |
| POST | `/logout` | role Admin | JSON `AdminRefreshTokenDto` | 200 `{ message }` |
| GET | `/organizations/pending` | role Admin | - | 200 `ReturnOrganizationDto[]`: `id`, `organizationName`, `ownerName`, `registerNumber`, `address`, `status`, `verificationDocument` (`{ id, filePath }` or null) |
| GET | `/organizations` | role Admin | - | 200 `ListOrganizationDto[]`: `id`, `organizationName` |
| PUT | `/organizations/approve` | role Admin | JSON `{ organizationId }` | 200 text; 404 text "Organization not found or is not pending." |
| DELETE | `/organizations/reject` | role Admin | JSON body `{ organizationId }` | 200 text; 404 text. Only works if Pending **and** created at least 1 day ago. Deletes the record |
| GET | `/restaurants/pending` | role Admin | - | 200 `ReturnRestaurantDto[]`: `id`, `restaurantName`, `ownerName`, `registerNumber`, `address`, `status`, `verificationDocument` |
| GET | `/restaurants` | role Admin | - | 200 `ListRestaurantDto[]`: `id`, `restaurantName` |
| PUT | `/restaurants/approve` | role Admin | JSON `{ restaurantId }` | 200 text; 404 text |
| DELETE | `/restaurants/reject` | role Admin | JSON body `{ restaurantId }` | 200 text; 404 text. Same 1-day rule |

## 4. RestaurantController — prefix `/api/Restaurant` (class-level role `Restaurant`)

| Method | URL | Policy | Request | Success response |
|---|---|---|---|---|
| POST | `/CreateProduct` | RestaurantApproved | **multipart/form-data** `CreateProducrDto`: `Name`, `Description`, `Quantity` (int), `Photo` (file, max 2 MB) | 200 text; 404 "Restaurant not found."; 400 text (ArgumentException) |
| GET | `/GetProduct` | RestaurantApproved | - | 200 `GetProductDto[]`: `id`, `restaurantId`, `name`, `description`, `quantity`, `pathPhoto`. Only products created in the last 24 h |
| PUT | `/UpdateProduct/{id:int}` | RestaurantApproved | **multipart** `UpdateProductDto`: `Name?`, `Description?`, `Quantity?`, `Photo?` | 200 text; 404 text; 400 text. Only products < 24 h old |
| DELETE | `/DeleteProduct/{id:int}` | RestaurantApproved | - | 200 text; 404 text. Only products < 24 h old |
| GET | `/MyProfile` | role only | - | 200 `RestaurantProfileDto`: `id`, `name`, `ownerName`, `email`, `address`, `latitude?`, `longitude?`, `status`; 404 |
| PUT | `/UpdateMyProfile` | role only | JSON `UpdateRestaurantProfileDto`: `name?`, `ownerName?`, `address?` | 200 text; 404; 400 if `name` differs from the current name |
| PUT | `/UpdateMyLocation` | role only | JSON `{ latitude, longitude }` | 200 text; 400 "Invalid location or restaurant not found." |

## 5. OrganizationController — prefix `/api/Organization` (class-level role `Organization`)

| Method | URL | Policy | Request | Success response |
|---|---|---|---|---|
| GET | `/GetMyProfile` | role only | - | 200 `OrganizationProfileDto`: `id`, `organizationName`, `ownerName`, `email`, `registrationNumber`, `address`, `latitude?`, `longitude?`, `status`, `phoneNumber?`; 404 |
| PUT | `/UpdateProfile` | role only | JSON `{ ownerName?, address?, phoneNumber? }` | 200 text; 404; 400 (empty owner/address, phone must be 7-15 digits with optional leading +) |
| PUT | `/UpdateLocation` | role only | JSON `{ latitude, longitude }` | 200 text "Profile updated successfully."; 404; 400 (out of range) |
| GET | `/GetProduct` | OrganizationApproved | query `search?`, `pageNumber` (default 1), `pageSize` (default 10, 1-100) | 200 `OrganizationProductDto[]`: `id`, `restaurantId`, `restaurantName`, `name`, `description`, `quantity`, `photoPath`, `createDate`. No total count |
| GET | `/GetProductAsync/{id:int}` | OrganizationApproved | - | 200 `OrganizationProductDto`; 404 text |
| GET | `/GetRestaurant` | OrganizationApproved | - | 200 `OrganizationRestaurantDto[]`: `id`, `restaurantName`, `address`, `latitude`, `longitude` |
| GET | `/GetRestaurantDetail/{id:int}` | OrganizationApproved | - | 200 `{ id, restaurantName, address, latitude, longitude, products: OrganizationProductDto[] }`; 404 |

## 6. FileUploadController — prefix `/api/FileUpload`

| Method | URL | Role | Request | Success response |
|---|---|---|---|---|
| POST | `/organization/upload` | Organization | **multipart** `UploadFile`: `File` | 200 text; 400 "Document upload failed." |
| POST | `/restaurant/upload` | Restaurant | **multipart** `UploadFile`: `File` | 200 text; 400 |
| DELETE | `/{fileId}` | Restaurant or Organization | - | 204; 404 "Document not found." |
| GET | `/organization/files` | Organization | - | 200 `FileClient[]`: `id`, `addressNameFile` |
| GET | `/restaurant/files` | Restaurant | - | 200 `FileClient[]`: `id`, `addressNameFile` |

## Known issues found in the source (for the backend team)

1. Gmail app password is hardcoded in both `SendOtpEmailAsync` methods. Treat it as leaked: revoke and move to secrets.
2. `JWT:Key` is in `appsettings.json`.
3. `Smtp:Host`, `Smtp:Port`, `Smtp:Security` are read from configuration but are not in `appsettings.json` / `appsettings.Development.json`. Without them, login (OTP email) fails.
4. `POST /api/Admin/register` has no `[Authorize]`.
5. Business errors are plain `Exception` -> HTTP 500 with a non-JSON body. Recommended: a global exception handler returning 400/401/404/409 with JSON `{ message }`.
6. `OrganizationVerifyOTPAsync` returns `RestaurantTokenResponceDto` (same shape, only the type name is confusing).
7. Product and restaurant file paths are built with `Path.Combine`, so on Windows `filePath` / `photoPath` contain backslashes. The frontend must convert `\` to `/` when building URLs.
8. A pending account can log in; only the approval policies block the product endpoints.
9. `UpdateLocation` (organization) returns the message "Profile updated successfully."
10. `RegisterStatus.Rejected` exists but is never assigned: reject deletes the account.
11. No endpoint exists for an organization to request/claim food.

## Frontend usage: Admin (verified against AdminController and tested against a mock)

- `POST /api/Admin/login` {email,password} -> tokens (no OTP); `POST /api/Admin/refresh-token` {email,refreshToken}
- `POST /api/Admin/logout` [Admin] {email,refreshToken}
- `GET /api/Admin/restaurants/pending`, `GET /api/Admin/organizations/pending`
- `GET /api/Admin/restaurants`, `GET /api/Admin/organizations` (id + name)
- `PUT .../approve` {restaurantId} | {organizationId}
- `DELETE .../reject` with a JSON body, only for Pending requests older than 1 day (else 404 text)
