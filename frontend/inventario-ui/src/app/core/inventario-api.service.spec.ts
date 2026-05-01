import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApiEndpointFactory } from './patterns/factory/api-endpoint.factory';
import { InventarioApiService } from './inventario-api.service';

describe('InventarioApiService', () => {
  let service: InventarioApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ApiEndpointFactory,
        InventarioApiService
      ]
    });
    service = TestBed.inject(InventarioApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GET inventario uses factory URL', (done) => {
    service.inventario().subscribe((rows) => {
      expect(rows.length).toBe(0);
      done();
    });
    const req = httpMock.expectOne((r) => r.url.endsWith('/productos/inventario'));
    req.flush([]);
  });

  it('POST movimiento sends body', (done) => {
    const body = { productoId: 2, tipo: 'salida', cantidad: 1 };
    service.movimiento(body).subscribe((p) => {
      expect(p.id).toBe(2);
      done();
    });
    const req = httpMock.expectOne((r) => r.url.endsWith('/productos/movimiento'));
    expect(req.request.body).toEqual(body);
    req.flush({ id: 2, nombre: 'Y', cantidad: 4 });
  });
});
