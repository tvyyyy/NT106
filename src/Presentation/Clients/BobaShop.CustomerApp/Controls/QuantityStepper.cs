using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls;

/// <summary>Ô chọn số lượng: nút trừ, số ở giữa, nút cộng. Dùng ở màn chi tiết món và giỏ hàng.</summary>
[DesignerCategory("Code")]
public class QuantityStepper : Panel
{
    private const int ButtonSize = 34;
    private const int ValueWidth = 44;

    private readonly Button _btnMinus;
    private readonly Label _lblValue;
    private readonly Button _btnPlus;
    private int _value = 1;

    public QuantityStepper()
    {
        BackColor = AppColors.Background;

        _btnMinus = CreateButton("−");
        _btnPlus = CreateButton("+");
        _lblValue = new Label
        {
            AutoSize = false,
            Font = AppFonts.BodyBold,
            ForeColor = AppColors.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
        };

        var buttonSize = UiScale.Px(ButtonSize);
        var valueWidth = UiScale.Px(ValueWidth);
        _btnMinus.SetBounds(0, 0, buttonSize, buttonSize);
        _lblValue.SetBounds(buttonSize, 0, valueWidth, buttonSize);
        _btnPlus.SetBounds(buttonSize + valueWidth, 0, buttonSize, buttonSize);
        Size = new Size((buttonSize * 2) + valueWidth, buttonSize);

        Controls.Add(_btnMinus);
        Controls.Add(_lblValue);
        Controls.Add(_btnPlus);

        _btnMinus.Click += (_, _) => Value = _value - 1;
        _btnPlus.Click += (_, _) => Value = _value + 1;
        UpdateView();
    }

    /// <summary>Số lượng thay đổi do khách bấm hoặc do code gán Value.</summary>
    public event EventHandler? ValueChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Minimum { get; set; } = 1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximum { get; set; } = 99;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, Minimum, Maximum);
            if (_value == clamped)
            {
                return;
            }

            _value = clamped;
            UpdateView();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Gán số lượng mà không phát ValueChanged, dùng khi đồng bộ lại từ giỏ hàng.</summary>
    public void SetValueSilently(int value)
    {
        _value = Math.Clamp(value, Minimum, Maximum);
        UpdateView();
    }

    private static Button CreateButton(string text)
    {
        var button = new Button
        {
            FlatStyle = FlatStyle.Flat,
            Font = AppFonts.Heading,
            Text = text,
            BackColor = AppColors.Surface,
            ForeColor = AppColors.Primary,
            Cursor = Cursors.Hand,
            UseMnemonic = false,
            UseVisualStyleBackColor = false,
            Padding = Padding.Empty,
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = AppColors.InputBorder;
        button.FlatAppearance.MouseOverBackColor = AppColors.Divider;
        button.FlatAppearance.MouseDownBackColor = AppColors.Divider;
        return button;
    }

    private void UpdateView()
    {
        _lblValue.Text = _value.ToString();
        _btnMinus.Enabled = _value > Minimum;
        _btnPlus.Enabled = _value < Maximum;
    }
}
