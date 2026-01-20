using Blog.Application.Users.Dtos;
using Blog.Application.Users.Requests;
using Blog.Application.Users.Results;
using FluentResults;
using System.Security.Claims;

namespace Blog.Application.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Signs up a user. Creates the user and an initial email address confirmation token.
        /// </summary>
        /// <param name="signUpRequest">
        /// Request containing sign up information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful and containing token creation result.
        /// </returns>
        public Task<Result<CreateEmailAddressConfirmationTokenResult>> SignUpAsync(SignUpRequest signUpRequest);

        /// <summary>
        /// Signs in the user by creating a claims principal.
        /// </summary>
        /// <param name="signInRequest">
        /// Request containing sign in information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful 
        /// and containing the user's <see cref="ClaimsPrincipal"/> on success.
        /// </returns>
        public Task<Result<ClaimsPrincipal>> SignInAsync(SignInRequest signInRequest);

        /// <summary>
        /// Refreshes a sign in by creating a new claims principal for a specific user.
        /// </summary>
        /// <param name="userId">
        /// The ID of the user.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful
        /// and containing the user's <see cref="ClaimsPrincipal"/> on success.
        /// </returns>
        public Task<Result<ClaimsPrincipal>> RefreshSignInAsync(Guid userId);

        /// <summary>
        /// Determines if the specified username is already being used.
        /// </summary>
        /// <param name="username">
        /// The username to be checked.
        /// </param>
        /// <returns>
        /// True if a user with the specified username exists, false otherwise.
        /// </returns>
        public Task<bool> IsUsernameTakenAsync(string username);

        /// <summary>
        /// Determines if the specified email address is already being used.
        /// </summary>
        /// <param name="emailAddress">
        /// The email address to be checked.
        /// </param>
        /// <returns>
        /// True if a user with the specified confirmed email address exists, false otherwise.
        /// </returns>
        public Task<bool> IsEmailAddressTakenAsync(string emailAddress);

        /// <summary>
        /// Retrieves the users that match the filter information provided in the request.
        /// </summary>
        /// <param name="getUsersRequest">
        /// The request containing the retrieval information.
        /// </param>
        /// <returns>
        /// A collection of users.
        /// </returns>
        public Task<IReadOnlyCollection<UserDto>> GetUsersAsync(GetUsersRequest getUsersRequest);

        /// <summary>
        /// Retrieves the user with the specified id if such a user exists.
        /// </summary>
        /// <param name="userId">
        /// The ID of the user.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result<UserDto>> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Updates a user's username to a new one.
        /// </summary>
        /// <param name="updateUsernameRequest">
        /// Request containing username update information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> UpdateUsernameAsync(UpdateUsernameRequest updateUsernameRequest);

        /// <summary>
        /// Updates a user's password to a new one.
        /// </summary>
        /// <param name="updatePasswordRequest">
        /// Request containing password update information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> UpdatePasswordAsync(UpdatePasswordRequest updatePasswordRequest);

        /// <summary>
        /// Creates a password reset token needed to reset a user's password.
        /// </summary>
        /// <param name="createPasswordResetTokenRequest">
        /// Request containing token creation information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful
        /// and containing the new token on success.
        /// </returns>
        public Task<Result<CreatePasswordResetTokenResult>> CreatePasswordResetTokenAsync(CreatePasswordResetTokenRequest createPasswordResetTokenRequest);

        /// <summary>
        /// Resets the user's password changing it to a new one.
        /// </summary>
        /// <param name="resetPasswordRequest">
        /// Request containing password reset information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);

        /// <summary>
        /// Creates an email address confirmation token needed to confirm a user's email address.
        /// </summary>
        /// <param name="createEmailAddressConfirmationTokenRequest">
        /// Request containing token creation information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful
        /// and containing the new token on success.
        /// </returns>
        public Task<Result<CreateEmailAddressConfirmationTokenResult>> CreateEmailAddressConfirmationTokenAsync(CreateEmailAddressConfirmationTokenRequest createEmailAddressConfirmationTokenRequest);

        /// <summary>
        /// Confirms a user's email address.
        /// </summary>
        /// <param name="confirmEmailAddressRequest">
        /// Request containing email address confirmation information.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> ConfirmEmailAddressAsync(ConfirmEmailAddressRequest confirmEmailAddressRequest);

        /// <summary>
        /// Updates a user's roles.
        /// </summary>
        /// <param name="updateUserRolesRequest">
        /// The request containing the update's information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user.
        /// </param>
        /// <returns>A <see cref="Result"/> determining if the operation was successful.</returns>
        public Task<Result> UpdateUserRolesAsync(UpdateUserRolesRequest updateUserRolesRequest, Guid authenticatedUserId);
    }
}
