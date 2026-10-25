
using System;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace BobaShop.CustomerApp
{
    public partial class CartItemControl : UserControl
    {
        // Khai báo sự kiện để thông báo cho FormCart cập nhật lại Tổng tiền
        public event EventHandler? OnItemUpdated;
        public event EventHandler? OnItemDeleted;

        public CartItem ItemData { get; private set; } = new CartItem();

        public CartItemControl()
        {
            InitializeComponent();

            // Khóa viền 2 nút tăng/giảm bằng code
            if (btnMinus != null)
            {
                btnMinus.FlatStyle = FlatStyle.Flat;
                btnMinus.FlatAppearance.BorderSize = 0;
            }
            if (btnPlus != null)
            {
                btnPlus.FlatStyle = FlatStyle.Flat;
                btnPlus.FlatAppearance.BorderSize = 0;
            }
        }

        // Hàm nạp dữ liệu từ Model lên thẻ món
        public void SetData(CartItem item)
        {
            ItemData = item;

            lblProductName.Text = item.ProductName;
            lblDetails.Text = $"Size: {item.Size} | Đường: {item.SugarLevel} | Đá: {item.IceLevel}";

            // Cập nhật danh sách Topping
            if (item.Toppings != null && item.Toppings.Count > 0)
            {
                lblToppings.Text = "Topping: " + string.Join(", ", item.Toppings.Select(t => $"{t.ToppingName} (+{t.Price / 1000}k)"));
            }
            else
            {
                lblToppings.Text = "Topping: Không";
            }

            txtNote.Text = item.Note;
            chkSelect.Checked = item.IsSelected;

            UpdateDisplay();
        }

        // Cập nhật lại hiển thị số lượng và đơn giá / tổng tiền
        private void UpdateDisplay()
        {
            lblQuantity.Text = ItemData.Quantity.ToString("D2"); // Hiển thị 01, 02...
            lblUnitPrice.Text = $"{ItemData.UnitPrice:N0}đ";
            lblTotalPrice.Text = $"{ItemData.TotalPrice:N0}đ";
        }

        // Bấm nút Tăng (+)
        private void btnPlus_Click(object sender, EventArgs e)
        {
            ItemData.Quantity++;
            UpdateDisplay();
            OnItemUpdated?.Invoke(this, EventArgs.Empty);
        }

        // Bấm nút Giảm (-)
        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (ItemData.Quantity > 1)
            {
                ItemData.Quantity--;
                UpdateDisplay();
                OnItemUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        // Bấm nút Xóa (✕)
        private void btnDelete_Click(object sender, EventArgs e)
        {
            OnItemDeleted?.Invoke(this, EventArgs.Empty);
        }

        // Tick chọn / bỏ chọn món
        private void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            ItemData.IsSelected = chkSelect.Checked;
            OnItemUpdated?.Invoke(this, EventArgs.Empty);
        }

        // Lưu ghi chú khi người dùng gõ
        private void txtNote_TextChanged(object sender, EventArgs e)
        {
            ItemData.Note = txtNote.Text;
        }

        private void picProduct_Click(object sender, EventArgs e)
        {

        }
    }
}