# SOCIAL MEDIA SIMULATOR — PART 02

# BACKEND ARCHITECTURE

## CURRENT PROJECT STATE

We are continuing the existing Social Media Simulator project.

Project location:

```text
D:\SMS
```

Completed:

```text
01A — Development Environment       COMPLETE
01B — Repository Foundation         COMPLETE
01C — ASP.NET Core Server           COMPLETE
01D — SQLite Foundation             COMPLETE
01E — Android Client Foundation     COMPLETE
01F — Foundation Checkpoint         COMPLETE
```

The foundation is working.

The current basic architecture is:

```text
Android Client
      ↓
HTTP
      ↓
ASP.NET Core
      ↓
SQLite
```

The project must now proceed to:

# PART 02 — BACKEND ARCHITECTURE

---

# MOST IMPORTANT RULE

Before doing anything:

> **INSPECT THE EXISTING PROJECT.**

Do not assume the current project exactly matches the previous description.

Inspect:

```text
D:\SMS
```

Inspect:

```text
Server/
Client/
Shared/
Database/
Tests/
README.md
```

Inspect the current solution/project files.

Inspect:

```text
Program.cs
.csproj
appsettings.json
Controllers
Services
Data/Persistence
Models
DTOs
Tests
```

Use the actual existing project as the source of truth.

Do not recreate anything that already exists.

Do not create duplicate classes.

Do not replace working code unnecessarily.

---

# OBJECTIVE

The purpose of Part 02 is to establish a clean backend architecture that can support the eventual social-media simulation.

We are NOT building the social network yet.

We are creating the structure that will allow future systems to be added without turning the backend into an unmaintainable monolith.

The backend will eventually contain:

```text
Authentication
Accounts
Profiles
Social Graph
Posts
Comments
Likes
Feed
Communities
Relationships
Opinions
NPC Simulation
Events
Trends
Virality
Rumors
News
Memories
LLM
Notifications
Moderation
Synchronization
```

But NONE of those systems should be implemented during this task unless required by the architecture itself.

---

# ARCHITECTURAL TARGET

Move toward a structure based on clear responsibilities.

A reasonable target is:

```text
Server/
│
├── API/
│   ├── Controllers/
│   ├── Contracts/
│   ├── Middleware/
│   └── Validation/
│
├── Application/
│   ├── Services/
│   ├── Interfaces/
│   └── DTOs/
│
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   └── Events/
│
├── Infrastructure/
│   ├── Persistence/
│   ├── Configuration/
│   └── External/
│
└── Program.cs
```

However:

> This is a target architecture, NOT a requirement to blindly create every folder.

If the existing project has a better equivalent structure, preserve it.

Do not create empty architectural folders just for appearance.

---

# CORE RESPONSIBILITIES

The backend should conceptually separate these responsibilities:

## API

Responsible for:

```text
HTTP
Routing
Request handling
Authentication boundary
Validation boundary
HTTP responses
```

Controllers/endpoints should be thin.

They should NOT contain large amounts of business logic.

Bad:

```text
Controller
    ↓
200 lines of simulation logic
    ↓
SQL queries
    ↓
database updates
```

Good:

```text
Controller
    ↓
Application Service
    ↓
Domain logic
    ↓
Persistence
```

---

# APPLICATION LAYER

The Application layer coordinates use cases.

Examples that will exist later:

```text
CreateAccount
FollowUser
CreatePost
LikePost
CreateComment
SendMessage
CreateEvent
ProcessSimulationTick
GenerateFeed
```

For now, do not implement all of these.

The architecture simply needs to provide an appropriate place for them.

Application services should coordinate operations rather than becoming giant global managers.

---

# DOMAIN LAYER

The Domain layer represents actual game concepts and business rules.

Eventually this will contain concepts such as:

```text
Account
Profile
Post
Relationship
Community
Event
Trend
Rumor
Memory
Notification
```

Do not create the entire domain model now.

Only create domain structures that are genuinely needed for the current architecture/checkpoint.

The domain must remain independent of:

```text
ASP.NET Core
SQLite
HTTP
Android
Ollama
```

The domain should not know about HTTP requests.

The domain should not directly execute SQL.

The domain should not call Ollama.

---

# INFRASTRUCTURE

Infrastructure handles external implementation details.

Examples:

```text
SQLite
Entity Framework Core
Database connections
File storage
External APIs
Ollama client
```

Future systems can depend on infrastructure through interfaces where appropriate.

Do not prematurely build the Ollama integration.

Do not prematurely build external service abstractions for services that do not exist yet.

---

# PERSISTENCE

The existing SQLite implementation must be preserved.

Do not replace it just because another database architecture might eventually be useful.

The goal is to make persistence accessible through a clean boundary.

Conceptually:

```text
Application
      ↓
Persistence abstraction
      ↓
SQLite implementation
```

Avoid:

```text
Controller
      ↓
SQL
```

Avoid scattering database code throughout the project.

---

# DEPENDENCY DIRECTION

Prefer dependency flow similar to:

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
implements required abstractions
```

The exact implementation can differ if the existing codebase has a sound alternative.

The important principle is:

> Core game logic should not become tightly coupled to ASP.NET Core or SQLite.

---

# DEPENDENCY INJECTION

Use ASP.NET Core's built-in dependency injection.

Register services through the existing application startup structure.

Avoid:

```text
new SomeService()
```

throughout controllers and business code when dependency injection is appropriate.

Prefer constructor injection.

Example concept:

```text
Controller
    ↓
IAccountService
    ↓
AccountService
```

Do not create a service locator.

Do not create a giant:

```text
GameManager
```

that contains every future system.

---

# INTERFACES

Use interfaces when they provide an actual architectural boundary.

Good examples:

```text
IAccountRepository
IUnitOfWork
IFeedService
ILLMService
```

when those systems actually exist.

Do NOT create 100 empty interfaces for every future system.

Avoid abstraction for abstraction's sake.

The architecture should be practical, not ceremonial.

---

# CONTROLLERS / ENDPOINTS

Keep API endpoints thin.

A controller should generally:

```text
Receive request
 ↓
Validate basic request
 ↓
Call application service
 ↓
Return response
```

It should not:

```text
Execute database queries
Calculate virality
Modify relationships
Generate NPC behavior
Call Ollama directly
```

Those responsibilities belong elsewhere.

---

# DTOs / API CONTRACTS

Do not expose internal domain/database entities directly through the API unless there is a strong reason.

Use request/response contracts where appropriate.

Conceptually:

```text
HTTP Request DTO
      ↓
Application
      ↓
Domain
      ↓
Response DTO
```

This prevents the API contract from becoming permanently tied to the database schema.

Do not create huge DTO frameworks prematurely.

Only establish the pattern.

---

# VALIDATION

Establish a clear place for request validation.

Validation should eventually cover:

```text
Required fields
Length limits
Valid identifiers
Allowed operations
Permissions
Business rules
```

For this checkpoint, only establish what is actually needed.

Do not build the complete validation system for every future feature.

---

# ERROR HANDLING

Establish consistent API error handling.

The backend should not randomly return different error formats from different endpoints.

Create an appropriate mechanism for consistent errors.

Conceptually:

```text
Exception / validation failure
        ↓
Central error handling
        ↓
Consistent HTTP response
```

Do not leak:

```text
Stack traces
Database connection strings
Secrets
Internal file paths
Sensitive server information
```

to clients in production responses.

Development logging may contain useful diagnostics, but API responses should remain appropriate.

---

# LOGGING

Use the existing ASP.NET Core logging system.

Do not create a custom logging framework.

Logging should eventually support:

```text
Information
Warning
Error
Debug
```

Do not flood the console with unnecessary logs.

Do not log secrets.

Do not log passwords or authentication credentials.

For now, establish a sensible foundation rather than building an enormous logging system.

---

# CONFIGURATION

Keep configuration centralized.

Relevant future configuration includes:

```text
Database
Server
API
WebSocket
Ollama
Logging
Environment
```

For now, only configure what actually exists.

Do not add fake configuration for systems that have not been implemented.

Do not hardcode environment-specific values throughout the source code.

Use:

```text
Development
Testing
Production
```

concepts where appropriate.

---

# ENVIRONMENT SEPARATION

The architecture should support:

```text
Development
Testing
Production
```

without requiring source-code rewrites.

Development configuration may point to local services.

Production configuration may point to:

```text
Public server
Production database
Separate Ollama server
```

Do not hardcode these values into business logic.

---

# REQUEST / RESPONSE PIPELINE

Establish a sensible ASP.NET Core request pipeline.

The exact order must follow ASP.NET Core requirements and the existing application.

The architecture should eventually support:

```text
Request
 ↓
Middleware
 ↓
Authentication
 ↓
Authorization
 ↓
Endpoint
 ↓
Application
 ↓
Domain
 ↓
Persistence
 ↓
Response
```

Do not implement authentication yet unless the existing project already requires it.

Do not pretend authentication exists if it has not been implemented.

---

# HEALTH ENDPOINT

Preserve:

```text
GET /api/health
```

It must continue working after architectural changes.

The existing health endpoint is part of the foundation and must not be broken.

---

# DATABASE SAFETY

This project has a permanent requirement:

# NEVER AUTOMATICALLY PRUNE HISTORY.

Do not add:

```text
Automatic deletion
Memory pruning
Old-record cleanup
History expiration
```

Database architecture must support permanent historical records.

Do not delete existing database data during this task.

Do not reset the database simply to make migrations easier.

Do not use destructive migrations unnecessarily.

---

# NO FUTURE FEATURE IMPLEMENTATION

Do NOT implement:

```text
Accounts
Authentication
Followers
Posts
Feed
NPCs
Relationships
Events
Virality
Trends
Rumors
News
Memory
Ollama
LLM queue
Drama
Romance
Notifications
Moderation
```

unless something extremely small is required to verify the architecture.

This task is architecture only.

---

# DO NOT OVERENGINEER

Avoid adding:

```text
Microservices
Redis
RabbitMQ
Kafka
Docker orchestration
Kubernetes
PostgreSQL
MongoDB
Elasticsearch
Distributed event streaming
Complex CQRS
Event sourcing everywhere
Generic repository frameworks
Massive dependency injection frameworks
```

simply because they might become useful later.

This project is initially based around:

```text
ASP.NET Core
C#
SQLite
Android
Ollama
```

Keep the architecture appropriate for the current scale.

We can introduce additional infrastructure later if measurements prove it necessary.

---

# IMPORTANT DISTINCTION

Do not confuse:

```text
Internal application events
```

with:

```text
The future Social Media Event System
```

The project will eventually have a major event system representing things happening in the simulated world.

That does NOT need to be implemented now.

Likewise, do not confuse:

```text
Domain events
```

with:

```text
NPC simulation events
```

Keep those concepts separate.

---

# TESTING

Update or create focused tests for the architectural changes.

At minimum verify:

```text
Server builds
Health endpoint works
Existing SQLite functionality works
Dependency injection resolves required services
Existing Android client still communicates successfully
```

Do not create hundreds of tests for nonexistent future systems.

Do not break the existing tests.

---

# REFACTORING RULE

If the current code is already clean enough:

> DO NOT REFACTOR IT JUST TO MAKE IT LOOK DIFFERENT.

Only change architecture where there is a concrete benefit.

If something currently violates the intended separation and would create problems for future development, fix it now.

If something is acceptable, leave it alone.

---

# README REQUIREMENT

This is now a permanent project rule.

After EVERY completed Part or Sub-Part:

```text
IMPLEMENT
 ↓
TEST
 ↓
VERIFY
 ↓
UPDATE README
 ↓
GIT CHECKPOINT
```

Before finishing this task, update:

```text
D:\SMS\README.md
```

The README must accurately document the architecture that actually exists.

Document:

```text
Backend architecture
Layer responsibilities
Project structure
Dependency direction
Persistence boundary
Configuration approach
Testing approach
```

Do not claim systems exist when they do not.

Do not copy the entire master specification into the README.

The README should describe the actual current project.

---

# GIT CHECKPOINT

After successful verification:

```text
git status
```

Review all modifications.

Make sure no unrelated files were changed.

Commit the completed architectural work.

Use a clear commit message such as:

```text
Establish backend architecture
```

or another accurate equivalent.

Then verify the working tree is clean.

---

# REQUIRED DEVELOPMENT PROCESS

Follow this exact process:

```text
1. Inspect
      ↓
2. Understand
      ↓
3. Plan minimal changes
      ↓
4. Implement
      ↓
5. Build
      ↓
6. Test
      ↓
7. Fix
      ↓
8. Rebuild
      ↓
9. Retest
      ↓
10. Update README
      ↓
11. Git checkpoint
      ↓
12. STOP
```

Do not skip the verification step.

---

# IF SOMETHING IS WRONG

If you discover that the current architecture is substantially different from what was expected:

STOP and inspect it carefully.

Do not blindly force the project into the target structure.

Determine whether:

```text
Existing architecture is already good
```

or:

```text
Existing architecture needs adjustment
```

Then make the smallest safe change.

---

# REQUIRED FINAL REPORT

When complete, report:

## 1. What Was Inspected

List the major existing backend components inspected.

## 2. What Changed

List actual architectural changes.

## 3. Final Structure

Show the actual relevant backend structure.

Example:

```text
Server/
├── ...
```

Do NOT show hypothetical folders that do not exist.

## 4. Responsibilities

Explain where:

```text
API
Application
Domain
Infrastructure
Persistence
Configuration
```

are located.

## 5. Tests

Report actual results:

```text
Server Build       PASS/FAIL
Health Endpoint    PASS/FAIL
SQLite             PASS/FAIL
DI                 PASS/FAIL
Android Connection PASS/FAIL
```

Only report PASS when actually verified.

## 6. README

Confirm:

```text
README updated: YES
```

and summarize what was added.

## 7. Git

Report:

```text
Commit:
Working tree:
```

## 8. Current Status

```text
01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
02 COMPLETE
```

## 9. NEXT

State:

```text
NEXT: PART 03 — PERSISTENCE
```

Then:

# STOP.

Do not automatically begin Part 03.

---

# PERMANENT RULES TO REMEMBER

These remain active for the entire project:

```text
Server authoritative
SQLite persistent
No automatic history pruning
No automatic memory deletion
Original historical data remains
Current state and history are separate
C# controls simulation
LLM generates language
Most NPCs do not need LLM calls
10,000 accounts do not mean 10,000 LLM calls
Use simulation tiers
Use aggregation
Use indexed queries
Use caching
Use batching
Keep Android replaceable
Keep backend portable
Avoid giant managers
Avoid unnecessary abstractions
Develop incrementally
Never stack features on broken code
Update README after every part
Create a Git checkpoint after every completed part
```

# START PART 02 NOW.

Work ONLY on the backend architecture.

Do not proceed to Part 03 automatically.