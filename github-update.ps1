# 1. Verificar el estado actual
Write-Host "📡 Revisando cambios en AHS.SaaS..." -ForegroundColor Cyan
git status

# 2. Añadir todos los nuevos proyectos y archivos de infraestructura
Write-Host "📦 Preparando archivos para commit..." -ForegroundColor Yellow
git add .

# 3. Hacer el commit con descripción técnica clara
# Nota: 'feat' indica una nueva funcionalidad, 'chore' cambios de infraestructura
git commit -m "feat: implement firebase persistence and compliance audit trail (SHA256)"

# 4. Empujar los cambios a GitHub
Write-Host "🚀 Subiendo a GitHub (https://github.com/adp2026LetUsGo/AHS.SaaS)..." -ForegroundColor Green
git push origin main

Write-Host "✅ ¡Misión cumplida! Tu progreso está a salvo en la nube." -ForegroundColor White