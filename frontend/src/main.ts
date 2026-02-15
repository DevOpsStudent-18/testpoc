import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { APP_INITIALIZER } from '@angular/core';
import { AppComponent } from './app/app.component';
import { ConfigService } from './app/config.service';

export function initConfigFactory(config: ConfigService) {
  return () => config.load();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter([]),
    { provide: APP_INITIALIZER, useFactory: initConfigFactory, deps: [ConfigService], multi: true }
  ],
}).catch(err => console.error(err));
