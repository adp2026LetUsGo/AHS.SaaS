using System.Security.Cryptography;
using System.Text;

namespace AHS.Platform.Compliance;

public static class ComplianceService
{
    public static string HashSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
