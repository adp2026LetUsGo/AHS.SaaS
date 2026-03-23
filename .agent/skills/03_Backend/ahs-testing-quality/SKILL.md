---
name: ahs-testing-quality
description: >
  Expert guidance on the AHS testing strategy: xUnit, FluentAssertions, NSubstitute,
  WebApplicationFactory integration tests, NetArchTest architecture tests, and Reqnroll BDD.
  Use this skill whenever the user mentions unit tests, integration tests, architecture tests,
  BDD, Reqnroll, xUnit, FluentAssertions, NSubstitute, WebApplicationFactory, NetArchTest,
  test doubles, mocking, GxP test validation, cold chain test scenarios, Oracle test,
  GxP audit test, electronic signature test, or Clean Architecture enforcement.
  Trigger on: xUnit, FluentAssertions, NSubstitute, WebApplicationFactory, NetArchTest,
  Reqnroll, BDD, unit test, integration test, architecture test, mock, stub, fake,
  GxP test, cold chain scenario, Oracle test, ledger test, tenant isolation test.
---

# AHS Testing & Quality — C# 14 / .NET 10

## Stack

```xml
<!-- AHS.Tests.Unit -->
<PackageReference Include="xunit"                        Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio"    Version="2.*" />
<PackageReference Include="FluentAssertions"             Version="7.*" />
<PackageReference Include="NSubstitute"                  Version="5.*" />
<PackageReference Include="NSubstitute.Analyzers.CSharp" Version="1.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk"       Version="17.*" />

<!-- AHS.Tests.Integration -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
<PackageReference Include="Testcontainers.PostgreSql"    Version="3.*" />
<PackageReference Include="Testcontainers.Redis"         Version="3.*" />
<PackageReference Include="Respawn"                      Version="6.*" />

<!-- AHS.Tests.Architecture -->
<PackageReference Include="NetArchTest.eNet"             Version="1.*" />

<!-- AHS.Tests.BDD -->
<PackageReference Include="Reqnroll"                     Version="2.*" />
<PackageReference Include="Reqnroll.xUnit"               Version="2.*" />
```

---

## 1. Unit Tests — xUnit + FluentAssertions + NSubstitute

### Logistics Oracle (REQ-001) — P99 < 10ms

```csharp
public class LogisticsOracleTests
{
    private readonly IRouteRepository    _routes   = Substitute.For<IRouteRepository>();
    private readonly ICarrierRepository  _carriers = Substitute.For<ICarrierRepository>();
    private readonly IWeatherService     _weather  = Substitute.For<IWeatherService>();
    private readonly ILogger<LogisticsOracle> _log = Substitute.For<ILogger<LogisticsOracle>>();

    private LogisticsOracle Sut => new(_routes, _carriers, _weather, _log);

    // Blueprint REQ-001: Passive insulation MUST add +15% penalty
    [Fact]
    public async Task Passive_insulation_adds_15_percent_base_penalty()
    {
        var activeReq  = BuildRequest(insulation: "Active",  routeCategory: RouteRiskCategory.Low);
        var passiveReq = BuildRequest(insulation: "Passive", routeCategory: RouteRiskCategory.Low);

        var activeResult  = await Sut.CalculateAsync(activeReq,  default);
        var passiveResult = await Sut.CalculateAsync(passiveReq, default);

        passiveResult.Risk.PassivePenalty.Should().Be(15.0);
        passiveResult.Risk.FinalRiskScore.Should().Be(
            activeResult.Risk.FinalRiskScore + 15.0,
            because: "Passive insulation MUST add exactly 15% per Blueprint REQ-001");
    }

    // XAI DNA must have exactly 14 factors — Blueprint contract
    [Fact]
    public async Task XAI_DNA_must_contain_exactly_14_factors()
    {
        var result = await Sut.CalculateAsync(BuildRequest(), default);

        result.Dna.Factors.Should().HaveCount(14,
            because: "Blueprint V2.0 defines a fixed 14-point diagnostic DNA");

        result.Dna.Factors.Select(f => f.Index)
            .Should().BeEquivalentTo(Enumerable.Range(1, 14),
                because: "Factor indices must be sequential 1–14");
    }

    // Pessimistic TTF must always be <= Physical TTF
    [Theory]
    [InlineData(0.0,   24.0)]
    [InlineData(50.0,  24.0)]
    [InlineData(100.0, 24.0)]
    public async Task Pessimistic_TTF_never_exceeds_physical_TTF(double riskScore, double physTtf)
    {
        var req = BuildRequest(insulation: "Active") with { PhysicalTtfHours = physTtf };
        var result = await Sut.CalculateAsync(req, default);

        result.Ttf.PessimisticTtfHours.Should().BeLessThanOrEqualTo(physTtf,
            because: "Pessimistic TTF can only reduce, never exceed, the physical TTF");
    }

    // Oracle result requires immediate action when risk >= 75 OR safe window < 4h
    [Fact]
    public async Task RequiresImmediateAction_when_critical_risk()
    {
        var req = BuildRequest(
            insulation:    "Passive",
            routeCategory: RouteRiskCategory.Critical,
            carrierReliability: 0.3,
            physTtf: 3.0);  // 3h physical TTF → safe window will be < 4h

        var result = await Sut.CalculateAsync(req, default);

        result.RequiresImmediateAction.Should().BeTrue();
        result.Ttf.RiskBand.Should().BeOneOf("Red", "Critical");
    }

    // Performance gate — Oracle P99 < 10ms (Capa 5 requirement)
    [Fact]
    public async Task Oracle_calculation_completes_within_10ms_P99()
    {
        var req = BuildRequest();
        var timings = new List<double>();

        // Warm up
        for (int i = 0; i < 10; i++) await Sut.CalculateAsync(req, default);

        // Measure 100 runs
        for (int i = 0; i < 100; i++)
        {
            var sw = Stopwatch.GetTimestamp();
            await Sut.CalculateAsync(req, default);
            timings.Add(Stopwatch.GetElapsedTime(sw).TotalMilliseconds);
        }

        var p99 = timings.OrderBy(t => t).ElementAt(98); // 99th percentile
        p99.Should().BeLessThan(10.0,
            because: "Blueprint Capa 5: Oracle P99 latency must be < 10ms");
    }

    private static OracleRequest BuildRequest(
        string insulation = "Active",
        RouteRiskCategory routeCategory = RouteRiskCategory.Medium,
        double carrierReliability = 0.90,
        double physTtf = 48.0) => new()
    {
        RouteId              = "R-EU-001",
        OriginCountry        = "ES",
        DestinationCountry   = "DE",
        RouteCategory        = routeCategory,
        CarrierId            = Guid.NewGuid(),
        CarrierReliability   = carrierReliability,
        CarrierIncidents12M  = 0,
        ForecastMaxCelsius   = 12.0,
        ForecastMinCelsius   = 5.0,
        ForecastHumidityPct  = 60.0,
        InsulationType       = insulation,
        CargoType            = "Pharma",
        SetpointMinCelsius   = 2.0,
        SetpointMaxCelsius   = 8.0,
        PhysicalTtfHours     = physTtf,
    };
}
```

### GxP Ledger — Electronic Signature & Reason for Change

```csharp
public class ElectronicSignatureTests
{
    [Fact]
    public void Command_without_reason_for_change_throws()
    {
        // FDA 21 CFR Part 11: every write command must carry a reason
        var act = () => new SealShipmentCommand(
            ShipmentId:      Guid.NewGuid(),
            FinalStatus:     "Compliant",
            QualityDecision: "Accept",
            SignedById:      Guid.NewGuid(),
            SignedByName:    "Dr. Ana López",
            ReasonForChange: "");  // ← empty reason

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ReasonForChange*",
                because: "FDA 21 CFR Part 11 §11.10(e): audit trail requires reason for each change");
    }

    [Fact]
    public void Electronic_signature_is_sealed_in_ledger_entry()
    {
        var cmd = new SealShipmentCommand(
            ShipmentId:      Guid.NewGuid(),
            FinalStatus:     "Compliant",
            QualityDecision: "Accept",
            SignedById:      Guid.NewGuid(),
            SignedByName:    "Dr. Ana López",
            ReasonForChange: "End of shipment — MKT within limits. Approved per SOP-QC-042.");

        var entry = LedgerEntryFactory.From(cmd, previousHash: "GENESIS", sequence: 1);

        entry.ActorName.Should().Be("Dr. Ana López");
        entry.EntryHash.Should().NotBeNullOrEmpty();
        using var aes = entry.Should().ContainReasonForChange(cmd.ReasonForChange);
    }

    [Fact]
    public void Hash_chain_detects_tampering()
    {
        var hasher = new LedgerHasher(TestKeys.HmacKey);
        var entries = BuildValidChain(hasher, count: 5).ToList();

        // Tamper entry #3
        entries[2] = entries[2] with { PayloadJson = """{"tampered":true}""" };

        hasher.VerifyChain(entries).Should().BeFalse(
            because: "SHA256 hash chain must detect any single-byte modification");
    }
}
```

### Tenant Isolation — Critical Security Test

```csharp
public class TenantIsolationTests
{
    [Fact]
    public async Task TenantA_cannot_read_TenantB_ledger_entries()
    {
        var tenantA = new TenantContext { TenantId = Guid.NewGuid(), TenantSlug = "pharma-a" };
        var tenantB = new TenantContext { TenantId = Guid.NewGuid(), TenantSlug = "pharma-b" };

        var repoA = BuildRepository(tenantA);
        var repoB = BuildRepository(tenantB);

        // Tenant B writes an entry
        var shipmentId = Guid.NewGuid();
        await repoB.AppendAsync(shipmentId, [BuildEvent(tenantB.TenantId)], default);

        // Tenant A tries to read it — must get nothing (RLS enforced)
        var events = await repoA.LoadAsync(shipmentId, default);

        events.Should().BeEmpty(
            because: "PostgreSQL RLS must prevent cross-tenant data access at DB level");
    }
}
```

### Excursion Detector — Cold Chain Domain Logic

```csharp
public class ExcursionDetectorTests
{
    private static TemperatureZone PharmaZone => ZoneProfiles.Pharmaceutical2to8;

    [Fact]
    public void No_alarm_within_grace_period()
    {
        var detector = new ExcursionDetector(PharmaZone, Substitute.For<ILogger<ExcursionDetector>>());
        var reading = BuildReading(celsius: 12.0, offset: TimeSpan.Zero);  // 4°C over max

        var result = detector.Process(reading);

        result.Type.Should().Be(DetectionResultType.Pending,
            because: $"Alarm delay is {PharmaZone.AlarmDelay.TotalMinutes}min — no alarm within grace period");
    }

    [Fact]
    public void Excursion_fires_after_alarm_delay()
    {
        var detector = new ExcursionDetector(PharmaZone, Substitute.For<ILogger<ExcursionDetector>>());
        var start = DateTimeOffset.UtcNow;

        // Feed readings past the alarm delay
        DetectionResultType? lastType = null;
        for (int min = 0; min <= 16; min++)
        {
            var r = BuildReading(celsius: 12.0, timestamp: start.AddMinutes(min));
            lastType = detector.Process(r).Type;
        }

        lastType.Should().Be(DetectionResultType.ExcursionStarted,
            because: "Excursion must fire after alarm delay is exceeded");
    }

    [Fact]
    public void MKT_within_specification_for_valid_readings()
    {
        var readings = Enumerable.Range(0, 100)
            .Select(i => 4.0 + Math.Sin(i * 0.1) * 1.5)  // oscillates 2.5–5.5°C
            .ToList();

        var mkt = MeanKineticTemperature.Calculate(readings);

        mkt.Should().BeInRange(2.0, 8.0,
            because: "MKT of readings within setpoint must itself be within setpoint");
    }

    private static TemperatureReading BuildReading(double celsius,
        DateTimeOffset? timestamp = null, TimeSpan? offset = null) =>
        new(Guid.NewGuid(), "zone-01",
            timestamp ?? DateTimeOffset.UtcNow + (offset ?? TimeSpan.Zero),
            celsius, SensorStatus.Ok);
}
```

---

## 2. Integration Tests — WebApplicationFactory

```csharp
// AHS.Tests.Integration/AhsWebAppFactory.cs
public class AhsWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("ahs_test")
        .WithUsername("ahs_test")
        .WithPassword("ahs_test_pw")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DB with Testcontainers PostgreSQL
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o =>
                o.UseNpgsql(_postgres.GetConnectionString()));

            // Replace real Redis with Testcontainers Redis
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddStackExchangeRedisCache(o =>
                o.Configuration = _redis.GetConnectionString());

            // Replace real Service Bus with in-memory fake
            services.AddSingleton<ISensorEventPublisher, InMemorySensorEventPublisher>();

            // Replace real Key Vault with test keys
            services.AddSingleton<LedgerKeyProvider>(_ =>
                new LedgerKeyProvider(TestKeys.HmacKey));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}

// Integration test — full Oracle pipeline
public class OraclePipelineIntegrationTests(AhsWebAppFactory factory)
    : IClassFixture<AhsWebAppFactory>
{
    [Fact]
    public async Task Oracle_returns_immediate_action_for_critical_passive_route()
    {
        var client = factory.CreateClient();

        var req = new OracleRequest
        {
            RouteId            = "R-CRITICAL-001",
            InsulationType     = "Passive",
            RouteCategory      = RouteRiskCategory.Critical,
            CarrierReliability = 0.5,
            CarrierIncidents12M = 3,
            ForecastMaxCelsius = 25.0,
            SetpointMaxCelsius = 8.0,
            SetpointMinCelsius = 2.0,
            ForecastMinCelsius = 5.0,
            ForecastHumidityPct = 75.0,
            CargoType          = "Pharma",
            CarrierId          = Guid.NewGuid(),
            OriginCountry      = "ES",
            DestinationCountry = "MX",
            PhysicalTtfHours   = 6.0,
        };

        var response = await client.PostAsJsonAsync("/api/oracle/calculate", req,
            OracleJsonContext.Default.OracleRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync(
            OracleJsonContext.Default.OracleResult);

        result!.RequiresImmediateAction.Should().BeTrue();
        result.Risk.PassivePenalty.Should().Be(15.0);
        result.Dna.Factors.Should().HaveCount(14);
    }
}
```

---

## 3. Architecture Tests — NetArchTest (Clean Architecture Enforcement)

```csharp
// AHS.Tests.Architecture/CleanArchitectureTests.cs
// Protege el guardrail del Blueprint: "Domain must have zero dependencies"

public class CleanArchitectureTests
{
    private static readonly Architecture Arch = new ArchLoader()
        .LoadAllAssemblies()
        .Build();

    // Blueprint guardrail: Domain has zero external dependencies
    [Fact]
    public void Domain_must_not_depend_on_infrastructure()
    {
        Types.InAssembly(typeof(Shipment).Assembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Npgsql", "Dapper", "Azure", "Microsoft.EntityFrameworkCore",
                "StackExchange.Redis", "MQTTnet", "System.Net.Http")
            .Because("AHS.Common (Domain) must have zero infrastructure dependencies — Blueprint guardrail");
    }

    // Application depends only on Domain — not on Infrastructure
    [Fact]
    public void Application_must_not_depend_on_infrastructure()
    {
        Types.InAssembly(typeof(LogisticsOracle).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Npgsql", "Azure.Messaging.ServiceBus", "StackExchange.Redis")
            .Because("Application layer must depend only on Domain abstractions");
    }

    // Infrastructure can depend on everything — but Presentation cannot call Infrastructure directly
    [Fact]
    public void Presentation_must_not_call_infrastructure_directly()
    {
        Types.InAssembly(typeof(Program).Assembly)
            .That().DoNotResideInNamespace("AHS.Web.UI.Configuration")
            .Should()
            .NotHaveDependencyOn("AHS.Infrastructure")
            .Because("Presentation calls Application, Application calls Infrastructure via interfaces");
    }

    // All domain models must be record types (Blueprint guardrail)
    [Fact]
    public void Domain_models_must_be_records()
    {
        Types.InAssembly(typeof(Shipment).Assembly)
            .That().ResideInNamespace("AHS.Common.Domain")
            .And().AreClasses()
            .Should()
            .BeRecord()
            .OrShould()
            .HaveNameStartingWith("Aggregate")  // AggregateRoot is a class by design
            .Because("Blueprint guardrail: All domain models must be record types");
    }

    // HPC engines must not depend on Web or Presentation
    [Fact]
    public void HPC_engines_must_not_depend_on_web()
    {
        Types.InAssembly(typeof(MeanKineticTemperature).Assembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.AspNetCore", "Blazor", "AHS.Web")
            .Because("AHS.Engines.HPC is a pure computation library");
    }
}
```

---

## 4. BDD — Reqnroll (GxP Scenarios)

### Feature file — Oracle Risk Assessment

```gherkin
# Features/OracleRiskAssessment.feature
Feature: Logistics Oracle Risk Assessment (REQ-001)
  As a quality officer in AHS
  I want the Oracle to calculate logistics risk accurately
  So that I can protect high-value pharmaceutical shipments

  Background:
    Given a pharmaceutical shipment with setpoint 2–8°C
    And a cargo type of "Pharma"

  @REQ-001 @GxP
  Scenario: Passive insulation always triggers base penalty
    Given the shipment uses "Passive" insulation
    And the route category is "Low"
    When the Oracle calculates the risk
    Then the risk breakdown should include a passive penalty of 15%
    And the final risk score should be higher than the weighted score

  @REQ-001 @GxP @Critical
  Scenario: Critical route with passive insulation requires immediate action
    Given the shipment uses "Passive" insulation
    And the route category is "Critical"
    And the carrier reliability is 50%
    And the physical TTF is 6 hours
    When the Oracle calculates the risk
    Then the Oracle should require immediate action
    And the risk band should be "Critical"
    And the XAI DNA should contain 14 diagnostic factors

  @REQ-001 @Performance
  Scenario: Oracle responds within latency SLA
    Given a standard pharmaceutical shipment
    When the Oracle calculates the risk 100 times
    Then the P99 latency should be less than 10 milliseconds

  @GxP @21CFR11
  Scenario: What-If simulation requires reason for change
    Given the user opens the What-If Simulator
    When the user changes the "Route Category" from "Low" to "Critical"
    And the user does not provide a reason for change
    Then the system should reject the change
    And display the message "Reason for change is required per FDA 21 CFR Part 11"

  @GxP @21CFR11
  Scenario: What-If change is sealed in the immutable ledger
    Given the user opens the What-If Simulator
    When the user changes the "Route Category" from "Low" to "Critical"
    And provides the reason "Emergency reroute — original route under weather alert"
    Then the change should be accepted
    And a ledger entry should be created with the reason for change
    And the ledger entry should be cryptographically sealed with SHA256
```

### Step definitions

```csharp
// StepDefinitions/OracleSteps.cs
[Binding]
public class OracleSteps(ScenarioContext ctx, AhsWebAppFactory factory)
{
    private OracleRequest _request = null!;
    private OracleResult  _result  = null!;

    [Given("a pharmaceutical shipment with setpoint (.*)–(.*)°C")]
    public void GivenPharmaceuticalShipment(double min, double max)
    {
        _request = OracleRequestBuilder.Default with
        {
            SetpointMinCelsius = min,
            SetpointMaxCelsius = max,
            CargoType          = "Pharma",
        };
    }

    [Given("the shipment uses \"(.*)\" insulation")]
    public void GivenInsulation(string insulationType)
        => _request = _request with { InsulationType = insulationType };

    [Given("the route category is \"(.*)\"")]
    public void GivenRouteCategory(string category)
        => _request = _request with
        {
            RouteCategory = Enum.Parse<RouteRiskCategory>(category)
        };

    [Given("the physical TTF is (.*) hours")]
    public void GivenPhysicalTtf(double hours)
        => _request = _request with { PhysicalTtfHours = hours };

    [When("the Oracle calculates the risk")]
    public async Task WhenOracleCalculates()
    {
        var client = factory.CreateClient();
        var resp   = await client.PostAsJsonAsync("/api/oracle/calculate", _request,
            OracleJsonContext.Default.OracleRequest);
        _result = (await resp.Content.ReadFromJsonAsync(OracleJsonContext.Default.OracleResult))!;
    }

    [Then("the risk breakdown should include a passive penalty of (.*)%")]
    public void ThenPassivePenalty(double expected)
        => _result.Risk.PassivePenalty.Should().Be(expected);

    [Then("the Oracle should require immediate action")]
    public void ThenRequiresImmediateAction()
        => _result.RequiresImmediateAction.Should().BeTrue();

    [Then("the XAI DNA should contain (.*) diagnostic factors")]
    public void ThenDnaFactorCount(int count)
        => _result.Dna.Factors.Should().HaveCount(count);

    [Then("the P99 latency should be less than (.*) milliseconds")]
    public void ThenP99Latency(double maxMs)
        => ctx.Get<List<double>>("timings")
            .OrderBy(t => t).ElementAt(98)
            .Should().BeLessThan(maxMs);
}

// StepDefinitions/WhatIfSimulatorSteps.cs
[Binding]
public class WhatIfSimulatorSteps(ScenarioContext ctx, ILedgerRepository ledger)
{
    private Exception? _lastException;
    private LedgerEntry? _createdEntry;

    [When("the user changes the \"(.*)\" from \"(.*)\" to \"(.*)\"")]
    public void WhenUserChangesParameter(string param, string from, string to)
        => ctx["change"] = new WhatIfChange(param, from, to);

    [When("the user does not provide a reason for change")]
    public async Task WhenNoReasonProvided()
    {
        try
        {
            var change = ctx.Get<WhatIfChange>("change");
            await ApplyChangeAsync(change, reasonForChange: "");
        }
        catch (Exception ex) { _lastException = ex; }
    }

    [When("provides the reason \"(.*)\"")]
    public async Task WhenReasonProvided(string reason)
    {
        var change = ctx.Get<WhatIfChange>("change");
        _createdEntry = await ApplyChangeAsync(change, reasonForChange: reason);
    }

    [Then("the system should reject the change")]
    public void ThenRejected()
        => _lastException.Should().NotBeNull()
            .And.BeOfType<ElectronicSignatureRequiredException>();

    [Then("a ledger entry should be created with the reason for change")]
    public void ThenLedgerEntryCreated()
        => _createdEntry.Should().NotBeNull();

    [Then("the ledger entry should be cryptographically sealed with SHA256")]
    public void ThenHashSealed()
        => _createdEntry!.EntryHash.Should().HaveLength(64,
            because: "SHA256 produces a 64-character hex string");
}
```

---

## 5. Electronic Signatures & Reason for Change (FDA 21 CFR Part 11)

```csharp
// Every write command carries an electronic signature — Capa 5 requirement
public abstract record SignedCommand
{
    public required Guid   SignedById    { get; init; }
    public required string SignedByName  { get; init; }
    public required string ReasonForChange { get; init; }

    protected SignedCommand()
    {
        if (string.IsNullOrWhiteSpace(ReasonForChange))
            throw new ElectronicSignatureRequiredException(
                "ReasonForChange is required per FDA 21 CFR Part 11 §11.10(e)");
    }
}

public record SealShipmentCommand : SignedCommand
{
    public required Guid   ShipmentId      { get; init; }
    public required string FinalStatus     { get; init; }
    public required string QualityDecision { get; init; }
}

public record ApplyWhatIfChangeCommand : SignedCommand
{
    public required string ParameterName  { get; init; }
    public required string PreviousValue  { get; init; }
    public required string NewValue       { get; init; }
    public required Guid   ShipmentId     { get; init; }
}

public class ElectronicSignatureRequiredException(string message) : Exception(message);
```

---

## 6. Test Helpers — Boring Infrastructure (DX)

```csharp
// Shared test infrastructure — keeps tests readable
public static class OracleRequestBuilder
{
    public static OracleRequest Default => new()
    {
        RouteId             = "R-TEST-001",
        OriginCountry       = "ES",
        DestinationCountry  = "DE",
        RouteCategory       = RouteRiskCategory.Medium,
        CarrierId           = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        CarrierReliability  = 0.90,
        CarrierIncidents12M = 0,
        ForecastMaxCelsius  = 10.0,
        ForecastMinCelsius  = 4.0,
        ForecastHumidityPct = 55.0,
        InsulationType      = "Active",
        CargoType           = "Pharma",
        SetpointMinCelsius  = 2.0,
        SetpointMaxCelsius  = 8.0,
        PhysicalTtfHours    = 48.0,
    };
}

public static class TestKeys
{
    // Deterministic test key — never use in production
    public static readonly byte[] HmacKey =
        SHA256.HashData(Encoding.UTF8.GetBytes("AHS_TEST_KEY_NOT_FOR_PRODUCTION"));
}

// FluentAssertions extension for domain-specific assertions
public static class AhsAssertionExtensions
{
    public static AndConstraint<StringAssertions> ContainReasonForChange(
        this StringAssertions assertions, string expected)
        => assertions.Subject.Should().Contain(expected);
}
```

---

## 7. Convenciones (Equipo pequeño / Boring DX)

| Convención | Regla |
|---|---|
| Nombre de test | `Subject_condition_expected_outcome` |
| Un `Assert` por test | Usa `using var scope = new AssertionScope()` si necesitas varios |
| No mocks de lo que posees | Usa fakes para boundaries externos (DB, HTTP, Service Bus) |
| Testcontainers por clase | `IClassFixture<AhsWebAppFactory>` — un contenedor por suite |
| `Respawn` para reset | Resetea BD entre tests sin recrear contenedor |
| NetArchTest en CI | Falla el build si se viola Clean Architecture |
| Reqnroll features en `/Features` | Un `.feature` por caso de uso de negocio, no por clase técnica |
| Tags GxP obligatorios | `@GxP`, `@21CFR11`, `@REQ-001` — trazabilidad regulatoria |
