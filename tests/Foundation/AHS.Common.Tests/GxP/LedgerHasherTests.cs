// tests/Foundation/AHS.Common.Tests/GxP/LedgerHasherTests.cs
using System.Security.Cryptography;
using System.Text;
using AHS.Common.Infrastructure.GxP;
using FluentAssertions;
using Xunit;

namespace AHS.Common.Tests.GxP;

public class LedgerHasherTests
{
    private static readonly byte[] TestKey =
        SHA256.HashData(Encoding.UTF8.GetBytes("AHS_TEST_KEY_NOT_FOR_PRODUCTION"));

    private readonly LedgerHasher _hasher = new(TestKey);

    [Fact]
    public void ComputeEntryHashIsDeterministic()
    {
        var entry = BuildEntry();
        LedgerHasher.ComputeEntryHash(entry).Should().Be(LedgerHasher.ComputeEntryHash(entry));
    }

    [Fact]
    public void VerifyChainDetectsTampering()
    {
        var entries = BuildValidChain(5);
        entries[2] = entries[2] with { PayloadJson = """{"tampered":true}""" };
        _hasher.VerifyChain(entries).Should().BeFalse();
    }

    [Fact]
    public void VerifyChainPassesForValidChain()
    {
        var entries = BuildValidChain(5);
        _hasher.VerifyChain(entries).Should().BeTrue();
    }

    private static LedgerEntry BuildEntry() => new()
    {
        Sequence = 1,
        TenantId = Guid.NewGuid(),
        AggregateId = Guid.NewGuid(),
        EventType = "TestEvent",
        PayloadJson = "{}",
        OccurredAt = DateTimeOffset.UtcNow
    };

    private static List<LedgerEntry> BuildValidChain(int count)
    {
        var entries = new List<LedgerEntry>();
        var prevHash = "GENESIS";
        for (int i = 0; i < count; i++)
        {
            var entry = BuildEntry() with { Sequence = i + 1, PreviousHash = prevHash };
            var hash = LedgerHasher.ComputeEntryHash(entry);
            entry = entry with { EntryHash = hash };
            var seal = LedgerHasher.ComputeHmac(TestKey, entry);
            entry = entry with { HmacSeal = seal };
            entries.Add(entry);
            prevHash = hash;
        }
        return entries;
    }
}
