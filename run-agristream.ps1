
$azPath = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin"
$env:PATH = "$azPath;$env:PATH"
$env:AZURE_TOKEN_CREDENTIALS = "AzureCliCredential"

$az = Join-Path $azPath "az.cmd"

if (-not (Test-Path $az)) {
    Write-Host "Azure CLI was not found." -ForegroundColor Red
    exit 1
}

& $az account show --query user.name --output tsv

if ($LASTEXITCODE -ne 0) {
    Write-Host "Please sign in to Azure..." -ForegroundColor Yellow
    & $az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Azure login failed." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Starting AgriStream..." -ForegroundColor Green
dotnet run
