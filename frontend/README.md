# EDF Frontend POC

Simple Angular frontend that fetches devices from the API at `http://localhost:5000`.

Run locally:

```bash
cd "D:/dev work/EDF POC/frontend"
npm install
npm start
```

The app runs at `http://localhost:4200` and calls `GET /api/devices` on the API.
under startup
pm2 serve /home/site/wwwroot --no-daemon --spa