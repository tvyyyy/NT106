using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace BobaShop.AdminApp
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:5227/") 
        };

        private Label lblTitle;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblEmail;
        private TextBox txtEmail;
        private Button btnRegister;

        public Form1()
        {
            InitializeComponent();
            InitRegisterUI();
        }

        private void InitRegisterUI()
        {
            this.Text = "BobaShop - Đăng Ký Tài Khoản";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "FORM ĐĂNG KÝ TÀI KHOẢN",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(85, 25),
                AutoSize = true
            };

            lblUsername = new Label { Text = "Tên đăng nhập:", Location = new Point(40, 80), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(160, 77), Width = 190 };

            lblEmail = new Label { Text = "Địa chỉ Email:", Location = new Point(40, 120), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(160, 117), Width = 190 };

            btnRegister = new Button
            {
                Text = "Đăng Ký",
                Location = new Point(160, 170),
                Width = 110,
                Height = 35,
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.Click += BtnRegister_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(btnRegister);
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng điền đầy đủ Tên người dùng và Email!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var userRequest = new { Username = username, Email = email };
                var response = await _httpClient.PostAsJsonAsync("api/register", userRequest);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Đã tạo tài khoản '{username}' thành công trên Firebase!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUsername.Clear();
                    txtEmail.Clear();
                }
                else
                {
                    MessageBox.Show("Đăng ký thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}