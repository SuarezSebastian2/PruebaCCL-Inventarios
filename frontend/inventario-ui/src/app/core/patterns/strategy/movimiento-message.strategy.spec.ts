import {
  EntradaMovimientoMessageStrategy,
  MovimientoMessageStrategyFactory,
  SalidaMovimientoMessageStrategy
} from './movimiento-message.strategy';

describe('Movimiento message strategies', () => {
  const producto = { id: 1, nombre: 'Bolígrafo', cantidad: 14 };

  it('entrada strategy mentions cantidades', () => {
    const s = new EntradaMovimientoMessageStrategy();
    const msg = s.buildMessage(producto, 3);
    expect(msg).toContain('Entrada');
    expect(msg).toContain('Bolígrafo');
    expect(msg).toContain('14');
  });

  it('salida strategy mentions stock final', () => {
    const s = new SalidaMovimientoMessageStrategy();
    const msg = s.buildMessage({ ...producto, cantidad: 7 }, 2);
    expect(msg).toContain('Salida');
    expect(msg).toContain('7');
  });

  it('factory resolves entrada and salida case-insensitive', () => {
    const factory = new MovimientoMessageStrategyFactory();
    expect(factory.get(' ENTRADA ')?.tipo).toBe('entrada');
    expect(factory.get('Salida')?.tipo).toBe('salida');
    expect(factory.get('traslado')).toBeUndefined();
  });
});
