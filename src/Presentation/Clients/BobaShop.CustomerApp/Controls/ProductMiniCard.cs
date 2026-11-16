using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls
{
    public class ProductMiniCard : UserControl
    {
        private PictureBox picProductImage;
        private Label lblProductName;
        private Label lblPrice;
        private string _reasonText;
        private int _borderRadius = 16;

        public ProductMiniCard()
        {
            this.Size = new Size(180, 250);
            this.BackColor = AppColors.Surface;
            this.Margin = new Padding(10);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;
            InitializeUI();
        }

        private void InitializeUI()
        {
            picProductImage = new PictureBox
            {
                Size = new Size(180, 140),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = AppColors.Background
            };
            this.Controls.Add(picProductImage);

            lblProductName = new Label
            {
                Text = "Trà Sữa Trân Châu",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(15, 150),
                Size = new Size(150, 40),
                AutoEllipsis = true
            };
            this.Controls.Add(lblProductName);

            lblPrice = new Label
            {
                Text = "35.000đ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                Location = new Point(15, 190),
                Size = new Size(150, 25)
            };
            this.Controls.Add(lblPrice);
        }

        // CẬP NHẬT: Thêm Image productImage vào tham số
        public void SetData(string name, decimal price, string reason, Image productImage)
        {
            lblProductName.Text = name;
            lblPrice.Text = price.ToString("N0") + "đ";
            _reasonText = reason;

            // Gán ảnh nếu có truyền vào
            if (productImage != null)
            {
                picProductImage.Image = productImage;
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = GetRoundedRectPath(this.ClientRectangle, _borderRadius);
            this.Region = new Region(path);

            using (Pen pen = new Pen(AppColors.Border, 1))
            {
                e.Graphics.DrawPath(pen, path);
            }

            if (!string.IsNullOrEmpty(_reasonText))
            {
                using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
                using (Brush brush = new SolidBrush(AppColors.Accent))
                {
                    SizeF textSize = e.Graphics.MeasureString(_reasonText, font);
                    Rectangle pillRect = new Rectangle(15, 225, (int)textSize.Width + 16, 20);
                    GraphicsPath pillPath = GetRoundedRectPath(pillRect, 10);
                    e.Graphics.FillPath(brush, pillPath);

                    using (Brush textBrush = new SolidBrush(Color.White))
                    {
                        e.Graphics.DrawString(_reasonText, font, textBrush, pillRect.X + 8, pillRect.Y + 2);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}