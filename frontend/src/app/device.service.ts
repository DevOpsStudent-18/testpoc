import { Injectable } from '@angular/core';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private base = '';

  constructor(private config: ConfigService) {
    this.base = this.config.apiBase;
  }

  async getAll() {
    const res = await fetch(this.base);
    if (!res.ok) throw new Error('Failed fetching devices');
    return await res.json();
  }
}
