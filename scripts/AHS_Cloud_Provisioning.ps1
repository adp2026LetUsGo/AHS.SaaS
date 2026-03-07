# =========================================================================
# AHS.SaaS - CLOUD PROVISIONING SCRIPT v1.5 (FINAL GOLD)
# Objetivo: Despliegue Zero-Cost con Native AOT en Azure
# =========================================================================

$RANDOM_ID = "1979" 
$RG_NAME = "RG-AHS-ELITE-FREE"
$LOCATION = "centralus" 
$APP_SERVICE_PLAN = "ASP-AHS-ELITE-FREE"
$API_NAME = "ahs-api-gateway-$RANDOM_ID"
$SWA_NAME = "ahs-bento-ui-$RANDOM_ID"
$REPO_FULL_NAME = "adp2026LetUsGo/AHS.SaaS" 

Write-Host "`n--- INICIANDO DESPLIEGUE FINAL AHS.SaaS ---" -ForegroundColor Cyan

# 1. Asegurar Grupo de Recursos y Plan F1
Write-Host "[1/6] Validando Infraestructura Base..." -ForegroundColor Gray
az group create --name $RG_NAME --location $LOCATION
az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RG_NAME --location $LOCATION --sku F1 --is-linux

# 2. Crear Web App para el API Gateway (Native AOT Host)
Write-Host "[2/6] Creando Web App para API Gateway (Runtime: DOTNETCORE|8.0)..." -ForegroundColor Gray
# Nota: Usamos DOTNETCORE|8.0 como imagen base para el binario autocontenido de .NET 10
az webapp create --name $API_NAME --resource-group $RG_NAME --plan $APP_SERVICE_PLAN --runtime "DOTNETCORE|8.0"

# 3. Crear Azure Static Web App para el Frontend
Write-Host "[3/6] Creando Static Web App..." -ForegroundColor Gray
$SWA_DATA = az staticwebapp create --name $SWA_NAME --resource-group $RG_NAME --location $LOCATION --source "https://github.com/$REPO_FULL_NAME" --branch "main" --app-location "Presentation/AHS.Web.BentoUI" --output-location "wwwroot" --login-with-github --query "{token:apiKey, host:defaultHostname}" -o json | ConvertFrom-Json

if (-not $SWA_DATA.token) {
    Write-Host "Reintentando obtener token de SWA..." -ForegroundColor Yellow
    $SWA_TOKEN_RAW = az staticwebapp secrets list --name $SWA_NAME --query "properties.apiKey" -o tsv
} else {
    $SWA_TOKEN_RAW = $SWA_DATA.token
}

# 4. Configurar CORS y Variables de Entorno
$SWA_URL = "https://$($SWA_DATA.host)"
Write-Host "[4/6] Configurando Seguridad CORS: $SWA_URL" -ForegroundColor Gray
az webapp config appsettings set --name $API_NAME --resource-group $RG_NAME --settings ALLOWED_ORIGINS=$SWA_URL

# 5. Extraer Perfil de Publicación de la API
Write-Host "[5/6] Extrayendo Perfil de Publicación..." -ForegroundColor Gray
$PUB_PROFILE = az webapp deployment list-publishing-profiles --name $API_NAME --resource-group $RG_NAME --xml

# 6. Sincronizar Secretos con GitHub (PHASE 10 COMPLETE)
Write-Host "[6/6] Sincronizando Secretos con GitHub CLI..." -ForegroundColor Yellow

# Sincronizar Token de SWA
if ($SWA_TOKEN_RAW) {
    gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$SWA_TOKEN_RAW" --repo $REPO_FULL_NAME
    Write-Host "OK: AZURE_STATIC_WEB_APPS_API_TOKEN" -ForegroundColor Green
}

# Sincronizar Perfil de API
if ($PUB_PROFILE) {
    gh secret set AZURE_APP_SERVICE_PUBLISH_PROFILE --body "$PUB_PROFILE" --repo $REPO_FULL_NAME
    Write-Host "OK: AZURE_APP_SERVICE_PUBLISH_PROFILE" -ForegroundColor Green
}

Write-Host "`n====================================================" -ForegroundColor Cyan
Write-Host "SISTEMA DESPLEGADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "URL FRONTEND: $SWA_URL"
Write-Host "URL API:      https://$API_NAME.azurewebsites.net"
Write-Host "====================================================`n"
Write-Host "Haga un 'git push' para activar los Workflows de CI/CD." -ForegroundColor White