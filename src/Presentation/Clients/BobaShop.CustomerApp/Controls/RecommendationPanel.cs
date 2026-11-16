using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls
{
    public class RecommendationPanel : UserControl
    {
        private FlowLayoutPanel flpCards;

        public RecommendationPanel()
        {
            this.Size = new Size(800, 280);
            this.BackColor = AppColors.Background;

            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };
            this.Controls.Add(flpCards);
        }

        // CẬP NHẬT Ở ĐÂY: Thêm Image Img vào List
        public void LoadData(List<(string Name, decimal Price, string Reason, Image Img)> mockData)
        {
            flpCards.Controls.Clear();

            if (mockData == null || mockData.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "Chưa có gợi ý nào",
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = AppColors.TextSecondary,
                    Margin = new Padding(20)
                };
                flpCards.Controls.Add(lblEmpty);
                return;
            }

            foreach (var item in mockData)
            {
                ProductMiniCard card = new ProductMiniCard();

                // CẬP NHẬT Ở ĐÂY: Truyền thêm item.Img vào hàm SetData
                card.SetData(item.Name, item.Price, item.Reason, item.Img);

                card.Click += (s, e) =>
                {
                    MessageBox.Show($"Bạn vừa chọn món: {item.Name}", "Thông báo");
                };

                flpCards.Controls.Add(card);
            }
        }
    }
}