import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { InventarioApiService } from '../../inventario-api.service';
import { ApiEndpointFactory } from '../factory/api-endpoint.factory';
import { ConsoleInventoryUiObserver } from '../observer/console-inventory-ui.observer';
import { InventorySubjectService } from '../observer/inventory-subject.service';
import { InventoryUiObserver } from '../observer/inventory-ui-observer.model';
import { CorrelationIdService } from '../singleton/correlation-id.service';
import { MovimientoMessageStrategyFactory } from '../strategy/movimiento-message.strategy';
import { InventoryUiFacade } from './inventory-ui.facade';

describe('InventoryUiFacade', () => {
  let facade: InventoryUiFacade;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ApiEndpointFactory,
        InventarioApiService,
        InventorySubjectService,
        CorrelationIdService,
        MovimientoMessageStrategyFactory,
        ConsoleInventoryUiObserver,
        InventoryUiFacade
      ]
    });

    facade = TestBed.inject(InventoryUiFacade);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('listInventario maps GET response', (done) => {
    facade.listInventario().subscribe((rows) => {
      expect(rows.length).toBe(1);
      expect(rows[0].nombre).toBe('Resma');
      done();
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/productos/inventario'));
    expect(req.request.method).toBe('GET');
    req.flush([{ id: 1, nombre: 'Resma', cantidad: 10 }]);
  });

  it('registrarMovimiento POST and notifies extra observers', (done) => {
    const subject = TestBed.inject(InventorySubjectService);
    const spyObs: InventoryUiObserver = {
      onInventoryEvent: jasmine.createSpy('onInventoryEvent')
    };
    subject.attach(spyObs);

    facade
      .registrarMovimiento({ productoId: 1, tipo: 'entrada', cantidad: 2 })
      .subscribe((p) => {
        expect(p.cantidad).toBe(12);
        expect(spyObs.onInventoryEvent).toHaveBeenCalled();
        done();
      });

    const req = httpMock.expectOne((r) => r.url.endsWith('/productos/movimiento'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      productoId: 1,
      tipo: 'entrada',
      cantidad: 2
    });
    req.flush({ id: 1, nombre: 'Resma', cantidad: 12 });
  });
});
