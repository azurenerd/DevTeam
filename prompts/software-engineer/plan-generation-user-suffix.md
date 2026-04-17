---
version: "1.0"
description: "SE engineering plan generation user prompt suffix"
variables: []
tags:
  - software-engineer
  - planning
---
Create an engineering plan mapping these Issues to tasks. REMEMBER:
- T1 MUST be the Project Foundation & Scaffolding task (High complexity, no dependencies). It sets up the solution structure, shared interfaces, base classes, config, and DI registration so all other tasks have a clear skeleton to build upon. T1 is in Wave W0 — it runs ALONE.
- T1 must create COMPREHENSIVE placeholders: every model, every interface, every component stub, every CSS section marker, sample data files, config files. After T1 merges the app must BUILD and RUN.
- ALL other tasks should depend on T1 at minimum and be in W1 or later.
- Design tasks for PARALLEL execution: each task should own distinct files with NO overlap.
- NEVER assign the same file as CREATE in two different tasks.
- Prefer vertical slices (one feature end-to-end) over horizontal layers.
- Maximize tasks that depend ONLY on T1 (star topology, not chains).
- Assign each task a WAVE: W0 for T1 only, W1 for tasks after T1, W2+ for later waves.

Output ONLY structured lines in this EXACT 10-field format:
TASK|<ID>|<IssueNumber>|<Name>|<Description>|<Complexity>|<Dependencies or NONE>|<FilePlan>|<Wave>|<SkillTags>

**CRITICAL — The Name field (field 4) MUST be a descriptive feature title** like "Build Dashboard Header Component" or "Implement Data Service". NEVER use a wave identifier (W1, W2, W3) as the Name — the Wave has its own dedicated field (field 9). A Name like "W2" is WRONG; a Name like "Implement Monthly Heatmap Grid" is CORRECT.

The FilePlan field should contain semicolon-separated file operations:
  CREATE:path/to/file.ext(namespace);MODIFY:path/to/existing.ext;USE:ExistingType(namespace)
  SHARED:path/to/file.ext — declare a file that multiple tasks may modify (use sparingly, T1 only)

The Wave field: W0 for T1 only, W1 for tasks parallelizable after T1, W2+ for later waves.

The SkillTags field: comma-separated domain tags for skill-based engineer assignment (e.g., frontend,blazor,css or backend,api,database or foundation).

Example:
TASK|T1|42|Project Foundation & Scaffolding|Create solution structure, shared models, interfaces, DI registration, and configuration|High|NONE|CREATE:.gitignore;CREATE:MyApp.sln;CREATE:MyApp/MyApp.csproj;CREATE:MyApp/Program.cs(MyApp);CREATE:MyApp/Models/AppConfig.cs(MyApp.Models);SHARED:MyApp/Program.cs|W0|foundation
TASK|T2|43|Implement Auth Module|Build JWT authentication with refresh tokens|Medium|T1|CREATE:MyApp/Services/AuthService.cs(MyApp.Services);MODIFY:MyApp/Program.cs;USE:IAuthService(MyApp.Interfaces)|W1|backend,api,security
TASK|T3|44|Build User Profile Page|Build user profile page with Blazor components|Medium|T1|CREATE:MyApp/Components/UserProfile.razor(MyApp.Components)|W1|frontend,blazor,css

Note: T1 is the ONLY task in W0. T2 and T3 are both in W1 (parallel-safe) and own completely separate files. The Name field is always a descriptive title, NEVER a wave identifier.

Only output TASK lines, nothing else.
