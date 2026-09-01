# Test-Ollama.ps1 - Test Ollama API connection
param(
    [string]$Model = "qwen3:4b",
    [string]$BaseUrl = "http://localhost:11434/v1",
    [string]$ApiKey = "eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG",
    [string]$Prompt = "Say 'Hello from Ollama!' in one sentence."
)

$headers = @{
    "Content-Type" = "application/json"
}

# Add API key if provided (Ollama local typically doesn't need it)
if ($ApiKey -and $ApiKey -ne "ollama") {
    $headers["Authorization"] = "Bearer $ApiKey"
}

$body = @{
    model = $Model
    messages = @(
        @{ role = "user"; content = $Prompt }
    )
    max_tokens = 200
    temperature = 0.7
} | ConvertTo-Json

Write-Host "Testing Ollama..." -ForegroundColor Cyan
Write-Host "URL: $BaseUrl/chat/completions"
Write-Host "Model: $Model"
if ($ApiKey -and $ApiKey -ne "ollama") {
    Write-Host "API Key: Configured" -ForegroundColor Green
}
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/chat/completions" `
        -Method POST `
        -Headers $headers `
        -Body $body `
        -TimeoutSec 60

    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Yellow
    Write-Host $response.choices[0].message.content
}
catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
    Write-Host "Make sure Ollama is running: ollama serve"
}
