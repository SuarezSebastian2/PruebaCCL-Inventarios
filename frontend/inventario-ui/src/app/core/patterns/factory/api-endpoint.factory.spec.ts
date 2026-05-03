import { ApiEndpointFactory } from './api-endpoint.factory';

describe('ApiEndpointFactory', () => {
  let factory: ApiEndpointFactory;

  beforeEach(() => {
    factory = new ApiEndpointFactory();
  });

  it('origin matches configured api base', () => {
    expect(factory.origin()).toContain('5088');
  });

  it('builds auth and product endpoints under origin', () => {
    expect(factory.login()).toBe(`${factory.origin()}/auth/login`);
    expect(factory.inventario()).toBe(`${factory.origin()}/productos/inventario`);
    expect(factory.movimiento()).toBe(`${factory.origin()}/productos/movimiento`);
  });
});
