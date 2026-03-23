---
name: sha256-cryptographic-sealing
description: >
  Expert guidance on SHA-256 hashing, cryptographic sealing, HMAC-SHA256, digital signatures,
  and tamper-evident document/data patterns in C#. Use this skill whenever the user mentions
  SHA-256, SHA256, HMAC, cryptographic hash, data integrity, tamper-evident, digital seal,
  content fingerprint, signed payload, IncrementalHash, hash chain, Merkle tree,
  or cryptographic verification of records or documents.
  Trigger on: SHA256, SHA-256, HMAC, cryptographic seal, hash chain, tamper proof,
  digital fingerprint, IncrementalHash, signing payload, integrity verification.
---

# SHA-256 Cryptographic Sealing in C#

## When to Use What

| Goal | Use |
|---|---|
| Data fingerprint (no secret) | `SHA256.HashData()` |
| Tamper detection with shared secret | `HMACSHA256` |
| Non-repudiation / signatures | `RSA` + `SHA256` or `ECDsa` |
| Streaming / large files | `IncrementalHash` |
| Hash chain / audit log | Chained `SHA256` |
| Password hashing | **NOT SHA-256** → use `Argon2`, `BCrypt`, or `PBKDF2` |

---

## 0. AOT / Trim Setup Requerido

> Todos los ejemplos de este skill usan `System.Text.Json`. En **Native AOT** o con **trimming**, registra tus tipos en un `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(SealedPayload))]
[JsonSerializable(typeof(OrderDto))]
[JsonSerializable(typeof(SealedRecord))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonContext : JsonSerializerContext { }
```

---

```csharp
using System.Security.Cryptography;

// One-shot (preferred for in-memory data)
byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
byte[] hash = SHA256.HashData(data);
string hex  = Convert.ToHexString(hash); // "DFFD6021..."

// Span-based (zero allocation)
Span<byte> hashBuffer = stackalloc byte[SHA256.HashSizeInBytes]; // 32 bytes
SHA256.HashData(data.AsSpan(), hashBuffer);

// File hashing (streaming)
await using var fs = File.OpenRead("document.pdf");
byte[] fileHash = await SHA256.HashDataAsync(fs);
```

---

## 2. HMAC-SHA256 — Sealing with a Shared Secret

```csharp
// HMAC = Hash-based Message Authentication Code
// Proves BOTH integrity AND authenticity (requires secret key)

public static class HmacSealer
{
    // Key must be at least 32 bytes; store in Azure Key Vault / secrets, never hardcoded
    private const int KeySizeBytes = 32;

    public static byte[] Seal(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        Span<byte> mac = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(key, data, mac);
        return mac.ToArray();
    }

    public static bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key, ReadOnlySpan<byte> expectedMac)
    {
        Span<byte> actualMac = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(key, data, actualMac);

        // Constant-time comparison — prevents timing attacks
        return CryptographicOperations.FixedTimeEquals(actualMac, expectedMac);
    }

    public static byte[] GenerateKey()
    {
        var key = new byte[KeySizeBytes];
        RandomNumberGenerator.Fill(key);
        return key;
    }
}

// Usage
byte[] key  = HmacSealer.GenerateKey();  // store securely
byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(record, AppJsonContext.Default.SealedRecord));
byte[] seal = HmacSealer.Seal(data, key);

bool valid = HmacSealer.Verify(data, key, seal); // true
```

---

## 3. Sealed Payload Pattern (JSON + Base64)

```csharp
// Common in webhooks, signed tokens, audit records
public record SealedPayload(string Data, string Mac, string Algorithm = "HMACSHA256");

// ✅ AOT-safe: usar overloads tipados del JsonSerializerContext, no GetTypeInfo(typeof(T))
// Para tipos conocidos en compilación: AppJsonContext.Default.MyType
// Para el patrón genérico Open<T> / Seal<T> en AOT, usa una clase concreta por tipo
// o un TypedJsonSealer<T> donde T está constrained a un JsonTypeInfo<T> registrado.

public class OrderSealer(byte[] secretKey) : PayloadSealer<OrderDto>(secretKey,
    AppJsonContext.Default.OrderDto) { }

public class PayloadSealer<T>(byte[] secretKey, JsonTypeInfo<T> typeInfo)
{
    public SealedPayload Seal(T value)
    {
        // ✅ JsonTypeInfo<T> resuelto en compile-time — AOT safe
        var json = JsonSerializer.Serialize(value, typeInfo);
        var dataBytes = Encoding.UTF8.GetBytes(json);

        Span<byte> mac = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(secretKey, dataBytes, mac);

        return new SealedPayload(
            Data: Convert.ToBase64String(dataBytes),
            Mac:  Convert.ToBase64String(mac));
    }

    public T? Open(SealedPayload payload)
    {
        var dataBytes  = Convert.FromBase64String(payload.Data);
        var expectedMac = Convert.FromBase64String(payload.Mac);

        Span<byte> actualMac = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(secretKey, dataBytes, actualMac);

        if (!CryptographicOperations.FixedTimeEquals(actualMac, expectedMac))
            throw new CryptographicException("Seal verification failed — data has been tampered.");

        // ✅ JsonTypeInfo<T> — sin reflection
        return JsonSerializer.Deserialize(Encoding.UTF8.GetString(dataBytes), typeInfo);
    }
}
```

---

## 4. RSA + SHA-256 Digital Signature (Non-repudiation)

```csharp
public class DocumentSigner
{
    // Sign with private key
    public static byte[] Sign(byte[] document, RSA privateKey)
        => privateKey.SignData(document, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    // Verify with public key (can be distributed)
    public static bool Verify(byte[] document, byte[] signature, RSA publicKey)
        => publicKey.VerifyData(document, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    // Generate key pair (do once, store private key in KeyVault)
    public static (RSA privateKey, string publicKeyXml) GenerateKeyPair()
    {
        var rsa = RSA.Create(keySizeInBits: 4096);
        return (rsa, rsa.ExportRSAPublicKeyPem());
    }
}
```

---

## 5. ECDsa + SHA-256 (Smaller signatures, faster)

```csharp
public class EcDocumentSigner
{
    public static byte[] Sign(byte[] data, ECDsa privateKey)
        => privateKey.SignData(data, HashAlgorithmName.SHA256);

    public static bool Verify(byte[] data, byte[] signature, ECDsa publicKey)
        => publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256);

    // P-256 curve — 32-byte keys, 64-byte signatures (vs RSA 512 bytes)
    public static ECDsa CreateKey() => ECDsa.Create(ECCurve.NamedCurves.nistP256);
}
```

---

## 6. IncrementalHash — Large Files / Streaming

```csharp
public static async Task<byte[]> HashLargeFileAsync(string path, CancellationToken ct)
{
    using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,
        FileShare.Read, bufferSize: 65536, useAsync: true);

    var buffer = new byte[65536];
    int bytesRead;
    while ((bytesRead = await fs.ReadAsync(buffer, ct)) > 0)
        hasher.AppendData(buffer.AsSpan(0, bytesRead));

    return hasher.GetCurrentHash(); // or GetHashAndReset() to reuse
}

// HMAC version
public static async Task<byte[]> HmacLargeFileAsync(string path, byte[] key, CancellationToken ct)
{
    using var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key);
    // same pattern as above with hmac.AppendData(...)
    return hmac.GetCurrentHash();
}
```

---

## 7. Hash Chain — Tamper-Evident Audit Log

```csharp
// Each entry's hash includes the previous hash → any tampering breaks the chain
public record AuditEntry(
    long   Sequence,
    string EventType,
    string Payload,
    string PreviousHash,  // hash of prior entry
    string EntryHash);    // SHA256(Sequence + EventType + Payload + PreviousHash)

public class AuditChain
{
    private string _lastHash = "GENESIS"; // known anchor

    public AuditEntry Append(string eventType, string payload)
    {
        var seq = _nextSequence++;
        var raw = $"{seq}|{eventType}|{payload}|{_lastHash}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        var entry = new AuditEntry(seq, eventType, payload, _lastHash, hash);
        _lastHash = hash;
        return entry;
    }

    public static bool VerifyChain(IReadOnlyList<AuditEntry> entries)
    {
        string expectedPrev = "GENESIS";
        foreach (var entry in entries)
        {
            if (entry.PreviousHash != expectedPrev) return false;

            var raw  = $"{entry.Sequence}|{entry.EventType}|{entry.Payload}|{entry.PreviousHash}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
            if (hash != entry.EntryHash) return false;

            expectedPrev = entry.EntryHash;
        }
        return true;
    }

    private long _nextSequence = 0;
}
```

---

## 8. Merkle Tree — Partial Verification

```csharp
// Hash binary tree — proves one item is in a set without revealing all items
public static class MerkleTree
{
    public static string BuildRoot(IReadOnlyList<byte[]> leaves)
    {
        var level = leaves.Select(l => SHA256.HashData(l)).ToList();

        while (level.Count > 1)
        {
            var next = new List<byte[]>();
            for (int i = 0; i < level.Count; i += 2)
            {
                var left  = level[i];
                var right = i + 1 < level.Count ? level[i + 1] : left; // duplicate last if odd
                var combined = new byte[64];
                left.CopyTo(combined, 0);
                right.CopyTo(combined, 32);
                next.Add(SHA256.HashData(combined));
            }
            level = next;
        }

        return Convert.ToHexString(level[0]);
    }
}
```

---

## 9. Security Checklist

| Rule | Why |
|---|---|
| Use `CryptographicOperations.FixedTimeEquals` | Prevents timing attacks |
| Never compare hashes with `==` or `SequenceEqual` | Timing attack vector |
| Never use SHA-256 for passwords | Not a KDF — use PBKDF2/Argon2/BCrypt |
| Always use `RSASignaturePadding.Pss` not `Pkcs1` | PKCS1 is malleable |
| Store keys in Azure Key Vault / DPAPI | Never hardcode keys |
| Use `RandomNumberGenerator.Fill()` for salts/keys | Cryptographically secure |
| Validate hash length before `FixedTimeEquals` | Short-circuit if lengths differ (safe) |
