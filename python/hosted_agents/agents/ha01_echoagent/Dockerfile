FROM python:3.13-slim

WORKDIR /app

# Copy all source files; secrets (.env) are excluded via .dockerignore
COPY ./ .

# Install dependencies
RUN pip install --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Port is informational only; Foundry routes traffic internally
EXPOSE 8088

CMD ["python", "main.py"]