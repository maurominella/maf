# hostedagent01_supersimple

Minimal hosted agent sample.

## Setup

### File preparation - `pyproject.toml`
```
[project]
name = "hostedagent01-supersimple"
version = "0.1.0"
description = "Minimal hosted agent sample"
readme = "README.md"
requires-python = ">=3.12"
dependencies = [
    "azure-ai-agentserver-agentframework>=1.0.0b17",
    "azure-identity>=1.26.0b2",
    "python-dotenv>=1.0.0",
    "tzdata>=2025.1",
]

[tool.uv]
prerelease = "allow"
override-dependencies = [
    "agent-framework-core>=1.0.0rc3",
    "agent-framework-azure-ai>=1.0.0rc3",
]
```

### PowerShell commands
```powershell
uv venv --python 3.12
.\.venv\Scripts\activate.ps1
uv sync
```

That's it. `uv sync` reads `pyproject.toml`, resolves all dependencies (including pre-releases), and creates a reproducible environment.

To upgrade to newer pre-releases in the future:

```powershell
uv sync --upgrade
```
