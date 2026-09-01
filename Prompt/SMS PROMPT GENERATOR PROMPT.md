# TASK: GENERATE NEXT SEQUENTIAL DEVELOPMENT PROMPT

You are an expert **Prompt Engineer and Software Architect** tasked with generating the exact markdown prompt for the **next development part** of the **Social Media Simulator (SMS)** project.

---

## 1. INPUT DOCUMENTS TO READ & ANALYZE

Before generating the prompt, read and analyze the following files in the local workspace:

1. `SMS MASTER PROMPT.MD`
   - Purpose: Understand the overall project roadmap, architecture, non-negotiable rules, database design, and global features.
2. `Social_Media_Simulator_AIO_Prompt_Template.md`
   - Purpose: Use this file as the strict structural blueprint and formatting layout for the prompt you will generate.
3. **The Most Recent Part File** (e.g., `SOCIAL_MEDIA_SIMULATOR__PART_14_DEVELOPMENT_PROMPT.md` or similar latest file in workspace)
   - Purpose: Determine the last completed checkpoint, the current git commit ID, the completed parts list, and what the logical next feature step should be.

---

## 2. DRAFTING INSTRUCTIONS & LOGIC

1. **Determine Next Part Details:**
   - Identify the previous part number (e.g., Part 14) and set the target part number (e.g., Part 15).
   - Determine the specific target feature set for Part [NEXT_PART_NUMBER] by cross-referencing `SMS MASTER PROMPT.MD` against the latest completed part file.

2. **Define Strict Scope:**
   - **Primary Objective:** Define a single focused, incremental development task for the next part.
   - **Out-of-Scope Boundaries:** List explicit features from future parts that must NOT be built yet.

3. **Populate Template Variables:**
   - Map all structural sections according to `Social_Media_Simulator_AIO_Prompt_Template.md`.
   - Maintain all core permanent rules (Server Authoritative, SQLite Persistence, Permanent History/No Pruning, Deterministic Simulation vs LLM, Inspect Before Modifying, Mandatory README update).

---

## 3. REQUIRED OUTPUT FORMAT

Output the complete, production-ready markdown content for the new file (e.g., `SOCIAL_MEDIA_SIMULATOR__PART_[NEXT_PART_NUMBER]_DEVELOPMENT_PROMPT.md`).

Ensure the output is directly copyable into a `.md` file, fully formatted using the structural layout of `Social_Media_Simulator_AIO_Prompt_Template.md`.

---

**BEGIN EXECUTION NOW:** Read `SMS MASTER PROMPT.MD`, `Social_Media_Simulator_AIO_Prompt_Template.md`, and the latest `SOCIAL_MEDIA_SIMULATOR__PART_*` file in the workspace, then generate the prompt for the next part.