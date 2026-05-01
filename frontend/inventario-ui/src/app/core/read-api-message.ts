/** Mensaje principal del cuerpo de error del API (demo CCL). */
export function readApiErrorMessage(err: unknown): string {
  const e = err as { error?: { message?: string } };
  return typeof e?.error?.message === 'string' ? e.error.message : 'Operación no completada.';
}
