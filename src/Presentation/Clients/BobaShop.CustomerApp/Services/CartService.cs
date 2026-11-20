namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Giỏ hàng của phiên đăng nhập hiện tại, chỉ nằm trong bộ nhớ: đăng xuất hoặc tắt ứng dụng là mất.
/// TODO: khi có API đặt hàng thì gửi các dòng trong giỏ lên server lúc bấm Đặt hàng.
/// </summary>
public sealed class CartService
{
    public const int MaxQuantity = 99;

    private readonly List<CartLine> _lines = new();

    /// <summary>Giỏ thay đổi (thêm, xóa, đổi số lượng). Nút giỏ hàng và trang giỏ hàng nghe sự kiện này.</summary>
    public event EventHandler? Changed;

    public IReadOnlyList<CartLine> Lines => _lines;

    /// <summary>Tổng số ly trong giỏ, hiện ở huy hiệu của nút giỏ hàng.</summary>
    public int TotalQuantity => _lines.Sum(line => line.Quantity);

    public decimal Subtotal => _lines.Sum(line => line.LineTotal);

    /// <summary>Thêm một dòng. Nếu giỏ đã có dòng cùng món và cùng lựa chọn thì cộng dồn số lượng.</summary>
    public void Add(CartLine line)
    {
        var existing = _lines.FirstOrDefault(item => item.HasSameChoices(line));
        if (existing is null)
        {
            line.Quantity = Math.Clamp(line.Quantity, 1, MaxQuantity);
            _lines.Add(line);
        }
        else
        {
            existing.Quantity = Math.Min(MaxQuantity, existing.Quantity + line.Quantity);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Đổi số lượng một dòng. Số lượng nhỏ hơn 1 thì bỏ dòng khỏi giỏ.</summary>
    public void SetQuantity(CartLine line, int quantity)
    {
        if (!_lines.Contains(line))
        {
            return;
        }

        if (quantity < 1)
        {
            Remove(line);
            return;
        }

        line.Quantity = Math.Min(MaxQuantity, quantity);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Remove(CartLine line)
    {
        if (_lines.Remove(line))
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Clear()
    {
        if (_lines.Count == 0)
        {
            return;
        }

        _lines.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
