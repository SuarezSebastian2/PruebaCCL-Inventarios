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

  it('GET producto by id', (done) => {
    service.obtenerProducto(3).subscribe((p) => {
      expect(p.nombre).toBe('Z');
      done();
    });
    const req = httpMock.expectOne((r) => r.url.endsWith('/productos/3'));
    req.flush({ id: 3, nombre: 'Z', cantidad: 0 });
  });

  it('POST crear producto', (done) => {
    const body = { nombre: 'N', cantidad: 1 };
    service.crearProducto(body).subscribe((p) => {
      expect(p.id).toBe(9);
      done();
    });
    const req = httpMock.expectOne((r) => r.method === 'POST' && r.url.endsWith('/productos'));
    expect(req.request.body).toEqual(body);
    req.flush({ id: 9, nombre: 'N', cantidad: 1 });
  });

  it('PUT actualizar producto solo envía nombre', (done) => {
    service.actualizarProducto(2, { nombre: 'X' }).subscribe((p) => {
      expect(p.nombre).toBe('X');
      expect(p.cantidad).toBe(99);
      done();
    });
    const req = httpMock.expectOne((r) => r.method === 'PUT' && r.url.endsWith('/productos/2'));
    expect(req.request.body).toEqual({ nombre: 'X' });
    req.flush({ id: 2, nombre: 'X', cantidad: 99 });
  });

  it('DELETE eliminar producto', (done) => {
    service.eliminarProducto(1).subscribe(() => {
      done();
    });
    const req = httpMock.expectOne((r) => r.method === 'DELETE' && r.url.endsWith('/productos/1'));
    req.flush(null);
  });
});
