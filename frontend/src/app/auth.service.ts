import { Injectable } from '@angular/core';
import { InteractionRequiredAuthError, PublicClientApplication, type AccountInfo, type AuthenticationResult } from '@azure/msal-browser';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private app: PublicClientApplication | null = null;
  private account: AccountInfo | null = null;
  private initialized = false;

  constructor(private readonly config: ConfigService) {}

  async initialize(): Promise<void> {
    if (this.initialized) return;

    if (!this.config.clientId || !this.config.tenantId || !this.config.apiScope) {
      this.initialized = true;
      return;
    }

    this.app = new PublicClientApplication({
      auth: {
        clientId: this.config.clientId,
        authority: this.config.authority,
        redirectUri: window.location.origin,
        postLogoutRedirectUri: window.location.origin
      },
      cache: {
        cacheLocation: 'localStorage'
      }
    });

    await this.app.initialize();
    const redirectResult = await this.app.handleRedirectPromise();
    this.account = redirectResult?.account ?? this.app.getActiveAccount() ?? this.app.getAllAccounts()[0] ?? null;

    if (this.account) {
      this.app.setActiveAccount(this.account);
    }

    this.initialized = true;
  }

  get isAuthenticated(): boolean {
    return this.account != null;
  }

  get username(): string {
    return this.account?.username ?? '';
  }

  async login(): Promise<void> {
    await this.ensureInitialized();
    if (!this.app) {
      throw new Error('Authentication is not configured.');
    }
    await this.app!.loginRedirect({ scopes: [this.config.apiScope] });
  }

  async logout(): Promise<void> {
    await this.ensureInitialized();
    if (!this.app) {
      return;
    }
    await this.app!.logoutRedirect();
  }

  async getAccessToken(): Promise<string> {
    await this.ensureInitialized();

    if (!this.account) {
      throw new Error('User is not authenticated.');
    }

    try {
      const result: AuthenticationResult = await this.app!.acquireTokenSilent({
        account: this.account,
        scopes: [this.config.apiScope]
      });
      return result.accessToken;
    } catch (err: unknown) {
      if (err instanceof InteractionRequiredAuthError) {
        await this.app!.acquireTokenRedirect({
          account: this.account,
          scopes: [this.config.apiScope]
        });
      }
      throw err;
    }
  }

  private async ensureInitialized(): Promise<void> {
    if (!this.initialized) {
      await this.initialize();
    }
  }
}
