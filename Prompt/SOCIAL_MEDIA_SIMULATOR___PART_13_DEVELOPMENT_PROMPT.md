# SOCIAL MEDIA SIMULATOR — PART 13 DEVELOPMENT PROMPT
## AI-POWERED NPC CONTENT GENERATION (PROVIDER-AGNOSTIC, USER-SWAPPABLE API KEY)

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment       COMPLETE
01B  Repository Foundation         COMPLETE
01C  ASP.NET Core Server           COMPLETE
01D  SQLite Foundation             COMPLETE
01E  Android Client Foundation     COMPLETE
01F  Foundation Checkpoint         COMPLETE
02   Backend Architecture          COMPLETE
03   Persistence                   COMPLETE
04   Accounts & Authentication     COMPLETE
05   Social Graph                  COMPLETE
06   Posts & Engagement            COMPLETE
07   Feed & Timeline               COMPLETE
08   NPC Simulator Foundation      COMPLETE
09   NPC Population Generation     COMPLETE
10   NPC Behavior Simulation       COMPLETE
11   NPC Background Simulation     COMPLETE
12   NPC Social Graph              COMPLETE
```

Latest commit:

```text
c507a75 — Implement NPC-to-NPC social graph behavior (Part 12)
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

---

# 0. FIRST ACTION — RESOLVE PART 12 PUSH STATE

Part 12's session report recorded, again:

```text
Push: TIMEOUT (local commit exists, push pending network)
Verified against origin: Previous commit e6c4c0e confirmed on remote
```

This is now the **second consecutive part** where the push reportedly timed out. Before any Part 13 work:

1. Run `git status` and `git log --oneline -5`. Confirm the local commit `c507a75` exists.
2. Attempt `git push origin main` again.
3. Verify the remote actually has `c507a75` (fetch and compare, or check the remote branch directly) — do not accept "the command didn't error" as proof.
4. If it fails again, investigate why pushes are timing out (network configuration, remote size, credentials, a large accidental file, etc.) rather than repeatedly retrying blindly. Report the actual root cause if found.
5. Do NOT force-push. Do NOT rewrite history.
6. Only proceed to Part 13 once local and remote are confirmed aligned.

Report this resolution explicitly in the final session report for this part.

---

# 1. THE EXISTING PROJECT

The existing backend already contains, among everything from Parts 01–12:

- `NpcSimulationHostedService` — autonomous background tick loop with pause/resume/status, overlap prevention, failure isolation (Part 11)
- `NpcBehaviorService`, `NpcDecisionService`, `ContentRelevanceService` — decision-making pipeline
- `IContentGeneratorService` / `ContentGeneratorService` — **template-based** post/comment generation (Part 10)
- `NpcSocialGraphService` — interest/reciprocity/exploration-driven follow behavior (Part 12)
- NPC profiles with personalities (Big Five), interests, and account types
- 138 passing automated tests

Currently, NPC-authored posts and comments come entirely from **static templates** (`ContentGeneratorService`). Part 13 replaces/augments this with real AI-generated text.

---

# 2. IMPORTANT CHANGE FROM THE ORIGINAL MASTER SPEC

The original project direction referenced **Ollama + Qwen** (a locally-hosted model) as the eventual content-generation engine.

**This is now changed.** Per explicit instruction:

> Do NOT natively restrict this to Ollama/local AI. Instead, support **any AI provider's API** (OpenAI, Anthropic, Google, DeepSeek, or any other API-key-based provider). The user must be able to **change the API key (and provider/model) at any time**, easily, to their heart's content.

This means:

- The system must be built around a **provider-agnostic abstraction**, not a single hardcoded HTTP client for one vendor.
- **API keys and provider/model selection must be runtime-configurable**, not baked into a single hardcoded config value that requires a rebuild to change.
- Local models (Ollama, etc.) may still be supported as *one* provider option if it's cheap to keep, but it must not be the only option, and nothing should assume it's the default going forward.
- Do NOT hardcode a single vendor's SDK as the only path through the application. Business logic (`NpcBehaviorService`, tick processing, etc.) must depend only on the abstraction, never on a specific vendor's client type.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

All AI calls happen server-side. The client never calls an AI provider directly and never sees API keys.

## Layered architecture

```text
API
Application
Domain
Infrastructure
Contracts
```

The AI provider abstraction and its concrete implementations belong in **Infrastructure**. `NpcBehaviorService`/`ContentGeneratorService` depend only on the **Application-layer interface**.

## Secrets safety

API keys must **never** be committed to Git, must **never** appear in the README, logs, or API responses, and must be stored/transmitted in a way consistent with the project's existing configuration/secrets conventions (inspect how JWT signing keys or other secrets are currently handled, and follow the same pattern).

## Reuse, don't duplicate

`ContentGeneratorService`'s existing responsibilities (assembling context: NPC personality, interests, recent posts, what's being replied to) should be reused/extended, not rebuilt, when constructing prompts for the AI provider.

---

# PART 13 OBJECTIVE

Build a **provider-agnostic AI content generation layer** that:

1. Can call **any** text-generation API (OpenAI-compatible, Anthropic, Google, DeepSeek, others) through a single internal abstraction.
2. Lets the user configure **which provider, which model, and which API key** to use — and **change any of these at runtime**, without redeploying or rebuilding the server.
3. Generates NPC posts/comments that reflect the NPC's personality, interests, and account type, replacing (or augmenting, with graceful fallback) the Part 10 template system.
4. Fails safely: if no key is configured, the provider is unreachable, or a call errors/times out, the system falls back to the existing template-based generator rather than breaking the tick loop.

Do NOT implement:

- Image/video generation.
- Fine-tuning or embeddings/vector search.
- A full plugin marketplace of providers — a small, clean set of first-class supported providers plus a generic "OpenAI-compatible" fallback is sufficient.
- Streaming responses to the client (not needed — this is server-side background content generation).

---

# PART 13 — REQUIRED FEATURES

## 1. Provider-agnostic abstraction

Define an application-layer interface, for example:

```text
IAiTextGenerationService
{
    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken ct);
}
```

`AiGenerationRequest`/`AiGenerationResult` should be simple, provider-neutral DTOs (prompt/system-context in, generated text + success/failure + basic metadata out). Nothing in this interface or its callers should reference a specific vendor's SDK types.

---

## 2. Concrete provider implementations

Implement at least:

```text
OpenAiProvider          (OpenAI-compatible chat/completions API)
AnthropicProvider       (Anthropic Messages API)
GenericHttpProvider     (Configurable OpenAI-compatible endpoint — covers DeepSeek, local
                         OpenAI-compatible servers, Ollama's OpenAI-compatible mode, etc.)
```

Adding Google/Gemini as a fourth first-class provider is encouraged if time allows, but at minimum ensure the abstraction makes adding it later a matter of implementing one more class, not restructuring the system.

Each provider implementation is responsible only for translating the neutral request/response into that vendor's actual HTTP request/response shape and handling that vendor's auth header convention. Business logic must not leak into these classes.

---

## 3. Runtime provider/key configuration

Design configuration so the **active provider, model name, and API key can be changed without a server rebuild or redeploy**. At minimum:

- Store the active configuration (provider selection, model, API key, base URL/endpoint override for the generic provider) in a way that can be updated at runtime — e.g., persisted in the existing SQLite database (a small settings table/entity) or another mechanism consistent with existing project conventions, NOT solely in `appsettings.json` (which would require a restart to change).
- On startup, load the last-saved configuration if present; otherwise start with AI generation disabled/falling back to templates.
- Changing configuration must take effect for subsequent generation calls without restarting the server.

Document exactly which mechanism you chose and why it satisfies "change the API key at any time, easily."

---

## 4. Admin configuration endpoints

Add authenticated admin endpoints to manage this at runtime, for example:

```http
GET  /api/admin/ai/config        -- current provider/model, key presence (never the raw key), status
PUT  /api/admin/ai/config        -- set provider, model, API key, optional base URL
POST /api/admin/ai/test          -- send a small test prompt to verify the current config actually works
```

- `GET` must never return the raw API key — return whether one is configured (e.g., `hasApiKey: true`) and perhaps a masked form (e.g., last 4 characters), never the full value.
- `PUT` should validate the provider name against the supported set and reject unknown providers with a clear error.
- `POST /test` should perform a real (small/cheap) call to confirm the key/provider/model combination actually works, and report success/failure with the actual error if it fails, without leaking the key in the error message.
- Use the existing authentication/authorization conventions from Part 11 (admin/authenticated endpoints) — do not build a new auth mechanism.

---

## 5. Secure storage of the API key

The API key must not be stored or logged in plaintext in application logs. At minimum:

- Never log the raw key.
- If stored in the database, document whether it is stored plaintext or encrypted, and if plaintext, explicitly flag this as a known limitation appropriate for a local/dev-oriented project (do not over-engineer a full secrets vault for this checkpoint, but do not pretend it's more secure than it is either).
- Ensure the key is excluded from any general-purpose "dump the database" style debug tooling if such tooling exists.

---

## 6. Prompt construction

Extend the existing content-generation context assembly (from `ContentGeneratorService`/Part 10) to build prompts that include, at minimum:

```text
NPC personality (Big Five summary)
NPC interests
NPC account type (affects tone: News writes differently than an ordinary user)
What is being responded to, for comments (the post/comment content)
A length/format constraint appropriate for a social media post or comment
```

Keep prompt construction in a dedicated, testable component (e.g., `AiPromptBuilder` or similar) rather than inline string concatenation scattered through the provider classes.

---

## 7. Fallback behavior

If any of the following occur, the system must **fall back to the existing Part 10 template-based `ContentGeneratorService`** rather than failing the tick or leaving a post/comment ungenerated:

```text
No AI provider configured
No API key configured
Provider call times out
Provider call returns an error (auth failure, rate limit, malformed response, etc.)
```

Log the fallback occurrence (without leaking the key) but do not let it crash or skip the NPC's turn. Reuse Part 11's failure-isolation guarantee — a single failed AI call must not take down the tick loop.

---

## 8. Timeouts and tick loop impact

AI provider calls are inherently slower and less predictable than local logic. Ensure:

- Each AI call has a reasonable, configurable timeout (document the default).
- A slow/hanging AI call for one NPC does not stall the entire tick or block other NPCs from being processed within the same tick's time budget.
- Reuse `MaxNpcsPerTick` (Part 11) and consider whether AI-generation calls need their own smaller per-tick cap to avoid excessive cost/latency (document your decision either way).

---

## 9. Cost/rate awareness (lightweight)

Do not build a full billing system. Do, however:

- Make it easy to disable AI generation entirely (falls back to templates) via the same runtime config from Section 3, e.g. a simple `Enabled` flag independent of whether a key is present, so a user can keep a key configured but temporarily pause AI usage.
- Optionally track and expose a simple running count of AI calls made (success/failure) via the observability extension in Section 10, so the user has some visibility into usage without needing a full dashboard.

---

## 10. Observability

Extend the existing `GET /api/admin/simulation/status` endpoint (Parts 11–12) or the new `GET /api/admin/ai/config` endpoint with basic AI generation metrics, for example:

```text
Total AI generation attempts
Total AI generation successes
Total AI generation fallbacks (to templates)
Last AI generation error (if any), key redacted
Currently configured provider/model (key redacted)
```

---

## 11. Tests

Add tests appropriate to this part. Real network calls to actual AI providers must **not** be part of the automated test suite (no live API calls in CI/tests — no committed real API keys, ever). At minimum verify, using mocked/fake implementations of `IAiTextGenerationService` or its HTTP dependencies:

### Abstraction correctness

```text
Business logic (NpcBehaviorService / content generation call site)
depends only on IAiTextGenerationService, not on any concrete provider type
```

### Provider selection

```text
Given configuration for Provider = "OpenAI", the OpenAiProvider is invoked
Given configuration for Provider = "Anthropic", the AnthropicProvider is invoked
Given configuration for Provider = "Generic" with a base URL, GenericHttpProvider is invoked
Given an unknown/unsupported provider name in PUT config, the request is rejected with a clear error
```

### Runtime reconfiguration

```text
Config is set to Provider A
A generation call is made (mocked) → Provider A's implementation is used
Config is updated to Provider B via PUT /api/admin/ai/config, WITHOUT restarting the app
A subsequent generation call (mocked) → Provider B's implementation is used
```

### Key redaction

```text
GET /api/admin/ai/config never returns the raw API key in the response body
Logs produced during a (mocked) failed call do not contain the raw API key
```

### Fallback behavior

```text
No provider configured → content generation falls back to ContentGeneratorService templates
Configured provider mock throws/times out → falls back to templates, tick continues
AI generation disabled via Enabled=false → falls back to templates even if a key is present
```

### Failure isolation regression

```text
A forced AI-call failure for one NPC does not crash the tick or stop the background loop
(reuse/extend Part 11's failure-isolation tests)
```

### Test endpoint

```text
POST /api/admin/ai/test with a valid mocked provider → reports success
POST /api/admin/ai/test with a mocked failure → reports failure with a non-key-leaking error message
```

### Regression

```text
Existing Parts 05-12 tests still pass, including social-graph and background-service tests
```

---

## 12. Database migration

Create a migration only if you choose to persist AI configuration in SQLite (Section 3), which is the expected approach. Document the new table/entity (e.g., `AiProviderConfig`: Provider, Model, ApiKey, BaseUrl, Enabled, UpdatedAt) and confirm the migration applies cleanly.

---

## 13. Android

Part 13 is backend-only. Do NOT build any Android UI for AI configuration in this part.

---

## 14. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:

- Part 13 completion
- Part 12 push-state resolution (Section 0), including root cause if found
- The change from the original Ollama-only plan to a provider-agnostic design, and why
- Supported providers and how to add a new one
- How to configure/change the provider, model, and API key at runtime (concrete steps: which endpoint, what payload)
- Where/how the API key is stored and the security caveat if stored plaintext
- Fallback behavior and when it triggers
- Timeout/cost-awareness decisions
- New admin endpoints
- Observability metrics
- Tests performed and results
- Current project status
- Next planned part

---

## 15. Git

After implementation and verification:

1. Inspect `git status`.
2. Review changed files — **triple-check that no real API key, `.env` file, or secret ever gets staged**, even accidentally during testing.
3. Ensure `.gitignore` covers any local secrets file you introduce for development convenience, if any.
4. Commit the completed work.

Suggested commit message:

```text
Implement provider-agnostic AI content generation (Part 13)
```

Push to `origin/main`. Verify the push actually reached the remote per Section 0's approach before reporting success.

---

## 16. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 13:

```text
Image/video generation
Embeddings / vector search / semantic memory
A full secrets vault or external key-management service
A provider marketplace/plugin system beyond the small first-class set
Android UI for AI configuration
Notifications
Trending/virality mechanics
Multi-tier simulation (active vs dormant NPC pools)
```

Those belong to later parts or are out of scope.

---

## 17. DEVELOPMENT PROCESS

Before changing anything:

1. Resolve the Part 12 push state (Section 0).
2. Inspect `ContentGeneratorService` / `IContentGeneratorService` and how it's currently invoked from `NpcBehaviorService`.
3. Inspect how existing secrets (e.g., JWT signing key) are currently configured, to follow a consistent convention.
4. Inspect `NpcSimulationHostedService` / `SimulationStateService` for how per-tick work and failure isolation currently work.
5. Inspect the existing admin endpoint conventions (`SimulationController` from Part 11) for auth patterns.
6. Inspect existing DI registration conventions (`ServiceCollectionExtensions.cs`).
7. Inspect the README.

Then implement Part 13. Do not assume a file does not exist merely because this prompt says to create it. Reuse existing functionality wherever appropriate. Do not duplicate business logic. Do not perform unrelated refactoring.

---

## 18. QUALITY REQUIREMENTS

The implementation must be:

- correct
- persistent (configuration survives restart)
- server-authoritative
- provider-agnostic at the abstraction boundary
- runtime-reconfigurable without redeploy
- secure with respect to API key handling (no leaks in logs/responses/README/Git)
- resilient to provider failures (falls back cleanly, reuses Part 11 isolation)
- testable without any live network/API calls
- maintainable
- compatible with the existing architecture

---

## 19. FINAL VERIFICATION

Before declaring Part 13 complete, verify:

```text
Part 12 commit confirmed pushed to origin/main (root cause of prior timeouts addressed or documented)
Server builds
IAiTextGenerationService abstraction has zero vendor-specific leakage into business logic
At least two concrete providers implemented (OpenAI, Anthropic) plus a generic HTTP provider
Provider/model/API key configurable and changeable at runtime via admin endpoint, no restart required
GET /api/admin/ai/config never exposes the raw key
POST /api/admin/ai/test correctly reports success/failure without leaking the key
Fallback to template generation works in all documented failure cases
AI generation can be fully disabled via config while keeping the key stored
A forced AI-call failure does not crash the tick loop (Part 11 isolation still holds)
No API keys committed to Git, ever, in any file
Existing Parts 05-12 tests still pass
README updated
Git commit created
Git push succeeds and is verified against origin
Working tree clean
```

---

## 20. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 13 — COMPLETE

## 1. Part 12 Push Resolution
...

## 2. What Was Inspected
...

## 3. What Already Existed
...

## 4. What Changed
...

## 5. Provider-Agnostic Architecture
...

## 6. Supported Providers
...

## 7. Runtime Configuration Mechanism
...

## 8. API Key Security
...

## 9. Fallback Behavior
...

## 10. Timeout / Cost Awareness
...

## 11. Observability
...

## 12. Tests
...

## 13. README
Updated: YES
...

## 14. Git
Commit: ...
Push: ...
Verified against origin: ...
Working tree: ...

## 15. Current Project Status

01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
02  COMPLETE
03  COMPLETE
04  COMPLETE
05  COMPLETE
06  COMPLETE
07  COMPLETE
08  COMPLETE
09  COMPLETE
10  COMPLETE
11  COMPLETE
12  COMPLETE
13  COMPLETE

## 16. Intentionally Not Implemented
- Image/video generation
- Embeddings / vector search
- Full secrets vault
- Android AI-configuration UI

## 17. NEXT

NEXT: PART 14 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 13 and reporting the session log.**
