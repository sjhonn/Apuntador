# Apuntador

Aplicación personal de notas, actividades, calendario y recordatorios desarrollada con Blazor WebAssembly y .NET 8.

## Ejecutar localmente

Desde la carpeta donde se encuentra `Apuntador.csproj`:

```bash
dotnet restore
dotnet run
```

No ejecutes `cd Apuntador` si la terminal ya muestra una ruta terminada en `/Apuntador`.

## Dónde está index.html

En un proyecto Blazor WebAssembly, el archivo fuente está en:

```text
wwwroot/index.html
```

Ese archivo no se publica directamente desde el código fuente. Durante `dotnet publish`, Blazor genera un sitio estático completo y coloca el `index.html` final en:

```text
publish/wwwroot/index.html
```

GitHub Pages necesita ese archivo compilado junto con `_framework`, CSS, JavaScript e imágenes. Por eso no basta con mover o copiar únicamente `wwwroot/index.html` a la raíz del repositorio.

## Publicación recomendada: GitHub Actions

1. Sube el contenido de esta carpeta al repositorio de GitHub.
2. En GitHub abre `Settings > Pages`.
3. En `Build and deployment`, selecciona `GitHub Actions`.
4. Haz un push a la rama `main`.
5. El workflow `.github/workflows/deploy-pages.yml` compilará el proyecto y publicará `publish/wwwroot`.

El workflow ajusta automáticamente `<base href>` tanto para repositorios normales como para repositorios llamados `usuario.github.io`.

## Publicación alternativa desde /docs

En Git Bash:

```bash
bash scripts/publicar-docs.sh NOMBRE-EXACTO-DEL-REPOSITORIO
git add docs
git commit -m "Publicar Apuntador"
git push
```

En PowerShell:

```powershell
.\scripts\publicar-docs.ps1 -RepoName "NOMBRE-EXACTO-DEL-REPOSITORIO"
git add docs
git commit -m "Publicar Apuntador"
git push
```

Luego configura `Settings > Pages > Deploy from a branch > main > /docs`.

No uses simultáneamente GitHub Actions y `/docs`; selecciona un solo método.

## Formatos de exportación

- JSON: copia completa restaurable.
- TXT: lectura simple de notas y actividades.
- CSV: notas y actividades por separado, compatible con Excel.
- Markdown: documentación y GitHub.
- HTML: archivo visual para navegador.
- ICS: actividades para calendarios externos.

Los datos de trabajo permanecen en LocalStorage. Para trasladarlos a otro navegador o equipo, exporta JSON e impórtalo en la otra instalación.
