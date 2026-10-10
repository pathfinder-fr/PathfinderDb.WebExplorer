# PathfinderDb.WebExplorer

Application de consultation des données Pathfinder première édition en français.
Le site courant est l’application ASP.NET Core Razor Pages sous
`.NET 10`; l’ancien site ASP.NET MVC 4 est conservé dans `archive\legacy`.

## Projets

- `src\PathfinderDb.Web` : site web ASP.NET Core Razor Pages.
- `src\PathfinderDb.Data` : import JSON, modèles, validation et catalogues.
- `tests\PathfinderDb.Modern.Tests` : tests unitaires du site courant.
- `tests\PathfinderDb.Modern.IntegrationTests` : tests opt-in sur un clone réel
  de `pf1-data`.
- `archive\legacy` : application MVC 4 historique et ses fichiers XML.

La solution courante est `PathfinderDb.sln`. Les commandes principales se
lancent depuis la racine du dépôt, ou utilisent le chemin de leur propre
emplacement pour fonctionner depuis un autre répertoire :

```powershell
.\build.cmd
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj
```

Le build requiert le SDK .NET 10. Pour lancer le site localement :

```powershell
dotnet run --project src\PathfinderDb.Web\PathfinderDb.Web.csproj
```

## Données

Le site lit `feats.json`, `spells.json`, `monsters.json` et, s’il existe,
`labels.json` depuis un clone externe de `pf1-data`. Ce clone n’est pas inclus
dans le dépôt ni dans la sortie de publication. Configure le chemin avec
`PathfinderData:RootPath` ou `PathfinderData__RootPath`. En production, le
chemin courant est `E:\websites\Pathfinder-FR\pf1-data`; en développement
local, `appsettings.Development.json` fournit le chemin de développement.

La publication IIS est framework-dependent. Le serveur doit avoir le Hosting
Bundle ASP.NET Core .NET 10 et l’AppPool IIS doit être configuré sur **No
Managed Code**. IIS conserve son chemin actuel :
`E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web`.
Le compte du site doit fournir `PathfinderData__RootPath` vers le clone de
données externe.

## Publication et mise à jour planifiée

`publish.bat` publie directement la version présente dans le clone, sans
opération Git. Le script PowerShell appelé gère `app_offline.htm`, attend la
libération des fichiers IIS, restaure un éventuel fichier offline préexistant,
et vérifie `web.config` et le code de sortie de `dotnet publish`.

Pour créer une tâche dans le Planificateur de tâches du serveur, configurer
l’action pour lancer :

```text
E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\job.cmd
```

Le job utilise son propre chemin plutôt que le répertoire de travail du
Planificateur. Il n’exécute que sur `master`, réinitialise les modifications
locales des fichiers suivis (`git reset --hard`), effectue
`git pull --ff-only origin master`, puis publie dans le dossier IIS ci-dessus.
Les fichiers non suivis ne sont pas supprimés; s’ils empêchent le pull, la
tâche s’arrête sans publier. Les échecs Git ou de publication sont retournés
au Planificateur. Le compte qui exécute la tâche doit disposer de Git, du SDK
.NET 10 et des permissions nécessaires sur le checkout, le clone de données
et le répertoire publié.

Pour plus de détails sur le chargement des données et les paramètres de
déploiement, voir [`docs\modern-data-pipeline.md`](docs/modern-data-pipeline.md).
