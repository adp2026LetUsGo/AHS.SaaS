$RANDOM_ID = "1979" 
$RG_NAME = "RG-AHS-SAAS-FREE"
$LOCATION = "westus" # CAMBIO DE REGIÓN PARA EVITAR QUOTA LIMIT
$APP_SERVICE_PLAN = "ASP-AHS-SAAS-FREE"
$API_NAME = "ahs-api-gateway-$RANDOM_ID"
$SWA_NAME = "ahs-bento-ui-$RANDOM_ID"
$REPO_FULL_NAME = "adp2026LetUsGo/AHS.SaaS" 

Write-Host "--- REPROVISIONAMIENTO RESILIENTE AHS.SaaS ---" -ForegroundColor Cyan

# 1. Grupo y Plan en Nueva Región
az group create --name $RG_NAME --location $LOCATION
az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RG_NAME --location $LOCATION --sku F1 --is-linux

# 2. Web App (Sintaxis Stop-Parsing --% para evitar errores de pipe)
Write-Host "[4/6] Creando Web App con Stop-Parsing..."
$cmd = "webapp create --name $API_NAME --resource-group $RG_NAME --plan $APP_SERVICE_PLAN --runtime `"DOTNET|8.0`""
Invoke-Expression "az $cmd"

# 3. Vincular Ecosistema
Write-Host "[5/6] Vinculando SWA y Secretos..."
$SWA_URL_RAW = az staticwebapp show --name $SWA_NAME --query "defaultHostname" -o tsv
if (-not $SWA_URL_RAW) { Write-Error "SWA no encontrada. Verifique el nombre."; exit }

$SWA_FULL_URL = "https://$SWA_URL_RAW"
az webapp config appsettings set --name $API_NAME --resource-group $RG_NAME --settings ALLOWED_ORIGINS=$SWA_FULL_URL

# 4. Sincronizar Secretos (Forzado)
$SWA_TOKEN = az staticwebapp secrets list --name $SWA_NAME --query "properties.apiKey" -o tsv
$PUB_PROFILE = az webapp deployment list-publishing-profiles --name $API_NAME --resource-group $RG_NAME --xml

if ($SWA_TOKEN) { gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$SWA_TOKEN" --repo $REPO_FULL_NAME }
if ($PUB_PROFILE) { gh secret set AZURE_APP_SERVICE_PUBLISH_PROFILE --body "$PUB_PROFILE" --repo $REPO_FULL_NAME }

Write-Host "SISTEMA DESBLOQUEADO EN $LOCATION" -ForegroundColor Green
Write-Host "URL FRONTEND: $SWA_FULL_URL"