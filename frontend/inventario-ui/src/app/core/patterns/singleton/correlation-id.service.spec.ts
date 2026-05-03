import { TestBed } from '@angular/core/testing';
import { CorrelationIdService } from './correlation-id.service';

describe('CorrelationIdService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('increments sequence per inject scope', () => {
    const first = TestBed.inject(CorrelationIdService);
    const second = TestBed.inject(CorrelationIdService);
    expect(first).toBe(second);
    expect(first.next()).toBe(1);
    expect(second.next()).toBe(2);
  });
});
