# Deployment + Entra SSO Runbook (Angular UI + .NET API + Azure SQL)

## 1. Current Readiness Review

### Ready
1. Backend workflow deploys .NET API to App Service (`net8.0`).
2. Frontend workflow deploys Angular app to App Service and injects runtime auth config from GitHub secrets.
3. API is configured for Entra JWT validation and requires auth in non-Development environments.
4. Local Development fallback exists for in-memory data and optional anonymous UI browsing.

### Important caveat
1. Frontend local build was not re-validated in this machine session due local Node/npm state. CI uses Node 20 and is expected to build in GitHub Actions.

---

## 2. Target URLs and App Registrations

1. Frontend URL: `https://ui-kodeapp-dev.azurewebsites.net`
2. Backend API URL: `https://api-kodeapp-dev.azurewebsites.net`
3. Entra app registrations:
   1. `edf-ui-spa` (SPA client)
   2. `edf-api` (resource API)

---

## 3. GitHub Repository Secrets (Required Before Deployment)

Set these in GitHub repo -> `Settings` -> `Secrets and variables` -> `Actions`.

### Frontend workflow secrets
1. `FRONTEND_APP_NAME`
2. `FRONTEND_PUBLISH_PROFILE`
3. `BACKEND_API_URL` = `https://api-kodeapp-dev.azurewebsites.net`
4. `ENTRA_TENANT_ID` = `<tenant-guid>`
5. `ENTRA_UI_CLIENT_ID` = `<edf-ui-spa-client-id>`
6. `ENTRA_API_SCOPE` = `api://<edf-api-client-id>/access_as_user`

### Backend workflow secrets
1. `BACKEND_APP_NAME`
2. `BACKEND_PUBLISH_PROFILE`

---

## 4. Deploy via GitHub Actions

1. Push to `dev` branch or run workflows manually.
2. Confirm both workflows pass:
   1. `Deploy Frontend`
   2. `Deploy API`
3. Open deployed endpoints:
   1. `https://ui-kodeapp-dev.azurewebsites.net`
   2. `https://api-kodeapp-dev.azurewebsites.net/health`

---

## 5. Microsoft Entra Configuration (UI Steps)

## 5.1 Create/Register API App (`edf-api`)
1. Azure Portal -> `Microsoft Entra ID` -> `App registrations` -> `New registration`.
2. Name: `edf-api`.
3. Save:
   1. `Application (client) ID` (API Client ID)
   2. `Directory (tenant) ID`

## 5.2 Expose API Scope
1. In `edf-api` -> `Expose an API`.
2. Set Application ID URI (default `api://<edf-api-client-id>` is fine).
3. `Add a scope`:
   1. Name: `access_as_user`
   2. State: `Enabled`
4. Save.

## 5.3 Create/Register SPA App (`edf-ui-spa`)
1. `Microsoft Entra ID` -> `App registrations` -> `New registration`.
2. Name: `edf-ui-spa`.
3. Platform: `Single-page application`.
4. Redirect URIs:
   1. `https://ui-kodeapp-dev.azurewebsites.net`
   2. `http://localhost:4200`
5. Save `Application (client) ID` (UI Client ID).

## 5.4 Grant UI Permission to API
1. In `edf-ui-spa` -> `API permissions` -> `Add permission` -> `My APIs`.
2. Select `edf-api`.
3. Add delegated scope `access_as_user`.
4. Grant admin consent if tenant policy requires.

## 5.5 Establish UI-API Trust
1. In `edf-api` -> `Expose an API` -> `Authorized client applications`.
2. Add `edf-ui-spa` client ID.
3. Authorize scope `api://<edf-api-client-id>/access_as_user`.

---

## 6. Azure App Service Configuration (Post Deployment)

## 6.1 Backend App Service (`api-kodeapp-dev`)
Path: `App Service` -> `api-kodeapp-dev` -> `Environment variables` -> `App settings`

Set:
1. `AllowedOrigins` = `https://ui-kodeapp-dev.azurewebsites.net`
2. `AzureAd__Instance` = `https://login.microsoftonline.com/`
3. `AzureAd__TenantId` = `<tenant-guid>`
4. `AzureAd__ClientId` = `<edf-api-client-id>`
5. `AzureAd__Audience` = `api://<edf-api-client-id>`
6. `AzureAd__Scope` = `api://<edf-api-client-id>/access_as_user`
7. `AzureAd__SwaggerClientId` = `<client-id-for-swagger-login>` (optional)
8. `ConnectionStrings__DefaultConnection` = `<azure-sql-connection-string>`

Then:
1. Save/Apply
2. Restart App Service

## 6.2 Frontend App Service (`ui-kodeapp-dev`)
1. No mandatory runtime app settings for auth if GitHub secrets are configured correctly.
2. Verify site loads from `https://ui-kodeapp-dev.azurewebsites.net`.

---

## 7. Azure SQL Configuration

1. Ensure database exists and table/data is present.
2. Ensure SQL networking allows backend App Service connectivity.
3. Ensure authentication mode used by your connection string is configured correctly (SQL login or Managed Identity).

---

## 8. Swagger OAuth (Optional)

Current code enables Swagger UI in Development environment.

If you use Swagger OAuth in a non-local environment:
1. Add redirect URI in the app used by `AzureAd__SwaggerClientId`:
   1. `https://api-kodeapp-dev.azurewebsites.net/swagger/oauth2-redirect.html`
2. Keep `AzureAd__SwaggerClientId` configured on backend App Service.

---

## 9. End-to-End Validation

1. Open UI in private browser window.
2. Click `Sign in`.
3. Complete Entra login.
4. Confirm device list loads in UI.
5. Confirm backend responses are `200` and not `401/403`.

API quick checks:
1. `GET https://api-kodeapp-dev.azurewebsites.net/health` should return OK.
2. `GET https://api-kodeapp-dev.azurewebsites.net/api/devices` should return:
   1. `401` without token in non-Development.
   2. `200` with valid bearer token from UI.

---

## 10. Troubleshooting

1. UI loops or login fails:
   1. Verify SPA redirect URI matches exactly.
   2. Verify `ENTRA_UI_CLIENT_ID` and tenant in frontend runtime config.
2. API returns `401` after login:
   1. Verify `AzureAd__Audience` matches the token audience.
   2. Verify UI requests `ENTRA_API_SCOPE` exactly.
3. CORS errors:
   1. Verify `AllowedOrigins` exactly equals frontend URL.
4. API startup failure:
   1. Check required app settings are present.
   2. Check SQL connection string is valid.

---

## 11. Local Development Notes

1. Local API uses in-memory data when `ConnectionStrings:DefaultConnection` is empty in Development.
2. Local frontend can allow anonymous browsing with `allowAnonymousLocal=true` and localhost host check.
3. Azure (non-Development) still enforces authentication on API endpoints.
