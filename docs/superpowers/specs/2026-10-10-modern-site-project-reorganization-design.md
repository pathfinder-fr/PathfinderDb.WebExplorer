# Modern site project reorganization

## Contexte et domaine

Le dépôt `PathfinderDb.WebExplorer` contient encore deux applications : le site
historique ASP.NET MVC 4 sous `src/PathfinderDb.WebExplorer`, et son
remplacement .NET 10 Razor Pages sous `src/PathfinderDb.Modern`. La solution et
`build.cmd` à la racine construisent encore le site historique, tandis que les
instructions de build et le script PowerShell de publication moderne sont
rangés ailleurs. Cette organisation ne rend pas évident quel site est le site
courant.

En production, le dépôt est cloné dans
`E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer`. IIS sert actuellement
`E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web`.
Un job du Planificateur de tâches doit pouvoir être configuré une seule fois
pour mettre à jour ce clone depuis Git puis publier le site courant.

## Intention

Rendre l’application .NET 10 clairement courante dans la structure et les
commandes du dépôt, tout en conservant les sources legacy consultables, et
permettre une mise à jour/publication périodique reproductible sur le serveur
IIS.

## Objectifs

- Promouvoir les projets .NET 10 à des chemins courts et canoniques sous `src`.
- Archiver sans supprimer les sources, solution et mécanismes de build legacy.
- Faire de la solution et du build à la racine ceux de l’application courante.
- Fournir des points d’entrée Windows simples à la racine pour construire,
  publier et planifier la publication.
- Faire échouer le job clairement si le reset, le pull ou la publication échoue.
- Conserver le chemin de publication IIS existant et ne jamais placer les
  exports `pf1-data` dans le dépôt ou la sortie publiée.
- Garder le processus actuel `app_offline.htm` et l’attente de déverrouillage
  IIS de la publication, ainsi que la configuration framework-dependent
  .NET 10 existante.
- Mettre à jour la documentation courante et les tests/scripts pour les
  nouveaux chemins.

## Hors périmètre

- Supprimer l’application legacy, son historique Git ou ses données XML.
- Modifier les fonctionnalités des sites, le contrat de données ou les routes.
- Changer la fréquence d’actualisation du clone externe `pf1-data`.
- Reconfigurer IIS, ses permissions, son AppPool ou installer le Hosting Bundle.
- Publier les fichiers du site dans le dépôt.

## Décisions exprimées

- Le site .NET moderne devient le projet courant; le site legacy peut être
  archivé ou supprimé. Le choix retenu ici est l’archivage non destructif.
- Le clone du serveur est
  `E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer`.
- Le dossier physique IIS reste
  `E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web`.
- Un job planifié doit faire la mise à jour Git puis lancer la publication.
- Le dépôt utilise actuellement `origin/master`; le job fera un
  fast-forward-only pull de cette branche.
- Avant le pull, le job réinitialisera les modifications locales des fichiers
  suivis avec `git reset --hard`; les fichiers non suivis seront conservés et
  `git clean` ne sera pas lancé.
- Le job refusera de démarrer si le checkout actif n’est pas sur `master`; il
  ne changera pas de branche implicitement.

## Organisation cible proposée

- Déplacer `src/PathfinderDb.Modern/PathfinderDb.Data` vers
  `src/PathfinderDb.Data`.
- Déplacer `src/PathfinderDb.Modern/PathfinderDb.Web` vers
  `src/PathfinderDb.Web`.
- Placer la solution moderne à la racine sous `PathfinderDb.sln`, avec les
  références de projets mises à jour.
- Déplacer `src/PathfinderDb.WebExplorer` vers
  `archive/legacy/src/PathfinderDb.WebExplorer`, l’ancienne solution vers
  `archive/legacy/PathfinderDb.WebExplorer.sln`, et `build` vers
  `archive/legacy/build`, afin de conserver les chemins relatifs de compilation
  legacy. L’ancien `build.cmd` racine devient `archive/legacy/build.cmd`.
- Créer un nouveau `build.cmd` à la racine pour le build Release de
  `PathfinderDb.sln`.
- Garder le script PowerShell de publication testable sous `scripts`, le
  mettre à jour pour le nouveau chemin du projet web et le chemin IIS.
- Ajouter `publish.bat` à la racine comme lanceur de publication directe.
- Ajouter `job.cmd` à la racine pour faire `git pull --ff-only origin master`
  dans le checkout identifié par `%~dp0`, puis lancer `publish.bat`.
- Garder le dossier `publish` ignoré par Git et distinct du dossier des sources
  de données.
- Mettre à jour `README.md`, `.github/copilot-instructions.md`, les documents
  opérationnels et les tests de scripts. Les anciennes specs/plans datés
  restent des archives historiques et ne sont pas réécrits.

## Contraintes et détails de réalisation

- Les scripts doivent fonctionner lorsqu’ils sont lancés par le Planificateur
  de tâches avec un répertoire de travail arbitraire; utiliser leur emplacement
  (`%~dp0`) pour retrouver le clone, pas le répertoire courant.
- `job.cmd` doit être sans interaction, afficher les étapes, et retourner un
  code d’erreur non nul sur échec du contrôle de branche, du reset, du pull ou
  de la publication. Le
  reset supprime les modifications et changements staged des fichiers suivis,
  conformément à la demande explicite; les fichiers non suivis restent présents
  et ne sont jamais nettoyés automatiquement. Si l’un d’eux bloque le pull, le
  job s’arrête sans publier.
- `publish.bat` doit permettre de publier directement sans faire de pull. Il
  appelle le script PowerShell existant et transmet ses paramètres; le job
  l’appelle avec le chemin IIS connu.
- Le script PowerShell continue à publier sans supprimer arbitrairement le
  dossier de sortie, préserver un `app_offline.htm` préexistant, vérifier le
  code de sortie de `dotnet publish`, et vérifier la présence de `web.config`.
- Le compte de service du Planificateur doit avoir les droits Git et IIS requis
  et disposer de `git`, PowerShell et du SDK .NET 10/Hosting Bundle adéquat.
  La configuration de ce compte est une responsabilité d’exploitation, pas du
  script.
- Les chemins relatifs entre projets, suites de tests, solution et
  documentation doivent être mis à jour ensemble. Les builds historiques ne
  sont pas un prérequis à la validation du site courant après archivage.

## Livrables testables

1. **Structure et solution courantes** — les projets .NET 10 sont sous leurs
   chemins canoniques, la solution racine construit l’application et les tests,
   et les sources/solution legacy sont accessibles dans l’archive.
2. **Commandes locales** — `build.cmd` construit le site courant; les tests
   unitaires et d’intégration gardent leurs références de projets valides; les
   tests du script de publication couvrent le nouveau chemin et les erreurs.
3. **Publication IIS directe** — `publish.bat` publie vers
   `publish\PathfinderDb.Web`, y compris depuis un répertoire courant différent,
   transmet les erreurs et laisse `web.config` au dossier attendu.
4. **Job planifiable** — `job.cmd`, lancé depuis le chemin du serveur, réinitialise
   les modifications locales suivies, exécute le pull fast-forward-only puis la
   publication; les erreurs Git empêchent la publication et les erreurs de
   publication sont retournées au Planificateur.
5. **Documentation courante** — le README et les instructions de contribution
   désignent le nouveau projet, les commandes racine et la configuration
   nécessaire au job/IIS.

## Vérification

- Vérifier les références des solutions et projets après déplacement.
- Depuis la racine, exécuter `build.cmd`, la suite complète des tests modernes
  et le build Release de la solution.
- Exécuter les tests PowerShell de publication, incluant les chemins avec
  espaces et les échecs simulés de `git pull` et `dotnet publish`.
- Tester les `.cmd` avec des doubles contrôlés de Git/dotnet pour valider
  l’ordre des étapes, la propagation des codes de sortie et l’indépendance du
  répertoire de travail; ne pas faire de pull ni publier réellement sur IIS
  pendant les tests.
- Vérifier qu’aucune commande ou documentation courante ne demande de
  construire le MVC archivé.
