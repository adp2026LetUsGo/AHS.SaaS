# AHS.SaaS - CLOUD PROVISIONING SCRIPT v1.2 (ULTIMATE)
$RANDOM_ID = "1979" 
$RG_NAME = "RG-AHS-SAAS-FREE"
$LOCATION = "eastus2"
$APP_SERVICE_PLAN = "ASP-AHS-SAAS-FREE"
$API_NAME = "ahs-api-gateway-$RANDOM_ID"
$SWA_NAME = "ahs-bento-ui-$RANDOM_ID"
$REPO_FULL_NAME = "adp2026LetUsGo/AHS.SaaS" 

Write-Host "--- REPAIRING AHS.SaaS PROVISIONING ---" -ForegroundColor Cyan

# 1. Ensure Resource Group and Plan exist
az group create --name $RG_NAME --location $LOCATION
az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RG_NAME --location $LOCATION --sku F1 --is-linux

# 2. Create Web App (Variable injection prevents PowerShell pipe errors)
Write-Host "[4/6] Provisioning Web App for API..." -ForegroundColor Gray
$dotnetRuntime = "DOTNET|8.0"
az webapp create --name $API_NAME --resource-group $RG_NAME --plan $APP_SERVICE_PLAN --runtime $dotnetRuntime

# 3. Configure CORS and Static Web App
Write-Host "[5/6] Link Ecosystem & Sync Secrets..." -ForegroundColor Gray
$SWA_URL_RAW = az staticwebapp show --name $SWA_NAME --query "defaultHostname" -o tsv
$SWA_FULL_URL = "https://$SWA_URL_RAW"

# Set API Settings
az webapp config appsettings set --name $API_NAME --resource-group $RG_NAME --settings ALLOWED_ORIGINS=$SWA_FULL_URL

# 4. Sync Secrets to GitHub (Forced)
$SWA_TOKEN = az staticwebapp secrets list --name $SWA_NAME --query "properties.apiKey" -o tsv
$PUB_PROFILE = az webapp deployment list-publishing-profiles --name $API_NAME --resource-group $RG_NAME --xml

# Set Secrets using GH CLI
if ($SWA_TOKEN) { gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$SWA_TOKEN" --repo $REPO_FULL_NAME }
if ($PUB_PROFILE) { gh secret set AZURE_APP_SERVICE_PUBLISH_PROFILE --body "$PUB_PROFILE" --repo $REPO_FULL_NAME }

Write-Host "`nSYSTEM READY FOR DEPLOYMENT" -ForegroundColor Green
Write-Host "FRONTEND URL: $SWA_FULL_URL"
Write-Host "API GATEWAY URL: https://$API_NAME.azurewebsites.net"