# Setup guide: reusable skills and lifecycle hooks

This repository includes reusable GitHub Copilot skills in `.github/skills/` and a sample Copilot hook configuration in `.github/hooks/`.

## Supported capabilities confirmed for this environment

The installed GitHub Copilot Chat extension is version `0.48.1`, and the supported identity and customization mechanisms are:

- Skills stored in `.github/skills/<skill-name>/SKILL.md` for repository-scoped usage.
- Custom instructions via `AGENTS.md` and `.github/instructions`.
- Custom agents in `.github/agents` (for agent-specific workflows).
- Copilot-compatible hooks under `.github/hooks/*.json` using the Copilot harness event model.

The documented Copilot hook events include:
- `sessionStart`
- `userPromptSubmitted`
- `preToolUse`
- `postToolUse`
- `sessionEnd`

These are the supported lifecycle points that can be configured for automatic capture. The repo can also log manually for events that are not reliably exposed by the harness or that require human intent.

## Files created

- `.github/skills/requirement-analysis/SKILL.md`
- `.github/skills/specification-driven-development/SKILL.md`
- `.github/skills/test-driven-development/SKILL.md`
- `.github/skills/code-review/SKILL.md`
- `.github/skills/playwright-e2e-testing/SKILL.md`
- `.github/skills/mutation-testing/SKILL.md`
- `.github/hooks/copilot-audit.json`
- `.github/hooks/copilot-audit.cjs`

## How to use the skills

1. Open the repository in VS Code with the GitHub Copilot Chat extension enabled.
2. Ensure the selected harness is the Copilot harness.
3. Open Copilot Chat and type `/` to see available skills.
4. Select the relevant skill by name:
   - `/requirement-analysis`
   - `/specification-driven-development`
   - `/test-driven-development`
   - `/code-review`
   - `/playwright-e2e-testing`
   - `/mutation-testing`
5. Supply the task-specific input requested by the skill.

## How to validate the skills work

1. Open a fresh chat session.
2. Invoke a skill directly with a representative request, such as `/test-driven-development for invoice approval duplicate approver logic`.
3. Confirm that the skill appears in the `/` menu and that the agent follows the instructions in the `SKILL.md` file.
4. Check the agent’s tool activity and response to verify the expected workflow.

## Hook installation and validation

1. The repository already contains `.github/hooks/copilot-audit.json` and `.github/hooks/copilot-audit.cjs`.
2. Open VS Code and switch the session target to Copilot.
3. Open the Copilot customizations or hook manager and confirm the hook is discovered.
4. Start a new Copilot session and trigger a harmless action that uses a tool.
5. Review `.github/hooks/copilot-audit.log` to confirm the script executed and the event payload was captured.
6. Verify the log omits secrets or credentials by inspecting the payload before saving or sharing it.

## Logging policy

### Automatically captured events
These events can be captured by supported Copilot hooks when the configuration is active:
- session start
- submitted prompt
- tool use before execution
- tool use after execution
- session completion

### Events requiring manual logging
Manual capture is still required for:
- human review and approval decisions
- final merge or rollback outcomes
- test result sign-off by the human reviewer
- outcomes that depend on user intent and are not emitted by the runtime

Use a structured log entry format like:

```json
{
  "timestamp": "2026-11-02T00:00:00.000Z",
  "agent": "Developer Agent",
  "model": "MAI-Code-1.1-Flash",
  "executionMode": "agent",
  "prompt": "Implement the duplicate approver rule",
  "toolUsage": ["read_file", "run_in_terminal"],
  "modifiedFiles": ["backend/src/InvoiceApp.Domain/ApprovalService.cs"],
  "testResult": "pass",
  "humanDecision": "approved for next increment",
  "outcome": "changes merged"
}
```

## Security and safety requirements

- Never log secrets, credentials, bearer tokens, passwords, or other sensitive configuration.
- Redact any keys or tokens in the logging script before they are written.
- Keep hook scripts limited to logging and diagnostics.
- Review hooks before enabling them in a shared or enterprise environment.
- Do not invent unsupported event names or lifecycle hooks.

## Notes for this repository

- AGENTS.md remains the source of project-level guidance.
- Skills should be invoked as needed and should not be used as a replacement for human review.
- Human approval is still required to advance from planning to implementation, reviews, and merge.
- The sample hook records metadata only; it does not alter execution or bypass approval flows.
