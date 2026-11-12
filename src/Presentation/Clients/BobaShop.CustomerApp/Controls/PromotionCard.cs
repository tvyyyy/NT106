using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Controls;

/// <summary>Thẻ một khuyến mãi trên trang chủ: tiêu đề, mô tả và mã áp dụng.</summary>
[DesignerCategory("Code")]
public class PromotionCard : Control
{
    /// <summary>Chiều cao thẻ, tính bằng pixel ở 100%.</summary>
    public const int CardHeight = 148;

    private const int CardPadding = 16;
    private const int CodePaddingX = 10;
    private const int CodePaddingY = 4;

    public PromotionCard(Promotion promotion)
    {
        Promotion = promotion;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);
        BackColor = AppColors.Primary;
        TabStop = false;
    }

    public Promotion Promotion { get; }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(AppColors.Primary);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var padding = UiScale.Px(CardPadding);
        var contentWidth = Width - (padding * 2);
        if (contentWidth <= 0)
        {
            return;
        }

        using var textBrush = new SolidBrush(AppColors.OnPrimary);
        using var wrapFormat = new StringFormat
        {
            Trimming = StringTrimming.EllipsisWord,
            FormatFlags = StringFormatFlags.LineLimit,
        };

        // Mã khuyến mãi nằm trong một ô trắng ở góc dưới trái.
        var codeText = $"Mã: {Promotion.Code}";
        var codeSize = graphics.MeasureString(codeText, AppFonts.BodyBold);
        var codeBounds = new RectangleF(
            padding,
            Height - padding - codeSize.Height - (UiScale.Px(CodePaddingY) * 2),
            codeSize.Width + (UiScale.Px(CodePaddingX) * 2),
            codeSize.Height + (UiScale.Px(CodePaddingY) * 2));

        var titleHeight = (float)Math.Ceiling(AppFonts.Heading.GetHeight(graphics));
        var titleBounds = new RectangleF(padding, padding, contentWidth, titleHeight + 2F);
        var descriptionTop = titleBounds.Bottom + UiScale.Px(4);
        var descriptionBounds = new RectangleF(
            padding,
            descriptionTop,
            contentWidth,
            Math.Max(0F, codeBounds.Top - descriptionTop - UiScale.Px(8)));

        graphics.DrawString(Promotion.Title, AppFonts.Heading, textBrush, titleBounds, wrapFormat);
        graphics.DrawString(Promotion.Description, AppFonts.Body, textBrush, descriptionBounds, wrapFormat);

        using var codeBackground = new SolidBrush(AppColors.Surface);
        using var codeBrush = new SolidBrush(AppColors.Primary);
        graphics.FillRectangle(codeBackground, codeBounds);
        graphics.DrawString(
            codeText,
            AppFonts.BodyBold,
            codeBrush,
            codeBounds.Left + UiScale.Px(CodePaddingX),
            codeBounds.Top + UiScale.Px(CodePaddingY));
    }
}
