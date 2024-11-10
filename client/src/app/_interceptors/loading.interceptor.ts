import { HttpInterceptorFn } from '@angular/common/http';
import { BusyService } from '../_services/busy.service';
import { inject } from '@angular/core';
import { delay, finalize, identity } from 'rxjs';
import { environment } from '../../environments/environment';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  //start our busy service before the request goes
  const busyService = inject(BusyService);
  busyService.busy();
  
  return next(req).pipe(
    (environment.production ? identity : delay(1000)),
    finalize(() => {
      busyService.idle()
    })
  );
  //after we get our request we will stop the loading spinner
};
