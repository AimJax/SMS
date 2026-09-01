# SOCIAL MEDIA SIMULATOR — PART 01B
# LOCAL REPOSITORY FOUNDATION

We are continuing the Social Media Simulator project from the completed **Part 01A Development Environment checkpoint**.

The development environment has already been verified.

Do NOT repeat Part 01A unless a tool is actually missing or broken.

---

# CURRENT CHECKPOINT

Part 01A is complete.

Verified development environment:

```text
.NET SDK:        10.0.400
Git:             2.55.0
SQLite CLI:      3.44.4
Android SDK:     Platforms 35/36
Build Tools:     36.0.0
OpenJDK:         21.0.8
ADB:             1.0.41
.NET MAUI:       Android workload installed
```

No game systems have been implemented yet.

The next task is:

# PART 01B — REPOSITORY FOUNDATION

---

# 1. IMPORTANT: LOCAL PROJECT ONLY

You are working with the **local project files I provide**.

Do NOT clone a GitHub repository.

Do NOT download the project from GitHub.

Do NOT assume GitHub is already configured.

Do NOT require me to give you a GitHub URL.

My GitHub username is:

```text
AimJax
```

This is useful for future Git configuration, but **DO NOT push anything to GitHub during this checkpoint unless I explicitly tell you to.**

The local filesystem is the source of truth.

---

# 2. FIRST ACTION — INSPECT

Before creating anything, inspect the current directory/project.

Determine:

```text
Current directory
Existing files
Existing folders
Existing solution files
Existing .csproj files
Existing .sln/.slnx files
Existing Git repository
Existing README
Existing .gitignore
Existing client project
Existing server project
Existing shared project
Existing tests
Existing configuration
```

Do NOT blindly create files.

If something already exists and is correct, reuse it.

If something exists but is incorrect, explain the problem before replacing it.

---

# 3. DO NOT DESTROY EXISTING WORK

Never casually:

```text
Delete directories
Delete projects
Delete source files
Overwrite working projects
Reset Git
Delete databases
Delete configuration
```

If the directory already contains project work, preserve it.

The goal is to establish the foundation around the existing project, not destroy it and start over.

---

# 4. TARGET REPOSITORY STRUCTURE

The intended high-level structure is:

```text
SocialMediaSimulator/
│
├── Client/
│
├── Server/
│
├── Shared/
│
├── Database/
│
├── Tests/
│
├── Documentation/
│
├── .gitignore
├── README.md
└── SocialMediaSimulator.sln
```

A `.slnx` solution is also acceptable if that is what the installed .NET tooling/project uses.

Do NOT force the exact structure if the existing project has a technically better organization.

The important requirement is separation of responsibilities.

---

# 5. PROJECT RESPONSIBILITIES

The intended responsibilities are:

## Client

Android application.

Eventually responsible for:

```text
UI
User input
API communication
WebSocket communication
Local cache
Client presentation
```

It must NOT own authoritative world state.

---

## Server

ASP.NET Core backend.

Eventually responsible for:

```text
Authentication
Game state
Simulation
Social graph
Posts
Feed
Relationships
Events
NPCs
Persistence
Moderation
LLM communication
```

The server is authoritative.

---

## Shared

Only genuinely shared contracts/models should go here.

Do NOT dump server domain logic into Shared.

Do NOT create Shared classes just because they might be useful someday.

---

## Database

Database-related tooling, schema/migrations/documentation where appropriate.

Do NOT create the entire future database schema now.

---

## Tests

Automated tests.

Do not create hundreds of empty test files.

Create the test infrastructure only if appropriate for the current foundation.

---

## Documentation

Project documentation that is actually useful.

Do not create massive documentation files describing systems that don't exist yet.

---

# 6. TECHNOLOGY

Preferred technology direction:

```text
Client:
C# / .NET MAUI Android

Server:
C# / ASP.NET Core

Database:
SQLite

AI later:
Ollama + Qwen
```

Do NOT add:

```text
Unity
Unreal Engine
Node.js backend
Python backend
MongoDB
PostgreSQL
MySQL
Redis
Docker
Kubernetes
Microservices
```

unless there is a concrete requirement discovered later that justifies changing the architecture.

We are keeping the initial project simple.

---

# 7. GIT FOUNDATION

Initialize Git locally if it is not already initialized.

Configure the repository appropriately.

The intended Git identity is associated with:

```text
GitHub username: AimJax
```

Do NOT invent an email address.

If Git user.name/user.email are not configured, report that rather than inventing values.

Create a proper `.gitignore`.

The `.gitignore` must exclude appropriate generated/development files such as:

```text
bin/
obj/
.vs/
.vscode/ where appropriate
.idea/ where appropriate
Android build output
temporary files
logs
local databases where appropriate
secrets
user-specific files
generated artifacts
```

However:

# DO NOT blindly ignore files that need to be version controlled.

For example, project files, source code, configuration templates, migrations, documentation, and other required project assets should remain tracked.

---

# 8. README

Create a concise initial README if one does not already exist.

It should identify:

```text
Project:
Social Media Simulator

Purpose:
Persistent online social-media simulation.

Technology:
C# / .NET
ASP.NET Core
.NET MAUI Android
SQLite

Architecture:
Android Client
      ↓
ASP.NET Core Server
      ↓
SQLite

Current Stage:
Part 01B — Repository Foundation
```

Do NOT write the entire master development specification into the README.

Keep it useful and maintainable.

---

# 9. SOLUTION

If the project does not already have a proper solution:

Create the appropriate .NET solution.

The eventual solution should allow the major projects to be managed together.

Potential structure:

```text
SocialMediaSimulator.sln
```

or the appropriate modern `.slnx` equivalent.

Do not create unnecessary projects.

At this stage, only create projects that are actually required for the foundation.

---

# 10. CLIENT

If the Android client does not exist yet, create the appropriate .NET MAUI Android project.

Do NOT implement the social-media UI yet.

Do NOT create:

```text
Feed
Profiles
Posts
Comments
DMs
Notifications
NPC UI
```

The client only needs a clean foundation.

---

# 11. SERVER

If the ASP.NET Core server does not exist yet, create the appropriate ASP.NET Core project.

Do NOT implement:

```text
Accounts
NPCs
Social graph
Feed
Relationships
Events
Virality
LLM
```

Those are future tasks.

The server only needs the foundation required for the next checkpoint.

---

# 12. DATABASE

Do NOT build the complete SQLite database yet.

Do NOT create dozens of tables.

Do NOT create:

```text
Posts
Relationships
Memories
Events
Rumors
Trends
News
NPCs
```

yet.

Database implementation belongs to the later foundation checkpoint.

At this stage, only establish the project structure needed to add SQLite cleanly later.

---

# 13. CONFIGURATION

Establish a clean configuration strategy.

Eventually we need configurable:

```text
Environment
Server URL
API URL
WebSocket URL
Database location
LLM endpoint
```

Do not hardcode developer IP addresses.

Do not put secrets into source control.

Do not add production credentials.

For now, create only configuration infrastructure actually required by the current project.

---

# 14. NO PREMATURE SYSTEMS

This is extremely important.

DO NOT create empty placeholder implementations for future systems such as:

```text
NPCManager
RelationshipManager
MemoryManager
EventManager
FeedManager
ViralityManager
DramaManager
LLMManager
WorldManager
SocialManager
```

unless the current foundation genuinely requires them.

Do not create architecture for architecture's sake.

---

# 15. NO GOD CLASS

Do not create:

```text
GameManager
SocialMediaManager
WorldManager
EverythingManager
```

that contains the entire application.

Keep responsibilities separated.

---

# 16. BUILD VERIFICATION

After the repository structure is established:

Build every project that currently exists.

For example:

```text
dotnet build
```

or the appropriate project-specific build command.

If Android tooling requires a different command, use the appropriate command.

Record:

```text
Build succeeded
```

or:

```text
Build failed
```

with the actual error.

---

# 17. DO NOT HIDE ERRORS

If something fails:

```text
STOP
 ↓
Read actual error
 ↓
Determine root cause
 ↓
Fix
 ↓
Build again
 ↓
Verify
```

Do not work around an error by disabling warnings/errors or changing unrelated settings.

Do not claim success if the build was not successful.

---

# 18. FIRST LOCAL GIT CHECKPOINT

Once the repository foundation is actually working:

Check:

```text
git status
```

Review what will be committed.

Make sure there are no accidental:

```text
Secrets
Credentials
Huge generated directories
Build artifacts
Temporary files
Personal files
```

Then create the first meaningful local commit.

Suggested commit message:

```text
Initialize Social Media Simulator project foundation
```

Do NOT push to GitHub yet.

---

# 19. WHAT COUNTS AS SUCCESS

Part 01B is successful when:

```text
Repository structure exists
        ↓
Solution/projects are valid
        ↓
Git repository is initialized
        ↓
.gitignore works
        ↓
README exists
        ↓
Projects build successfully
        ↓
No unnecessary future systems were created
        ↓
First local Git commit succeeds
```

---

# 20. WHAT NOT TO DO

Absolutely do NOT:

- Jump to NPCs.
- Create 10,000 accounts.
- Implement LLM integration.
- Install Ollama.
- Implement Qwen.
- Build the feed algorithm.
- Build relationships.
- Build events.
- Build virality.
- Build romance.
- Build drama.
- Build memories.
- Build trends.
- Build news.
- Build the social graph.
- Build the full database.
- Build advanced UI.
- Deploy to a server.
- Push to GitHub.
- Create unnecessary abstractions.
- Rewrite working code.
- Delete existing project files without justification.

---

# 21. PERMANENT DATA RULE

This project requires permanent history.

Never introduce automatic pruning.

Never create systems that automatically delete:

```text
Posts
Events
Messages
Memories
Relationships
Metrics
Account history
Historical records
```

Performance must eventually be solved using:

```text
Indexes
Pagination
Caching
Aggregation
Batching
Efficient queries
Background processing
Simulation tiers
```

NOT deletion.

---

# 22. REQUIRED RESPONSE FORMAT

After inspection, report:

## CURRENT DIRECTORY

Where you are working.

## PROJECT STRUCTURE

What currently exists.

## EXISTING PROJECTS

List the actual projects discovered.

## EXISTING GIT STATE

Whether Git already exists and its current status.

## CHANGES NEEDED

Only the changes actually required.

## IMPLEMENTATION

Perform the required foundation work.

## VERIFICATION

Show actual build/test results.

## GIT CHECKPOINT

Create the local commit if everything succeeds.

## FINAL STATE

Show the resulting structure and what is now working.

---

# 23. STOP CONDITION

When Part 01B is complete:

# STOP.

Do not continue to Part 01C.

Do not create the ASP.NET health endpoint yet unless it is required as part of the current repository setup.

Do not create SQLite functionality yet.

Do not start Android ↔ Server communication yet.

Those are separate checkpoints.

---

# NEXT CHECKPOINT

After Part 01B is verified, the next task will be:

# PART 01C — ASP.NET CORE SERVER

That task will establish:

```text
ASP.NET Core
      ↓
GET /api/health
      ↓
{
    "status": "ok"
}
```

But DO NOT implement Part 01C during this task.

---

# FINAL INSTRUCTION

You are not being asked to build the game yet.

You are building the foundation that the game will eventually grow on.

Work from the actual local files.

Inspect first.

Change only what is necessary.

Build.

Verify.

Commit.

Stop.

# ONE WORKING CHECKPOINT AT A TIME.