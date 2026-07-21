using System.Security.Claims;
using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Users;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService) => _userService = userService;

    [HttpGet]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserListDto>>>> GetUsers([FromQuery] UserFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResponse<UserListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetUser(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UserDetailDto>.Ok(result));
    }

    [HttpPost]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateUserAsync(request, GetCurrentUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = result.Id }, ApiResponse<UserDetailDto>.Ok(result, "User created"));
    }

    [HttpPut("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateUserAsync(id, request, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<UserDetailDto>.Ok(result, "User updated"));
    }

    [HttpDelete("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteUserAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok("User deleted"));
    }

    [HttpPatch("{id:int}/activate")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse>> ActivateUser(int id, CancellationToken cancellationToken)
    {
        await _userService.ActivateUserAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok("User activated"));
    }

    [HttpPatch("{id:int}/deactivate")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse>> DeactivateUser(int id, CancellationToken cancellationToken)
    {
        await _userService.DeactivateUserAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok("User deactivated"));
    }

    [HttpPut("{id:int}/roles")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> AssignRoles(int id, [FromBody] AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.AssignRolesAsync(id, request, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<UserDetailDto>.Ok(result, "Roles assigned"));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<UserLookupDto>>>> Lookup([FromQuery] string? searchTerm, CancellationToken cancellationToken)
    {
        var result = await _userService.GetLookupAsync(searchTerm, cancellationToken);
        return Ok(ApiResponse<List<UserLookupDto>>.Ok(result));
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly UserService _userService;

    public RolesController(UserService userService) => _userService = userService;

    [HttpGet]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _userService.GetRolesAsync(cancellationToken);
        return Ok(ApiResponse<List<RoleDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRole(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetRoleByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<RoleDto>.Ok(result));
    }
}
