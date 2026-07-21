using AutoMapper;
using JiraTrack.Models.DTOs.Auth;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;
using Microsoft.Extensions.Options;

namespace JiraTrack.BusinessLogic;

public class AuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        TokenService tokenService,
        IMapper mapper,
        IOptions<AppSettings> appSettings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(request.Email, cancellationToken);

        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            throw new UnauthorizedBusinessException("Invalid email or password.");
        }

        if (!user.IsActive)
            throw new ForbiddenBusinessException("Account is deactivated. Contact administrator.");

        user.LastLoginDate = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);

        var response = await GenerateAuthResponseAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);
        return response;
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = PasswordHasher.HashToken(request.RefreshToken);
        var storedToken = await _unitOfWork.Users.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);

        if (storedToken == null || !storedToken.IsActive)
            throw new UnauthorizedBusinessException("Invalid or expired refresh token.");

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshHash = PasswordHasher.HashToken(newRefreshToken);

        await _unitOfWork.Users.RevokeRefreshTokenAsync(storedToken, newRefreshHash, cancellationToken);

        var refreshToken = new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = newRefreshHash,
            ExpiresAt = _tokenService.GetRefreshTokenExpiration(),
            CreatedByIp = GetClientIp()
        };

        await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(storedToken.User);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _tokenService.GetAccessTokenExpirationSeconds(),
            User = _mapper.Map<UserProfileDto>(storedToken.User)
        };
    }

    public async Task LogoutAsync(int userId, string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var tokenHash = PasswordHasher.HashToken(refreshToken);
            var storedToken = await _unitOfWork.Users.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);
            if (storedToken != null && storedToken.UserId == userId)
                await _unitOfWork.Users.RevokeRefreshTokenAsync(storedToken, cancellationToken: cancellationToken);
        }
        else
        {
            await _unitOfWork.Users.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        var response = new ForgotPasswordResponse
        {
            Message = "If the email exists, a password reset link has been sent."
        };

        if (user == null) return response;

        var resetToken = _tokenService.GenerateResetToken();
        var resetTokenEntity = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = PasswordHasher.HashToken(resetToken),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        await _unitOfWork.Users.AddPasswordResetTokenAsync(resetTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_appSettings.ExposeResetTokenInDev)
            response.ResetToken = resetToken;

        _logger.LogInformation("Password reset requested for {Email}", request.Email);
        return response;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = PasswordHasher.HashToken(request.Token);
        var resetToken = await _unitOfWork.Users.GetPasswordResetTokenByHashAsync(tokenHash, cancellationToken);

        if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("Invalid or expired reset token.");

        resetToken.IsUsed = true;
        resetToken.User.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
        resetToken.User.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Users.Update(resetToken.User);
        await _unitOfWork.Users.RevokeAllUserRefreshTokensAsync(resetToken.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new BusinessException("Current password is incorrect.");

        user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
        user.UpdatedBy = userId;
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.Users.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.TimeZone = request.TimeZone;
        user.UpdatedBy = userId;
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserProfileDto>(user);
    }

    private async Task<LoginResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = PasswordHasher.HashToken(refreshTokenValue),
            ExpiresAt = _tokenService.GetRefreshTokenExpiration(),
            CreatedByIp = GetClientIp()
        };

        await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = _tokenService.GetAccessTokenExpirationSeconds(),
            User = _mapper.Map<UserProfileDto>(user)
        };
    }

    private string? GetClientIp() =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
