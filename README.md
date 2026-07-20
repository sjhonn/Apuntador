# Apuntador

Aplicación personal de notas, calendario, actividades y recordatorios creada con Blazor WebAssembly. Funciona completamente en el navegador y guarda los datos en LocalStorage.

## Funciones

- Crear, editar, buscar, clasificar y eliminar notas.
- Marcar notas importantes.
- Calendario mensual con actividades por fecha.
- Actividades pendientes, completadas y vencidas.
- Exportación JSON y TXT; importación de copias JSON.
- Modo claro y oscuro persistente.
- Diseño responsive para móvil, tablet y escritorio.
- Despliegue automático y gratuito en GitHub Pages.

## Ejecución local

Requiere .NET 8 SDK.

```bash
dotnet restore
dotnet run
```

Abre la URL HTTPS indicada en la consola. Para simular producción:

```bash
dotnet publish -c Release -o publish
```

Puedes servir `publish/wwwroot` con cualquier servidor HTTP estático. No abras `index.html` directamente como archivo, porque WebAssembly requiere HTTP.

## Publicación en GitHub Pages

1. Crea un repositorio de GitHub y sube todo el proyecto a la rama `main`.
2. En `Settings > Pages`, selecciona `GitHub Actions` como fuente.
3. El workflow `.github/workflows/deploy-pages.yml` compilará y publicará el sitio automáticamente.
4. La ruta base se ajusta al nombre real del repositorio durante el despliegue.

## Privacidad

No existe backend ni base de datos externa. La información queda guardada en el navegador del usuario. Se recomienda exportar una copia JSON periódicamente.
