import { Injectable } from '@angular/core';

export interface RuntimeConfig {
  backendUrl: string;
  tenantId: string;
  clientId: string;
  apiScope: string;
  allowAnonymousLocal: boolean;
}

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private config: RuntimeConfig = {
    backendUrl: 'http://localhost:5000',
    tenantId: '',
    clientId: '',
    apiScope: '',
    allowAnonymousLocal: false
  };

  async load(): Promise<void> {
    try {
      const res = await fetch('/assets/runtime-config.json', { cache: 'no-store' });
      if (!res.ok) return;
      const json = await res.json();
      this.config = { ...this.config, ...json };
    } catch {
      // Keep defaults when runtime config is missing.
    }
  }

  get backendUrl(): string {
    return this.config.backendUrl;
  }

  get apiBase(): string {
    return this.backendUrl.replace(/\/$/, '') + '/api/devices';
  }

  get tenantId(): string {
    return this.config.tenantId;
  }

  get clientId(): string {
    return this.config.clientId;
  }

  get apiScope(): string {
    return this.config.apiScope;
  }

  get authority(): string {
    return `https://login.microsoftonline.com/${this.tenantId}`;
  }

  get allowAnonymousLocal(): boolean {
    return this.config.allowAnonymousLocal;
  }

  get canUseAnonymousLocal(): boolean {
    const host = window.location.hostname;
    return this.allowAnonymousLocal && (host === 'localhost' || host === '127.0.0.1');
  }
}
