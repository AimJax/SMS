# 🚀 Social Media Simulator (SMS) — Modular Agent Task Execution Template

This template is designed for **iterative, multi-part, agent-driven software development**. Use this prompt structure whenever you need an AI Coding Agent (or LLM Assistant) to perform a specific, single-part development step without premature implementation or breaking existing features.

---

## 📌 HOW TO USE THIS TEMPLATE
1. **Copy the template below** for the specific task/checkpoint you are currently executing.
2. **Fill in the variables in brackets** (e.g., `[PART_NUMBER]`, `[PART_NAME]`, `[CURRENT_GIT_COMMIT]`, `[PREVIOUS_VERIFIED_STATE]`).
3. **Send the prompt to your coding agent**.
4. **Enforce the strict STOP condition** at the end of every response before moving to the next part.

---

```markdown
# SOCIAL MEDIA SIMULATOR — PART [PART_NUMBER]: [PART_NAME]

## 1. CURRENT PROJECT STATE & CHECKPOINT VERIFICATION

You are continuing the development of the **Social Media Simulator** project.

* **Local Project Path:** `D:\SMS` (The local filesystem is the single source of truth. Do NOT clone, re-download, or push to GitHub unless explicitly instructed).
* **GitHub Repository / Owner:** `AimJax`
* **Latest Completed Git Commit:** `[CURRENT_GIT_COMMIT]` (e.g., `628d6de Implement accounts and auth`)
* **Working Tree State:** Clean
* **Verified Completed Parts:**
[PREVIOUS_COMPLETED_PARTS_LIST]
(e.g.,01A	Development Environment	COMPLETE
01B	Repository Foundation	COMPLETE
01C	ASP.NET Core Server	COMPLETE
01D	SQLite Foundation	COMPLETE
01E	Android Client Foundation	COMPLETE
01F	Foundation Checkpoint	COMPLETE
02	Backend Architecture	COMPLETE
03	Persistence	COMPLETE
04	Accounts & Authentication	COMPLETE
05	Social Graph	COMPLETE
06	Posts & Engagement	COMPLETE
07	Feed & Timeline	COMPLETE
08	NPC Simulator Foundation	COMPLETE
09	NPC Population Generation	COMPLETE
10	NPC Behavior Simulation	COMPLETE
11	NPC Background Simulation	COMPLETE
12	NPC Social Graph	COMPLETE
13	AI Content Generation	COMPLETE
14	Notifications System	COMPLETE
)

---

## 2. STRICT SCOPE & SINGLE OBJECTIVE

### 🎯 Primary Objective for Part [PART_NUMBER]:
[INSERT_SPECIFIC_OBJECTIVE_HERE]

### ⛔ WHAT NOT TO BUILD YET (STRICT OUT-OF-SCOPE BOUNDARIES):
Do NOT jump ahead or create empty placeholder implementations for future systems:
[LIST_EXCLUDED_FUTURE_SYSTEMS]
(e.g.,
  - Do NOT implement posts, feeds, comments, or likes.
  - Do NOT implement NPC simulation, 10,000 accounts, or AI personalities.
  - Do NOT implement LLM integration, Ollama, Qwen, or background LLM queues.
  - Do NOT implement virality, trends, rumors, news, or romance systems.
  - Do NOT implement complex UI styling or unrequested frameworks.
)

---

## 3. CORE PERMANENT PROJECT RULES & PHILOSOPHY

These rules apply continuously and must NEVER be violated:

1. **Server Authoritative:** The ASP.NET Core server owns world state. The Android client is purely presentational and never authoritative.
2. **SQLite Persistence:** Reality resides in SQLite. Persistent data must survive complete server/application restarts.
3. **NEVER DELETE HISTORY AS PERFORMANCE OPTIMIZATION:**
   - NEVER automatically prune or delete posts, events, memories, messages, relationships, metrics, or historical records.
   - History is permanent.
   - Solve scale via **indexes, caching, pagination, batching, aggregation, efficient queries, and background simulation tiers**.
4. **Deterministic Simulation vs LLM:** C# controls simulation state, game rules, and world logic. The LLM ONLY generates language/text and does NOT control authoritative simulation rules.
5. **Inspect Before Modifying:** Always inspect local files, solution structure, existing classes, migrations, and tests BEFORE writing code. Re-use existing working infrastructure.
6. **Error Handling:** If build/test fails, **STOP immediately**, inspect errors, fix root cause, compile, and re-verify. Never build features on broken code.
7. **Incremental Checkpoints:** Implement only ONE checkpoint at a time. Never dump multi-part implementations in one prompt pass.

---

## 4. STEP-BY-STEP EXECUTION WORKFLOW

Follow this exact order of execution:

```text
1. INSPECT LOCAL FILES & GIT STATUS
      ↓
2. ANALYZE EXISTING ARCHITECTURE & REUSE CODE
      ↓
3. PLAN MINIMAL SAFE CHANGES
      ↓
4. IMPLEMENT REQUIRED CHANGES / MIGRATIONS
      ↓
5. COMPILE & BUILD ALL PROJECTS
      ↓
6. RUN & VERIFY TESTS (INCL. RESTART PERSISTENCE TEST)
      ↓
7. UPDATE README.md WITH ACTUAL REALITY
      ↓
8. CREATE LOCAL GIT COMMIT
      ↓
9. REPORT FINAL RESULTS & STOP
```

---

## 5. REQD MANDATORY README.md RULE

Before creating the local Git commit, you MUST update `D:\SMS\README.md`:
* Record what was actually built, configured, and verified in Part [PART_NUMBER].
* Update the current completed checkpoint list.
* Document new endpoints, schemas, environment configurations, or architectural shifts.
* Do NOT document functionality that was not implemented or tested.

---

## 6. REQUIRED RESPONSE FORMAT & REPORTING

Your execution output must strictly follow this structure:

### 📑 1. INSPECTION & CURRENT STATE
* Summary of discovered local files, existing structure, and git status.

### 🛠️ 2. IMPLEMENTATION SUMMARY
* Exact details of code, domain models, services, migrations, or APIs added.

### 📁 3. FILES CREATED & MODIFIED
* **Created:** `[List of paths]`
* **Modified:** `[List of paths]`

### 🧪 4. VERIFICATION & TEST RESULTS
* Build Result: `SUCCESS / FAIL`
* Endpoint Status: `PASS / FAIL`
* SQLite Restart Persistence: `PASS / FAIL`
* Functional Tests: `PASS / FAIL`

### 📝 5. README UPDATE CONFIRMATION
* Confirm `README.md` was updated with actual features tested.

### ⚙️ 6. GIT CHECKPOINT
* **Commit Message:** `[Commit Message]`
* **Git Status:** Working tree clean.

### 🚦 7. CURRENT PROJECT STATUS SUMMARY
```text
[PREVIOUS_COMPLETED_PARTS]
[PART_NUMBER] — COMPLETE ✅
```

### 🛑 8. STOP CONDITION
**State clearly:** `NEXT: PART [NEXT_PART_NUMBER] — [NEXT_PART_NAME]`
**Then STOP immediately. Do NOT automatically proceed to the next part.**
```
