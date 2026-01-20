using Blog.Application.Interfaces;
using Blog.Application.Users.Requests;
using Blog.Domain.Enums;
using Blog.Infrastructure.Security;
using Blog.MVC.Mapping;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Blog.MVC.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IArticleService _articleService;

        public AdminController(IUserService userService, IArticleService articleService)
        {
            _userService = userService;
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var getAdminDashboardDataResult = await _articleService.GetAdminDashboardDataAsync(authenticatedUserId);

            if (getAdminDashboardDataResult.IsSuccess)
            {
                var result = getAdminDashboardDataResult.Value;
                var adminDashboardViewModel = new AdminDashboardViewModel
                {
                    TotalUsers = result.TotalUsers,
                    ReportedCommentsCount = result.ReportedCommentsCount,
                    HiddenCommentsCount = result.HiddenCommentsCount,
                    RecentlyReportedComments = result.RecentlyReportedComments.Select(commentDto => new ReportedCommentPreviewViewModel
                    {
                        CommentId = commentDto.Id,
                        PreviewText = commentDto.Text.Length > 100
                            ? commentDto.Text.Substring(0, 97) + "..."
                            : commentDto.Text,
                        ReportCount = commentDto.Reports!.Count,
                        LastReportedAt = commentDto.Reports.Select(report => report.CreatedAt).Max()
                    })
                    .ToList()
                };

                return View(adminDashboardViewModel);
            }
            else
            {
                return getAdminDashboardDataResult.Errors.First().ToErrorActionResult();
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> UserIndex(AdminUserListViewModel adminUserListViewModel)
        {
            if (!Request.Query.Any())
            {
                adminUserListViewModel.SelectedUserRoles = Enum.GetValues<UserRoleEnum>().ToList();
            }

            var getUsersRequest = new GetUsersRequest
            {
                Username = adminUserListViewModel.Username,
                EmailAddress = adminUserListViewModel.EmailAddress,
                HasConfirmedEmailAddress = adminUserListViewModel.HasConfirmedEmailAddress,
                UserRoleIds = adminUserListViewModel.SelectedUserRoles,
                Page = adminUserListViewModel.Page,
                PageSize = adminUserListViewModel.PageSize
            };

            var users = await _userService.GetUsersAsync(getUsersRequest);

            adminUserListViewModel.Users = users;

            return View("Users/Index", adminUserListViewModel);
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<IActionResult> UserDetails(Guid userId)
        {
            var getUserResult = await _userService.GetUserByIdAsync(userId);

            if (getUserResult.IsSuccess)
            {
                var user = getUserResult.Value;
                var userDetailsViewModel = new AdminUserDetailsViewModel
                {
                    Id = userId,
                    Username = user.Username,
                    EmailAddress = user.EmailAddress,
                    UserRoles = user.Roles.ToList()
                };

                return View("Users/Details", userDetailsViewModel);
            }
            else
            {
                return getUserResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [HttpGet("comments")]
        public async Task<IActionResult> CommentIndex()
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var getReportedCommentsResult = await _articleService.GetReportedCommentsAsync(authenticatedUserId);
            
            if (getReportedCommentsResult.IsSuccess)
            {
                var reportedCommentsListViewModel = new ReportedCommentsListViewModel
                {
                    Comments = getReportedCommentsResult.Value.Select(commentDto => new ReportedCommentListItemViewModel
                    {
                        CommentId = commentDto.Id,
                        PreviewText = commentDto.Text.Length > 100
                            ? commentDto.Text.Substring(0, 97) + "..."
                            : commentDto.Text,
                        CreatedAt = commentDto.CreatedAt,
                        IsHidden = commentDto.IsHidden,
                        CommentatorId = commentDto.CommentatorId,
                        CommentatorUsername = commentDto.CommentatorUsername!,
                        ArticleId = commentDto.ArticleId,
                        ArticleTitle = commentDto.ArticleTitle!,
                        ReportCount = commentDto.Reports!.Count
                    })
                    .ToList()
                };

                return View("Comments/Index", reportedCommentsListViewModel);
            }
            else
            {
                return getReportedCommentsResult.Errors.First().ToErrorActionResult();
            }
        }

        [HttpGet("comments/{commentId:guid}")]
        public async Task<IActionResult> CommentDetails(Guid commentId)
        {
            var getCommentResult = await _articleService.GetArticleCommentByIdAsync(commentId);

            if (getCommentResult.IsSuccess)
            {
                var adminCommentDetailsViewModel = new AdminCommentDetailsViewModel
                {
                    CommentId = getCommentResult.Value.Id,
                    Text = getCommentResult.Value.Text,
                    CreatedAt = getCommentResult.Value.CreatedAt,
                    IsHidden = getCommentResult.Value.IsHidden,
                    CommentatorId = getCommentResult.Value.CommentatorId,
                    CommentatorUsername = getCommentResult.Value.CommentatorUsername!,
                    ArticleId = getCommentResult.Value.ArticleId,
                    ArticleTitle = getCommentResult.Value.ArticleTitle!,
                    Reports = getCommentResult.Value.Reports!.Select(report => new AdminArticleCommentReportViewModel
                    {
                        ReporterUsername = report.ReporterUsername!,
                        ReporterId = report.ReporterId!,
                        Reason = report.Reason,
                        Description = report.Description,
                        CreatedAt= report.CreatedAt,
                    })
                    .ToList()
                };

                return View("Comments/Details", adminCommentDetailsViewModel);
            }
            else
            {
                return getCommentResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [HttpPost("comments/{commentId:guid}/hidden")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCommentHidden(
            Guid commentId,
            bool isHidden)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();
            
            var hiddenUpdateResult = await _articleService.UpdateArticleCommentHiddenAsync(commentId, isHidden, authenticatedUserId);
            if (hiddenUpdateResult.IsSuccess)
            {
                TempData["Success"] = "Comment's hidden status successfully updated.";

                return RedirectToAction(nameof(CommentDetails), new { commentId });
            }
            else
            {
                return hiddenUpdateResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [HttpGet("users/{userId:guid}/roles")]
        public async Task<IActionResult> UpdateUserRoles(Guid userId)
        {
            var getUserResult = await _userService.GetUserByIdAsync(userId);

            if (getUserResult.IsSuccess)
            {
                var userDto = getUserResult.Value;
                var currentlyAssignedUserRoles = userDto.Roles.ToList();
                var selectedUserRoles = userDto.Roles.ToList();

                var updateUserRolesViewModel = new UpdateUserRolesViewModel
                {
                    Id = userId,
                    Username = userDto.Username,
                    AssignedUserRoles = currentlyAssignedUserRoles,
                    SelectedUserRoles = selectedUserRoles
                };

                return View("Users/UpdateUserRoles", updateUserRolesViewModel);
            }
            else
            {
                return getUserResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }

        [HttpPost("users/{userId:guid}/roles")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(Guid userId, UpdateUserRolesViewModel updateUserRolesViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when updating user roles.";
                return View("Users/UpdateUserRoles", updateUserRolesViewModel);
            }

            var authenticatedUserId = User.GetAuthenticatedUserId();

            var updateUserRolesRequest = new UpdateUserRolesRequest
            {
                Id = userId,
                UserRoles = updateUserRolesViewModel.SelectedUserRoles
            };

            var updateUserRolesResult = await _userService.UpdateUserRolesAsync(updateUserRolesRequest, authenticatedUserId);

            if (updateUserRolesResult.IsSuccess)
            {
                TempData["Success"] = "User's roles have been successfully updated.";

                return RedirectToAction(nameof(UserDetails), new { userId });
            }
            else
            {
                return updateUserRolesResult.Errors.FirstOrDefault().ToErrorActionResult();
            }
        }
    }
}
