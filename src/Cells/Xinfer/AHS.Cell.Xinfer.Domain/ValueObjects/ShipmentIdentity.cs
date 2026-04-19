namespace AHS.Cell.Xinfer.Domain.ValueObjects;

public readonly record struct ShipmentIdentity(
    string ProductName,
    string ProductCategory,
    string PackagingType,
    string RouteId,
    DateTimeOffset DepartureDate
);

public static class SeasonHelper
{
    public static string GetSeason(DateTimeOffset date) => date.Month switch
    {
        12 or 1 or 2 => "Winter",
        3 or 4 or 5 => "Spring",
        6 or 7 or 8 => "Summer",
        _ => "Autumn"
    };
}
