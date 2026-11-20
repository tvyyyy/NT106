using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls;

/// <summary>
/// Hàng nút lựa chọn dạng thẻ, tự xuống dòng khi hết chỗ. Chọn một (size, đường, đá)
/// hoặc chọn nhiều (topping). Nút đang chọn tô màu chủ đạo, giống hàng danh mục ở trang Thực đơn.
/// </summary>
[DesignerCategory("Code")]
public class ChoiceChips<T> : FlowLayoutPanel
{
    private const int ChipHeight = 38;
    private const int ChipGap = 8;
    private const int ChipPadding = 14;

    private readonly bool _multiSelect;
    private readonly List<Button> _chips = new();
    private readonly List<T> _values = new();
    private readonly HashSet<int> _selectedIndexes = new();

    public ChoiceChips(bool multiSelect)
    {
        _multiSelect = multiSelect;
        FlowDirection = FlowDirection.LeftToRight;
        WrapContents = true;
        BackColor = AppColors.Background;
        Margin = Padding.Empty;
    }

    /// <summary>Khách bấm chọn hoặc bỏ chọn một nút.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Giá trị đang chọn ở chế độ chọn một. Chưa chọn gì thì trả về default.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public T? SelectedValue => _selectedIndexes.Count == 0 ? default : _values[_selectedIndexes.Min()];

    /// <summary>Các giá trị đang chọn, theo thứ tự hiển thị.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<T> SelectedValues => _selectedIndexes.OrderBy(index => index).Select(index => _values[index]).ToList();

    /// <summary>Thay toàn bộ lựa chọn. Lựa chọn cũ bị bỏ.</summary>
    public void SetChoices(IEnumerable<(T Value, string Label)> choices)
    {
        SuspendLayout();
        Controls.Clear();
        foreach (var chip in _chips)
        {
            chip.Dispose();
        }

        _chips.Clear();
        _values.Clear();
        _selectedIndexes.Clear();

        foreach (var (value, label) in choices)
        {
            var index = _chips.Count;
            var chip = new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(UiScale.Px(56), UiScale.Px(ChipHeight)),
                Padding = new Padding(UiScale.Px(ChipPadding), 0, UiScale.Px(ChipPadding), 0),
                Margin = new Padding(0, 0, UiScale.Px(ChipGap), UiScale.Px(ChipGap)),
                FlatStyle = FlatStyle.Flat,
                Font = AppFonts.Body,
                Text = label,
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                UseVisualStyleBackColor = false,
            };
            chip.FlatAppearance.BorderSize = 1;
            chip.Click += (_, _) => Toggle(index);

            _chips.Add(chip);
            _values.Add(value);
            Controls.Add(chip);
        }

        ResumeLayout(true);
        UpdateStyles();
    }

    /// <summary>Chọn đúng một giá trị (chế độ chọn một) hoặc thêm giá trị vào danh sách chọn (chế độ chọn nhiều).</summary>
    public void SelectValue(T value)
    {
        var index = _values.FindIndex(item => EqualityComparer<T>.Default.Equals(item, value));
        if (index < 0)
        {
            return;
        }

        if (!_multiSelect)
        {
            _selectedIndexes.Clear();
        }

        _selectedIndexes.Add(index);
        UpdateStyles();
    }

    public void ClearSelection()
    {
        _selectedIndexes.Clear();
        UpdateStyles();
    }

    private void Toggle(int index)
    {
        if (_multiSelect)
        {
            if (!_selectedIndexes.Remove(index))
            {
                _selectedIndexes.Add(index);
            }
        }
        else
        {
            // Chọn một thì luôn phải có một nút được chọn, bấm lại nút đang chọn không bỏ chọn.
            if (_selectedIndexes.Contains(index))
            {
                return;
            }

            _selectedIndexes.Clear();
            _selectedIndexes.Add(index);
        }

        UpdateStyles();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateStyles()
    {
        for (var i = 0; i < _chips.Count; i++)
        {
            var chip = _chips[i];
            var selected = _selectedIndexes.Contains(i);
            chip.BackColor = selected ? AppColors.Primary : AppColors.Surface;
            chip.ForeColor = selected ? AppColors.OnPrimary : AppColors.TextPrimary;
            chip.FlatAppearance.BorderColor = selected ? AppColors.Primary : AppColors.InputBorder;
            chip.FlatAppearance.MouseOverBackColor = selected ? AppColors.PrimaryDark : AppColors.Divider;
            chip.FlatAppearance.MouseDownBackColor = selected ? AppColors.PrimaryDark : AppColors.Divider;
        }
    }
}
