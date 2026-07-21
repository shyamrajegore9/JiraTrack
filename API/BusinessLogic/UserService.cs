using AutoMapper;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Users;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;

namespace JiraTrack.BusinessLogic;

public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResponse<UserListDto>> GetUsersAsync(UserFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Users.GetPagedAsync(filter, cancellationToken);
        return new PagedResponse<UserListDto>
        {
            Items = _mapper.Map<List<UserListDto>>(result.Items),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<UserDetailDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        return _mapper.Map<UserDetailDto>(user);
    }

    public async Task<UserDetailDto> CreateUserAsync(CreateUserRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken) != null)
            throw new BusinessException("Email already exists.", 409);

        if (await _unitOfWork.Users.GetByUserNameAsync(request.UserName, cancellationToken) != null)
            throw new BusinessException("Username already exists.", 409);

        await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            UserName = request.UserName.Trim(),
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.Users.SetUserRolesAsync(user.Id, request.RoleIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} created by {CreatedBy}", user.Email, createdBy);

        return await GetUserByIdAsync(user.Id, cancellationToken);
    }

    public async Task<UserDetailDto> UpdateUserAsync(int id, UpdateUserRequest request, int updatedBy, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var emailExists = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (emailExists != null && emailExists.Id != id)
            throw new BusinessException("Email already exists.", 409);

        var userNameExists = await _unitOfWork.Users.GetByUserNameAsync(request.UserName, cancellationToken);
        if (userNameExists != null && userNameExists.Id != id)
            throw new BusinessException("Username already exists.", 409);

        user.Email = request.Email.Trim().ToLowerInvariant();
        user.UserName = request.UserName.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        user.TimeZone = request.TimeZone;
        user.UpdatedBy = updatedBy;
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDetailDto>(user);
    }

    public async Task DeleteUserAsync(int id, int deletedBy, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        _unitOfWork.Users.SoftDelete(user, deletedBy);
        await _unitOfWork.Users.RevokeAllUserRefreshTokensAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} soft-deleted by {DeletedBy}", id, deletedBy);
    }

    public async Task ActivateUserAsync(int id, int updatedBy, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.IsActive = true;
        user.UpdatedBy = updatedBy;
        user.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateUserAsync(int id, int updatedBy, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.IsActive = false;
        user.UpdatedBy = updatedBy;
        user.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.Users.RevokeAllUserRefreshTokensAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDetailDto> AssignRolesAsync(int id, AssignRolesRequest request, int updatedBy, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);

        await _unitOfWork.Users.SetUserRolesAsync(id, request.RoleIds, cancellationToken);
        user.UpdatedBy = updatedBy;
        user.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<List<UserLookupDto>> GetLookupAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetLookupAsync(searchTerm, cancellationToken);
        return users.Select(u => new UserLookupDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = $"{u.FirstName} {u.LastName}".Trim()
        }).ToList();
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync(cancellationToken);
        return _mapper.Map<List<RoleDto>>(roles);
    }

    public async Task<RoleDto> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Role not found.");
        return _mapper.Map<RoleDto>(role);
    }

    private async Task ValidateRoleIdsAsync(IEnumerable<int> roleIds, CancellationToken cancellationToken)
    {
        var ids = roleIds.Distinct().ToList();
        if (ids.Count == 0)
            throw new BusinessException("At least one role is required.");

        var roles = await _unitOfWork.Roles.GetAllAsync(cancellationToken);
        if (ids.Any(id => roles.All(r => r.Id != id)))
            throw new BusinessException("One or more invalid role IDs.");
    }
}
