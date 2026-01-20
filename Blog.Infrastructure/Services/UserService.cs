using Blog.Application.Errors;
using Blog.Application.Interfaces;
using Blog.Application.Users.Dtos;
using Blog.Application.Users.Mapping;
using Blog.Application.Users.Requests;
using Blog.Application.Users.Results;
using Blog.Domain.Entities.User;
using Blog.Domain.Enums;
using Blog.Domain.Validation.User;
using Blog.Infrastructure.Options;
using Blog.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Blog.Infrastructure.Services
{
    internal class UserService : IUserService
    {
        private readonly BlogDbContext _context;
        private readonly AuthOptions _authOptions;
        private readonly PasswordHasher<object> _passwordHasher;

        public UserService(BlogDbContext context, IOptions<AuthOptions> authOptions)
        {
            _context = context;
            _authOptions = authOptions.Value;
            _passwordHasher = new PasswordHasher<object>();
        }

        #region Sign Up
        public async Task<Result<CreateEmailAddressConfirmationTokenResult>> SignUpAsync(SignUpRequest signUpRequest)
        {
            var validationErrors = await ValidateSignUpRequestAsync(signUpRequest);

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var normalizedUsername = signUpRequest.Username.ToLowerInvariant();

            var passwordHash = _passwordHasher.HashPassword(null, signUpRequest.Password);

            var newUser = new User()
            {
                Username = signUpRequest.Username,
                NormalizedUsername = normalizedUsername,
                PasswordHash = passwordHash
            };

            var token = GenerateToken();
            var tokenHash = HashToken(token);

            var emailAddressConfirmationToken = new EmailAddressConfirmationToken
            {
                EmailAddress = signUpRequest.EmailAddress,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.EmailAddressConfirmationTokenTTLDays),
                UserId = newUser.Id
            };

            using (var signUpUserTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Users.AddAsync(newUser);

                    await _context.EmailAddressConfirmationTokens.AddAsync(emailAddressConfirmationToken);

                    await _context.SaveChangesAsync();
                    await signUpUserTransaction.CommitAsync();
                }
                catch
                {
                    await signUpUserTransaction.RollbackAsync();
                    throw;
                }
            }

            var createEmailAddressConfirmationTokenResult = new CreateEmailAddressConfirmationTokenResult()
            {
                UserId = newUser.Id,
                EmailAddress = signUpRequest.EmailAddress,
                EmailAddressConfirmationToken = token
            };

            return Result.Ok(createEmailAddressConfirmationTokenResult);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            var normalizedUsername = username.ToLowerInvariant();
            return await _context.Users.AnyAsync(user => user.NormalizedUsername == normalizedUsername);
        }

        public async Task<bool> IsEmailAddressTakenAsync(string emailAddress)
        {
            var normalizedEmailAddress = emailAddress.ToLowerInvariant();
            return await _context.Users.AnyAsync(
                user => user.NormalizedEmailAddress == normalizedEmailAddress
                );
        }

        #endregion

        public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(GetUsersRequest getUsersRequest)
        {
            IQueryable<User> users = _context.Users;

            if (!string.IsNullOrWhiteSpace(getUsersRequest.Username))
            {
                users = users.Where(user =>
                    user.NormalizedUsername.Contains(getUsersRequest.Username.ToLowerInvariant()));
            }

            if (!string.IsNullOrWhiteSpace(getUsersRequest.EmailAddress))
            {
                users = users.Where(user =>
                    user.NormalizedEmailAddress!.Contains(getUsersRequest.EmailAddress.ToLowerInvariant()));
            }

            if (getUsersRequest.HasConfirmedEmailAddress.HasValue)
            {
                users = getUsersRequest.HasConfirmedEmailAddress.Value
                    ? users.Where(user => user.EmailAddress != null)
                    : users.Where(user => user.EmailAddress == null);
            }

            if (getUsersRequest.UserRoleIds != null)
            {
                users = users.Where(user =>
                    user.Roles.Any(role => getUsersRequest.UserRoleIds.Contains(role.Id)));
            }

            return await users
                .OrderBy(user => user.Username)
                .Skip((getUsersRequest.Page - 1) * getUsersRequest.PageSize)
                .Take(getUsersRequest.PageSize)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    EmailAddress = user.EmailAddress,
                    Roles = user.Roles.Select(r => r.Id).ToList(),
                })
                .ToListAsync();
        }

        public async Task<Result> UpdateUserRolesAsync(UpdateUserRolesRequest updateUserRolesRequest, Guid authenticatedUserId)
        {
            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Admin))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage user roles."));
            }

            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == updateUserRolesRequest.Id);

            if (user == null)
            {
                return Result.Fail(new NotFoundError("User not found."));
            }

            if (user.Id != authenticatedUser.Id && user.Roles.Any(role => role.Id == UserRoleEnum.Admin))
            {
                return Result.Fail(new ForbiddenError("Cannot change other admins' roles."));
            }

            var newUserRoles = await _context.UserRoles
                .Where(role => updateUserRolesRequest.UserRoles.Contains(role.Id))
                .ToListAsync();

            user.Roles.Clear();

            foreach (var role in newUserRoles)
            {
                user.Roles.Add(role);
            }

            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        #region Sign In

        public async Task<Result<ClaimsPrincipal>> SignInAsync(SignInRequest signInRequest)
        {
            var validationErrors = ValidateSignInRequest(signInRequest);

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var normalizedUsername = signInRequest.Username.ToLowerInvariant();
            var user = await _context.Users.Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.NormalizedUsername == normalizedUsername);

            if (user == null)
            {
                return Result.Fail("Invalid credentials.");
            }
            else
            {
                var verificationResult = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, signInRequest.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return Result.Fail("Invalid credentials.");
                }
                else
                {
                    if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        var upgradedPasswordHash = _passwordHasher.HashPassword(null, signInRequest.Password);
                        user.PasswordHash = upgradedPasswordHash;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                    };

                    if (!string.IsNullOrEmpty(user.EmailAddress))
                    {
                        claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));
                    }

                    foreach (var role in user.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Id.ToString()));
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme
                        );
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    return Result.Ok(claimsPrincipal);
                }
            }
        }

        public async Task<Result<ClaimsPrincipal>> RefreshSignInAsync(Guid userId)
        {
            var user = await _context.Users.Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                return Result.Fail("Invalid user sign in.");
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                };

                if (!string.IsNullOrEmpty(user.EmailAddress))
                {
                    claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));
                }

                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Id.ToString()));
                }

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                    );
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                return Result.Ok(claimsPrincipal);
            }
        }

        #endregion

        #region Profile

        public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                return Result.Fail(
                    new Error($"The user with the provided id {userId} was not found.")
                    .WithMetadata("StatusCode", StatusCodes.Status404NotFound)
                    );
            }
            else
            {
                return Result.Ok(user.ToDto());
            }
        }

        public async Task<Result> UpdateUsernameAsync(UpdateUsernameRequest updateUsernameRequest)
        {
            var validationErrors = await ValidateUsernameAsync(
                updateUsernameRequest.NewUsername,
                nameof(updateUsernameRequest.NewUsername)
                );

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == updateUsernameRequest.UserId);

            if (user == null)
            {
                return Result.Fail(
                    new Error($"The user with the provided id {updateUsernameRequest.UserId} was not found.")
                    .WithMetadata("StatusCode", StatusCodes.Status404NotFound)
                    );
            }
            else
            {
                user.Username = updateUsernameRequest.NewUsername;
                user.NormalizedUsername = updateUsernameRequest.NewUsername.ToLowerInvariant();
                _context.Update(user);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }

        public async Task<Result> UpdatePasswordAsync(UpdatePasswordRequest updatePasswordRequest)
        {
            var validationErrors = ValidateUpdatePasswordRequest(updatePasswordRequest);

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == updatePasswordRequest.UserId);

            if (user == null)
            {
                return Result.Fail(
                    new Error($"The user with the provided id {updatePasswordRequest.UserId} was not found.")
                    .WithMetadata("StatusCode", StatusCodes.Status404NotFound)
                    );
            }
            else
            {
                user.PasswordHash = _passwordHasher.HashPassword(null, updatePasswordRequest.NewPassword);
                _context.Update(user);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }

        #endregion

        #region Password reset

        public async Task<Result<CreatePasswordResetTokenResult>> CreatePasswordResetTokenAsync(CreatePasswordResetTokenRequest createPasswordResetTokenRequest)
        {
            var validationErrors = ValidateCreatePasswordResetTokenRequest(createPasswordResetTokenRequest);

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var user = await _context.Users.FirstOrDefaultAsync(user => 
                user.NormalizedEmailAddress == createPasswordResetTokenRequest.EmailAddress.ToLowerInvariant()
            );

            if (user != null)
            {
                var token = GenerateToken();
                var tokenHash = HashToken(token);

                var passwordResetToken = new PasswordResetToken
                {
                    TokenHash = tokenHash,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.PasswordResetTokenTTLDays),
                    UserId = user.Id
                };

                using (var issueNewPasswordResetTokenTransaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.PasswordResetTokens
                            .Where(prt => prt.UserId == user.Id && prt.RevokedAt == null && prt.ExpiresAt > DateTime.UtcNow)
                            .ExecuteUpdateAsync(t => t
                                .SetProperty(x => x.RevokedAt, DateTime.UtcNow)
                                .SetProperty(x => x.RevokedReason, "New reset token requested."));

                        await _context.PasswordResetTokens.AddAsync(passwordResetToken);

                        await _context.SaveChangesAsync();
                        await issueNewPasswordResetTokenTransaction.CommitAsync();
                    }
                    catch
                    {
                        await issueNewPasswordResetTokenTransaction.RollbackAsync();
                        throw;
                    }
                }

                var createPasswordResetTokenResult = new CreatePasswordResetTokenResult()
                {
                    EmailAddress = createPasswordResetTokenRequest.EmailAddress,
                    PasswordResetToken = token
                };

                return Result.Ok(createPasswordResetTokenResult);
            }
            else
            {
                return Result.Fail("Email address is not used by any user.");
            }
        }

        private static string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Base64UrlEncoder.Encode(bytes);
        }
        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var validationErrors = ValidateResetPasswordRequest(resetPasswordRequest);

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var tokenHash = HashToken(resetPasswordRequest.Token);
            var passwordResetToken = await _context.PasswordResetTokens.Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.TokenHash == tokenHash);

            if (passwordResetToken == null || !passwordResetToken.IsValid)
            {
                return Result.Fail("Invalid password reset token.");
            }
            else
            {
                using (var resetPasswordTransaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var user = passwordResetToken.User;

                        await _context.PasswordResetTokens
                            .Where(prt => prt.UserId == user.Id && prt.RevokedAt == null && prt.ExpiresAt > DateTime.UtcNow)
                            .ExecuteUpdateAsync(prt => prt
                                .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                                .SetProperty(t => t.RevokedReason, "Password reset completed."));

                        user.PasswordHash = _passwordHasher.HashPassword(null, resetPasswordRequest.NewPassword);
                        _context.Update(user);

                        await _context.SaveChangesAsync();
                        await resetPasswordTransaction.CommitAsync();

                        return Result.Ok();
                    }
                    catch
                    {
                        await resetPasswordTransaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region Email confirmation

        public async Task<Result<CreateEmailAddressConfirmationTokenResult>> CreateEmailAddressConfirmationTokenAsync(CreateEmailAddressConfirmationTokenRequest createEmailAddressConfirmationTokenRequest)
        {
            var validationErrors = await ValidateCreateEmailAddressConfirmationTokenRequestAsync(createEmailAddressConfirmationTokenRequest);

            if (!validationErrors.IsNullOrEmpty())
            {
                return Result.Fail(validationErrors);
            }

            var token = GenerateToken();
            var tokenHash = HashToken(token);

            var emailAddressConfirmationToken = new EmailAddressConfirmationToken
            {
                EmailAddress = createEmailAddressConfirmationTokenRequest.EmailAddress,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                UserId = createEmailAddressConfirmationTokenRequest.UserId
            };

            using (var createTokenTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.EmailAddressConfirmationTokens.AddAsync(emailAddressConfirmationToken);

                    await _context.SaveChangesAsync();
                    await createTokenTransaction.CommitAsync();
                }
                catch
                {
                    await createTokenTransaction.RollbackAsync();
                    throw;
                }
            }

            var createEmailAddressConfirmationTokenResult = new CreateEmailAddressConfirmationTokenResult()
            {
                UserId = createEmailAddressConfirmationTokenRequest.UserId,
                EmailAddress = createEmailAddressConfirmationTokenRequest.EmailAddress,
                EmailAddressConfirmationToken = token
            };

            return Result.Ok(createEmailAddressConfirmationTokenResult);
        }

        public async Task<Result> ConfirmEmailAddressAsync(ConfirmEmailAddressRequest confirmEmailAddressRequest)
        {
            var user = await _context.Users
                .Include(user => user.EmailAddressConfirmationTokens)
                .FirstOrDefaultAsync(user => user.Id == confirmEmailAddressRequest.UserId);
            
            if (user == null)
            {
                return Result.Fail(new Error("Invalid email confirmation. Please resequest a new confirmation."));
            }
            
            var tokenHash = HashToken(confirmEmailAddressRequest.Token);

            var emailAddressConfirmationToken = user.EmailAddressConfirmationTokens
                .FirstOrDefault(token => token.TokenHash == tokenHash);

            if (emailAddressConfirmationToken == null || !emailAddressConfirmationToken.IsValid)
            {
                return Result.Fail(new Error("Invalid email confirmation. Please resequest a new confirmation."));
            }
            else{
                using (var confirmEmailAddressTransaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        user.EmailAddress = emailAddressConfirmationToken.EmailAddress;
                        user.NormalizedEmailAddress = emailAddressConfirmationToken.EmailAddress.ToLowerInvariant();
                        _context.Update(user);

                        emailAddressConfirmationToken.ConfirmedAt = DateTime.UtcNow;
                        _context.Update(emailAddressConfirmationToken);

                        await _context.EmailAddressConfirmationTokens
                            .Where(token => 
                            token.UserId == user.Id 
                            && token.Id != emailAddressConfirmationToken.Id
                            && token.RevokedAt == null 
                            && token.ExpiresAt > DateTime.UtcNow
                            && token.ConfirmedAt == null)
                            .ExecuteUpdateAsync(t => t
                                .SetProperty(x => x.RevokedAt, DateTime.UtcNow)
                                .SetProperty(x => x.RevokedReason, "A different email address has already been confirmed."));

                        await _context.SaveChangesAsync();
                        await confirmEmailAddressTransaction.CommitAsync();
                    }
                    catch
                    {
                        await confirmEmailAddressTransaction.RollbackAsync();
                        throw;
                    }
                }

                return Result.Ok();
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates the provided email address and returns any validation errors that have been encountered.
        /// </summary>
        /// <param name="emailAddress">
        /// The email address that is validated.
        /// </param>
        /// <param name="propertyName">
        /// The name of the corresponding request property that is specified in error metadata.
        /// </param>
        /// <returns>An enumerable containing the validation errors.</returns>
        private async Task<IEnumerable<IError>> ValidateEmailAddressAsync(string emailAddress, string propertyName)
        {
            var validationErrors = new List<IError>();

            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                validationErrors.Add(
                    new Error(EmailAddressSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", propertyName)
                );
            }
            else
            {
                if (emailAddress.Length > EmailAddressSpecifications.MAX_LENGTH)
                {
                    validationErrors.Add(
                        new Error(EmailAddressSpecifications.MAX_LENGTH_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }

                var emailAddressAttribute = new EmailAddressAttribute();
                if (!emailAddressAttribute.IsValid(emailAddress))
                {
                    validationErrors.Add(
                        new Error(EmailAddressSpecifications.EMAIL_ADDRESS_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }

                if (await IsEmailAddressTakenAsync(emailAddress))
                {
                    validationErrors.Add(
                        new Error(EmailAddressSpecifications.AVAILABILITY_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Validates the provided username and returns any validation errors that have been encountered.
        /// </summary>
        /// <param name="username">
        /// The username that is validated.
        /// </param>
        /// <param name="propertyName">
        /// The name of the corresponding request property that is specified in error metadata.
        /// </param>
        /// <returns>An enumerable containing the validation errors.</returns>
        private async Task<IEnumerable<IError>> ValidateUsernameAsync(string username, string propertyName)
        {
            var validationErrors = new List<IError>();

            if (string.IsNullOrWhiteSpace(username))
            {
                validationErrors.Add(
                    new Error(UsernameSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", propertyName)
                );
            }
            else
            {
                if (username.Length < UsernameSpecifications.MIN_LENGTH)
                {
                    validationErrors.Add(
                        new Error(UsernameSpecifications.MIN_LENGTH_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }
                else if (username.Length > UsernameSpecifications.MAX_LENGTH)
                {
                    validationErrors.Add(
                        new Error(UsernameSpecifications.MAX_LENGTH_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }

                if (!Regex.IsMatch(username, UsernameSpecifications.COMPLEXITY_REGEX))
                {
                    validationErrors.Add(
                        new Error(UsernameSpecifications.COMPLEXITY_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }

                if (await IsUsernameTakenAsync(username))
                {
                    validationErrors.Add(
                        new Error(UsernameSpecifications.AVAILABILITY_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Validates the provided password and returns any validation errors that have been encountered.
        /// </summary>
        /// <param name="password">
        /// The password that is validated.
        /// </param>
        /// <param name="propertyName">
        /// The name of the corresponding request property that is specified in error metadata.
        /// </param>
        /// <returns>An enumerable containing the validation errors.</returns>
        private IEnumerable<IError> ValidatePassword(string password, string propertyName)
        {
            var validationErrors = new List<IError>();

            if (string.IsNullOrWhiteSpace(password))
            {
                validationErrors.Add(
                    new Error(PasswordSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", propertyName)
                );
            }
            else
            {
                if (password.Length < PasswordSpecifications.MIN_LENGTH)
                {
                    validationErrors.Add(
                        new Error(PasswordSpecifications.MIN_LENGTH_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }
                else if (password.Length > PasswordSpecifications.MAX_LENGTH)
                {
                    validationErrors.Add(
                        new Error(PasswordSpecifications.MAX_LENGTH_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }

                if (!Regex.IsMatch(password, PasswordSpecifications.COMPLEXITY_REGEX, RegexOptions.CultureInvariant))
                {
                    validationErrors.Add(
                        new Error(PasswordSpecifications.COMPLEXITY_ERROR_MESSAGE)
                            .WithMetadata("Key", propertyName)
                    );
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Validates the provided password confirmation and returns any validation errors that have been encountered.
        /// </summary>
        /// <param name="confirmPassword">
        /// The password confirmation that is validated.
        /// </param>
        /// <param name="propertyName">
        /// The name of the corresponding request property that is specified in error metadata.
        /// </param>
        /// <param name="password">
        /// The password that is used in the comparison validation.
        /// </param>
        /// <returns>An enumerable containing the validation errors.</returns>
        private IEnumerable<IError> ValidateConfirmPassword(string confirmPassword, string propertyName, string password)
        {
            var validationErrors = new List<IError>();

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                validationErrors.Add(
                    new Error(ConfirmPasswordSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", propertyName)
                );
            }
            else if (!string.Equals(confirmPassword, password, StringComparison.Ordinal))
            {
                validationErrors.Add(
                    new Error(ConfirmPasswordSpecifications.COMPARISON_ERROR_MESSAGE)
                        .WithMetadata("Key", propertyName)
                );
            }

            return validationErrors;
        }

        /// <summary>
        /// Validates the sign in request and returns any validation errors that have been encountered.
        /// </summary>
        /// <param name="signInRequest">
        /// The sign in request that is validated.
        /// </param>
        /// <returns>An enumerable containing the validation errors.</returns>
        private IEnumerable<IError> ValidateSignInRequest(SignInRequest signInRequest)
        {
            var validationErrors = new List<IError>();

            if (string.IsNullOrWhiteSpace(signInRequest.Username))
            {
                validationErrors.Add(
                    new Error(UsernameSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", nameof(signInRequest.Username))
                );
            }

            if (string.IsNullOrWhiteSpace(signInRequest.Password))
            {
                validationErrors.Add(
                    new Error(PasswordSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", nameof(signInRequest.Password))
                );
            }

            return validationErrors;
        }

        /// <summary>
        /// Validates the sign up request and returns any validation errors that have been encountered.
        /// </summary>
        /// <param name="signUpRequest">
        /// The sign up request that is validated.
        /// </param>
        /// <returns>An enumerable containing the validation errors.</returns>
        private async Task<IEnumerable<IError>> ValidateSignUpRequestAsync(SignUpRequest signUpRequest)
        {
            var validationErrors = new List<IError>();

            validationErrors.AddRange(
                await ValidateEmailAddressAsync(signUpRequest.EmailAddress, nameof(signUpRequest.EmailAddress))
                );

            validationErrors.AddRange(
                await ValidateUsernameAsync(signUpRequest.Username, nameof(signUpRequest.Username))
                );

            validationErrors.AddRange(
                ValidatePassword(signUpRequest.Password, nameof(signUpRequest.Password))
                );

            validationErrors.AddRange(
                ValidateConfirmPassword(signUpRequest.ConfirmPassword, nameof(signUpRequest.ConfirmPassword), signUpRequest.Password)
                );

            return validationErrors;
        }

        private IEnumerable<IError> ValidateUpdatePasswordRequest(UpdatePasswordRequest updatePasswordRequest)
        {
            var validationErrors = new List<IError>();

            validationErrors.AddRange(
                ValidatePassword(updatePasswordRequest.NewPassword, nameof(updatePasswordRequest.NewPassword))
                );

            validationErrors.AddRange(
                ValidateConfirmPassword(updatePasswordRequest.ConfirmPassword, nameof(updatePasswordRequest.ConfirmPassword), updatePasswordRequest.NewPassword)
                );

            return validationErrors;
        }

        private IEnumerable<IError> ValidateCreatePasswordResetTokenRequest(CreatePasswordResetTokenRequest createPasswordResetTokenRequest)
        {
            var validationErrors = new List<IError>();

            if (string.IsNullOrWhiteSpace(createPasswordResetTokenRequest.EmailAddress))
            {
                validationErrors.Add(
                    new Error(EmailAddressSpecifications.REQUIRED_ERROR_MESSAGE)
                        .WithMetadata("Key", nameof(createPasswordResetTokenRequest.EmailAddress))
                );
            }
            else
            {
                var emailAddressAttribute = new EmailAddressAttribute();
                if (!emailAddressAttribute.IsValid(createPasswordResetTokenRequest.EmailAddress))
                {
                    validationErrors.Add(
                        new Error(EmailAddressSpecifications.EMAIL_ADDRESS_ERROR_MESSAGE)
                            .WithMetadata("Key", nameof(createPasswordResetTokenRequest.EmailAddress))
                    );
                }
            }

            return validationErrors;
        }

        private IEnumerable<IError> ValidateResetPasswordRequest(ResetPasswordRequest resetPasswordRequest)
        {
            var validationErrors = new List<IError>();

            validationErrors.AddRange(
                ValidatePassword(resetPasswordRequest.NewPassword, nameof(resetPasswordRequest.NewPassword))
                );

            validationErrors.AddRange(
                ValidateConfirmPassword(resetPasswordRequest.ConfirmPassword, nameof(resetPasswordRequest.ConfirmPassword), resetPasswordRequest.NewPassword)
                );

            if (string.IsNullOrWhiteSpace(resetPasswordRequest.Token))
            {
                validationErrors.Add(
                    new Error("Password reset token must be provided.")
                    );
            }

            return validationErrors;
        }

        private async Task<IEnumerable<IError>> ValidateCreateEmailAddressConfirmationTokenRequestAsync(CreateEmailAddressConfirmationTokenRequest createEmailAddressConfirmationTokenRequest)
        {
            var validationErrors = await ValidateEmailAddressAsync(
                createEmailAddressConfirmationTokenRequest.EmailAddress,
                nameof(createEmailAddressConfirmationTokenRequest.EmailAddress)
                );

            return validationErrors;
        }

        #endregion
    }
}
