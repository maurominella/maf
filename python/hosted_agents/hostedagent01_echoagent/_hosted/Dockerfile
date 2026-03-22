FROM python:3.13-slim

WORKDIR /app

COPY . user_agent/

WORKDIR /app/user_agent

# Install agentserver packages first (they pull in older agent-framework versions),
# then upgrade agent-framework-* to the required rc versions.
RUN pip install --no-cache-dir --pre -r requirements.txt

EXPOSE 8088

CMD ["python", "main.py"]