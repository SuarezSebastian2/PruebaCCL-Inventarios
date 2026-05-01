-- Datos iniciales manuales (prueba CCL).
-- Ejecutar contra la base `ccl_inventario` después de que la API haya aplicado la migración (tabla `productos` vacía).
-- Idempotente: solo inserta si la tabla está vacía.

INSERT INTO productos (nombre, cantidad)
SELECT v.nombre, v.cantidad
FROM (VALUES
  ('Resma papel A4', 120),
  ('Bolígrafo azul', 300),
  ('Tóner impresora', 15)
) AS v(nombre, cantidad)
WHERE NOT EXISTS (SELECT 1 FROM productos);
