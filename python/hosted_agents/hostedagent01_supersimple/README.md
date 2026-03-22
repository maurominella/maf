# hostedagent01_supersimple

Minimal hosted agent sample.

## Setup

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
