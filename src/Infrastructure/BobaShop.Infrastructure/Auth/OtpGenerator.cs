using System.Security.Cryptography;
using BobaShop.Application.Auth;

namespace BobaShop.Infrastructure.Auth;

public class OtpGenerator : IOtpGenerator
{
    public string Generate() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}
