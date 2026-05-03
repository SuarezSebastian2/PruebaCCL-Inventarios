import { ConsoleInventoryUiObserver } from './console-inventory-ui.observer';

describe('ConsoleInventoryUiObserver', () => {
  it('logs via console.debug', () => {
    const observer = new ConsoleInventoryUiObserver();
    spyOn(console, 'debug');
    observer.onInventoryEvent({
      tipo: 'entrada',
      producto: { id: 1, nombre: 'A', cantidad: 2 },
      cantidad: 1,
      correlationId: 3,
      detail: 'test'
    });
    expect(console.debug).toHaveBeenCalledWith(
      '[Inventory UI #3] entrada · test'
    );
  });
});
