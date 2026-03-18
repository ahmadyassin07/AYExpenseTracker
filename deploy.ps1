# deploy.ps1
# This script builds the Blazor app and prepares the 'docs' folder for GitHub Pages.

Write-Host "🚀 Starting Build Process..." -ForegroundColor Cyan

# 1. Clean previous builds
if (Test-Path "docs") { Remove-Item -Recurse -Force "docs" }
if (Test-Path "release") { Remove-Item -Recurse -Force "release" }

# 2. Publish the project
dotnet publish AYExpenseTracker.csproj -c Release -o release

# 3. Create docs folder and copy content
if (Test-Path "docs") {
    # Keep .git and other essential files if they were there, but clean the rest
    Get-ChildItem -Path "docs" -Exclude ".git", ".nojekyll" | Remove-Item -Recurse -Force
} else {
    New-Item -ItemType Directory -Path "docs" -Force
}

$publishPath = "bin/Release/net9.0/browser-wasm/publish/wwwroot/*"
if (!(Test-Path $publishPath)) {
    $publishPath = "release/wwwroot/*"
}
Copy-Item -Path $publishPath -Destination "docs" -Recurse -Force

# 3b. Ensure .nojekyll exists (Critical for GitHub Pages _framework folder)
New-Item -ItemType File -Path "docs/.nojekyll" -Force | Out-Null

# 3c. Clean up any accidental nested folder
if (Test-Path "docs/AYExpenseTracker") {
    Copy-Item -Path "docs/AYExpenseTracker/*" -Destination "docs" -Recurse -Force
    Remove-Item -Recurse -Force "docs/AYExpenseTracker"
}

# 4. Success check & 404 fallback
if (Test-Path "docs/index.html") {
    Copy-Item -Path "docs/index.html" -Destination "docs/404.html" -Force
}

# 4b. Force the custom service-worker (Avoid Blazor default)
if (Test-Path "wwwroot/service-worker.js") {
    Copy-Item -Path "wwwroot/service-worker.js" -Destination "docs/service-worker.js" -Force
    Write-Host "✅ Custom service-worker.js applied." -ForegroundColor Cyan
}

# 4. Cleanup compressed files (GitHub Pages serves uncompressed)
Get-ChildItem -Path "docs" -Filter "*.gz" -Recurse | Remove-Item
Get-ChildItem -Path "docs" -Filter "*.br" -Recurse | Remove-Item

# 5. Success message
Write-Host "✅ Done! All files are in the 'docs' folder." -ForegroundColor Green
Write-Host "👉 Next steps:" -ForegroundColor Yellow
Write-Host "1. Push the 'docs' folder to your GitHub repository."
Write-Host "2. In GitHub Repository Settings -> Pages, set the Source to '/docs' folder instead of root."
Write-Host "3. Your app will be live at: https://ahmadyassin07.github.io/AYExpenseTracker/"
