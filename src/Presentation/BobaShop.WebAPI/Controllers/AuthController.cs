using BobaShop.Application.Auth;
using BobaShop.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 100 || !AuthValidation.ValidEmail(request.Email) || !AuthValidation.ValidPhone(request.Phone) || !AuthValidation.StrongPassword(request.Password))
            return BadRequest(ApiResponse<object>.Fail("Thông tin đăng ký không hợp lệ."));
        var result = await auth.RegisterAsync(request, ct);
        if (result is null) return Conflict(ApiResponse<object>.Fail("Email hoặc số điện thoại đã được sử dụng."));
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RegisterResponse>.Ok("Đăng ký thành công.", result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        if (!AuthValidation.ValidEmail(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(ApiResponse<object>.Fail("Thông tin đăng nhập không hợp lệ."));
        var result = await auth.LoginAsync(request, ct);
        return result is null
            ? Unauthorized(ApiResponse<object>.Fail("Email hoặc mật khẩu không đúng."))
            : Ok(ApiResponse<LoginResponse>.Ok("Đăng nhập thành công.", result));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        if (AuthValidation.ValidEmail(request.Email)) await auth.ForgotPasswordAsync(request.Email, ct);
        return Ok(ApiResponse<object>.Ok("Nếu email tồn tại, mã xác nhận sẽ được gửi.", null));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        if (!AuthValidation.ValidEmail(request.Email) || request.Otp.Length != 6 || !request.Otp.All(char.IsAsciiDigit) || !AuthValidation.StrongPassword(request.NewPassword))
            return BadRequest(ApiResponse<object>.Fail("Thông tin đặt lại mật khẩu không hợp lệ."));
        var success = await auth.ResetPasswordAsync(request, ct);
        return success ? Ok(ApiResponse<object>.Ok("Đặt lại mật khẩu thành công.", null)) : BadRequest(ApiResponse<object>.Fail("Mã xác nhận không hợp lệ hoặc đã hết hạn."));
    }
}
