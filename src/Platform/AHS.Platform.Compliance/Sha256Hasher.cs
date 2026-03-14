using System.Security.Cryptography;
using System.Text;
namespace AHS.Platform.Compliance; public static class Sha256Hasher { public static string Hash(string input) { var bytes = Encoding.UTF8.GetBytes(input); return Convert.ToHexString(SHA256.HashData(bytes)); } }
