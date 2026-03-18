# deploy.ps1
# This script builds the Blazor app and prepares the 'docs' folder for GitHub Pages.

Write-Host "🚀 Starting Build Process..." -ForegroundColor Cyan

# 1. Clean previous builds
if (Test-Path "docs") { Remove-Item -Recurse -Force "docs" }
if (Test-Path "release") { Remove-Item -Recurse -Force "release" }

# 2. Publish the project
dotnet publish AYExpenseTracker.csproj -c Release -o release

# 3. Create docs folder and copy content
New-Item -ItemType Directory -Path "docs" -Force
$publishPath = "bin/Release/net9.0/browser-wasm/publish/wwwroot/*"
if (!(Test-Path $publishPath)) {
    # Fallback to general release path if exists
    $publishPath = "release/wwwroot/*"
}
Copy-Item -Path $publishPath -Destination "docs" -Recurse -Force

# 3b. Ensure index.html and 404.html are at the root
if (Test-Path "docs/AYExpenseTracker") {
    Copy-Item -Path "docs/AYExpenseTracker/*" -Destination "docs" -Recurse -Force
    Remove-Item -Recurse -Force "docs/AYExpenseTracker"
}

# 4. Success check
if (!(Test-Path "docs/404.html")) {
    Copy-Item -Path "docs/index.html" -Destination "docs/404.html"
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
