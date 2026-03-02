import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceListComponent } from './device-list/device-list.component';
import { AuthService } from './auth.service';
import { ConfigService } from './config.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, DeviceListComponent],
  template: `
    <main class="container">
      <h1>EDF Devices</h1>

      <div class="auth-bar">
        <span *ngIf="isAuthenticated">Signed in as {{ username }}</span>
        <button *ngIf="!isAuthenticated" (click)="signIn()">Sign in</button>
        <button *ngIf="!isAuthenticated && canContinueWithoutSignIn && !anonymousMode" (click)="continueWithoutSignIn()">
          Continue without sign-in
        </button>
        <button *ngIf="isAuthenticated" (click)="signOut()">Sign out</button>
      </div>

      <app-device-list *ngIf="isAuthenticated || anonymousMode"></app-device-list>
      <p *ngIf="!isAuthenticated && !anonymousMode && !error">Sign in to view devices.</p>
      <p *ngIf="error" class="error">{{ error }}</p>
    </main>
  `,
  styles: [
    '.container { padding: 1rem; font-family: Arial, sans-serif; }',
    '.auth-bar { display:flex; gap:0.75rem; align-items:center; margin-bottom:1rem; }',
    '.error { color: crimson; }'
  ]
})
export class AppComponent implements OnInit {
  isAuthenticated = false;
  username = '';
  error = '';
  canContinueWithoutSignIn = false;
  anonymousMode = false;

  constructor(
    private readonly auth: AuthService,
    private readonly config: ConfigService
  ) {}

  async ngOnInit(): Promise<void> {
    this.canContinueWithoutSignIn = this.config.canUseAnonymousLocal;
    try {
      await this.auth.initialize();
      this.syncAuthState();
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to initialize authentication.';
      this.error = message;
    }
  }

  async signIn(): Promise<void> {
    this.error = '';
    this.anonymousMode = false;
    await this.auth.login();
  }

  async signOut(): Promise<void> {
    this.error = '';
    await this.auth.logout();
  }

  private syncAuthState(): void {
    this.isAuthenticated = this.auth.isAuthenticated;
    this.username = this.auth.username;
  }

  continueWithoutSignIn(): void {
    this.error = '';
    this.anonymousMode = true;
  }
}
