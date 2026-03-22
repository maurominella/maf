FROM python:3.12-slim

WORKDIR /app

COPY . user_agent/

WORKDIR /app/user_agent

# Install agentserver packages first (they pull in older agent-framework versions),
# then upgrade agent-framework-* to the required rc versions.
RUN pip install --no-cache-dir --pre \
        azure-ai-agentserver-agentframework==1.0.0b17 \
        azure-ai-agentserver-core==1.0.0b17 \
        azure-ai-agents==1.2.0b5 \
        azure-ai-inference==1.0.0b9 \
        azure-ai-projects==2.0.0b4 \
        azure-core==1.39.0 \
        azure-core-tracing-opentelemetry==1.0.0b12 \
        azure-identity==1.26.0b2 \
        python-dotenv

EXPOSE 8088

CMD ["python", "main.py"]