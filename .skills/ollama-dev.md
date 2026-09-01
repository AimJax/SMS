# Ollama Development Skill

## Purpose
Configure and test Ollama for AI content generation in the Social Media Simulator.

## Prerequisites
1. Install Ollama: https://ollama.ai
2. Pull a model: `ollama pull qwen3:4b`

## Quick Setup Commands

### Start Ollama Server
```powershell
ollama serve
```

### Verify Ollama is Running
```powershell
Invoke-RestMethod -Uri "http://localhost:11434/api/tags" -Method GET
```

### Test Ollama Chat
```powershell
$body = @{
    model = "qwen3:4b"
    messages = @(
        @{role = "system"; content = "You are a social media user."}
        @{role = "user"; content = "Write a short post about coding."}
    )
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:11434/v1/chat/completions" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

## Configure Server for Ollama

### API Endpoint
```
POST /api/admin/ai/config
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
    "provider": "Generic",
    "model": "qwen3:4b",
    "apiKey": "eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG",
    "baseUrl": "http://localhost:11434/v1",
    "isEnabled": true,
    "timeoutSeconds": 60
}
```

### Test AI Connection
```
POST /api/admin/ai/test
Authorization: Bearer <your-jwt-token>
```

## Testing Workflow

1. Start Ollama: `ollama serve`
2. Verify running: GET `http://localhost:11434/api/tags`
3. Test locally with curl/pwsh
4. Configure server via `/api/admin/ai/config`
5. Test via `/api/admin/ai/test`

## Notes
- **API Key** is optional for local Ollama, but configured key works
- **Base URL** must end with `/v1` for OpenAI-compatible API
- **Model:** qwen3:4b (8B parameters)
- Default port: **11434**
- For Android emulator: use `http://10.0.2.2:11434/v1`

## Troubleshooting

### Ollama not responding?
```powershell
# Check if Ollama is running
tasklist | findstr ollama
# Or restart
ollama serve
```

### Model not found?
```powershell
ollama pull qwen3:4b
```
