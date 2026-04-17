# {{role_name}}

A specialist engineer with deep expertise in specific domains. Created dynamically by the PM during team composition to fill skill gaps identified in the project requirements.

## When This Agent Is Created
- The PM analyzes the project description, research, and PM spec
- Identifies that the project requires domain expertise not covered by generic Software Engineers
- Creates this specialist with a focused role description and capability tags
- The leader SE assigns tasks matching this specialist's capabilities

## Capabilities
This agent has **full engineering capabilities** (unlike custom/SME agents):
- Complete PR lifecycle (create, update, rework)
- Build and test verification
- Rework loops with reviewer feedback
- Clarification requests when requirements are unclear
- Screenshot capture for UI work

## How It Differs From Generic SE
- Has a specialized persona injected from the PM's role definition
- Tagged with specific capabilities for skill-based task matching
- Leader SE prefers assigning matching tasks to this specialist
- Uses the same `engineer-base/` prompt templates but with specialist context overlay
