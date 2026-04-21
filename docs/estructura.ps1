# Define the base path
$basePath = "C:\Users\armando\Documents\_AHS\projects\AHS.SaaS\docs"

# List of all required subdirectories
$folders = @(
    "product",
    "architecture",
    "architecture\adr",
    "architecture\diagrams",
    "technical",
    "technical\api",
    "technical\data-schemas",
    "research"
)

# Ensure the base directory exists
if (!(Test-Path -Path $basePath)) {
    New-Item -Path $basePath -ItemType Directory | Out-Null
    Write-Host "Base directory created: $basePath" -ForegroundColor Cyan
}

# Create subdirectories
foreach ($folder in $folders) {
    $targetPath = Join-Path -Path $basePath -ChildPath $folder
    if (!(Test-Path -Path $targetPath)) {
        New-Item -Path $targetPath -ItemType Directory | Out-Null
        Write-Host "Created: $targetPath" -ForegroundColor Green
    } else {
        Write-Host "Already exists: $targetPath" -ForegroundColor Yellow
    }
}

Write-Host "`nStructure setup complete." -ForegroundColor Cyan