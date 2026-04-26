FROM python:3.13-slim

WORKDIR /app

COPY . ha01_echoagent/

WORKDIR /app/ha01_echoagent

# Install agentserver packages first (they pull in older agent-framework versions that are compatible with agentserver)
# then upgrade agent-framework-* to the required rc versions
RUN pip install --no-cache-dir -r requirements.txt

# This is actually not needed
EXPOSE 8088

CMD ["python", "main.py"]