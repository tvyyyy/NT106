using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.ClientNetworkLogic.Controls;

/// <summary>Nút hành động chính, dùng chung cho hai client.</summary>
[DesignerCategory("Code")]
public class PrimaryButton : Button
{
    public PrimaryButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = AppColors.PrimaryDark;
        FlatAppearance.MouseDownBackColor = AppColors.PrimaryDark;
        UseVisualStyleBackColor = false;
        BackColor = AppColors.Primary;
        ForeColor = AppColors.OnPrimary;
        Font = AppFonts.Button;
        Cursor = Cursors.Hand;
        Height = AppSpacing.ButtonHeight;
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        BackColor = Enabled ? AppColors.Primary : AppColors.Divider;
    }
}
