# Install the MDCP skill pack

## Parent skill

```bash
npx skills add betsalel-williamson/mdcp --skill mdcp
```

Use `-a` / `--agent` to target a specific host when you have more than one agent
installed (for example `-a cursor` or `-a claude-code`).

The [skills CLI](https://github.com/vercel-labs/skills) copies or symlinks the
skill into the **selected agent’s project (or global) skills directory**. Exact
paths depend on the agent — see [Supported Agents](https://github.com/vercel-labs/skills#supported-agents)
(for example `.agents/skills/` for Cursor/Amp project installs, `.claude/skills/`
for Claude Code, `.windsurf/skills/` for Windsurf).

**Zero-install:** copy the `mdcp` skill folder into the skills directory **your
agent discovers** (same Supported Agents list), not a single fixed path.

## After install

The `mdcp` parent skill provides the core system. To perform specific tasks, you should install the helper skills alongside it (e.g., `mdcp-getting-started`, `mdcp-feature-level`, `mdcp-doc-only`, `mdcp-design-architecture`, `mdcp-ux`):

```bash
npx skills add betsalel-williamson/mdcp --skill mdcp-getting-started
```

Use the same `-a` / `--agent` flag when targeting a host. Replace the skill name
for other helpers.

Start a bootstrap session with a natural-language turn under the getting-started helper skill:

```text
/mdcp-getting-started
```

The agent asks for `FEATURE` and
`PERSONA` before installing or writing shards. After bootstrap succeeds, it can
offer a guided first feature (design → feature → UX → doc-only) using a
recommended example or one you choose.

Optional archetype skills under `skills/mdcp-arch-*` are WIP and are not part of
the consumer install path yet.

## CLI still required (build, validate, cross-link registry)

The skills rely on the `mdcp` CLI. You need Node.js 18+ and must install
`@bwilliamson/mdcp-cli` globally or locally in your project:

```bash
npm install -g @bwilliamson/mdcp-cli
# or locally
npm install -D @bwilliamson/mdcp-cli
```

This provides the `mdcp` commands for:

- **compile** — build compiled docs from Markdown shards
- **check** — validate the documentation tree (links, structure, optional lint)
- **refs** — inspect/regenerate the cross-link fragment registry
- **fix** — format shards (Prettier / markdownlint auto-fix)
- **prose** — Vale prose lint

```bash
mdcp compile --config <config> --docs-root <docs-root>
mdcp check --config <config> --docs-root <docs-root>
mdcp refs list --config <config> --docs-root <docs-root>
```

Details: `cli-and-scripts.md` in this folder (linked from `SKILL.md`).
