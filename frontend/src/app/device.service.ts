import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  constructor(
    private readonly auth: AuthService,
    private readonly config: ConfigService
  ) {}

  async getAll() {
    const headers: Record<string, string> = {};
    if (this.auth.isAuthenticated) {
      const token = await this.auth.getAccessToken();
      headers.Authorization = `Bearer ${token}`;
    } else if (!this.config.canUseAnonymousLocal) {
      throw new Error('Sign in is required to load devices.');
    }

    const res = await fetch(this.config.apiBase, { headers });

    if (!res.ok) {
      throw new Error(`Failed fetching devices (${res.status})`);
    }

    return await res.json();
  }
}
