import { HttpInterceptorFn } from '@angular/common/http';
import { BusyService } from '../_services/busy.service';
import { inject } from '@angular/core';
import { delay, finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  //start our busy service before the request goes
  const busyService = inject(BusyService);
  busyService.busy();
  
  return next(req).pipe(
    delay(1000),
    finalize(() => {
      busyService.idle()
    })
  );
  //after we get our request we will stop the loading spinner
};
