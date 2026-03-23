---
name: azure-devops-coldchain
description: >
  Expert guidance on Azure CI/CD pipelines, Azure infrastructure, and claims-based
  identity for AHS cold chain applications using C# 14 / .NET 10 / Native AOT.
  Use this skill whenever the user mentions Azure DevOps, GitHub Actions for Azure,
  Azure Container Apps, AKS, Azure Key Vault, Entra ID, managed identity, claims-based
  access control, zero-cost pipelines, AOT container deployment, Azure Service Bus,
  bicep, ARM templates, or Azure Monitor for cold chain. Trigger on: Azure DevOps,
  GitHub Actions Azure, Container Apps, AKS, Key Vault, Entra ID, managed identity,
  claims-based, zero-cost pipeline, AOT deploy, bicep, Azure Monitor, Application Insights,
  RBAC Azure, multi-tenant Azure.
---

# Azure DevOps & Cloud — AHS Cold Chain
## C# 14 / .NET 10 / Native AOT

## AHS Azure Architecture

```
GitHub / Azure DevOps
    ↓ CI/CD (zero-cost)
Azure Container Registry
    ↓ Native AOT Linux image (~35MB)
Azure Container Apps  ←→  Azure Service Bus (sensor events)
    ↓                          ↓
Azure SQL (Event Store)    Azure Key Vault (HMAC keys)
    ↓
Azure Monitor + App Insights (cold chain telemetry)
    ↑
Entra ID (claims-based multitenancy)
```

---

## 1. Native AOT Docker Image (Linux, minimal)

```dockerfile
# Stage 1: Build Native AOT binary
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install clang (required for Native AOT on Linux)
RUN apt-get update && apt-get install -y clang zlib1g-dev

COPY ["AHS.Web.UI/AHS.Web.UI.csproj", "AHS.Web.UI/"]
COPY ["AHS.Common/AHS.Common.csproj", "AHS.Common/"]
COPY ["AHS.Engines.HPC/AHS.Engines.HPC.csproj", "AHS.Engines.HPC/"]
RUN dotnet restore "AHS.Web.UI/AHS.Web.UI.csproj"

COPY . .
RUN dotnet publish "AHS.Web.UI/AHS.Web.UI.csproj" \
    -r linux-x64 \
    -c Release \
    --no-restore \
    -o /app/publish

# Stage 2: Minimal runtime image (no .NET runtime needed — AOT)
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS final
WORKDIR /app

# Non-root user (security hardening)
USER app
COPY --from=build --chown=app:app /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD ["/app/AHS.Web.UI", "--health-check"] || exit 1

EXPOSE 8080
ENTRYPOINT ["/app/AHS.Web.UI"]
```

```xml
<!-- AHS.Web.UI.csproj — AOT publish settings -->
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <StripSymbols>true</StripSymbols>
  <InvariantGlobalization>true</InvariantGlobalization>
  <OptimizationPreference>Size</OptimizationPreference>
  <!-- Target: ~35MB image -->
</PropertyGroup>
```

---

## 2. GitHub Actions — Zero-Cost Pipeline

```yaml
# .github/workflows/ahs-deploy.yml
name: AHS Cold Chain — Build & Deploy

on:
  push:
    branches: [main, release/*]
  pull_request:
    branches: [main]

env:
  REGISTRY:     ${{ vars.ACR_NAME }}.azurecr.io
  IMAGE_NAME:   ahs-web-ui
  DOTNET_VERSION: '10.0.x'

jobs:
  # ── Build & Test ──────────────────────────────────────────
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ hashFiles('**/*.csproj') }}

      - name: Install clang (AOT requirement)
        run: sudo apt-get install -y clang zlib1g-dev

      - name: Restore
        run: dotnet restore AHS.sln

      - name: AOT Trim Analysis (catch IL warnings early)
        run: |
          dotnet build AHS.Web.UI/AHS.Web.UI.csproj \
            /p:PublishAot=true \
            /p:EnableTrimAnalyzer=true \
            /warnaserror:IL2026,IL2067,IL3050 \
            -c Release

      - name: Test
        run: dotnet test AHS.sln --no-build -c Release \
          --logger "trx;LogFileName=results.trx" \
          --collect:"XPlat Code Coverage"

      - name: Publish test results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: AHS Tests
          path: '**/results.trx'
          reporter: dotnet-trx

      - name: Verify GxP Ledger chain integrity
        run: dotnet run --project AHS.Tools/AHS.Tools.csproj -- verify-ledger --environment CI

  # ── Docker Build & Push ───────────────────────────────────
  docker:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' || startsWith(github.ref, 'refs/heads/release/')
    outputs:
      image-tag: ${{ steps.meta.outputs.tags }}
      digest:    ${{ steps.push.outputs.digest }}
    steps:
      - uses: actions/checkout@v4

      - name: Login to ACR
        uses: azure/docker-login@v2
        with:
          login-server: ${{ env.REGISTRY }}
          username:     ${{ secrets.ACR_USERNAME }}
          password:     ${{ secrets.ACR_PASSWORD }}

      - name: Docker metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=sha,prefix=sha-
            type=ref,event=branch
            type=semver,pattern={{version}}

      - name: Build & Push AOT image
        id: push
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags:   ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to:   type=gha,mode=max

      - name: Image size gate (AOT must be < 80MB)
        run: |
          SIZE=$(docker image inspect ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }} \
            --format='{{.Size}}')
          MAX=$((80 * 1024 * 1024))
          if [ "$SIZE" -gt "$MAX" ]; then
            echo "Image too large: ${SIZE} bytes (max: ${MAX})"
            exit 1
          fi
          echo "Image size OK: $((SIZE / 1024 / 1024))MB"

  # ── Deploy to Azure Container Apps ───────────────────────
  deploy:
    needs: docker
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Azure Login (OIDC — no stored credentials)
        uses: azure/login@v2
        with:
          client-id:       ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id:       ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Deploy to Container Apps
        uses: azure/container-apps-deploy-action@v2
        with:
          resourceGroup:        ahs-prod-rg
          containerAppName:     ahs-web-ui
          imageToDeploy:        ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}@${{ needs.docker.outputs.digest }}
          # Digest-based deploy = immutable, no tag ambiguity

      - name: Verify deployment health
        run: |
          az containerapp revision list \
            --name ahs-web-ui \
            --resource-group ahs-prod-rg \
            --query "[0].properties.healthState" -o tsv
```

---

## 3. Azure Key Vault — HMAC Keys for GxP Ledger

```csharp
// Program.cs — retrieve HMAC key at startup, zero secrets in config files
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential()); // uses Managed Identity in Azure, developer identity locally

// Typed accessor — AOT-safe (no reflection)
public class LedgerKeyProvider(IConfiguration config, ILogger<LedgerKeyProvider> logger)
{
    private byte[]? _cachedKey;
    private DateTimeOffset _keyExpiry = DateTimeOffset.MinValue;

    public async Task<byte[]> GetHmacKeyAsync(CancellationToken ct)
    {
        if (_cachedKey != null && DateTimeOffset.UtcNow < _keyExpiry)
            return _cachedKey;

        var keyBase64 = config["GxPLedger:HmacKeyBase64"]
            ?? throw new InvalidOperationException("GxP HMAC key not found in Key Vault.");

        _cachedKey  = Convert.FromBase64String(keyBase64);
        _keyExpiry  = DateTimeOffset.UtcNow.AddMinutes(55); // refresh before 1h Key Vault cache

        logger.LogInformation("GxP HMAC key loaded from Key Vault.");
        return _cachedKey;
    }
}

// Registration
builder.Services.AddSingleton<LedgerKeyProvider>();
builder.Services.AddScoped<LedgerHasher>(sp =>
{
    var provider = sp.GetRequiredService<LedgerKeyProvider>();
    var key = provider.GetHmacKeyAsync(default).GetAwaiter().GetResult();
    return new LedgerHasher(key);
});
```

---

## 4. Entra ID — Claims-Based Multitenancy

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId"));

// Entra ID JWT includes custom claims:
// "tenant_id"    — maps to AHS tenant
// "tenant_slug"  — e.g. "pharma-corp-eu"
// "cargo_types"  — ["Pharma", "Food"]
// "ahs_role"     — "Operator" | "QualityOfficer" | "Admin"

public class EntradIdTenantResolver(IConfiguration config) : ITenantResolver
{
    public Task<ITenantContext?> ResolveAsync(HttpContext ctx)
    {
        var user = ctx.User;
        if (!user.Identity?.IsAuthenticated ?? true) return Task.FromResult<ITenantContext?>(null);

        var tenantId   = user.FindFirstValue("tenant_id");
        var tenantSlug = user.FindFirstValue("tenant_slug");
        if (tenantId is null || tenantSlug is null) return Task.FromResult<ITenantContext?>(null);

        var context = new TenantContext
        {
            TenantId   = Guid.Parse(tenantId),
            TenantSlug = tenantSlug,
            Plan       = Enum.Parse<TenantPlan>(user.FindFirstValue("ahs_plan") ?? "Standard"),
        };

        return Task.FromResult<ITenantContext?>(context);
    }
}

// appsettings.json — no secrets here
// {
//   "EntraId": {
//     "Instance": "https://login.microsoftonline.com/",
//     "TenantId": "<your-entra-tenant-id>",
//     "ClientId": "<ahs-api-app-registration-id>",
//     "Audience": "api://ahs-cold-chain"
//   }
// }
```

---

## 5. Azure Bicep — Zero-Cost Infrastructure

```bicep
// infra/main.bicep — deploy everything with: az deployment group create
@description('AHS Cold Chain Infrastructure')
param location string = resourceGroup().location
param environmentName string = 'prod'
param acrName string

// Container Apps Environment (serverless — zero cost at idle)
resource caEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'ahs-${environmentName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey:  logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// AHS Web UI — Native AOT Container App
resource ahsWebUi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ahs-web-ui'
  location: location
  identity: { type: 'SystemAssigned' }  // Managed Identity — no passwords
  properties: {
    environmentId: caEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external:   true
        targetPort: 8080
        transport:  'http2'   // gRPC support
      }
      secrets: []  // no secrets in config — all via Key Vault
    }
    template: {
      containers: [{
        name:  'ahs-web-ui'
        image: '${acrName}.azurecr.io/ahs-web-ui:latest'
        resources: {
          cpu:    '0.5'   // AOT startup: cold start < 50ms on 0.5 vCPU
          memory: '1.0Gi'
        }
        env: [
          { name: 'ASPNETCORE_ENVIRONMENT', value: environmentName }
          { name: 'KeyVaultName',           value: keyVault.name   }
          { name: 'ASPNETCORE_URLS',        value: 'http://+:8080' }
        ]
      }]
      scale: {
        minReplicas: 0    // scale to zero when idle = zero cost
        maxReplicas: 10
        rules: [{
          name: 'http-scaling'
          http: { metadata: { concurrentRequests: '20' } }
        }]
      }
    }
  }
}

// Key Vault — HMAC keys + connection strings
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'ahs-${environmentName}-kv'
  location: location
  properties: {
    sku:      { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableSoftDelete:      true
    softDeleteRetentionInDays: 90  // GxP: 90-day retention minimum
    enablePurgeProtection: true    // GxP: irreversible once set
    accessPolicies: []
  }
}

// Grant Container App Managed Identity access to Key Vault secrets
resource kvAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  name: 'add'
  parent: keyVault
  properties: {
    accessPolicies: [{
      tenantId:    subscription().tenantId
      objectId:    ahsWebUi.identity.principalId
      permissions: { secrets: ['get', 'list'] }  // read-only, no write
    }]
  }
}

// Azure SQL — Event Store (GxP: geo-redundant, soft-delete)
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'ahs-${environmentName}-sql'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    administrators: {
      administratorType:         'ActiveDirectory'
      azureADOnlyAuthentication: true   // no SQL passwords
      login:                     'ahs-sql-admin'
      sid:                       '<entra-group-object-id>'
    }
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  name: 'AhsEventStore'
  parent: sqlServer
  sku: { name: 'GP_S_Gen5_2', tier: 'GeneralPurpose' }  // serverless — pauses at idle
  properties: {
    autoPauseDelay:           60    // pause after 60 min idle
    minCapacity:              '0.5'
    requestedBackupStorageRedundancy: 'Geo'  // GxP: geo-redundant backups
  }
}

// Log Analytics (zero cost tier up to 5GB/day)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'ahs-${environmentName}-logs'
  location: location
  properties: {
    retentionInDays: 365  // GxP: 1 year minimum
    sku: { name: 'PerGB2018' }
  }
}
```

---

## 6. Azure Monitor — Cold Chain Telemetry

```csharp
// Program.cs — structured telemetry for cold chain events
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddSqlClientInstrumentation(o => o.SetDbStatementForText = false)  // GxP: no PII in traces
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddMeter("AHS.ColdChain")
        .AddOtlpExporter());

// Custom cold chain metrics (Azure Monitor dashboard)
public class AhsMetrics
{
    private readonly Counter<long>   _excursions;
    private readonly Histogram<double> _riskScore;
    private readonly Histogram<double> _ttfHours;

    public AhsMetrics(IMeterFactory factory)
    {
        var meter = factory.Create("AHS.ColdChain");
        _excursions = meter.CreateCounter<long>("ahs.excursions.total",
            description: "Total temperature excursions detected");
        _riskScore  = meter.CreateHistogram<double>("ahs.oracle.risk_score",
            description: "Logistics Oracle risk scores", unit: "score");
        _ttfHours   = meter.CreateHistogram<double>("ahs.oracle.ttf_hours",
            description: "Pessimistic TTF in hours", unit: "h");
    }

    public void RecordExcursion(string tenantId, string severity, string zone)
        => _excursions.Add(1,
            new("tenant_id", tenantId), new("severity", severity), new("zone", zone));

    public void RecordOracleResult(string tenantId, double riskScore, double ttf)
    {
        var tags = new TagList { new("tenant_id", tenantId) };
        _riskScore.Record(riskScore, tags);
        _ttfHours.Record(ttf, tags);
    }
}
```

---

## 7. Local Development — Azurite + Entra ID

```yaml
# docker-compose.yml — local stack mirrors Azure exactly
services:
  ahs-web-ui:
    build: .
    ports: ["8080:8080"]
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      KeyVaultName: local
      ConnectionStrings__Default: "Host=postgres;Database=ahs;Username=ahs_app;Password=ahs_dev_2024"
      Redis__ConnectionString: "redis:6379"
    depends_on: [postgres, redis, servicebus-emulator]

  postgres:
    image: postgres:17-alpine
    environment:
      POSTGRES_DB:       ahs
      POSTGRES_USER:     ahs_app
      POSTGRES_PASSWORD: ahs_dev_2024
    ports: ["5432:5432"]
    volumes: ["pgdata:/var/lib/postgresql/data"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  # Azure Service Bus emulator para desarrollo local
  servicebus-emulator:
    image: mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
    ports: ["5672:5672"]
    environment:
      ACCEPT_EULA: "Y"

volumes:
  pgdata:
```

```csharp
// Local Key Vault fallback — reads from user-secrets or env vars
// DefaultAzureCredential handles this automatically:
// 1. Managed Identity (Azure)
// 2. Azure CLI credentials (local dev)
// 3. Visual Studio 2026 credentials (local dev)
// 4. Environment variables (CI)
// No code changes between environments.
```

---

## 8. Deployment Checklist (GxP + Azure)

| Step | Command / Action |
|---|---|
| Provision infrastructure | `az deployment group create -f infra/main.bicep` |
| Set HMAC key in Key Vault | `az keyvault secret set --vault-name ahs-prod-kv --name GxPLedger--HmacKeyBase64 --value <base64>` |
| Verify Key Vault access | `az keyvault secret show --vault-name ahs-prod-kv --name GxPLedger--HmacKeyBase64` |
| Deploy via GitHub Actions | Push to `main` — pipeline runs automatically |
| Verify image digest | `az containerapp revision show --name ahs-web-ui --revision <rev>` |
| Check GxP ledger hash chain | `dotnet run --project AHS.Tools -- verify-ledger --environment prod` |
| Confirm scale-to-zero | Wait 60 min idle → `az containerapp show --name ahs-web-ui --query "properties.runningStatus"` |

## Zero-Cost Strategy

| Resource | Zero-Cost Mechanism |
|---|---|
| Container Apps | `minReplicas: 0` — pay per request only |
| Azure SQL | Serverless tier — autopause at 60 min idle |
| Log Analytics | Free tier: 5GB/day included |
| ACR | Basic tier: ~$5/month (fixed) |
| Key Vault | ~$0.03/10,000 operations |
| GitHub Actions | 2,000 min/month free for public repos; 500 min for private |
