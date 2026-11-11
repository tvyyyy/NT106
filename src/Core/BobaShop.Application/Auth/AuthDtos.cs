namespace BobaShop.Application.Auth;

public record RegisterRequest(string FullName, string Phone, string Email, string Password);
public record RegisterResponse(int UserId, string FullName, string Phone, string Email);
public record LoginRequest(string Email, string Password);
public record LoginUserResponse(int UserId, string FullName, string Email, string Role);
public record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt, LoginUserResponse User);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Otp, string NewPassword);
