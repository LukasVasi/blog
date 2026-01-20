using Blog.Application.Email.Requests;
using Blog.Application.Interfaces;
using Blog.Application.Users.Requests;
using Blog.Infrastructure.Options;
using Blog.MVC.Mapping;
using Blog.MVC.Utility;
using Blog.MVC.ViewModels.Account;
using Blog.MVC.ViewModels.Account.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Blog.MVC.Controllers
{
    [Authorize]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly AuthOptions _authOptions;

        public AccountController(IUserService userService, IEmailService emailService, IOptions<AuthOptions> authOptions)
        {
            _userService = userService;
            _emailService = emailService;
            _authOptions = authOptions.Value;
        }

        #region Sign up

        [AllowAnonymous]
        [HttpGet("sign-up")]
        public async Task<IActionResult> SignUp()
        {
            var signUpViewModel = new SignUpViewModel();
            return View(signUpViewModel);
        }

        [AllowAnonymous]
        [HttpPost("sign-up")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel signUpViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when signing up.";
                return View(signUpViewModel);
            }

            var signUpRequest = new SignUpRequest()
            {
                EmailAddress = signUpViewModel.EmailAddress,
                Username = signUpViewModel.Username,
                Password = signUpViewModel.Password,
                ConfirmPassword = signUpViewModel.ConfirmPassword,
            };

            var signUpResult = await _userService.SignUpAsync(signUpRequest);

            if (signUpResult.IsSuccess)
            {
                var confirmEmailAddressUrl = Url.Action(
                    action: "ConfirmEmailAddress",
                    controller: "Account",
                    values: new {
                        userId = signUpResult.Value.UserId,
                        token = signUpResult.Value.EmailAddressConfirmationToken
                    },
                    protocol: Request.Scheme,
                    host: Request.Host.ToString()
                    );

                var sendEmailConfirmationRequest = new SendEmailAddressConfirmationRequest()
                {
                    EmailAddress = signUpViewModel.EmailAddress,
                    ConfirmEmailAddressUrl = confirmEmailAddressUrl!
                };

                await _emailService.SendEmailAddressConfirmationAsync(sendEmailConfirmationRequest);

                TempData["Success"] = "Successfully signed up!";
                if (_authOptions.EmailAddressConfirmationRequired)
                {
                    return RedirectToAction(nameof(EmailAddressConfirmationRequired));
                }
                else
                {
                    return RedirectToAction(nameof(SignIn));
                }
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, signUpResult);
                TempData["Error"] = "An error has been encountered when signing up.";
                return View(signUpViewModel);
            }
        }

        [AllowAnonymous]
        [HttpGet("confirmation-required")]
        public async Task<IActionResult> EmailAddressConfirmationRequired()
        {
            return View();
        }

        #endregion

        [AllowAnonymous]
        [HttpGet("sign-in")]
        public async Task<IActionResult> SignIn(string? returnUrl = null)
        {
            var signInViewModel = new SignInViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(signInViewModel);
        }

        [AllowAnonymous]
        [HttpPost("sign-in")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel signInViewModel, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                signInViewModel.ReturnUrl = returnUrl;
                TempData["Error"] = "An error has been encountered when signing in.";
                return View(signInViewModel);
            }

            var signInRequest = new SignInRequest()
            {
                Username = signInViewModel.Username,
                Password = signInViewModel.Password
            };

            var signInResult = await _userService.SignInAsync(signInRequest);

            if (signInResult.IsSuccess)
            {
                var claimsPrincipal = signInResult.Value;

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties
                    );

                TempData["Success"] = "Successfully signed in!";

                // Validate returnUrl to avoid open redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    if (_authOptions.EmailAddressConfirmationRequired && !claimsPrincipal.Claims.Any(claim => claim.Type == ClaimTypes.Email))
                    {
                        return RedirectToAction(nameof(SendEmailAddressConfirmation));
                    }
                    else
                    {
                        return RedirectToAction(nameof(HomeController.Index), "home");
                    }
                }
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, signInResult);
                TempData["Error"] = "An error has been encountered when signing in.";
                return View(signInViewModel);
            }
        }

        [AllowAnonymous]
        [HttpPost("sign-out")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            TempData["Success"] = "Successfully signed out!";
            return RedirectToAction(nameof(HomeController.Index), "home");
        }

        #region Profile

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = (Guid)GetSignedInUserId()!;
            var getUserResult = await _userService.GetUserByIdAsync((Guid) userId);

            if (getUserResult.IsSuccess)
            {
                var user = getUserResult.Value;
                var profileViewModel = new ProfileViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    EmailAddress = user.EmailAddress,
                    UserRoles = user.Roles
                };

                return View("Profile/Index", profileViewModel);
            }
            else
            {
                return getUserResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpGet("profile/email-address")]
        public async Task<IActionResult> UpdateEmailAddress()
        {
            var userId = (Guid) GetSignedInUserId()!;
            var getUserResult = await _userService.GetUserByIdAsync(userId);

            if (getUserResult.IsSuccess)
            {
                var user = getUserResult.Value;
                var updateEmailAddressViewModel = new UpdateEmailAddressViewModel
                {
                    CurrentEmailAddress = user.EmailAddress!
                };

                return View("Profile/UpdateEmailAddress", updateEmailAddressViewModel);
            }
            else
            {
                return getUserResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpPost("profile/email-address")]
        public async Task<IActionResult> UpdateEmailAddress(UpdateEmailAddressViewModel updateEmailAddressViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when updating email address.";
                return View("Profile/UpdateEmailAddress", updateEmailAddressViewModel);
            }

            var userId = (Guid)GetSignedInUserId()!;

            var createEmailAddressConfirmationTokenRequest = new CreateEmailAddressConfirmationTokenRequest()
            {
                UserId = userId,
                EmailAddress = updateEmailAddressViewModel.NewEmailAddress
            };

            var tokenCreationResult = await _userService.CreateEmailAddressConfirmationTokenAsync(createEmailAddressConfirmationTokenRequest);

            if (tokenCreationResult.IsSuccess)
            {
                var confirmEmailAddressUrl = Url.Action(
                    action: "ConfirmEmailAddress",
                    controller: "Account",
                    values: new
                    {
                        userId = tokenCreationResult.Value.UserId,
                        token = tokenCreationResult.Value.EmailAddressConfirmationToken
                    },
                    protocol: Request.Scheme,
                    host: Request.Host.ToString()
                );

                var sendEmailConfirmationRequest = new SendEmailAddressConfirmationRequest()
                {
                    EmailAddress = updateEmailAddressViewModel.NewEmailAddress,
                    ConfirmEmailAddressUrl = confirmEmailAddressUrl!
                };

                await _emailService.SendEmailAddressConfirmationAsync(sendEmailConfirmationRequest);

                TempData["Success"] = "Email address update confirmation successfully sent!";
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, tokenCreationResult);
                TempData["Error"] = "An error has been encountered when updating email address.";
                return View(updateEmailAddressViewModel);
            }
        }

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpGet("profile/username")]
        public async Task<IActionResult> UpdateUsername()
        {
            var userId = (Guid)GetSignedInUserId()!;
            var getUserResult = await _userService.GetUserByIdAsync(userId);

            if (getUserResult.IsSuccess)
            {
                var user = getUserResult.Value;
                var updateUsernameViewModel = new UpdateUsernameViewModel
                {
                    CurrentUsername = user.Username
                };

                return View("Profile/UpdateUsername", updateUsernameViewModel);
            }
            else
            {
                return getUserResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpPost("profile/username")]
        public async Task<IActionResult> UpdateUsername(UpdateUsernameViewModel updateUsernameViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when updating username.";
                return View("Profile/UpdateUsername", updateUsernameViewModel);
            }

            var userId = (Guid)GetSignedInUserId()!;

            var updateRequest = new UpdateUsernameRequest()
            {
                UserId = userId,
                NewUsername = updateUsernameViewModel.NewUsername
            };

            var updateResult = await _userService.UpdateUsernameAsync(updateRequest);

            if (updateResult.IsSuccess)
            {
                var refreshResult = await _userService.RefreshSignInAsync(userId);

                if (refreshResult.IsSuccess)
                {
                    var updatedClaimsPrincipal = refreshResult.Value;

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                        AllowRefresh = true,
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        updatedClaimsPrincipal,
                        authProperties
                        );

                    HttpContext.User = updatedClaimsPrincipal;
                }

                TempData["Success"] = "Successfully updated username!";
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, updateResult);
                TempData["Error"] = "An error has been encountered when updating username.";
                return View(updateUsernameViewModel);
            }
        }

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpGet("profile/password")]
        public async Task<IActionResult> UpdatePassword()
        {
            var userId = (Guid)GetSignedInUserId()!;
            var getUserResult = await _userService.GetUserByIdAsync(userId);

            if (getUserResult.IsSuccess)
            {
                var user = getUserResult.Value;
                var updatePasswordViewModel = new UpdatePasswordViewModel();

                return View("Profile/UpdatePassword", updatePasswordViewModel);
            }
            else
            {
                return getUserResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [Authorize(Policy = "RequireConfirmedEmail")]
        [HttpPost("profile/password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordViewModel updatePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when updating password.";
                return View("Profile/UpdatePassword", updatePasswordViewModel);
            }

            var userId = (Guid)GetSignedInUserId()!;

            var updateRequest = new UpdatePasswordRequest()
            {
                UserId = userId,
                NewPassword = updatePasswordViewModel.NewPassword,
                ConfirmPassword = updatePasswordViewModel.ConfirmPassword
            };

            var updateResult = await _userService.UpdatePasswordAsync(updateRequest);

            if (updateResult.IsSuccess)
            {
                TempData["Success"] = "Successfully updated password!";
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, updateResult);
                TempData["Error"] = "An error has been encountered when updating password.";
                return View(updatePasswordViewModel);
            }
        }

        #endregion

        #region Password reset

        [AllowAnonymous]
        [HttpGet("forgot-password")]
        public async Task<IActionResult> ForgotPassword()
        {
            var forgotPasswordViewModel = new ForgotPasswordViewModel();

            return View(forgotPasswordViewModel);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when sending password reset email.";
                return View(forgotPasswordViewModel);
            }

            var createPasswordResetTokenRequest = new CreatePasswordResetTokenRequest()
            {
                EmailAddress = forgotPasswordViewModel.EmailAddress
            };

            var tokenCreationResult = await _userService.CreatePasswordResetTokenAsync(createPasswordResetTokenRequest);

            if (tokenCreationResult.IsSuccess)
            {
                var resetPasswordUrl = Url.Action(
                    action: nameof(ResetPassword),
                    controller: "Account",
                    values: new
                    {
                        token = tokenCreationResult.Value.PasswordResetToken
                    },
                    protocol: Request.Scheme,
                    host: Request.Host.ToString()
                );

                var sendPasswordResetRequest = new SendPasswordResetRequest()
                {
                    EmailAddress = createPasswordResetTokenRequest.EmailAddress,
                    ResetPasswordUrl = resetPasswordUrl!
                };

                await _emailService.SendPasswordResetAsync(sendPasswordResetRequest);
            }

            TempData["Success"] = "Password reset sent!";
            return View();
        }

        [AllowAnonymous]
        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var resetPasswordViewModel = new ResetPasswordViewModel 
            { 
                Token = token 
            };

            return View(resetPasswordViewModel);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when resetting password.";
                return View(resetPasswordViewModel);
            }

            var resetPasswordRequest = new ResetPasswordRequest()
            {
                NewPassword = resetPasswordViewModel.NewPassword,
                ConfirmPassword = resetPasswordViewModel.ConfirmPassword,
                Token = resetPasswordViewModel.Token
            };

            var resetPasswordResult = await _userService.ResetPasswordAsync(resetPasswordRequest);

            if (resetPasswordResult.IsSuccess)
            {
                TempData["Success"] = "Successfully reset password!";
                return RedirectToAction(nameof(SignIn));
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, resetPasswordResult);
                TempData["Error"] = "An error has been encountered when resetting password.";
                return View(resetPasswordViewModel);
            }
        }

        #endregion

        #region Email confirmation

        [HttpGet("send-email-address-confirmation")]
        public async Task<IActionResult> SendEmailAddressConfirmation()
        {
            if (
                User.Identity != null
                && User.Identity.IsAuthenticated
                && User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email) != null
                )
            {
                return RedirectToAction(nameof(UpdateEmailAddress));
            }
            else
            {
                var sendEmailAddressConfirmationViewModel = new SendEmailAddressConfirmationViewModel();
                return View(sendEmailAddressConfirmationViewModel);
            }
        }

        [HttpPost("send-email-address-confirmation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailAddressConfirmation(SendEmailAddressConfirmationViewModel sendEmailAddressConfirmationViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when sending email address confirmation.";
                return View(sendEmailAddressConfirmationViewModel);
            }

            var userId = (Guid)GetSignedInUserId()!;

            var createEmailAddressConfirmationTokenRequest = new CreateEmailAddressConfirmationTokenRequest()
            {
                UserId = userId,
                EmailAddress = sendEmailAddressConfirmationViewModel.EmailAddress
            };

            var tokenCreationResult = await _userService.CreateEmailAddressConfirmationTokenAsync(createEmailAddressConfirmationTokenRequest);

            if (tokenCreationResult.IsSuccess)
            {
                var confirmEmailAddressUrl = Url.Action(
                    action: nameof(ConfirmEmailAddress),
                    controller: "Account",
                    values: new { 
                        userId = tokenCreationResult.Value.UserId,
                        token = tokenCreationResult.Value.EmailAddressConfirmationToken 
                    },
                    protocol: Request.Scheme,
                    host: Request.Host.ToString()
                );

                var sendEmailConfirmationRequest = new SendEmailAddressConfirmationRequest()
                {
                    EmailAddress = sendEmailAddressConfirmationViewModel.EmailAddress,
                    ConfirmEmailAddressUrl = confirmEmailAddressUrl!
                };

                await _emailService.SendEmailAddressConfirmationAsync(sendEmailConfirmationRequest);
            }

            TempData["Success"] = "Email address confirmation sent!";
            return View();
        }

        [AllowAnonymous]
        [HttpGet("confirm-email-address")]
        public async Task<IActionResult> ConfirmEmailAddress([FromQuery] Guid userId, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var confirmEmailAddressRequest = new ConfirmEmailAddressRequest()
            {
                UserId = userId,
                Token = token
            };

            var confirmEmailAddressResult = await _userService.ConfirmEmailAddressAsync(confirmEmailAddressRequest);

            if (confirmEmailAddressResult.IsSuccess)
            {
                if (
                    User.Identity != null 
                    && User.Identity.IsAuthenticated 
                    && Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value, out var signedInUserId)
                    && signedInUserId == userId
                    )
                {
                    var refreshResult = await _userService.RefreshSignInAsync(userId);

                    if (refreshResult.IsSuccess)
                    {
                        var updatedClaimsPrincipal = refreshResult.Value;

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                            AllowRefresh = true,
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            updatedClaimsPrincipal,
                            authProperties
                            );

                        HttpContext.User = updatedClaimsPrincipal;
                    }
                }

                TempData["Success"] = "Email address confirmed successfully.";
                return View();
            }
            else
            {
                ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, confirmEmailAddressResult);
                TempData["Error"] = "An error has been encountered when confirming email address.";
                return View();
            }
        }

        #endregion

        #region Validation

        [AllowAnonymous]
        [HttpGet("check-email-address-availability")]
        public async Task<IActionResult> CheckEmailAddressAvailability()
        {
            var emailAddress =
                Request.Query["emailAddress"].FirstOrDefault() ??
                Request.Query["EmailAddress"].FirstOrDefault() ??
                Request.Query["NewEmailAddress"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return Json(true);
            }
            else
            {
                return Json(!await _userService.IsEmailAddressTakenAsync(emailAddress));
            }
        }

        [AllowAnonymous]
        [HttpGet("check-username-availability")]
        public async Task<IActionResult> CheckUsernameAvailability()
        {
            var username =
                Request.Query["username"].FirstOrDefault() ??
                Request.Query["Username"].FirstOrDefault() ??
                Request.Query["NewUsername"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(true);
            }
            else
            {
                return Json(!await _userService.IsUsernameTakenAsync(username));
            }
        }

        #endregion

        private Guid? GetSignedInUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdString))
            {
                if (Guid.TryParse(userIdString, out var userId))
                {
                    return userId;
                }
            }

            return null;
        }
    }
}
