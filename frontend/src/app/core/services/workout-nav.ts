import { Injectable, NgZone, inject } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class WorkoutNav {
  private readonly router = inject(Router);
  private readonly zone = inject(NgZone);

  open(id: string | undefined): Promise<boolean> {
    if (!id) {
      return Promise.resolve(false);
    }

    return new Promise((resolve) => {
      this.zone.run(() => {
        setTimeout(() => {
          void this.router.navigateByUrl(`/workout/${id}`, { replaceUrl: false }).then((opened) => {
            resolve(!!opened);
          });
        }, 0);
      });
    });
  }
}
