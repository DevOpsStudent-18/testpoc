import { Injectable } from '@angular/core';

export interface RuntimeConfig {
  backendUrl: string;
}

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private config: RuntimeConfig = { backendUrl: 'http://localhost:5000' };

  async load(): Promise<void> {
    try {
      const res = await fetch('/assets/runtime-config.json', { cache: 'no-store' });
      if (!res.ok) return; // keep defaults
      const json = await res.json();
      this.config = { ...this.config, ...json };
    } catch {
      // ignore and keep defaults
    }
  }

  get backendUrl(): string {
    return this.config.backendUrl;
  }

  get apiBase(): string {
    return this.backendUrl.replace(/\/$/, '') + '/api/devices';
  }
}
