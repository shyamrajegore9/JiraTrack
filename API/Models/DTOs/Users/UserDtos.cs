namespace JiraTrack.Models.DTOs.Users;

public class UserListDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class UserDetailDto : UserListDto
{
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string TimeZone { get; set; } = "UTC";
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<int> RoleIds { get; set; } = [];
}

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string TimeZone { get; set; } = "UTC";
}

public class AssignRolesRequest
{
    public List<int> RoleIds { get; set; } = [];
}

public class UserLookupDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UserFilterRequest
{
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value < 1 ? 20 : value;
    }
    public string? SearchTerm { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";
}
