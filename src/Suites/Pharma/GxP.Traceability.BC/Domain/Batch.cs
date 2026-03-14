using System;

namespace AHS.Suites.Pharma.GxP.Traceability.BC.Domain;

public class Batch
{
    public Guid Id { get; private set; }
    public string ProductCode { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Batch(string productCode)
    {
        Id = Guid.NewGuid();
        ProductCode = productCode;
        CreatedAt = DateTime.UtcNow;
    }
}
