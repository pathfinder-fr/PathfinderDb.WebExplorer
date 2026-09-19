# PathfinderDb.WebExplorer

Projet legacy d'exploration de données Pathfinder 1e, historiquement publié sur https://db.pathfinder-fr.org/.

Ce dépôt contient l'application web qui servait à parcourir les fiches de dons et de sorts de Pathfinder, sur la base de jeux de données XML produits à partir du wiki Pathfinder-fr.org.

## Résumé de l'analyse

Le site est une application ASP.NET MVC 4 vieille génération, ciblant le .NET Framework 4.6. L'architecture actuelle est très monolithique et repose sur des fichiers XML embarqués, sans base de données relationnelle ni pipeline d'import moderne.

La logique métier est centrée sur le package `PathfinderDb.Schema`, qui fournit les types de données métier (`Feat`, `Spell`, `Source`, etc.) et la structure de données à partir des fichiers XML.

## Stack technique actuelle

- ASP.NET MVC 4
- .NET Framework 4.6
- Razor Views
- Bootstrap 2.x / jQuery
- packages NuGet legacy (`Microsoft.AspNet.*`, `Newtonsoft.Json`, `PathfinderDb.Schema`)
- build Windows/MSBuild via `build.cmd` et `build/build.proj`

## Structure du dépôt

### `src/PathfinderDb.WebExplorer`

- `Controllers/`
  - `HomeController.cs` : page d'accueil
  - `FeatController.cs` : recherche et filtrage de dons
  - `SpellController.cs` : recherche et filtrage de sorts
  - `ControllerExtensions.cs` : accès au jeu de données courant
- `Models/`
  - `MemoryDataSet.cs` : charge les données XML en mémoire
  - gestion de filtres et vues de recherche (`FeatIndexQuery`, `SpellIndexQuery`, etc.)
- `Views/`
  - pages Razor pour les vues de dons, sorts et layout global
- `App_Data/`
  - fichiers XML de données : `apg.xml`, `pfrpg.xml`, `uc.xml`, `um.xml`, `bestiary*.xml`
- `App_Start/`
  - routing et configuration MVC
- `Web.config` / `Global.asax`
  - configuration ASP.NET legacy

## Source de données et fonctionnement

Le cœur de l'application est `MemoryDataSet.LoadDataSet(...)`.

Cette méthode :

1. parcourt les fichiers XML dans `App_Data/`
2. ne garde que ceux correspondant à des jeux de données connus
3. les charge via `DataSet.Load(reader)`
4. fusionne et met en cache le résultat dans un `PathfinderDb.Schema.DataSet`

Les données contenues dans les XML décrivent des éléments comme :

- sorts
- dons
- sources / références
- niveaux, composants, prérequis, descriptions courtes
- traductions / libellés

Exemple de structure observée dans les XML :

```xml
<dataSet xmlns="urn:pathfinderDb">
  <sources>
    <source id="uc" />
  </sources>
  <spells>
    <spell id="abondance-de-munitions" school="conjuration">
      <name>Abondance de munitions</name>
      <levels>
        <level list="bard" level="1" />
      </levels>
      <summary>...</summary>
    </spell>
  </spells>
</dataSet>
```

Autrement dit, le site n'utilise pas une base de données persistante ; il charge tout en mémoire au démarrage.

## Ce que le site fait

Le site permet de naviguer et filtrer :

- dons (`FeatController`)
- sorts (`SpellController`)

Les filtres portent sur :

- source / livre
- type de don
- attributs requis
- niveau de classe
- liste de sort
- vue liste ou vue tabulaire

L'application expose donc un navigateur de données Pathfinder, à la fois lisible et orienté recherche, mais fortement couplé à la structure XML d'origine.

## Points de fragilité / limites de la solution legacy

- dépendance au framework .NET Framework 4.6 très ancien
- app monolithique sans séparation `domain / data / api / ui`
- données statiques dans le dépôt, donc difficilement évolutives/contributives
- pas de pipeline de validation ni de tests automatisés visibles
- dépendance implicite à la structure XML de `PathfinderDb.Schema`
- front-end legacy (Bootstrap + jQuery 2010, rendu server-side Razor)
- présence de références historiques à `pathfinder-fr.org` et sources externes dans les données

## Ce qu'il faudra faire pour moderniser / réécrire

Le chantier ressemble à une refonte de la source de données et de l'architecture applicative, pas seulement un simple refactor UI.

### Objectif fonctionnel

Créer une nouvelle application qui expose la même base de connaissance Pathfinder, mais avec une source de données plus fiable, plus structurielle et plus maintenable.

### Architecture cible probable

- source canonique de données distincte du front
- schéma de données explicite (DTO / domain model / database schema)
- import des données depuis une source fiable (JSON, export, API, ou données structurées)
- API backend moderne (`ASP.NET Core`, ou équivalent)
- frontend moderne (React/Vue/Blazor/SSR selon besoin)
- tests d'intégration sur les imports et les filtres

### Étapes de migration probables

1. cartographier le schéma actuel du dataset XML
2. définir le nouveau modèle canonique de données
3. construire un import depuis la nouvelle source
4. valider les règles métier (sorts, dons, prérequis, sources)
5. reconstruire les filtres et les vues à partir de ce modèle
6. moderniser le frontend et l'UX
7. déployer une version plus robuste et maintenable

## Conclusion

Le projet est un bon exemple de site de navigateur de données historique : fonctionnel, riche en contenu, mais construit autour d'un modèle de données XML legacy et d'un framework web ancien.

La vraie modernisation ne passera pas seulement par un redesign visuel : il faut d'abord remplacer la source de données et séparer clairement le modèle, l'import, l'API et le front.

## Prochaines étapes de travail

- confirmer la nouvelle source de données cible
- définir le schéma canonique (sorts, dons, sources, références)
- identifier les différences entre les XML actuels et le format de destination
- définir l'architecture finale de l'application moderne
- construire le premier import de données en environnement de test

## Nouvelle application .NET 10

La modernisation est développée en parallèle dans `src/PathfinderDb.Modern` afin
de préserver l'application MVC 4 existante pendant la transition.

La nouvelle application consomme exclusivement les trois exports JSON racine du
clone externe `pf1-data` :

- `feats.json`
- `spells.json`
- `monsters.json`

Le chemin du clone est configuré par `PathfinderData:RootPath`, ou par la
variable d'environnement `PathfinderData__RootPath`. En développement local,
la valeur attendue est `D:\code\perso\pf\pf1-data`. Les fichiers JSON de
production ne doivent pas être copiés dans ce dépôt.

Commandes de validation de la fondation moderne :

```powershell
dotnet build src\PathfinderDb.Modern\PathfinderDb.Modern.sln --configuration Release
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj
```
