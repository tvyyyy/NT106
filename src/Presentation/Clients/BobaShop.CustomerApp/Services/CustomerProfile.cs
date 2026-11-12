namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Hồ sơ khách hàng hiển thị ở màn Cá nhân. Trường nào chưa có dữ liệu thì để trống hoặc null,
/// giao diện sẽ hiện "Chưa cập nhật".
/// TODO: bảng Users hiện chỉ có họ tên, email, số điện thoại và ngày tạo. Ngày sinh và hạng thành viên
/// chưa có trong ERD và chưa có API, nên hai trường này mới chỉ có ở bản giả.
/// </summary>
public sealed record CustomerProfile(
    string FullName,
    string Email,
    string Phone,
    DateOnly? DateOfBirth,
    DateTime? RegisteredAt,
    string TierName);
