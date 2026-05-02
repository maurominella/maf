FROM python:3.13-slim

WORKDIR /app

# If you want to copy the entire ha01_echoagent directory, you can use "." instead of listing each file individually
COPY main.py requirements.txt .env echoagent/

WORKDIR /app/echoagent

# Install agentserver packages first (they pull in older agent-framework versions that are compatible with agentserver)
# then upgrade agent-framework-* to the required rc versions
RUN pip install --no-cache-dir -r requirements.txt

# This is actually not needed
EXPOSE 8088

CMD ["python", "main.py"]