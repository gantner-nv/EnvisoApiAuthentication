FROM python:3.12-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY run.py .

# Override with real credentials, e.g.:
#   docker run --rm -e ENVISO_API_KEY=... -e ENVISO_PUBLIC_KEY="$(cat key.pem)" envisoapiauthentication-python
ENTRYPOINT ["python", "run.py"]
