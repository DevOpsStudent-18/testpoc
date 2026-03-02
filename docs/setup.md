# Microsoft Entra SSO Setup (Angular UI + .NET API on Azure App Service)

## Target Architecture
- Frontend (SPA): `https://ui-kodeapp-dev.azurewebsites.net`
- Backend API: `https://api-kodeapp-dev.azurewebsites.net`
- Identity provider: Microsoft Entra ID

This guide uses **2 app registrations**:
1. `edf-ui-spa` (Angular SPA)
2. `edf-api` (.NET API resource)

---

## Prerequisites
1. You have permission in Entra ID to create app registrations.
2. Backend is reachable at `https://api-kodeapp-dev.azurewebsites.net`.
3. Frontend is reachable at `https://ui-kodeapp-dev.azurewebsites.net`.
4. Decide account type:
- Single tenant (recommended for internal org use)
- Multi-tenant (if external organizations must sign in)

---

## Step 1: Register Backend API App (`edf-api`)
1. Azure Portal -> `Microsoft Entra ID` -> `App registrations` -> `New registration`.
2. Name: `edf-api`.
3. Supported account types: choose your required tenant model.
4. Redirect URI: leave empty for now.
5. Click `Register`.
6. Copy and store:
- `Application (client) ID` (API Client ID)
- `Directory (tenant) ID`

### 1A. Set Application ID URI
1. In `edf-api` app -> `Expose an API`.
2. Click `Set` next to Application ID URI.
3. Use default (`api://<api-client-id>`) or custom verified URI.
4. Save.

### 1B. Add API Scope
1. In `Expose an API` -> `Add a scope`.
2. Scope name: `access_as_user`.
3. Who can consent: `Admins and users` (or Admins only if policy requires).
4. Admin consent display name: `Access EDF API`.
5. Admin consent description: `Allow the application to access EDF API on behalf of the signed-in user.`
6. User consent display name: `Access EDF API`.
7. User consent description: `Allow this app to call EDF API for your data.`
8. State: `Enabled`.
9. Click `Add scope`.

### 1C. (Optional) Define App Roles
Use app roles only if you need role-based access from token claims.
1. `App roles` -> `Create app role`.
2. Example role: `Api.Reader`.
3. Allowed member types: `Users/Groups`.
4. Save.

---

## Step 2: Register Frontend SPA App (`edf-ui-spa`)
1. Azure Portal -> `Microsoft Entra ID` -> `App registrations` -> `New registration`.
2. Name: `edf-ui-spa`.
3. Supported account types: same model as API app.
4. Redirect URI:
- Platform: `Single-page application (SPA)`
- URI: `https://ui-kodeapp-dev.azurewebsites.net`
5. Click `Register`.
6. Copy `Application (client) ID` (UI Client ID).

### 2A. Add SPA Redirect URIs (Production + Local)
In `edf-ui-spa` -> `Authentication` -> `Single-page application`:
1. Ensure these Redirect URIs exist:
- `https://ui-kodeapp-dev.azurewebsites.net`
- `http://localhost:4200`
2. Add Logout URL:
- `https://ui-kodeapp-dev.azurewebsites.net`
3. Under `Implicit grant and hybrid flows`: keep unchecked unless explicitly required by your library.
4. Save.

### 2B. Add API Permission to Call Your Backend
1. `API permissions` -> `Add a permission`.
2. `My APIs` -> select `edf-api`.
3. Choose `Delegated permissions`.
4. Select scope: `access_as_user`.
5. Click `Add permissions`.
6. Click `Grant admin consent` (if your tenant requires admin pre-consent).

### 2C. (Optional) Add Graph Basic Scope
If you need profile info from Graph:
1. `Add a permission` -> `Microsoft Graph` -> `Delegated permissions`.
2. Add `User.Read`.
3. Grant admin consent if required.

---

## Step 3: Establish UI-API Trust (Authorized Client App)
This prevents consent friction and explicitly trusts UI app for API scope.

1. Open `edf-api` app registration.
2. Go to `Expose an API`.
3. Under `Authorized client applications` -> `Add a client application`.
4. Client ID: paste `edf-ui-spa` Client ID.
5. Select authorized scope: `api://<edf-api-client-id>/access_as_user`.
6. Save.

---

## Step 4: Configure API App Service Settings
In backend App Service (`api-kodeapp-dev`) -> `Environment variables` -> `App settings`, add:

- `AzureAd__Instance` = `https://login.microsoftonline.com/`
- `AzureAd__TenantId` = `<tenant-id>`
- `AzureAd__ClientId` = `<edf-api-client-id>`
- `AzureAd__Audience` = `api://<edf-api-client-id>`

Keep your existing:
- `AllowedOrigins` = `https://ui-kodeapp-dev.azurewebsites.net`
- `ConnectionStrings__DefaultConnection` = `<your sql connection string>`

Save and restart backend app service.

---

## Step 5: Configure Frontend App Settings (Angular)
Your Angular app should request tokens for API scope:
- Scope: `api://<edf-api-client-id>/access_as_user`
- Authority: `https://login.microsoftonline.com/<tenant-id>`
- Redirect URI: `https://ui-kodeapp-dev.azurewebsites.net`

If you maintain runtime config JSON, add keys such as:
```json
{
  "backendUrl": "https://api-kodeapp-dev.azurewebsites.net",
  "tenantId": "<tenant-id>",
  "clientId": "<edf-ui-spa-client-id>",
  "apiScope": "api://<edf-api-client-id>/access_as_user"
}
```

---

## Step 6: Token Validation in .NET API
Your API must validate bearer JWTs from Entra.

High-level requirements in API code:
1. Add JWT Bearer auth middleware.
2. Validate issuer (`tenant`), audience (`api://<edf-api-client-id>`), signature, expiry.
3. Apply `[Authorize]` on controllers/endpoints.

If needed, use Microsoft.Identity.Web (`AddMicrosoftIdentityWebApi`) to simplify setup.

---

## Step 7: End-to-End Validation Checklist
1. Open UI in private browser window.
2. Sign in using Entra account.
3. In browser dev tools, verify access token is requested for API scope.
4. Call API endpoint from UI.
5. API returns `200` with data (not `401/403`).
6. If `401`:
- Check API audience in token
- Check API app setting `AzureAd__Audience`
- Check UI permission includes `access_as_user`

---

## Redirect URL Reference

### UI App Registration (`edf-ui-spa`)
Add under SPA platform:
- `https://ui-kodeapp-dev.azurewebsites.net`
- `http://localhost:4200`

### API App Registration (`edf-api`)
Usually no redirect URI needed for pure resource API.
If you enable Swagger OAuth login later, add web redirect URI:
- `https://api-kodeapp-dev.azurewebsites.net/swagger/oauth2-redirect.html`

---

## Common Mistakes
1. Using API client ID as SPA client ID (must be separate apps).
2. Missing `Authorized client application` in API expose settings.
3. Wrong scope format (must be `api://<api-client-id>/access_as_user`).
4. Not granting admin consent when tenant policy requires it.
5. Audience mismatch between token and API validation config.
6. Forgetting to restart App Service after changing env vars.

---

## Security Recommendations
1. Keep API as single-tenant unless multi-tenant is required.
2. Use app roles/groups for authorization beyond sign-in.
3. Enforce Conditional Access and MFA in Entra.
4. Do not use implicit flow unless required by legacy constraints.

---

## What to Capture for Handover
- Tenant ID
- UI Client ID
- API Client ID
- API scope URI
- Configured redirect URIs
- Admin consent status
- App Service app settings keys
