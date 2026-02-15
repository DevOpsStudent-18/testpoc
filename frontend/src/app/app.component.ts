import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceListComponent } from './device-list/device-list.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, DeviceListComponent],
  template: `
    <main class="container">
      <h1>EDF Devices</h1>
      <app-device-list></app-device-list>
    </main>
  `,
  styles: [`.container { padding: 1rem; font-family: Arial, sans-serif; }`]
})
export class AppComponent {}
