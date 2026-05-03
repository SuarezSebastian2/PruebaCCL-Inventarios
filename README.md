# PruebaCCL — Mini sistema de inventario

Aplicación web de **inventario** para la prueba técnica CCL: autenticación **JWT**, API **ASP.NET Core 9** (`net9.0`) con **Entity Framework Core** y **PostgreSQL**, y cliente **Angular 19**.

## Requisitos previos

| Herramienta | Notas |
|-------------|--------|
| [.NET SDK 9+ o 10+](https://dotnet.microsoft.com/download) | El proyecto compila con **`net9.0`**. Instala el [**runtime ASP.NET Core 9.0**](https://dotnet.microsoft.com/download/dotnet/9.0). |
| [PostgreSQL](https://www.postgresql.org/download/) | Servidor local accesible con usuario/clave que configures. |
| [Node.js 20+](https://nodejs.org/) | Para Angular y `npm`. |

## Configuración de PostgreSQL

1. Crea una base de datos vacía, por ejemplo:

   ```sql
   CREATE DATABASE ccl_inventario;
   ```

2. Ajusta la cadena en `backend/CclInventario.Api/appsettings.json` (`ConnectionStrings:Default`) con tu **host**, **puerto**, **usuario** y **contraseña**.

3. **Esquema `productos`:** al **arrancar la API**, EF Core ejecuta `Database.EnsureCreatedAsync()` y crea la tabla si la base existe. **O bien** puedes crear base + tabla con el script (ver paso 4) y luego arrancar la API.

4. **Script SQL manual** `database/seed-manual.sql` (desde `psql`, pgAdmin, etc.):

   - **BLOQUE 1** (solo conectado a la base **`postgres`**): crea la base `ccl_inventario` (omítelo si ya existe).
   - **BLOQUE 2 y 3** (solo conectado a **`ccl_inventario`**): crea la tabla `productos` si no está (`IF NOT EXISTS`) e inserta datos de ejemplo.

   El `INSERT` es idempotente: no duplica filas si la tabla ya tiene datos.

## Credenciales de demostración (login)

Definidas en `appsettings.json` → sección `Auth:Users` (en memoria / configuración):

- **Usuario:** `admin`  
- **Clave:** `Ccl2026!`

*(Cámbialas en tu entorno local si lo deseas.)*

## Cómo ejecutar en local

### 1. API (backend)

```powershell
cd backend/CclInventario.Api
dotnet run
```

**Tests unitarios (xUnit):**

```powershell
dotnet test backend/CclInventario.sln
```

Por defecto escucha en **http://localhost:5088** (perfil `http` en `launchSettings.json`). Swagger en desarrollo: `http://localhost:5088/swagger`.

Endpoints principales (como en el enunciado):

- `POST /auth/login` — obtiene el JWT (público).  
- `GET /productos/inventario` — inventario actual (**requiere** `Authorization: Bearer …`).  
- `POST /productos/movimiento` — entrada o salida (**requiere** Bearer). Cuerpo JSON ejemplo:

  ```json
  { "productoId": 1, "tipo": "entrada", "cantidad": 10 }
  ```

  `tipo`: `entrada` o `salida`. No se permite salida que deje stock negativo.

### 2. Cliente Angular (frontend)

```powershell
cd frontend/inventario-ui
npm install
npm start
```

**Lint (ESLint + reglas recomendadas de Angular):**

```powershell
npm run lint
```

**Tests unitarios (Karma, una corrida):**

```powershell
npm run test:ci
```

Abre **http://localhost:4200**. La URL de la API está en `src/app/core/environment.ts` (`apiBaseUrl`); debe coincidir con la API (por defecto `http://localhost:5088`). La API tiene **CORS** habilitado para `http://localhost:4200`.

En el cliente Angular (`src/app/core/patterns/`) hay una versión **explícita** alineada al backend: **Singleton** (`CorrelationIdService`), **Factory** (`ApiEndpointFactory`), **Strategy** + factory de mensajes (`MovimientoMessageStrategyFactory`), **Observer** (`InventorySubjectService` + `ConsoleInventoryUiObserver`) y **Facade** (`InventoryUiFacade`). Las pantallas de inventario/movimiento consumen el facade; `AuthService` y el interceptor usan la factory de URLs.

## Estructura del repositorio

```text
backend/CclInventario.sln          # Solución .NET
backend/CclInventario.Api/       # Web API + EF Core + JWT (net9.0)
database/seed-manual.sql         # Carga manual de datos iniciales
frontend/inventario-ui/          # Angular 19 (standalone, rutas lazy)
githooks/commit-msg              # Hook opcional para limpiar pies de herramientas en commits
README.md
```

---

## Flujo de trabajo con Git y GitHub

Se trabaja con dos ramas principales:

| Rama | Propósito |
|------|-----------|
| **`main`** | Rama estable de entrega; refleja lo que se considera “listo” para revisión. |
| **`develop`** | Rama de integración donde se van acumulando los commits de implementación **por etapas** (cada commit representa un paso lógico del trabajo). |

### Mensajes de commit sin pies de herramientas

Tras clonar, activa el hook local (Git para Windows / bash):

```powershell
git config core.hooksPath githooks
```

En Git Bash (recomendado una vez):

```bash
chmod +x githooks/commit-msg
```

El script `githooks/commit-msg` elimina líneas `Made-with: Cursor` del mensaje final para que el historial quede limpio aunque tu editor las añada.

### Flujo típico del desarrollador

1. Clonar el repositorio:

   ```powershell
   git clone https://github.com/SuarezSebastian2/PruebaCCL-Inventarios.git
   cd PruebaCCL-Inventarios
   git config core.hooksPath githooks
   ```

2. Trabajar en **`develop`** (nuevas funciones, correcciones):

   ```powershell
   git checkout develop
   # ... cambios ...
   git add .
   git commit -m "feat: descripcion-corta-en-imperativo"
   git push origin develop
   ```

3. Cuando haya un hito estable, integrar en **`main`** (localmente o vía **Pull Request** en GitHub):

   ```powershell
   git checkout main
   git merge develop
   git push origin main
   ```

### Commits

Se recomienda **Conventional Commits** (`feat:`, `fix:`, `chore:`, `docs:`) y mensajes en **imperativo**, **un cambio lógico por commit**, de modo que el historial refleje etapas claras del desarrollo (como pide la prueba).

### Remoto

- Origen: `https://github.com/SuarezSebastian2/PruebaCCL-Inventarios.git`

---

## Seguridad

- No subas **secretos reales** ni cadenas de producción.  
- El JWT usa una clave simétrica de ejemplo en `appsettings.json`; en producción debe provenir de **variables de entorno** o un almacén de secretos.

## Licencia

Proyecto de prueba técnica — uso académico / evaluación.
