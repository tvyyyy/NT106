using System.ComponentModel;
using System.Windows.Forms;

namespace BobaShop.CustomerApp.Controls
{
    public class PromotionCard : UserControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static int CardHeight { get; set; }

        public PromotionCard(object data)
        {
        }

        public PromotionCard()
        {
        }
    }
}