using System.Security.Claims;
using Application.DTOs;
using Application.UseCases.Authentication.Commands.Login;
using Application.UseCases.Authentication.Commands.Logout;
using Application.UseCases.Authentication.Commands.RefreshToken;
using Application.UseCases.Authentication.Commands.Register;
using Application.UseCases.Profile.Commands.ChangePassword;
using Application.UseCases.Profile.Commands.UpdateProfile;
using Application.UseCases.Profile.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountController(IMediator mediator) : ControllerBase
{
    #region Authentication Endpoints

    /// <summary>
    /// User Login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication token</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid credentials or validation error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await mediator.Send(command, ct);

        return Ok(new ApiResponse<AuthDto>(result, "Login successful"));
    }

    /// <summary>
    /// User Registration
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Registration successful</response>
    /// <response code="400">Validation error or user already exists</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct = default)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            UserName = request.UserName,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword
        };

        await mediator.Send(command, ct);

        return Ok(new ApiResponse("Registration successful"));
    }

    /// <summary>
    /// User Logout
    /// </summary>
    /// <param name="request">Refresh token to invalidate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="400">Invalid refresh token</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken
        };

        await mediator.Send(command, ct);

        return Ok(new ApiResponse("Logout successful"));
    }

    /// <summary>
    /// Refresh Authentication Token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>New authentication token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid refresh token</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("invoke-token")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<AuthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthDto>>> InvokeToken(
        [FromBody] InvokeTokenRequest request,
        CancellationToken ct = default)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var result = await mediator.Send(command, ct);

        return Ok(new ApiResponse<AuthDto>(result, "Token invoked successfully"));
    }

    #endregion

    #region Profile Management Endpoints

    /// <summary>
    /// Get User Profile
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetProfile(
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = new GetProfileQuery
        {
            UserId = userId
        };

        var result = await mediator.Send(query, ct);

        return Ok(new ApiResponse<UserInfoDto>(result, "Profile retrieved successfully"));
    }

    /// <summary>
    /// Update User Profile
    /// </summary>
    /// <param name="request">Profile update details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("update-profile")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UpdateProfileCommand
        {
            UserId = userId,
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        await mediator.Send(command, ct);

        return Ok(new ApiResponse("Profile updated successfully"));
    }

    /// <summary>
    /// Change User Password
    /// </summary>
    /// <param name="request">Password change details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password changed successfully</response>
    /// <response code="400">Validation error or incorrect old password</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("change-password")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "User")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        await mediator.Send(command, ct);

        return Ok(new ApiResponse("Password changed successfully"));
    }

    #endregion
}