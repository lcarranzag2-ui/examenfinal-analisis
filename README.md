# PruebasSoftware - Examen Final (Pruebas unitarias, de integración y de API)

**Curso:** Análisis de Sistemas II - Universidad Mariano Gálvez de Guatemala
**Estudiante:** Ludin Eduardo Carranza Guerra
**Carné:** 0907-22-5817

Proyecto basado en **DemoTesting** del repositorio original
[eruanoc5/PruebasSoftware](https://github.com/eruanoc5/PruebasSoftware).
Esta es una copia independiente donde se agregaron las pruebas solicitadas.

## Estructura

| Proyecto | Descripción |
|---|---|
| `ClienteApp` (ClienteApp.Core) | Modelo, servicio, repositorio y acceso a SQLite |
| `ClienteApp.Api` | API REST con ASP.NET Core |
| `ClienteApp.Tests` | Pruebas con xUnit, Moq y WebApplicationFactory |

## Pruebas agregadas

| # | Nivel | Archivo | Prueba | Qué comprueba |
|---|---|---|---|---|
| 1 | Unitaria | `Unitarias/ClienteServiceTests.cs` | `ObtenerPorId_ClienteExistente_DebeRetornarClienteYConsultarRepositorioUnaVez` | Con un mock de `IClienteRepository`, el servicio devuelve el cliente esperado y llama a `ObtenerPorId` del repositorio **una sola vez** (`Times.Once`). |
| 2 | Integración | `Integracion/ClienteRepositoryTests.cs` | `ObtenerPorId_IdInexistente_DebeRetornarNull` | Con `ClienteService` + `ClienteRepository` reales y una base SQLite temporal, consultar un id que no existe devuelve `null`. |
| 3 | API | `Api/ClientesApiTests.cs` | `ObtenerPorId_ClienteExistente_DebeRetornar200` | Registra un cliente con `POST` y luego `GET /api/Clientes/{id}` devuelve **200 OK** con sus datos. |
| 4 | API | `Api/ClientesApiTests.cs` | `ObtenerPorId_ClienteInexistente_DebeRetornar404` | `GET /api/Clientes/99999` devuelve **404 Not Found** con el mensaje "Cliente no encontrado". |

### Decisiones tomadas

- **Prueba de integración:** antes de consultar se registra un cliente, así la base no está vacía
  y se comprueba que la búsqueda por id funciona de verdad.
- **Prueba 200 OK:** cada `ClienteApiFactory` crea una base temporal nueva y vacía, por eso no se
  usa un id fijo (por ejemplo 1). Primero se crea el cliente y se usa el id que devuelve la API.
- **Prueba 404:** además del código de estado se revisa el mensaje, para asegurar que el 404 lo
  devuelve el controlador (cliente no encontrado) y no porque la ruta no exista.

## Cómo ejecutar las pruebas

Requisito: **.NET 10 SDK**.

```bash
dotnet restore
dotnet build
dotnet test --logger "console;verbosity=detailed"
```

Resultado esperado: **18 pruebas, 18 correctas, 0 fallidas** (14 que ya traía el proyecto + 4 nuevas).

Las pruebas también se ejecutan automáticamente con **GitHub Actions** en cada push a `main`
(archivo `.github/workflows/pruebas.yml`).

## Evidencias

Las capturas están en la carpeta [`evidencias/`](evidencias/).

| Captura | Descripción |
|---|---|
| `00 pruebas originales del proyecto.png` | Proyecto original ejecutado en mi equipo: 14 pruebas correctas |
| `01 actions exitosa.png` | Ejecución en GitHub Actions finalizada con éxito |
| `02 pruebas parte1.png` | Detalle de la ejecución: prueba de integración y prueba unitaria nuevas aprobadas |
| `03 pruebas parte 2.png` | Pruebas de API nuevas aprobadas: cliente existente (200 OK) y cliente inexistente (404 Not Found) |
| `04 pruebas totales.png` | Resumen final: 18 pruebas en total, 18 correctas |
| `05 bloqueo local de windows.png` | Error en mi equipo al cargar la DLL de pruebas (parte 1) |
| `06 bloqueo local de windows.png` | Error en mi equipo al cargar la DLL de pruebas (parte 2) |

### Nota sobre la ejecución

En mi equipo el proyecto original corrió bien (14 pruebas). Al agregar las pruebas nuevas,
Windows 11 bloqueó la DLL de pruebas con el error *"Una directiva de Control de aplicaciones
bloqueó este archivo (0x800711C7)"*, causado por el **Control inteligente de aplicaciones**,
que bloquea archivos sin firma digital. Para no desactivar esa protección, configuré
**GitHub Actions** (`.github/workflows/pruebas.yml`), que compila y ejecuta todas las pruebas
en un servidor Linux con .NET 10 cada vez que se hace push a `main`.