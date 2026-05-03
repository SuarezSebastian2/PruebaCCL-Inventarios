import { InventorySubjectService } from './inventory-subject.service';
import { InventoryUiEvent, InventoryUiObserver } from './inventory-ui-observer.model';

describe('InventorySubjectService', () => {
  let subject: InventorySubjectService;

  beforeEach(() => {
    subject = new InventorySubjectService();
  });

  it('notifies attached observers once per publish', () => {
    const calls: InventoryUiEvent[] = [];
    const obs: InventoryUiObserver = {
      onInventoryEvent(evt) {
        calls.push(evt);
      }
    };

    subject.attach(obs);
    subject.attach(obs);

    const evt: InventoryUiEvent = {
      tipo: 'entrada',
      producto: { id: 1, nombre: 'X', cantidad: 1 },
      cantidad: 1,
      correlationId: 9,
      detail: 'ok'
    };
    subject.publish(evt);

    expect(calls.length).toBe(1);
    expect(calls[0].correlationId).toBe(9);
  });
});
