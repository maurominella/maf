# Guida per la Pubblicazione e il Test di un Hosted Agent su Microsoft Foundry con azd

## Introduzione
Questa guida offre un approccio conciso per la pubblicazione e il test di un hosted agent sulla piattaforma Microsoft Foundry utilizzando azd. Seguire questi passaggi per configurare, fornire e distribuire il vostro agent.

## 1. Setup dell'Ambiente

1. Clona il repository per l’agent:
   ```bash
   cd hostedagent04_echoagent
   ```

2. Inizializza il progetto:
   ```bash
   azd init -t https://github.com/Azure-Samples/azd-ai-starter-basic
   ```

3. Imposta le variabili d'ambiente:
   ```bash
   azd env set AIF_STD_PROJECT_ENDPOINT "https://<...>"
   azd env set MODEL_DEPLOYMENT_NAME "<deployment>"
   ```

### Nome dell'ambiente
Il nome dell’ambiente è `hostedagent04_echoagent`, il quale verrà utilizzato per creare il Resource Group in Azure: `rg-hostedagent04_echoagent`.

## 2. Progetto Agent

1. Crea la cartella `agent` nella root del progetto:
   ```bash
   mkdir agent
   ```

2. Copia all'interno della cartella i seguenti file:
   - `agent.yaml`
   - `main.py`
   - `requirements.txt`

3. Inizializza l'agent:
   ```bash
   azd ai agent init -m agent\agent.yaml
   ```

## 3. Provision e Deploy

1. Provisiona le risorse tramite azd:
   ```bash
   azd provision
   ```

2. Distribuisci l'agent:
   ```bash
   azd deploy hostedagent04-echoagent
   ```

## 4. Costruzione e Esecuzione Locale del Docker

1. Costruzione dell'immagine Docker:
   ```bash
   docker build -t hostedagent04_echoagent .\src\hostedagent04-echoagent\.\
   ```

2. Visualizza le immagini Docker:
   ```bash
   docker images -a
   ```

3. Esegui il container Docker:
   ```bash
   docker run -p 8080:8080 hostedagent04_echoagent
   ```

## 5. Mappatura delle Porte

1. **Porta interna reale dell’app:** `8088`
   - La tua applicazione Python si espone sulla porta `8088` all'interno del container. Questa è la porta che devi mappare.

2. **Porta EXPOSE nel Dockerfile:** `8080`
   - Nel Dockerfile hai:
     ```dockerfile
     EXPOSE 8080
     ```
   - **Nota:** `EXPOSE` è documentativo. Se la tua app ascolta sulla 8088, considera di modificare il Dockerfile in:
     ```dockerfile
     EXPOSE 8088
     ```

3. **Porta esterna desiderata:** `8089`
   - Questa è la porta del tuo host da cui desideri accedere all’app.

### Comando corretto
Se la tua app ascolta sulla `8088`, esegui:
```bash
docker run -p 8089:8088 nome-immagine
```
### Significato della mappatura delle porte
| Porta Host | Porta Container | Significato |
|------------|----------------|-------------|
| 8089       | 8088           | Quando visiti http://localhost:8089, Docker inoltra il traffico alla porta 8088 interna |

### Correzioni nel Dockerfile
Se desideri aggiornare anche il Dockerfile, modifica:
```dockerfile
EXPOSE 8088
```
Ricostruisci:
```bash
docker build -t nome-immagine .
```
E avvia:
```bash
docker run -p 8089:8088 nome-immagine
```

## Ricapitolando
- La tua app ascolta su `8088` → questa è la porta interna.
- Vuoi esporla su `8089` → questa è la porta esterna.
- `EXPOSE 8080` non influisce → puoi cambiarlo o ignorarlo.
- Comando finale:
```bash
docker run -p 8089:8088 nome-immagine
```