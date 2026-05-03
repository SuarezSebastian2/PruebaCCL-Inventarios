# Documentación técnica — Prueba CCL Inventario

Este documento complementa el [`README.md`](../README.md) raíz con **arquitectura**, **diagramas** y **mapa de componentes**. Las rutas de archivos son relativas a la raíz del repositorio.

---

## 1. Resumen ejecutivo

| Capa | Tecnología | Rol |
|------|--------------|-----|
| Cliente | Angular 19 (standalone, lazy routes) | Login, listado de inventario, movimientos, CRUD de productos |
| API | ASP.NET Core 9 (`net9.0`), controllers | JWT, CORS, rate limiting en login, Swagger en desarrollo |
| Persistencia | EF Core + Npgsql | Tabla `public.productos`; `EnsureCreated` + seed opcional al arranque |

Patrones **GoF** aplicados de forma explícita en backend (`Patterns/`) y un espejo conceptual en frontend (`src/app/core/patterns/`).

---

## 2. Diagrama de contexto

Relación del sistema con usuario y base de datos.

[![](https://img.plantuml.biz/plantuml/svg/LP11JyCm38NlbV8Vp7O1eOq9SUe1hHi33KrJMJiXf5syrdIHigGa3l3pSGeNRf7z_BsNfpALpGEZvtWsAhlW4vq9UeFQgemr6uAbiqpVxA0b0q47fZCgEJFqpY1ZhxiJGwizDQoTJIjis7aOxKD4hzq8CC1UPKc0jXFag2eBA6mN3Vg3lJIkGNDoWVExszdiQj0dnNYJq0iJwT_nShc1jVqaouaEHRa-sA8gfzjLBM4zmJKylzO3HNP_b2TF9DbQP3nY44efVfsdwcLpi7qy6jrCU-_QAD73SFkmWF4R6UE-qfVsC9dS3wboY44AICyqIXOvx5PLBRuBGay-sQiiBP90E3ci7d-ZtS2sxyA7L3qdsmwT_m00)](https://editor.plantuml.com/uml/LP11JyCm38NlbV8Vp7O1eOq9SUe1hHi33KrJMJiXf5syrdIHigGa3l3pSGeNRf7z_BsNfpALpGEZvtWsAhlW4vq9UeFQgemr6uAbiqpVxA0b0q47fZCgEJFqpY1ZhxiJGwizDQoTJIjis7aOxKD4hzq8CC1UPKc0jXFag2eBA6mN3Vg3lJIkGNDoWVExszdiQj0dnNYJq0iJwT_nShc1jVqaouaEHRa-sA8gfzjLBM4zmJKylzO3HNP_b2TF9DbQP3nY44efVfsdwcLpi7qy6jrCU-_QAD73SFkmWF4R6UE-qfVsC9dS3wboY44AICyqIXOvx5PLBRuBGay-sQiiBP90E3ci7d-ZtS2sxyA7L3qdsmwT_m00)

---

## 3. Arquitectura lógica (contenedores)

[![](https://img.plantuml.biz/plantuml/svg/LP11JWCn34NtaN87CojOb0EmG4rdYbI4O63O8HPkngGMQJ8adcuGf-88N8oa0WbMblxt-r_sCWiccVTUgnFPqPvWyiX1As7n12QzpInaPKu8_klJiKKujJ481HfZedocbLQUNWGaGcAt4nWv5O9ZgDQ4zXKTGTDwfe8ryAuLm9C9RlQOOEZDCs2kKwkFF_i3fGENYb-WjNuJ3YK24yTpC_4HC_tcFzQXnFp3Z2YunLpKFcPnYORxcwPItUe8QLLQOR6uh35m0G_huH6kXxjRhQfGZMvLzFKLjELMhPOKnlAhRm00)](https://editor.plantuml.com/uml/LP11JWCn34NtaN87CojOb0EmG4rdYbI4O63O8HPkngGMQJ8adcuGf-88N8oa0WbMblxt-r_sCWiccVTUgnFPqPvWyiX1As7n12QzpInaPKu8_klJiKKujJ481HfZedocbLQUNWGaGcAt4nWv5O9ZgDQ4zXKTGTDwfe8ryAuLm9C9RlQOOEZDCs2kKwkFF_i3fGENYb-WjNuJ3YK24yTpC_4HC_tcFzQXnFp3Z2YunLpKFcPnYORxcwPItUe8QLLQOR6uh35m0G_huH6kXxjRhQfGZMvLzFKLjELMhPOKnlAhRm00)

---

## 4. Backend — capas y patrones

### 4.1 Pipeline HTTP (orden relevante)

Definido en `backend/CclInventario.Api/Program.cs`: HTTPS, middleware de cabeceras de seguridad, CORS, autenticación/autorización, rate limiter global + política `login` en `AuthController`, controladores.

[![](https://img.plantuml.biz/plantuml/svg/NP71JiCm38RlbVeEPgVjC7W3jAfKwg63adOtBc6njaXfKXAN2PwU98MWzk94lp_RlvE38svf6bIUtT4L1uHHSQdpZ2GfX5QEgAH6gFk-XMtHjVVFLG-biRZBCq-P4SZ0ko4o0mXfyKpIw90ALwullic6uSU4ZZR07R3gTP4xEQo9HiSmLKSk3EmMP8VdoKhwgf4BjEueXL1uunOZtrNbkgztwM3xX9-5laoAsrq4onVMhSbYyhjhacSUviF3c_uLZPNVF7agJdszBcMSi9639BJpVis2EFAnD9giKSfxdWtqWSap_nAm5-Qczuz_syOZYN4b7v8mMmynIREb49CKkhFcloYEEw0M_fT_0000)](https://editor.plantuml.com/uml/NP71JiCm38RlbVeEPgVjC7W3jAfKwg63adOtBc6njaXfKXAN2PwU98MWzk94lp_RlvE38svf6bIUtT4L1uHHSQdpZ2GfX5QEgAH6gFk-XMtHjVVFLG-biRZBCq-P4SZ0ko4o0mXfyKpIw90ALwullic6uSU4ZZR07R3gTP4xEQo9HiSmLKSk3EmMP8VdoKhwgf4BjEueXL1uunOZtrNbkgztwM3xX9-5laoAsrq4onVMhSbYyhjhacSUviF3c_uLZPNVF7agJdszBcMSi9639BJpVis2EFAnD9giKSfxdWtqWSap_nAm5-Qczuz_syOZYN4b7v8mMmynIREb49CKkhFcloYEEw0M_fT_0000)

### 4.2 Módulo de inventario (Facade + estrategias + observer)

[![](https://img.plantuml.biz/plantuml/svg/TPB1IiD048RlWRp3U5DeyUv1McC31Ti4jWT1FKoICLtOx8RD9jZdyX5yCMTMSwZIs-J_lvdvPybg2EZ3s9aqkGf7wWXwWzgcIT314Eo-FflHED1s8ikaTd2TOuqDBK05Zu7QqmAAQY0_aRz9apHvARrhnZgu8NCsU6SC-JV00SgCpUqsJdB-z3C6Rb_j7od2-LheTTy_LZA6FaAKiyCpopitwKvBpvo1EsL0P5JEo8PNOuF_oOXi52CAZRv8A7KMDZkYRMdlWdxNvCz2drknULySf7icdbpRQjlEhnIHua79OvKPB9VtSYtSmJXWcaWfKhnq_f2JL1vBYVvRIiQPu9KyeHyhesi4Yfk7D15PR5x7jeqXsNSn2ZRE4qChiWt_0Ty0)](https://editor.plantuml.com/uml/TPB1IiD048RlWRp3U5DeyUv1McC31Ti4jWT1FKoICLtOx8RD9jZdyX5yCMTMSwZIs-J_lvdvPybg2EZ3s9aqkGf7wWXwWzgcIT314Eo-FflHED1s8ikaTd2TOuqDBK05Zu7QqmAAQY0_aRz9apHvARrhnZgu8NCsU6SC-JV00SgCpUqsJdB-z3C6Rb_j7od2-LheTTy_LZA6FaAKiyCpopitwKvBpvo1EsL0P5JEo8PNOuF_oOXi52CAZRv8A7KMDZkYRMdlWdxNvCz2drknULySf7icdbpRQjlEhnIHua79OvKPB9VtSYtSmJXWcaWfKhnq_f2JL1vBYVvRIiQPu9KyeHyhesi4Yfk7D15PR5x7jeqXsNSn2ZRE4qChiWt_0Ty0)

| Patrón GoF | Ubicación principal | Función en el proyecto |
|------------|---------------------|-------------------------|
| Facade | `Patterns/Facade/InventoryFacade.cs` | Orquesta listado, movimientos y CRUD sin exponer detalles internos al controlador |
| Strategy | `Patterns/Strategies/*MovimientoStrategy.cs` | Entrada vs salida de stock |
| Factory | `Patterns/Strategies/MovimientoStrategyFactory.cs`, `Patterns/Factory/*Jwt*` | Resolución de estrategia; creación/configuración JWT |
| Observer | `Patterns/Observer/InventoryChangeNotifier.cs`, `InventoryLoggingObserver.cs` | Notificación tras movimientos (p. ej. logging) |
| Singleton | `Patterns/Singleton/OperationSequenceGenerator.cs` | Generación de secuencias en el proceso |

### 4.3 Autenticación

- `Controllers/AuthController.cs` — `POST auth/login` (anónimo, rate limited).
- `Services/DemoUserStore.cs` — usuarios demo desde configuración.
- `Patterns/Factory/JwtTokenFactory.cs` — emisión de JWT.

---

## 5. Modelo de datos

Tabla física: `public.productos` (mapeo en `backend/CclInventario.Api/Data/AppDbContext.cs`).

[![](https://img.plantuml.biz/plantuml/svg/FOv12eCm44Nt0tE7wKu35kcoYBWlXLvXp0mOY8d4M2YbTszQqErxdySVrimfRxD7EEL9PwN5imi8sMMlT8-YFf8e2UUuKhLietVslAGecosaHa3GKDOlEY2z4OWC6UF46BgI2vcwxd7h-vzecbs7E8z9_vCd9pjngYvjM-_QSWaB7ms43yAWGShVBm00)](https://editor.plantuml.com/uml/FOv12eCm44Nt0tE7wKu35kcoYBWlXLvXp0mOY8d4M2YbTszQqErxdySVrimfRxD7EEL9PwN5imi8sMMlT8-YFf8e2UUuKhLietVslAGecosaHa3GKDOlEY2z4OWC6UF46BgI2vcwxd7h-vzecbs7E8z9_vCd9pjngYvjM-_QSWaB7ms43yAWGShVBm00)

Entidad: `backend/CclInventario.Api/Entities/Producto.cs`.

---

## 6. API REST — rutas principales

Prefijos: controladores usan `[Route("auth")]` y `[Route("productos")]` (sin prefijo `/api/v1` en el código actual). Ajusta la URL base según `launchSettings.json` (típico `http://localhost:5088`).

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/auth/login` | No | Devuelve JWT |
| GET | `/productos/inventario` | Bearer | Lista inventario |
| POST | `/productos/movimiento` | Bearer | Entrada/salida de stock |
| GET | `/productos/{id}` | Bearer | Detalle para edición |
| POST | `/productos` | Bearer | Alta con stock inicial |
| PUT | `/productos/{id}` | Bearer | Actualiza nombre |
| DELETE | `/productos/{id}` | Bearer | Baja solo si cantidad = 0 |

Swagger UI (solo desarrollo): `{baseUrl}/swagger`.

---

## 7. Secuencia — login y consulta de inventario

[![](https://img.plantuml.biz/plantuml/svg/TL9BJm8n4ButwNyOEMa8Sj-3OP5780RMx18lNCRT4HfBkscxAFvwfujP5UEfwSmttsFQaG_eGdFGKbo5FHq8QetAI15Kq0GvbGsPKc42skwKWHEKrlX61uIAG9aZ6IPGLWefi0pMmSOtiG3eOID5pUogL3MQ0BtKx1gD3l8ixSK-dvU8R05fdjskfaMBuBiK5GPyHa_GowmFEqVvuqFRdOoZx0O6Tv4A4hXtLBLsDNafOerRaJE1R9qNCCGcx8TjajQkEcAWiqX4TU097Up6ayiv445_QvPleR2lP6PjnjCNlDCDiGTzeFTQEV8BPg_eDtzdvpmnvoLLl8hbKx4rrodRoiaTLKatFw1dC_Cf6w-ThHhMziFldMzDdBDEVM1GrYGm9dJa_aas6JDNztZJcHHS6NGgBqgZlyorpDShq8h_oDz08p8L_vjF)](https://editor.plantuml.com/uml/TL9BJm8n4ButwNyOEMa8Sj-3OP5780RMx18lNCRT4HfBkscxAFvwfujP5UEfwSmttsFQaG_eGdFGKbo5FHq8QetAI15Kq0GvbGsPKc42skwKWHEKrlX61uIAG9aZ6IPGLWefi0pMmSOtiG3eOID5pUogL3MQ0BtKx1gD3l8ixSK-dvU8R05fdjskfaMBuBiK5GPyHa_GowmFEqVvuqFRdOoZx0O6Tv4A4hXtLBLsDNafOerRaJE1R9qNCCGcx8TjajQkEcAWiqX4TU097Up6ayiv445_QvPleR2lP6PjnjCNlDCDiGTzeFTQEV8BPg_eDtzdvpmnvoLLl8hbKx4rrodRoiaTLKatFw1dC_Cf6w-ThHhMziFldMzDdBDEVM1GrYGm9dJa_aas6JDNztZJcHHS6NGgBqgZlyorpDShq8h_oDz08p8L_vjF)

---

## 8. Secuencia — movimiento de inventario

[![](https://img.plantuml.biz/plantuml/svg/NPBBQiCm44Nt1l_3DAkMKX9ALrcKF8oXq3OXphARYJHr1cJ9Z6IJ_5gt_R7AJjgatWdftBbdG3CVK49JcJIv2Nkg26g3RDCaS3041Qc6h6ASGkLQhfXiSA09sBRnYC8kJTAaZZDOSOqsm6XkoyQWZ00z5Elvu7KjJZSgEBzqDeWpXaxaShjv7Q0lVOUJOuuADVLOFc3U_goA81YeF9vwXqvrdUswHZg47iWMdNVKWuVdlXkci5uLMnZNluBZoyPfWYfm6-UVFVj4JEOnjg6IVIoNYylS7wsw2aNhlCEBY6DjMA60f_BxotxOo0jgX35uDApnZ6QBo1ROqdAFjYH_lkuAtrtWJoQ1szNEaxGaT_thPHCXtvY0EefhkhOuUNScSVCfF4ucS0_PTlLX7P284tWw7Fw5JllEoEhuINu0)](https://editor.plantuml.com/uml/NPBBQiCm44Nt1l_3DAkMKX9ALrcKF8oXq3OXphARYJHr1cJ9Z6IJ_5gt_R7AJjgatWdftBbdG3CVK49JcJIv2Nkg26g3RDCaS3041Qc6h6ASGkLQhfXiSA09sBRnYC8kJTAaZZDOSOqsm6XkoyQWZ00z5Elvu7KjJZSgEBzqDeWpXaxaShjv7Q0lVOUJOuuADVLOFc3U_goA81YeF9vwXqvrdUswHZg47iWMdNVKWuVdlXkci5uLMnZNluBZoyPfWYfm6-UVFVj4JEOnjg6IVIoNYylS7wsw2aNhlCEBY6DjMA60f_BxotxOo0jgX35uDApnZ6QBo1ROqdAFjYH_lkuAtrtWJoQ1szNEaxGaT_thPHCXtvY0EefhkhOuUNScSVCfF4ucS0_PTlLX7P284tWw7Fw5JllEoEhuINu0)

---

## 9. Frontend Angular

### 9.1 Rutas (`frontend/inventario-ui/src/app/app.routes.ts`)

| Ruta | Guard | Componente (lazy) |
|------|-------|---------------------|
| `/login` | — | `LoginComponent` |
| `/inventario` | `authGuard` | `InventarioComponent` |
| `/movimiento` | `authGuard` | `MovimientoComponent` |
| `/productos/nuevo` | `authGuard` | `ProductoFormComponent` |
| `/productos/:id/editar` | `authGuard` | `ProductoFormComponent` |
| `''` | — | redirect → `inventario` |

### 9.2 Módulo core

- `core/environment.ts` — `apiBaseUrl` (debe coincidir con la API).
- `core/auth.service.ts`, `core/auth.guard.ts`, `core/auth.interceptor.ts` — sesión y adjunto del Bearer.
- `core/inventario-api.service.ts` — llamadas HTTP a productos/auth.
- `core/patterns/` — Singleton (`CorrelationIdService`), Factory (`ApiEndpointFactory`), Strategy (`movimiento-message.strategy`), Observer + Facade (`inventory-ui.facade.ts`, etc.), alineados al backend como ejercicio de prueba.

[![](https://img.plantuml.biz/plantuml/svg/LP71QiCm38Rl1h-3bftx0ZsCPgMXWHR2ahB1sC4uRYBgsCPH0cNiOVOiUx4vJZRQYv5-IVydv4rFmjFGQSuUg5MT0gS56iu8IIl8l3MaJ0sfQGOj_1hSptU3HlHm1MczuemphSu4PC5ZqnBKw9Kaj6OgEI4leb6G5E7j4_Za3E1Tss3n0S7a47Cqep9X3xHHpAFOsH4x3Fei7gFelAq7INPrjhwBUi7Prnrf6xRw0-NHsFhh2JCXHQtYI9hT52Tgwh1KVaIvr8kyZ1tfGErTuLJj5vpKTgWXUGxKwJmlkx9AffRTMmMFhorIlCm1LgkdPIoVuy0-_aV574LUcjqcuOtfvBU6FP7RwkaOd6q2B_pNBm00)](https://editor.plantuml.com/uml/LP71QiCm38Rl1h-3bftx0ZsCPgMXWHR2ahB1sC4uRYBgsCPH0cNiOVOiUx4vJZRQYv5-IVydv4rFmjFGQSuUg5MT0gS56iu8IIl8l3MaJ0sfQGOj_1hSptU3HlHm1MczuemphSu4PC5ZqnBKw9Kaj6OgEI4leb6G5E7j4_Za3E1Tss3n0S7a47Cqep9X3xHHpAFOsH4x3Fei7gFelAq7INPrjhwBUi7Prnrf6xRw0-NHsFhh2JCXHQtYI9hT52Tgwh1KVaIvr8kyZ1tfGErTuLJj5vpKTgWXUGxKwJmlkx9AffRTMmMFhorIlCm1LgkdPIoVuy0-_aV574LUcjqcuOtfvBU6FP7RwkaOd6q2B_pNBm00)

---

## 10. Configuración clave

| Archivo | Contenido relevante |
|---------|---------------------|
| `backend/CclInventario.Api/appsettings.json` | `ConnectionStrings:Default`, JWT, `Cors:AllowedOrigins`, usuarios demo `Auth:Users` |
| `backend/CclInventario.Api/Properties/launchSettings.json` | URLs y perfiles de ejecución |
| `database/seed-manual.sql` | Creación opcional de BD/tabla y datos demo |
| `frontend/inventario-ui/src/app/core/environment.ts` | Base URL de la API |

---

## 11. Tests y calidad

| Ámbito | Comando |
|--------|---------|
| Backend | `dotnet test backend/CclInventario.sln` |
| Frontend unit | `cd frontend/inventario-ui && npm run test:ci` |
| Frontend lint | `npm run lint` |

---

## 13. Referencias cruzadas

- Instalación, credenciales demo y comandos: [`README.md`](../README.md).
- Contrato detallado de cuerpos JSON: Swagger en desarrollo y comentarios XML en controladores.

---

*Documento generado para el repositorio Prueba CCL — inventario. Mantener alineado con cambios en rutas, entidades y `Program.cs`.*
