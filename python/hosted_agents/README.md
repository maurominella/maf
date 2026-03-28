# Guide to Publishing and Testing a Hosted Agent on Microsoft Foundry with azd

## Introduction
This guide provides a concise approach for publishing and testing a hosted agent on Microsoft Foundry using `azd`. Follow these steps to configure, provision, and deploy your agent.

## 1. Environment Setup

1. Clone the agent repository:
   ```bash
   cd hostedagent04_echoagent
   ```

2. Initialize the project:
   ```bash
   azd init -t https://github.com/Azure-Samples/azd-ai-starter-basic
   ```

3. Set the environment variables:
   ```bash
   azd env set AIF_STD_PROJECT_ENDPOINT "https://<...>"
   azd env set MODEL_DEPLOYMENT_NAME "<deployment>"
   ```

### Environment name
The environment name is `hostedagent04_echoagent`. Based on it, Azure will create the corresponding Resource Group: `rg-hostedagent04_echoagent`.

## 2. Agent Project

1. Create the `agent` folder in the project root:
   ```bash
   mkdir agent
   ```

2. Copy the following files into that folder:
   - `agent.yaml`
   - `main.py`
   - `requirements.txt`

3. Initialize the agent:
   ```bash
   azd ai agent init -m agent\agent.yaml
   ```

## 3. Provision and Deploy

1. Provision the resources with azd:
   ```bash
   azd provision
   ```

2. Deploy the agent:
   ```bash
   azd deploy hostedagent04-echoagent
   ```

## 4. Local Docker Build and Run

1. Build the Docker image:
   ```bash
   docker build -t hostedagent04_echoagent .\src\hostedagent04-echoagent\.\
   ```

2. List Docker images:
   ```bash
   docker images -a
   ```

3. Run the Docker container:
   ```bash
   docker run -p 8080:8080 hostedagent04_echoagent
   ```

## 5. Port Mapping

1. **Actual internal application port:** `8088`
   - Your Python application exposes itself on port `8088` inside the container. This is the port you need to map.

2. **Dockerfile EXPOSE port:** `8080`
   - In the Dockerfile you have:
     ```dockerfile
     EXPOSE 8080
     ```
   - **Note:** `EXPOSE` is only informational. If your app listens on `8088`, it is a good practice to change it to:
     ```dockerfile
     EXPOSE 8088
     ```

3. **Desired external port:** `8089`
   - This is the host port you want to use to access the app.

### Correct command
If your app listens on `8088`, run:

```bash
docker run -p 8089:8088 name-image
```

### Port mapping meaning

| Host Port | Container Port | Meaning |
|-----------|----------------|---------|
| 8089      | 8088           | When you visit http://localhost:8089, Docker forwards traffic to internal port 8088 |

### Dockerfile adjustment
If you want to update the Dockerfile too, change:

```dockerfile
EXPOSE 8088
```

Rebuild:

```bash
docker build -t name-image .
```

Then run:

```bash
docker run -p 8089:8088 name-image
```

## Summary
- Your app listens on `8088` → this is the internal port.
- You want to expose it on `8089` → this is the external port.
- `EXPOSE 8080` does not affect runtime behavior → you can change it or ignore it.
- Final command:
```bash
docker run -p 8089:8088 name-image
```
