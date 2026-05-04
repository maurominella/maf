FROM python:3.13-slim

WORKDIR /app

# Copy only the application files — do NOT copy .env (secrets must be
# injected at runtime via environment variables or --env-file)
COPY main.py requirements.txt .env ./

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

EXPOSE 8088

CMD ["python", "main.py"]