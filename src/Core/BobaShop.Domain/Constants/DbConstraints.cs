namespace BobaShop.Domain.Constants;

public static class DbConstraints
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 150;
    public const int PhoneMaxLength = 15;
    public const int PasswordHashMaxLength = 255;
    public const int UrlMaxLength = 500;
    public const int CodeMaxLength = 50;
    public const int EnumStringMaxLength = 30;
    public const int NoteMaxLength = 500;

    public const int PriceTotalDigits = 18;
    public const int PriceScale = 2;

    public const int RatingMin = 1;
    public const int RatingMax = 5;
}