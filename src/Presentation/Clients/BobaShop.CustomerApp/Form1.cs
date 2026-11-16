using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO; // Cần thêm để đọc file ảnh
using System.Windows.Forms;
using BobaShop.CustomerApp.Controls;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // 1. Cấu hình Form
            this.BackColor = AppColors.Background;
            this.Size = new Size(900, 600);            this.Text = "BobaShop - Trang chủ";

            // 2. Tạo Tiêu đề
            Label lblTitle = new Label
            {
                Text = "🔥 Bán chạy tuần này",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 3. Tạo Khung chứa thẻ món (BẠN BỊ THIẾU ĐOẠN NÀY)
            var testPanel = new RecommendationPanel();
            testPanel.Location = new Point(20, 60);
            this.Controls.Add(testPanel);

            // 4. Lấy đường dẫn tới thư mục Resources
            string resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

            // 5. Tạo dữ liệu giả (Mock Data) với ảnh thật
            // Lưu ý: Đổi tên file "tra_sua.jpg", "tra_dao.jpg"... cho khớp với tên file thật trong thư mục Resources của bạn
            var mockData = new List<(string, decimal, string, Image)>
            {
                ("Trà sữa trân châu", 35000, "Bán chạy nhất", Image.FromFile(Path.Combine(resourcePath, "tra_sua.jpg"))),
                ("Trà đào cam sả", 38000, "Khách hay gọi", Image.FromFile(Path.Combine(resourcePath, "tra_dao.jpg"))),
                ("Matcha đá xay", 45000, "Món mới", Image.FromFile(Path.Combine(resourcePath, "matcha.jpg"))),
                ("Cà phê sữa đá", 29000, "Bán chạy tuần này", Image.FromFile(Path.Combine(resourcePath, "cafe.jpg")))
            };

            // 6. Nạp dữ liệu vào khung
            testPanel.LoadData(mockData);
        }
    }
}