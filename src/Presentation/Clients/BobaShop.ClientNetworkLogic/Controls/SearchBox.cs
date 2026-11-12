using System.ComponentModel;

namespace BobaShop.ClientNetworkLogic.Controls;

/// <summary>Ô tìm kiếm dùng chung. Form nghe sự kiện TextChanged để lọc danh sách.</summary>
[DesignerCategory("Code")]
public class SearchBox : TextField
{
    public SearchBox()
    {
        PlaceholderText = "Tìm kiếm";
        MaxLength = 100;
    }
}
