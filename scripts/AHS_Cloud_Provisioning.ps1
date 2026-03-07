# ============================================================================
# AHS.SaaS: SILENT CLOUD PROVISIONING SCRIPT (F1 ZERO-COST)
# Purpose: Auto-deploy Azure Resources and Link GitHub Secrets
# ============================================================================

$ErrorActionPreference = "Stop"

# 1. AUTO-DETECTION: GITHUB REPOSITORY
Write-Host "🔍 Detectando Repositorio GitHub..." -ForegroundColor Cyan
$RepoUrl = git remote get-url origin
if ($RepoUrl -match "github\.com[:/](.+)\.git") {
    $RepoName = $Matches[1]
    Write-Host "✅ Repositorio Detectado: $RepoName" -ForegroundColor Green
} else {
    Write-Host "❌ Error: No se pudo detectar el repositorio de GitHub." -ForegroundColor Red
    exit 1
}

# 2. AZURE LOGIN CHECK
Write-Host "🔐 Verificando Sesión de Azure..." -ForegroundColor Cyan
$AzAccount = az account show --query name -o tsv
if (-not $AzAccount) {
    Write-Host "❌ No hay sesión activa. Por favor, ejecute 'az login'." -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Azure Account: $AzAccount"

# 3. RESOURCE GROUP (FREE ZONE)
$RG = "AHS_SaaS_Cloud"
$Location = "eastus"
Write-Host "🏗️ Creando Grupo de Recursos ($RG)..." -ForegroundColor Cyan
az group create --name $RG --location $Location

# 4. API: APP SERVICE (F1 FREE TIER)
$ApiName = "ahs-api-free"
$PlanName = "AHS_Free_Plan"
Write-Host "🚀 Provisionando API en App Service (SKU: F1)..." -ForegroundColor Cyan
az appservice plan create --name $PlanName --resource-group $RG --sku F1 --is-linux
az webapp create --name $ApiName --resource-group $RG --plan $PlanName --runtime "DOTNETCORE|10.0"

# 5. FRONTEND: STATIC WEB APP (FREE PLAN)
$SwaName = "ahs-web-bento"
Write-Host "🎨 Provisionando Frontend en Static Web App (Plan: Free)..." -ForegroundColor Cyan
$SwaOutput = az staticwebapp create --name $SwaName --resource-group $RG --location $Location --source "https://github.com/$RepoName" --branch "main" --app-location "src/Presentation/AHS.Web.BentoUI" --output-location "wwwroot" -o json | ConvertFrom-Json
$SwaUrl = "https://$($SwaOutput.defaultHostname)"
$DeploymentToken = az staticwebapp secrets list --name $SwaName --query properties.apiKey -o tsv

# 6. AUTO-LINKING: GITHUB SECRETS & CORS
Write-Host "🔗 Enlazando Ecosistema (CORS & Secrets)..." -ForegroundColor Cyan

# Guardar Token de SWA en GitHub
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$DeploymentToken" --repo "$RepoName"

# Configurar ALLOWED_ORIGINS en la API (Azure)
az webapp config appsettings set --name $ApiName --resource-group $RG --settings ALLOWED_ORIGINS="$SwaUrl"

# Obtener Perfil de Publicación de la API para GitHub
$PublishProfile = az webapp deployment list-publishing-profiles --name $ApiName --resource-group $RG --xml
gh secret set AZURE_WEBAPP_PUBLISH_PROFILE --body "$PublishProfile" --repo "$RepoName"

Write-Host "============================================================================" -ForegroundColor Green
Write-Host "✅ INFRAESTRUCTURA LISTA (COSTO CERO)" -ForegroundColor Green
Write-Host "🌐 Frontend URL: $SwaUrl" -ForegroundColor Cyan
Write-Host "⚡ API URL: https://$ApiName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "🔐 Secretos de GitHub inyectados automáticamente." -ForegroundColor Cyan
Write-Host "💻 Próximo Paso: Push a 'main' para activar el despliegue automático." -ForegroundColor Yellow
Write-Host "============================================================================" -ForegroundColor Green
