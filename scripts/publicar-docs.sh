#!/usr/bin/env bash
set -euo pipefail
REPO_NAME="${1:-Apuntador}"
rm -rf docs publish
dotnet publish Apuntador.csproj -c Release -o publish
cp -R publish/wwwroot docs
sed -i "s|<base href=\"/\" />|<base href=\"/${REPO_NAME}/\" />|" docs/index.html
cp docs/index.html docs/404.html
touch docs/.nojekyll
echo "Sitio generado en docs/. Configura GitHub Pages: Deploy from a branch > main > /docs"
