# Control de Ventas — Gorditas & Tacos

App web (Blazor WebAssembly, .NET 9) para calcular la cuenta de un pedido grupal de
gorditas/tacos, incluyendo la regla del cumpleañero que no paga.

## ¿Cómo correrlo?

Requisitos: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado.

```bash
cd GorditasApp
dotnet run
```

Abre la URL que te indique la consola (algo como `https://localhost:5001`).

> Nota: este proyecto se generó sin acceso a internet en el entorno donde lo escribí,
> así que **no pude ejecutar `dotnet restore`/`dotnet build` para verificarlo**. La primera
> vez que corras `dotnet run` o `dotnet restore` en tu máquina, .NET descargará los paquetes
> de NuGet necesarios (`Microsoft.AspNetCore.Components.WebAssembly` y su DevServer) — esto
> es normal y solo pasa una vez. Si algo no compila a la primera, dímelo con el error exacto
> y lo corrijo.

## ¿Cómo publicarlo "en línea" y como app?

1. **Como sitio web público**: `dotnet publish -c Release` genera archivos estáticos en
   `bin/Release/net9.0/publish/wwwroot`. Esa carpeta se puede subir gratis a
   **Azure Static Web Apps**, **GitHub Pages**, **Netlify** o **Cloudflare Pages** (es pura
   web estática, no necesita servidor con backend).
2. **Como "app" instalable (PWA)**: ya incluye `manifest.json` y `service-worker.js`, así que
   en Chrome/Edge (escritorio o Android) el navegador va a ofrecer el botón "Instalar app".
   En iPhone, desde Safari: compartir → "Agregar a pantalla de inicio".
3. **Como app nativa real** (opcional, a futuro): esta misma lógica de negocio
   (carpetas `Models/` y `ViewModels/`) se puede reutilizar tal cual dentro de un proyecto
   **.NET MAUI Blazor Hybrid** para publicar en Play Store / App Store sin reescribir nada.

## Arquitectura (MVVM)

- **Models/** — Entidades del dominio: `Persona`, `ItemPedido`, `TipoAlimento` (Gordita/Taco),
  `TipoRelleno` (Maciza/Surtida), `ConfiguracionPrecios`, `ResultadoPersona`.
- **ViewModels/PedidoViewModel.cs** — Todo el estado de la ronda de pedido y el método
  `Calcular()` con las reglas de negocio. No sabe nada de HTML/Blazor — se podría reusar
  igual en una app de escritorio (WPF/MAUI) o probar con unit tests sin tocar la UI.
- **Pages/Home.razor** — La Vista. Se inyecta el ViewModel (`@inject PedidoViewModel Vm`)
  y solo se encarga de mostrar datos y llamar métodos del ViewModel — no calcula nada por
  su cuenta.

## Reglas de negocio implementadas

1. Se configuran los precios del día: precio de gordita, precio de taco y costo total
   del refresco a repartir.
2. Cada persona agrega su pedido (gorditas y/o tacos, maciza o surtida, cantidad libre).
3. Si una persona se marca como **cumpleañero 🎂**, no paga su propio pedido.
4. El costo del pedido del cumpleañero se reparte **entre quienes NO son cumpleañero Y
   marcaron "coopera para el cumpleañero"**. Si alguien decide no cooperar, simplemente
   no se le cobra esa parte y el monto se reparte solo entre quienes sí cooperan.
5. El costo del refresco se reparte por separado entre quienes marcaron "coopera para
   el refresco" (puede ser un grupo distinto al de arriba).
6. Al calcular, se muestra el desglose por persona y un total general para verificar que
   lo recaudado cubra el gasto real (comida + refresco).

## Ejemplo rápido

- Precio gordita: $15, sin refresco.
- Ana (cumpleañera): 3 gorditas maciza → cuesta $45, pero paga $0.
- Beto: 2 gorditas surtida, coopera con el cumpleañero → paga sus $30 + aporte.
- Carla: 2 tacos maciza, **no** coopera con el cumpleañero → solo paga sus $30.

Si solo Beto coopera, absorbe él solo los $45 de Ana (además de sus $30). Si Ana también
tuviera más cooperantes, ese aporte se dividiría entre todos ellos.

## Reporte imprimible / descargable

Después de darle "Calcular cuenta" aparecen dos botones sobre la tabla de resultados:

- **🖨️ Imprimir / Guardar como PDF** — abre el diálogo de impresión del navegador mostrando
  *solo* la tabla de resultados (los formularios de captura se ocultan automáticamente).
  Ahí mismo, en el diálogo de impresión, se puede elegir "Guardar como PDF" en vez de una
  impresora física — así queda el PDF sin necesitar ninguna librería extra.
- **⬇️ Descargar CSV** — descarga un archivo `.csv` con el desglose completo (se abre bien
  en Excel/Google Sheets, incluye acentos y "ñ" correctamente).

## Ideas para siguientes pasos (si luego quieres más)

- Guardar historial de "rondas" pasadas en base de datos (SQLite/Postgres vía Entity
  Framework) — hoy todo vive solo en memoria del navegador mientras la pestaña esté abierta.
- Compartir el resultado por WhatsApp con un botón (armar el texto y usar `mailto`/`https://wa.me/`).
- Login simple para que solo el organizador del pedido pueda editar precios.
