# ============================================================================
# AHS.SaaS - CLOUD PROVISIONING SCRIPT v1.1 (CLEAN VERSION)
# ============================================================================

$ErrorActionPreference = "Stop"
$ProjectRoot = Get-Location

# Configuración de Nombres (Costo Cero)
$RG_NAME = "RG-AHS-SAAS-FREE"
$LOCATION = "eastus2" 
$APP_SERVICE_PLAN = "ASP-AHS-SAAS-FREE"
$RANDOM_ID = Get-Random -Minimum 1000 -Maximum 9999
$API_NAME = "ahs-api-gateway-$RANDOM_ID"
$SWA_NAME = "ahs-bento-ui-$RANDOM_ID"

Write-Host "--- INICIANDO PROVISION AHS.SaaS (PHASE 10.2) ---" -ForegroundColor Cyan

# 1. Autenticación
Write-Host "[1/6] Autenticando en Azure (Se abrira el navegador)..."
az login

# 2. Crear Grupo de Recursos
Write-Host "[2/6] Creando Grupo de Recursos ($RG_NAME)..."
az group create --name $RG_NAME --location $LOCATION

# 3. Crear Plan de App Service (F1 FREE)
Write-Host "[3/6] Creando Plan App Service (SKU F1 - FREE)..."
az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RG_NAME --location $LOCATION --sku F1 --is-linux

# 4. Crear Web App para el API Gateway
Write-Host "[4/6] Creando Web App para API Gateway..."
az webapp create --name $API_NAME --resource-group $RG_NAME --plan $APP_SERVICE_PLAN --runtime "DOTNET|8.0"

# 5. Crear Azure Static Web App para el Dashboard
Write-Host "[5/6] Creando Static Web App para el Frontend..."
$REPO_URL = git remote get-url origin
$SWA_DATA = az staticwebapp create --name $SWA_NAME --resource-group $RG_NAME --location $LOCATION --source $REPO_URL --branch "main" --app-location "src/Presentation/AHS.Web.BentoUI" --output-location "wwwroot" --login-with-github --query "{token:apiKey, url:defaultHostname}" -o json | ConvertFrom-Json

$SWA_DEPLOY_TOKEN = $SWA_DATA.token
$SWA_FULL_URL = "https://$($SWA_DATA.url)"

# 6. Configurar CORS y Variables de Entorno
Write-Host "[6/6] Configurando Enlace de Ecosistema (CORS)..."
az webapp config appsettings set --name $API_NAME --resource-group $RG_NAME --settings ALLOWED_ORIGINS=$SWA_FULL_URL

# --- SINCRONIZACIÓN DE SECRETOS CON GITHUB ---
Write-Host "--- SINCRONIZANDO SECRETOS CON GITHUB ---" -ForegroundColor Yellow

# Extraer Perfil de Publicación del API
$PUBLISH_PROFILE = az webapp deployment list-publishing-profiles --name $API_NAME --resource-group $RG_NAME --xml

# Usar GitHub CLI para subir los secretos
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$SWA_DEPLOY_TOKEN"
gh secret set AZURE_APP_SERVICE_PUBLISH_PROFILE --body "$PUBLISH_PROFILE"

Write-Host "PROVISION COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "URL Frontend: $SWA_FULL_URL"
Write-Host "URL API Gateway: https://$API_NAME.azurewebsites.net"
Write-Host "Haga un 'git push' para iniciar el despliegue automatico."