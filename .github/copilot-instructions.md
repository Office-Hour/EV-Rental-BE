# SDLC GenAI — GitHub Copilot Workflow & Prompt Templates

## Folder conventions

```
.prompt/                                   # All reusable prompt templates (generic, not per-feature)
  user-story-generator.md
  plan-user-story-generator.md
  code-generator.md
  code-review-generator.md

.prompt-result/<FEATURE_NAME>/             # Outputs for a specific feature run
  user-story-1.md
  user-story-2.md
  ...
  plan-user-story-1.md
  ...
  code-implementation-user-story-1.md
  ...
  code-review-user-story-1.md
  ...
```

## Workflow (TDD-first, 4 stages)

1) **User Story Generation** → read the feature name + description; produce 1..N user stories with AC & DoD that **explicitly include tests** (unit, integration, e2e if applicable).  
2) **Plan per User Story** → for each user story, produce a detailed **implementation plan** (with Implementation Checklist + Tasks) that fully satisfies its AC/DoD (TDD order).  
3) **Code Generation** → implement code **following the plan**, writing tests **first**, then code; document everything in a `code-implementation-user-story-{n}.md`.  
4) **Code Review** → review the produced code & docs; output a `code-review-user-story-{n}.md` with concrete findings and fixes.

### Global rules (applies to every stage)

- **TDD**: include & prioritize tests (unit, integration, e2e if applicable).  
- **Deterministic outputs**: strictly follow the file paths & naming below.  
- **Traceability**: maintain mapping from Requirements → User Stories → AC → Tests → Code.  
- **Security & Secrets**: detect & block secret keys and unsafe patterns; call them out explicitly.  
- **Unknowns**: list assumptions and open questions; never invent external API details.  
- **Stack**: auto-detect from repo (e.g., .NET, Angular, NestJS, SQL, etc.).  
- **Languages**: write code and tests in repo’s stack; docs in English unless specified.

---

## How to run (suggested flow)

1. **Kick off stories**: Open `.prompt/user-story-generator.md` in Copilot Chat, provide:
   - `FEATURE_NAME`: e.g., `ev-rental-billing`
   - `DESCRIPTION` & `REQUIREMENTS`
   - Ask Copilot to **create** files under `.prompt-result/<FEATURE_NAME>/user-story-*.md`.

2. **Plan each story**: For every generated `user-story-{i}.md`, run `.prompt/plan-user-story-generator.md` and have Copilot **create** `plan-user-story-{i}.md`.

3. **Generate code (TDD)**: For each plan, run `.prompt/code-generator.md`. Copilot should:
   - Write tests first, then code, then **create** `code-implementation-user-story-{i}.md`.

4. **Review**: Run `.prompt/code-review-generator.md` to produce `code-review-user-story-{i}.md` with actionable fixes.