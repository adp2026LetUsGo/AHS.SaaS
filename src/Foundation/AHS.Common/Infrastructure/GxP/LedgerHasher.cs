// src/Foundation/AHS.Common/Infrastructure/GxP/LedgerHasher.cs
using System.Security.Cryptography;
using System.Text;

namespace AHS.Common.Infrastructure.GxP;

public class LedgerHasher(byte[] hmacKey)
{
    private static string Canonical(LedgerEntry e) =>
        $"{e.Sequence}|{e.TenantId}|{e.AggregateId}|{e.EventType}|{e.PayloadJson}|{e.OccurredAt:O}|{e.PreviousHash}";

    public static string ComputeEntryHash(LedgerEntry e)
    {
        ArgumentNullException.ThrowIfNull(e);
        var raw = Encoding.UTF8.GetBytes(Canonical(e));
        return Convert.ToHexString(SHA256.HashData(raw));
    }

    public static string ComputeHmac(byte[] hmacKey, LedgerEntry e)
    {
        ArgumentNullException.ThrowIfNull(e);
        Span<byte> mac = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(hmacKey, Encoding.UTF8.GetBytes(e.EntryHash), mac);
        return Convert.ToBase64String(mac);
    }

    public bool VerifyEntry(LedgerEntry e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (ComputeEntryHash(e) != e.EntryHash) return false;

        Span<byte> actual   = stackalloc byte[HMACSHA256.HashSizeInBytes];
        Span<byte> expected = stackalloc byte[HMACSHA256.HashSizeInBytes];

        HMACSHA256.HashData(hmacKey, Encoding.UTF8.GetBytes(e.EntryHash), actual);
        if (!Convert.TryFromBase64String(e.HmacSeal, expected, out _)) return false;

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    public bool VerifyChain(IReadOnlyList<LedgerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var expectedPrev = "GENESIS";
        foreach (var entry in entries)
        {
            if (entry.PreviousHash != expectedPrev) return false;
            if (!VerifyEntry(entry)) return false;
            expectedPrev = entry.EntryHash;
        }
        return true;
    }
}
