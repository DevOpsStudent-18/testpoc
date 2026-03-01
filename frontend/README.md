# EDF Frontend POC

Simple Angular frontend that fetches devices from the API.  The default
configuration assumes the backend is running at `http://localhost:5000`.

## Running locally

```bash
cd "D:/dev work/EDF POC/frontend"
npm install
npm start
```

The development server listens on `http://localhost:4200`.

## Configuration

The client reads a small JSON file at `/assets/runtime-config.json` during
startup; the shipped default merely points to localhost.  You can override any
setting by editing or replacing this file before deploying.  Example:

```json
{
  "backendUrl": "https://my-api.azurewebsites.net"
}
```

`ConfigService` in `src/app/config.service.ts` handles the merge with built‑in
defaults.

## Deployment

When deploying the static build to Azure App Service (or another host), ensure
that the `runtime-config.json` file is updated with the production API URL.
You might script this as part of your CI/CD pipeline or use App Service's
`App Settings` to dynamically rewrite the file on start.

> The line about `pm2 serve ...` in earlier versions was an example command for
> serving the built files from a Linux container; keep it if you intend to use
> pm2 in production.