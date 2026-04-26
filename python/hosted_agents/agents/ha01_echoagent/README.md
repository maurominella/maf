# hosted_agents
Microsoft Foundry Hosted Agents

## UV Installation  (just for testing)
- On Linux / macOS: `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows: `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Setup Steps
- **CD** into the folder
- Create the environment: `uv init . --python 3.13`
- Create the local virtual environment: `uv venv`
- Activate the environment:
  - On Linux/macOS: `source .venv/bin/activate`
  - On Windows: `.\.venv\Scripts\activate.ps1`
- Add libraries (it's KEY to use `--active`):
  - Automatically: `uv add --active $(cat requirements.txt) --prerelease=allow`
  - Manually: `uv add --active <package-name> --prerelease=allow`
- Check that the packges are installed: `uv pip list`
- Synchronize to create the file structure: `uv sync --active --prerelease=allow`
- To deactivate: `deactivate`
- Create kernel for the jupyter notebook: ```python -m ipykernel install --name maf --use```
- Test Python:
```
python - << 'EOF'
import agent_framework
print("OK:", agent_framework)
EOF
```
# Docker tests locally
```
# build the image
docker build -t ha01_echoagent .

# run the container, mapping external 8089 to internal 8088
docker run -p 8089:8088 ha01_echoagent
```

# Logging
Logging is activated with the following lines of code:
```python
# Configure logging - INFO for this module only, WARNING for everything else
logging.basicConfig(level=logging.WARNING) # this is the "father" logger, set to WARNING to avoid too much noise from other modules
logger = logging.getLogger(__name__) # this is the "child" logger for our module (this module)
logger.setLevel(logging.INFO) # we set the child logger to INFO to get more detailed logs from our module
if not logger.handlers: # avoid adding multiple handlers if this code is reloaded multiple times (e.g. during development)
    _handler = logging.StreamHandler()
    _handler.setLevel(logging.INFO)
    logger.addHandler(_handler)
    # propagate=True (default) so logs also reach the root logger,
    # where configure_azure_monitor() attaches the App Insights handler
```

Given the following line:<br/>
![alt text](./_README%20images/image-1.png)

, while here is the output in ***Foundry --> Traces --> Log stream***: <br/>
![alt text](./_README%20images/image-2.png)

, while this is the *much more detailed* trace sent to ***Application Insights***:<br/>
![alt text](./_README%20images/image-3.png)
![alt text](./_README%20images/image-4.png)