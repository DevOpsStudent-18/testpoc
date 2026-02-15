import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceService } from '../device.service';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="loading">Loading devices…</div>
    <div *ngIf="error" class="error">{{error}}</div>
    <ul *ngIf="devices?.length">
      <li *ngFor="let d of devices">
        <strong>{{d.name}}</strong> — {{d.type}} @ {{d.location}} <small>({{d.createdAt | date:'short'}})</small>
      </li>
    </ul>
    <div *ngIf="devices && devices.length === 0">No devices found.</div>
  `,
  styles: [`.error{color:crimson}`]
})
export class DeviceListComponent {
  devices: any[] | null = null;
  loading = true;
  error: string | null = null;

  constructor(private svc: DeviceService) {
    this.load();
  }

  async load() {
    try {
      this.devices = await this.svc.getAll();
    } catch (ex: any) {
      this.error = ex?.message ?? 'Error';
    } finally {
      this.loading = false;
    }
  }
}
