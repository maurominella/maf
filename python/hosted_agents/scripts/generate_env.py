#!/usr/bin/env python3
"""
generate_env.py — Scaffold a .env.example file for a Microsoft Foundry Hosted Agent.

Usage:
    python scripts/generate_env.py --agent <agent-folder-name> [--force]

Arguments:
    --agent   Name of the agent folder under agents/ (e.g. ha01_echoagent)
    --force   Overwrite an existing .env.example without prompting
    --list    List available agent folders and exit
    --help    Show this help message and exit

Examples:
    python scripts/generate_env.py --agent ha01_echoagent
    python scripts/generate_env.py --agent ha02-azureopenaiagent --force
    python scripts/generate_env.py --list

The script writes (or updates) agents/<agent>/.env.example with placeholder
values.  Real secrets are never committed — copy the generated file to .env
and fill in your actual values.
"""

import argparse
import os
import sys
import textwrap

# ── Template definitions ─────────────────────────────────────────────────────
# Each agent can have its own template.  The DEFAULT template is used when
# no specific template is registered for the requested agent.

TEMPLATES: dict[str, str] = {
    # ── Default template (covers most agents) ────────────────────────────────
    "_default": textwrap.dedent("""\
        # -----------------------------------------------------------------------
        # .env.example — copy this file to .env and fill in your real values.
        # NEVER commit .env to source control.
        # -----------------------------------------------------------------------

        # Microsoft Foundry project endpoint
        # Format: https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>
        PROJECT_ENDPOINT=https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>

        # Chat model deployment name in the Foundry project (e.g. gpt-4o, gpt-4.1-mini)
        MODEL_DEPLOYMENT_NAME=gpt-4o

        # -----------------------------------------------------------------------
        # Optional: Application Insights (leave blank to disable telemetry)
        # -----------------------------------------------------------------------
        # APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxxx;IngestionEndpoint=https://...
        # AZURE_EXPERIMENTAL_ENABLE_GENAI_TRACING=true
    """),

    # ── ha01_echoagent ────────────────────────────────────────────────────────
    "ha01_echoagent": textwrap.dedent("""\
        # -----------------------------------------------------------------------
        # .env.example for ha01_echoagent — copy to .env and fill in real values.
        # NEVER commit .env to source control.
        # -----------------------------------------------------------------------

        # Microsoft Foundry project endpoint
        # Format: https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>
        AIF_STD_PROJECT_ENDPOINT=https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>

        # Chat model deployment name (e.g. gpt-4o, gpt-4.1)
        MODEL_DEPLOYMENT_NAME=gpt-4o

        # -----------------------------------------------------------------------
        # Optional: Application Insights
        # -----------------------------------------------------------------------
        # APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxxx;IngestionEndpoint=https://...
        # AZURE_EXPERIMENTAL_ENABLE_GENAI_TRACING=true
    """),

    # ── ha02-azureopenaiagent ─────────────────────────────────────────────────
    "ha02-azureopenaiagent": textwrap.dedent("""\
        # -----------------------------------------------------------------------
        # .env.example for ha02-azureopenaiagent — copy to .env and fill in values.
        # NEVER commit .env to source control.
        # -----------------------------------------------------------------------

        # Microsoft Foundry project endpoint
        # Format: https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>
        PROJECT_ENDPOINT=https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>

        # Chat model deployment name (e.g. gpt-4.1-mini, gpt-4o)
        MODEL_DEPLOYMENT_NAME=gpt-4.1-mini

        # -----------------------------------------------------------------------
        # Optional: Application Insights / sensitive data logging
        # -----------------------------------------------------------------------
        # APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxxx;IngestionEndpoint=https://...
        # ENABLE_SENSITIVE_DATA=false
    """),
}


# ── Helpers ───────────────────────────────────────────────────────────────────

def repo_root() -> str:
    """Return the repository root (two levels above this script's directory)."""
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.abspath(os.path.join(here, ".."))


def agents_dir() -> str:
    return os.path.join(repo_root(), "agents")


def list_agents() -> list[str]:
    base = agents_dir()
    return sorted(
        d for d in os.listdir(base)
        if os.path.isdir(os.path.join(base, d)) and not d.startswith(".")
    )


def write_env_example(agent: str, force: bool) -> None:
    target = os.path.join(agents_dir(), agent, ".env.example")

    if os.path.exists(target) and not force:
        answer = input(f"{target} already exists. Overwrite? [y/N] ").strip().lower()
        if answer not in ("y", "yes"):
            print("Aborted.")
            sys.exit(0)

    template = TEMPLATES.get(agent, TEMPLATES["_default"])
    with open(target, "w", encoding="utf-8") as fh:
        fh.write(template)

    print(f"Written: {target}")
    print("Next step: copy to .env and fill in your real values.")


# ── Main ──────────────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Scaffold a .env.example for a Microsoft Foundry Hosted Agent.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--agent", "-a",
        metavar="AGENT",
        help="Agent folder name under agents/ (e.g. ha01_echoagent)",
    )
    parser.add_argument(
        "--force", "-f",
        action="store_true",
        help="Overwrite existing .env.example without prompting",
    )
    parser.add_argument(
        "--list", "-l",
        action="store_true",
        help="List available agent folders and exit",
    )
    args = parser.parse_args()

    if args.list:
        agents = list_agents()
        if agents:
            print("Available agents:")
            for a in agents:
                print(f"  {a}")
        else:
            print("No agent folders found under agents/")
        sys.exit(0)

    if not args.agent:
        parser.error("--agent is required (use --list to see available agents)")

    available = list_agents()
    if args.agent not in available:
        print(f"ERROR: Agent '{args.agent}' not found under agents/", file=sys.stderr)
        print(f"Available agents: {', '.join(available)}", file=sys.stderr)
        sys.exit(1)

    write_env_example(args.agent, args.force)


if __name__ == "__main__":
    main()
