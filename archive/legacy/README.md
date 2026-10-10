# Application legacy

Ce dossier archive l’ancien site ASP.NET MVC 4, ciblant .NET Framework 4.6.
Il est conservé pour référence et n’est plus l’application courante du dépôt.

- `src\PathfinderDb.WebExplorer` contient les contrôleurs, modèles, vues Razor,
  configuration MVC et exports XML historiques sous `App_Data`.
- `PathfinderDb.WebExplorer.sln` est l’ancienne solution Visual Studio.
- `build.cmd` et `build\build.proj` sont l’ancien wrapper MSBuild.

Le site courant est l’application ASP.NET Core Razor Pages .NET 10 décrite dans
le [README à la racine](../../README.md). Utiliser les commandes de build,
tests et publication à la racine du dépôt pour le site actif. Les scripts
archivés ne font pas partie du job IIS courant.
