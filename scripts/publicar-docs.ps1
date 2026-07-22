param([string]$RepoName = "Apuntador")
$ErrorActionPreference = "Stop"
Remove-Item docs,publish -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish .\Apuntador.csproj -c Release -o publish
Copy-Item .\publish\wwwroot .\docs -Recurse
$index = Get-Content .\docs\index.html -Raw
$index = $index.Replace('<base href="/" />', '<base href="/' + $RepoName + '/" />')
Set-Content .\docs\index.html $index -Encoding utf8
Copy-Item .\docs\index.html .\docs\404.html
New-Item .\docs\.nojekyll -ItemType File -Force | Out-Null
Write-Host "Sitio generado en docs/. Configura GitHub Pages: Deploy from a branch > main > /docs"
