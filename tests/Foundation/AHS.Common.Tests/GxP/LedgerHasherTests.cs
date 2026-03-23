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
    public void ComputeEntryHash_is_deterministic()
    {
        var entry = BuildEntry();
        _hasher.ComputeEntryHash(entry).Should().Be(_hasher.ComputeEntryHash(entry));
    }

    [Fact]
    public void VerifyChain_detects_tampering()
    {
        var entries = BuildValidChain(5).ToList();
        entries[2] = entries[2] with { PayloadJson = """{"tampered":true}""" };
        _hasher.VerifyChain(entries).Should().BeFalse();
    }

    [Fact]
    public void VerifyChain_passes_for_valid_chain()
    {
        var entries = BuildValidChain(5).ToList();
        _hasher.VerifyChain(entries).Should().BeTrue();
    }

    private LedgerEntry BuildEntry() => new()
    {
        Sequence = 1,
        TenantId = Guid.NewGuid(),
        AggregateId = Guid.NewGuid(),
        EventType = "TestEvent",
        PayloadJson = "{}",
        OccurredAt = DateTimeOffset.UtcNow
    };

    private IEnumerable<LedgerEntry> BuildValidChain(int count)
    {
        var entries = new List<LedgerEntry>();
        var prevHash = "GENESIS";
        for (int i = 0; i < count; i++)
        {
            var entry = BuildEntry() with { Sequence = i + 1, PreviousHash = prevHash };
            var hash = _hasher.ComputeEntryHash(entry);
            entry = entry with { EntryHash = hash };
            var seal = _hasher.ComputeHmac(entry);
            entry = entry with { HmacSeal = seal };
            entries.Add(entry);
            prevHash = hash;
        }
        return entries;
    }
}
