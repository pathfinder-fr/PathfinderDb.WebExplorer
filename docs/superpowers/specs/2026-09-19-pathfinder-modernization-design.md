# Spécification de modernisation du site Pathfinder FR DB

> **Révision 2** — relecture technique après exploration approfondie du site actuel et du dépôt `pf1-data`.
> Les sections modifiées ou ajoutées par rapport à la V1 sont signalées par `[MAJ]`.
> Cette révision ajoute le détail technique nécessaire pour qu'un agent d'implémentation puisse démarrer sans avoir à re-explorer le dépôt de données.

## Demande initiale (texte source, verbatim)

Pour traçabilité, voici la demande exacte formulée par le porteur de produit, qui sert de référence pour toute divergence d'interprétation :

- migrer en .NET 10 ;
- se baser sur un clone du repo https://github.com/pathfinder-fr/pf1-data qui contient les données qui seront actualisées fréquemment ; ce sera aussi ce clone qui sera utilisé en prod ; en local il est présent dans `D:\code\perso\pf\pf1-data` ;
- optimiser le démarrage de l'app (rapide) ;
- moderniser l'affichage en terme de lib : le site doit rester sobre, simple et très rapide à charger ;
- mieux gérer la navigation : la page d'accueil des feats et des spells les liste TOUS, c'est beaucoup trop couteux et provoque beaucoup trop de volume de transfert réseau ;
- rendre le site plus résistant au crawling de bots et d'IA : les éléments changent rarement et pourraient être facilement mis en cache CDN ;
- rendre la navigation le plus prévisible possible, moins personnalisable pour éviter l'explosion combinatoire des cas possibles affichés et les crawlers qui explorent toutes les combinaisons, quitte à supprimer des features dans un premier temps ;
- gérer les monstres ;
- améliorer sorts et dons pour gérer les nouveaux cas.

Chacun de ces points est repris et détaillé plus bas ; le tableau de correspondance est en fin de document (section « Traçabilité demande → livrables »).

## Contexte et domaine

Le projet actuel est un site de référence historique pour la base de données Pathfinder 1e, répondant à l’URL https://db.pathfinder-fr.org/.
Il a été construit comme un petit moteur de navigation sur des données extraites de la communauté Pathfinder-fr.org, avec des listes de dons et de sorts publiées sous forme HTML générée côté serveur.

Le site est fonctionnel, mais il a été conçu pour un contexte de données statiques et un front web legacy.
L’analyse du site et du dépôt montre des points de friction majeurs :

- le site charge et affiche la totalité des dons et des sorts dans les pages d’index, ce qui produit un volume réseau excessif et un rendu lourd ;
- la navigation est très ouverte et combinatoire ;
- les données sont traitées via des fichiers XML embarqués et une logique monolithique ASP.NET MVC 4 ;
- les pages ne sont pas construites pour la mise en cache CDN ni pour la résistance aux crawlers IA/bots ;
- le domaine est incomplet avec le besoin d’ajouter explicitement les monstres et de mieux couvrir les cas nouveaux sur dons et sorts.

Le nouveau projet doit moderniser l’application, la migrer vers .NET 10, et s’appuyer sur le dépôt source de données `D:\code\perso\pf\pf1-data` (clone Git du repo GitHub `pathfinder-fr/pf1-data`).
Ce clone doit être la source de vérité du contenu produit et mis à jour régulièrement, et il sera également la source utilisée en production.

Le domaine métier couvre :

- dons (`feats`),
- sorts (`spells`),
- monstres (`monsters`),
- sources et références de contenu,
- listes de classes / listes de sorts / types / catégories,
- navigation déterministe et lisible pour un lecteur humain, sans explosion de cas combinatoires.

## Objectif du projet

Moderniser le site pour qu’il soit :

- rapide au démarrage,
- léger à charger,
- facile à maintenir,
- robuste face aux bots et aux IA,
- basé sur une source de données versionnée et mise à jour fréquemment,
- compatible avec les besoins de contenu actuels et futurs `feats`, `spells` et `monsters`.

## Principes de conception

1. La source de vérité est le dépôt `pf1-data`, pas le front.
2. Le produit doit rester sobre, simple et très rapide à charger.
3. La navigation doit être prédictive, stable et raisonnablement limitée.
4. Les pages les plus souvent consultées doivent être mises en cache de manière agressive.
5. Le système doit éviter de générer des pages ou des URLs qui explosent en combinaisons.
6. Le site doit rester lisible pour un humain, sans se transformer en moteur de recherche combinatoire.
7. Le modèle de données doit gérer les cas avancés et les nouvelles règles sans bricolage technique.

## État des lieux du site actuel

### Ce que l’on observe aujourd’hui

- page d’accueil minimale : `https://db.pathfinder-fr.org/`
- page `/feat` liste de nombreuses entrées, avec environ 1133 dons affichés dans un HTML très dense ;
- page `/spell` liste environ 1324 sorts dans un HTML très large ;
- aucune pagination ni segmentation visible sur les index ;
- les pages sont servies selon une logique HTML générée côté serveur, sans cache explicite ni stratégie CDN ;
- le site a déjà des règles `robots.txt` ciblées contre certains crawlers IA (`GPTBot`, `Google-Extended`, `ClaudeBot`, etc.) ;
- les données provenaient de fichiers XML embarqués dans le dépôt historique (`App_Data/*.xml`), ce qui rend la mise à jour plus fragile.

### Ce que cela implique pour la refonte

Le site actuel démontre un besoin clair :

- ne plus exposer les index complets dans un seul payload massif,
- découper la navigation par lettres / sections / filtres sûrs,
- faire du cache CDN un premier niveau de performance,
- éviter les pages de résultats combinatoires trop larges ou trop nombreuses,
- mettre la source de données au cœur du système pour supprimer la dépendance locale à des XML statiques.

## `[MAJ]` Analyse détaillée du dépôt `pf1-data`

Cette section documente précisément ce qui a été trouvé dans `D:\code\perso\pf\pf1-data` (remote `https://github.com/pathfinder-fr/pf1-data.git`), pour éviter à l'agent technique de devoir ré-explorer le dépôt.

### Nature du dépôt

- `pf1-data` n'est pas un dépôt édité à la main : c'est un **corpus généré** par extraction du wiki Pathfinder-fr.org
  (fichiers XML MediaWiki bruts référencés dans `diagnostics.json`, ex. `D:\code\perso\pf\pf1-xml\Pathfinder-RPG\*.xml`),
  transformé par un pipeline d'extraction externe au présent projet.
- Le dépôt local n'a qu'un seul commit observé (« Initialiser le corpus XML Pathfinder »), ce qui suggère une régénération complète périodique (pas de commits incrémentaux fins).
  **L'implémentation ne doit donc pas supposer un historique Git riche ni un diff incrémental fiable** : il faut traiter chaque nouvelle version comme un remplacement complet des fichiers de données, et recalculer les index/caches en conséquence.
- Le dépôt fournit à la fois des exports `*.xml` (format `PathfinderDb.Schema`, normalisé via un schéma XSD, compatible avec l'ancien site) et des exports `*.json` plus récents et plus riches (`feats.json`, `spells.json`, `monsters.json`).
  `[MAJ]` **Décision actée** : la nouvelle application consomme **exclusivement le format JSON**, désérialisé via `System.Text.Json` (avec source generation) pour un démarrage rapide sans coût de réflexion — c'est le critère de performance qui a tranché en faveur du JSON plutôt que du XML. Conséquences :
  - **pas d'abstraction multi-format en v1** : l'`Import / normalization` cible uniquement `feats.json` / `spells.json` / `monsters.json` (+ `diagnostics.json` pour la validation qualité) ; l'ancien format `.xml` du dépôt n'est pas consommé par la nouvelle application ;
  - l'absence de schéma JSON formel aujourd'hui est compensée par une **validation applicative explicite** à l'import (champs requis vérifiés par le mapping C# lui-même, échec clair et loggué si un champ obligatoire manque ou change de type), plutôt que par une validation de schéma externe (XSD) ;
  - si `pf1-data` produit un jour un schéma JSON (JSON Schema) pour ses exports, ce schéma pourra être branché en complément de la validation applicative, sans remettre en cause le choix du format ;
  - le statut futur du XML côté `pf1-data` (conservé ou supprimé) n'a plus d'impact sur cette application : elle ne le lit pas, qu'il continue d'exister ou non.
- `diagnostics.json` / `diagnostics.md` / `diagnostics.csv` contiennent un **rapport qualité de l'extraction** (`CandidateCount`, `AnalyzedCount`, `ProducedCount`, `DiagnosticCount`,
  avec des entrées `Severity: info/warning/error` et des règles nommées comme `spell.candidate-not-spell`).
  Ce rapport doit être exploité par le pipeline d'import : au minimum, bloquer la publication si `ErrorCount > 0` sur une catégorie critique, et logguer/exposer les `WarningCount` pour suivi qualité dans le temps.

### Tailles réelles (ordre de grandeur pour le dimensionnement du cache et du démarrage)

| Fichier | Taille | Contenu |
|---|---|---|
| `feats.json` | ~2,8 Mo | ~1133 dons, avec prérequis structurés |
| `spells.json` | ~3,85 Mo | ~1324+ sorts (2074 produits selon diagnostics), avec écoles, niveaux par liste, composants |
| `monsters.json` | ~304 Ko | index de monstres, **métadonnées seulement** (voir limitation ci-dessous) |
| `diagnostics.json` | ~70 Ko | rapport qualité d'extraction |

Ces volumes (au total < 10 Mo) tiennent largement en mémoire ; le chargement initial doit être un simple `JsonSerializer.Deserialize` (idéalement avec **`System.Text.Json` source generation** pour éviter le coût de réflexion au démarrage), pas une base de données externe.

### Schéma observé — `Feat` (`feats.json`)

Champs observés : `Id` (slug), `Name`, `Types` (tableau, ex. `Combat`), `Prerequisites` (tableau hétérogène, voir ci-dessous), `Description`, `Benefit`, `Normal`, `Source` (objet avec `Id` + `References[]`), potentiellement `Localization`.

Les prérequis (`Prerequisites[].Type` / `OtherType`) prennent au moins les valeurs suivantes constatées : `BBA`, `Attribute`, `SkillRank`, `ClassLevel`, `Feat`, `SpellCast`, et un type libre `OtherType` (ex. `ExoticWeaponProficiency`) avec un champ `Value` et une `Description` textuelle de repli.
Il existe aussi des groupes de choix (prérequis alternatifs, cf. `FeatPrerequisiteChoice` dans l'ancien code `FeatController.cs`) — **ce mécanisme de choix doit être conservé dans le nouveau modèle**, car il porte une sémantique métier (« l'un OU l'autre des prérequis suffit »).

### Schéma observé — `Spell` (`spells.json`)

Champs observés : `Id`, `Name`, `School`, `Levels[]` (`List` + `Level`, une entrée par liste de classe, ex. `bard`, `sorcerer-wizard`, `cleric`, `psychiste`), `Components.Kinds` (texte libre du type `"Verbal, Somatic, FocusOrDivineFocus"`), `Range`, `Target`, `CastingTime`, `Source` (`Id` + `References[]`), `Localization.Languages[]` (traductions, ex. nom anglais).

Point notable : de nouvelles listes de classes apparaissent déjà dans les données (`psychiste`) qui n'existaient pas forcément dans l'ancien modèle `PathfinderDb.Schema` — **le modèle de listes de classes ne doit pas être une énumération figée côté code**, mais dérivé dynamiquement des données (`SpellLists` présent dans le fichier, actuellement vide mais prévu comme référentiel).

### Schéma observé — `Monster` (`monsters.json`) — **limitation importante**

Contrairement à `Feat` et `Spell`, les entrées `Monster` observées ne contiennent **que des métadonnées d'index** :

```json
{
  "Id": "tortue-de-mer",
  "Name": "Tortue de mer",
  "CR": 1.0,
  "Climate": "Temperate",
  "Environment": "Aquatic",
  "Type": "Animal",
  "Source": { "Id": "um" }
}
```

Il n'y a **pas de bloc de statistiques complet** (PV, CA, attaques, capacités spéciales, texte descriptif) dans l'export actuel.
Cela a un impact direct sur la portée réalisable de la fonctionnalité « Gérer les monstres » :

- **Ce qui est faisable dès maintenant avec les données existantes** : liste/index des monstres, filtres par CR / type / environnement / climat / source, lien vers la source (livre) — c'est-à-dire un catalogue de références, pas des fiches de statblock complètes.
- **Ce qui nécessite une évolution du dépôt `pf1-data` en amont** : toute fiche de détail avec statistiques de jeu complètes.
  Ce point doit être vérifié avec le mainteneur de `pf1-data` avant de promettre des fiches de monstre détaillées ; voir la question ouverte correspondante en fin de document.

### Sources (`Sources[]`)

Liste commune aux trois fichiers : `uc`, `pfrpg`, `um`, `apg`, `paizoBlog`, `bestiary`, `bestiary2`, `bestiary3`, `bestiary4`, `bestiary5`, `codexmonstrueux`, `bookofthedamned`.
Cette liste est plus riche que l'ancien site (qui ne chargeait que `apg`, `pfrpg`, `uc`, `um` dans `MemoryDataSet.DataSetNames` et laissait de côté les bestiaires) : **la nouvelle app doit traiter la liste des sources comme dynamique**, dérivée du fichier `Sources[]` à chaque chargement, jamais codée en dur.

## Cible technique

### Stack

- ASP.NET Core 10 / .NET 10, **Razor Pages** recommandé plutôt que MVC classique (moins de cérémonie pour des pages essentiellement en lecture, correspond mieux à un découpage par route/section) ; MVC reste acceptable si l'équipe a une préférence forte, mais éviter de mélanger les deux styles.
- HTML/CSS minimal, sans framework JS de rendu (pas de React/Vue/Blazor WASM). Un peu de JS vanilla est acceptable pour des interactions ponctuelles (ex. repli d'une section), mais ce n'est pas un SPA.
- `System.Text.Json` avec **source generation** (`JsonSerializerContext`) pour désérialiser `feats.json` / `spells.json` / `monsters.json` sans réflexion au démarrage.
- **Output Caching** (`Microsoft.AspNetCore.OutputCaching`, intégré à ASP.NET Core depuis .NET 7+) pour le cache serveur des pages de liste et de détail, avec des tags d'invalidation par entité (`feat`, `spell`, `monster`) purgés uniquement au rechargement d'une nouvelle version de `pf1-data`.
- **Response Compression** (Brotli + Gzip) activée par défaut.
- Envisager la **compilation AOT / ReadyToRun** (`PublishAot` ou `PublishReadyToRun`) pour réduire le temps de démarrage JIT, à valider en étape 2 selon compatibilité avec Razor (AOT complet et Razor runtime compilation ne font pas toujours bon ménage ; ReadyToRun est le choix le plus sûr si Razor Pages est compilé au build).
- Pas de base de données relationnelle en v1 : les données tiennent en mémoire (~7 Mo de JSON), donc un simple modèle « chargé au démarrage, réindexé en dictionnaires par clé/slug » est suffisant et plus rapide qu'une base externe.

### Source de vérité et cycle de mise à jour `[MAJ]`

Le dépôt `pf1-data` est le référentiel de production des données, sous forme de fichiers `.json` (pas les `.xml`, qui sont un format hérité conservé pour compatibilité avec l'ancien site mais pas utilisé ici).

Points à trancher techniquement (cf. questions ouvertes) :

- **Mode de synchronisation en production** : clone Git mis à jour par un job planifié (ex. `git pull` + redémarrage/rechargement à chaud), vs. déploiement qui embarque une version figée de `pf1-data` à chaque build.
  Le besoin exprimé (« actualisées fréquemment ») pousse vers un rafraîchissement **sans redéploiement complet de l'app** : un service de fond qui vérifie périodiquement (ex. toutes les X minutes/heures) si le HEAD du dépôt a changé,
  recharge les fichiers JSON en mémoire, et purge les caches de sortie taggés en conséquence.
- Le chargement doit rester **atomique** du point de vue des lecteurs : construire le nouveau jeu de données en mémoire, puis substituer la référence (pattern « double buffering » / `Interlocked.Exchange` sur une référence immuable),
  jamais de mutation en place pendant qu'une requête est en cours.
- Le format `.json` n'ayant pas de garantie de compatibilité de schéma dans le temps (pas de fichier de schéma JSON versionné identifié dans le dépôt), l'import doit être tolérant aux champs additionnels (désérialisation permissive)
  et strict sur les champs requis, avec échec explicite et log clair si un champ obligatoire disparaît.

### Architecture proposée

- `Data source` : clone Git de `pf1-data`, rafraîchi périodiquement en production (voir ci-dessus), monté en local via `D:\code\perso\pf\pf1-data` pour le développement.
- `Import / normalization` : bibliothèque dédiée (projet `PathfinderDb.Data` ou équivalent) qui désérialise les fichiers JSON (`System.Text.Json` + source generation), valide (champs requis vérifiés applicativement + `diagnostics.json` pour la qualité du corpus), et construit des modèles internes typés + index par slug/lettre/section/CR/etc.
- `Content model` : entités `Feat`, `Spell`, `Monster`, `Source`, `Reference`, `Prerequisite` (avec support des groupes de choix), `SpellLevel`, indépendantes de la forme JSON source.
- `Runtime app` : site web ASP.NET Core 10 (Razor Pages) qui expose des routes déterministes en lecture seule sur ces modèles.
- `Cache layer` : Output Cache serveur (tags par entité) + en-têtes HTTP orientés CDN pour les pages stables ; pas de cache applicatif ad hoc supplémentaire nécessaire vu le faible volume de données.
- `Refresh pipeline` : tâche d'arrière-plan (`IHostedService`) qui surveille `pf1-data`, recharge et republie sans redémarrage du processus.

## Exigences fonctionnelles

### 1. Navigation et performance

- supprimer la page d’index globale “tout afficher” pour dons et sorts ;
- exposer des index segmentés par lettre / section (ex. `A–C`, `D–F`, etc.) ou par page, avec limite raisonnable de résultats par page ;
- chaque page de liste doit être de volume maîtrisé ;
- les pages de détail doivent être directement accessibles et facilement cacheables ;
- l’application doit être capable de servir des pages de liste de 20 à 100 éléments sans charge inutile.

Critère de validation :

- un index de dons ou de sorts ne contient jamais des milliers d’éléments dans un même HTML,
- la page d’accueil ne sert plus de liste intégrale, mais un portail orienté navigation.

### 2. Cache et crawl resistance

- les pages de contenu stable doivent être sérialisables avec cache HTTP long (CDN-friendly),
- utiliser `ETag`, `Last-Modified`, `Cache-Control`, et éventuellement des réponses statiques ou pré-générées ;
- les objets qui changent rarement doivent être traités comme du contenu “cacheable”; les pages très dynamiques restent à part ;
- les pages de navigation doivent être lisibles et prédictibles ; pas de “query explosion” ni de “combinoisons illimitées”.

Critère de validation :

- les headers HTTP indiquent des politiques de cache compatibles CDN,
- les pages de données publiques de référence restent stables sur des périodes longues.

### 3. Simplicité et sobriété du front

- interface sobre, lisible, sans dépendances lourdes ;
- pas de framework JS volumineux ni de rendu client complexe ;
- design minimaliste, rapide, accessible et facile à maintenir ;
- conserver une expérience élégante sans surcharge de ressources.

Critère de validation :

- le chargement initial de la page d’accueil est faible,
- la bundle size de front est compatible avec un rendu très léger,
- l’application reste fluide même sous réseau modeste.

### 4. Démarrage rapide

- le site doit démarrer sans chargement coûteux des jeux de données complets au premier hit ;
- les données doivent être prêtes et indexées au démarrage, avec chargement quasi-immediate en mémoire ou via artefacts statiques ;
- le système doit éviter les IO lourds sur la requête initiale.

Critère de validation :

- temps de démarrage mesuré et régressions visibles en CI,
- le chargement en production ne dépend plus d’un parse XML lent à chaque lancement.

### 5. Gestion des monstres

- introduire un domaine explicite pour `Monster` dans le modèle métier et l’API ;
- exposer une navigation dédiée aux monstres, séparée de dons et sorts ;
- ajouter les filtres utiles : CR, type, environnement, climat, source.

Critère de validation :

- la base de données de monstres est exploitable dans le site,
- les lists et detail pages sont cohérentes avec les filtres métier définis.

### 6. Amélioration des dons et des sorts

- mieux gérer les cas “nouveaux” : prérequis complexes, types multiples, variantes, sources multiples, listes de sorts ;
- stabiliser le rendu des conditions et avantages,
- expliciter les données structurées de manière plus fiable que le simple HTML d’origine.

Critère de validation :

- les cas complexes de dons et sorts ne sont plus mal rendus ou incomplets,
- les modèles de données incluent les champs manquants déjà observés dans les exports JSON.

## Exigences non fonctionnelles

### Performance

- startup rapide,
- faible temps de réponse,
- taille des pages limitée,
- cache agressif pour contenus non interactifs.

### Sécurité / stabilité

- ne pas exposer de surfaces de requêtes de filtrage arbitraires ;
- garder des routes stables et limitées,
- éviter les chemins de navigation “combinaisons infinies”.

### Maintenabilité

- source de données distincte du webapp,
- modèles typés,
- validations de schéma sur import,
- tests d’intégration sur régression de contenu,
- génération de données versionnées.

## Décisions de conception

### 1. Limiter les dimensions de navigation `[MAJ]` schéma de routes définitif

La navigation ne doit plus proposer une combinatoire illimitée. Concrètement, l’application doit exposer des chemins lisibles et prévisibles, pas un moteur de recherche paramétré infini. Le principe retenu : **une seule dimension de filtrage par route, jamais de combinaison de plusieurs paramètres de query string arbitraires**.

Routes proposées (à valider avec le porteur de produit, mais servent de défaut pour l'implémentation) :

- `/` : portail d'accueil, liens vers les 3 catalogues + présentation courte
- `/dons` : index alphabétique paginé (pas de liste intégrale) — tranches fixes type `/dons?page=A` ou sous-chemins `/dons/a`, `/dons/b`, … `/dons/0-9` (26 + 1 valeurs possibles, fermé, non combinatoire)
- `/dons/{slug}` : fiche de détail d'un don (ex. `/dons/adepte-de-la-matraque`)
- `/dons/type/{type}` : une sous-vue par type de don, où `{type}` est une valeur fermée dérivée des `Types` réellement présents dans les données (ex. `combat`, `general`) — **liste énumérée générée depuis les données au démarrage, pas une route libre**
- `/sorts` : index alphabétique paginé, même principe que `/dons`
- `/sorts/{slug}` : fiche de détail d'un sort
- `/sorts/niveau/{classe}/{niveau}` : une vue par (liste de classe, niveau), les deux valeurs étant des ensembles fermés dérivés des données (`Levels[].List` / `Levels[].Level`)
- `/monstres` : index paginé par CR ou alphabétique (à trancher, cf. questions ouvertes)
- `/monstres/{slug}` : fiche de détail (catalogue, cf. limitation du modèle de données ci-dessus)
- `/monstres/cr/{cr}` : sous-vue par CR fermé (valeurs réellement présentes dans les données)
- `/sources` et `/sources/{id}` : page listant les livres sources et leur contenu, utile pour la navigation et pour les références

Règle générale anti-combinatoire : **chaque route ne doit accepter qu'un seul paramètre de segmentation à la fois** (soit alphabet, soit type, soit niveau, soit CR), jamais une combinaison libre du type `?type=x&source=y&classe=z`. Si un besoin de croisement de filtres apparaît plus tard, il devra être explicitement re-brainstormé plutôt qu'ajouté de façon incrémentale (risque d'explosion combinatoire et de re-création de l'ancien problème).

Pas de route générique type :

- `/search?type=x&source=y&class=z&...` explosant les combinaisons
- pas de query string libre acceptant une combinaison de filtres non prévue à l'avance
- pas de tri/pagination paramétrable à volonté (taille de page cible fixée à **50 éléments** par tranche, cf. question ouverte n°4 tranchée ; sous-pagination numérique bornée à l'intérieur d'une tranche si celle-ci dépasse ce seuil, jamais de filtre combiné en plus)

### 2. Préférer le server-rendered / pre-rendered

Le site ne doit pas dépendre d’une application frontend lourde et dynamique. Un rendu côté serveur propre, avec CSS minimal et HTML simple, est meilleur pour la vitesse, la cacheabilité et la simplicité.

### 3. Insérer un pipeline de données explicite

Le projet doit traiter le contenu comme un pipeline :

- import depuis `pf1-data`,
- validation des données,
- génération des index / pages / caches,
- publication du site.

### 4. Préparer la cacheabilité CDN `[MAJ]` mécanique concrète

Les pages qui sont des références stables doivent être publiées comme contenu quasi statique. Cela couvre :

- fiches détaillées,
- index par lettre / section,
- listes de sources ou types connus,
- résultats de répertoires déterministes.

Mécanique HTTP concrète recommandée :

- **En-têtes** : `Cache-Control: public, max-age=3600, stale-while-revalidate=86400` sur les pages de contenu stable (fiches + index), ajustable selon la fréquence réelle de mise à jour de `pf1-data` (probablement une actualisation par jour ou moins fréquente).
- **`ETag`** calculé à partir d'un hash du contenu de la page (ou d'une version globale du jeu de données, ex. hash du commit `pf1-data` chargé), pour permettre les réponses `304 Not Modified` sans recalcul.
- **Output Cache serveur** en complément, avec des tags par entité (`feat:{slug}`, `spell:{slug}`, `monster:{slug}`, `dons-index`, `sorts-index`, …) purgés uniquement lors du rechargement d'une nouvelle version de `pf1-data` — pas de TTL court arbitraire qui recalculerait inutilement des pages qui ne changent jamais entre deux mises à jour de données.
- Le calcul de la version « globale » du jeu de données (utilisée pour l'ETag et l'invalidation) doit être dérivé du commit SHA du clone `pf1-data` chargé, disponible facilement via `git rev-parse HEAD` au moment du chargement.
- Pas de cookies ni d'état de session sur les pages cacheables : toute personnalisation (thème, préférences) doit être gérée côté client uniquement (CSS/JS local), jamais via une réponse serveur variable par utilisateur, pour ne pas casser la cacheabilité CDN.

## Livrables par étapes

### Étape 1 — Fondations du nouveau système

Livrable testable : le projet est migré vers .NET 10 et la source des données est connectée au dépôt `pf1-data`.

À livrer :

- projet ASP.NET Core 10 fonctionnel,
- modèle de données internes pour dons, sorts, monstres et sources,
- pipeline de chargement depuis le clone `pf1-data` (fichiers JSON uniquement),
- validation applicative des exports JSON (champs requis, cohérence des types, exploitation de `diagnostics.json`),
- preuve que les données de production ne sont plus embarquées dans le repo web.

Validation :

- le site démarre sans dépendre des anciens fichiers `App_Data/*.xml` du dépôt legacy,
- la donnée est chargée depuis les fichiers JSON du clone `pf1-data` au build/déploiement,
- les tests de chargement de données passent sur un jeu de données réaliste.

### Étape 2 — Performance et démarrage

Livrable testable : le site démarre rapidement et les pages lisent une structure de données optimisée.

À livrer :

- chargement pré-calculé / indexé des données,
- cache mémoire pour les listes et les références,
- réduction du poids des pages d’index,
- réduction des IO coûteux au démarrage.

Validation :

- benchmark du temps de démarrage under acceptable threshold,
- aucune liste massive de 1000+ éléments dans un même document HTML,
- temps de réponse sur les routes principales stable.

### Étape 3 — Navigation prédictive et anti-combinatoire

Livrable testable : la navigation est simplifiée et rendue stable, sans explosion de pages résultantes.

À livrer :

- page d’accueil orientée portail,
- index par sections déterministes,
- routes lisibles pour dons, sorts et monstres,
- suppression des filtres combinés non maîtrisés en version 1,
- navigation plus “catalogue” que “moteur de recherche libre”.

Validation :

- la page `/dons` n’expose plus la totalité des éléments en un seul document,
- les routes ne produisent pas une combinatoire infinie,
- le comportement est stable entre sessions.

### Étape 4 — Mise en cache CDN et robustesse face aux bots

Livrable testable : les contenus stables sont parfaitement cacheables par le CDN.

À livrer :

- stratégie de cache HTTP pour fiches + index,
- génération / publication des pages les plus demandées,
- headers `Cache-Control`/`ETag` adaptés,
- règle de protection sur la pages les plus lourdes ou les plus ramifiées.

Validation :

- un navigateur ou un CDN peut mettre en cache les pages de contenu de référence,
- le trafic de bots IA est limité sans nuire au lecteur humain,
- les pages “stables” ne sont pas recalculées à chaque hit.

### Étape 5 — Modernisation visuelle et UX

Livrable testable : le site reste sobre, simple et très rapide à charger.

À livrer :

- design minimaliste et cohérent,
- CSS léger,
- navigation claire,
- rendu serveur simple sans dépendances JS lourdes,
- priorité au texte, l’architecture et la lisibilité.

Validation :

- le site est lisible sur mobile et desktop,
- le chargement initial reste rapide,
- les ressources statiques sont minimales et bien cacheables.

### Étape 6 — Monstres et contenu avancé `[MAJ]` livraison incrémentale actée

Livrable testable : le domaine `Monster` est intégré à la base de données et à la navigation via une **v1 volontairement légère** (catalogue de références, cf. question ouverte n°1 — tranchée), puis étendu par incréments successifs sans attendre une refonte complète de la donnée source.

**Découpage en incréments explicites** (chacun est un livrable testable indépendant, à planifier séparément dans le plan d'implémentation) :

- **Incrément 6.1 (v1 légère)** : catalogue basé strictement sur les champs déjà présents dans `monsters.json` — nom, CR, type, environnement, climat, source. Aucune donnée inventée ou approximée.
- **Incrément 6.2+ (à planifier quand la donnée source évolue)** : ajout progressif de tout champ supplémentaire que `pf1-data` viendrait à exposer (statblock, capacités spéciales, description). Chaque nouveau champ disponible peut être ajouté indépendamment, sans attendre que l'ensemble du statblock soit disponible.

À livrer (incrément 6.1) :

- page d’index monstres (alphabétique et/ou par CR),
- pages de détail avec les champs disponibles (nom, CR, type, environnement, climat, source, lien vers la référence externe le cas échéant),
- filtres CR / type / environnement / climat / source, un seul à la fois (cf. règle anti-combinatoire),
- support de génération des données depuis `pf1-data`,
- **placeholder explicite et honnête** sur la fiche de détail si aucune statistique de jeu n'est disponible (ne pas inventer de contenu ni laisser un vide silencieux).

Validation :

- les monstres sont visibles et navigables,
- les propriétés métier disponibles sont cohérentes et représentées,
- les pages sont compatibles avec la stratégie de cache et de navigation simplifiée,
- il est explicite pour l'utilisateur que la fiche de monstre est un résumé/catalogue et non un statblock complet, tant que `pf1-data` ne fournit pas plus de détail.

### Étape 7 — Dons, sorts et nouveaux cas

Livrable testable : le modèle supporte les cas complexes déjà observés et les nouveaux cas de contenu.

À livrer :

- modélisation des prérequis avancés, données multi-sources, listes de sorts, descriptions, références,
- représentation des cas inhabituels pour dons et sorts,
- robustesse des sorties au regard des nouveaux contenus.

Validation :

- l’application rend correctement les cas complexifiés du corpus,
- les données de `pf1-data` ne nécessitent plus de hacks ad hoc,
- les erreurs structurelles sont détectées avant publication.

## `[MAJ]` Questions ouvertes / risques à trancher avant ou pendant l'implémentation

Ces points ne sont pas bloquants pour démarrer l'étape 1, mais doivent être arbitrés avant les étapes concernées :

1. ~~**Portée réelle des monstres**~~ — **Tranché** : v1 volontairement légère (catalogue de références avec les métadonnées disponibles : nom, CR, type, environnement, climat, source), sans statblock complet. Les fonctionnalités monstre seront ensuite étendues par incréments successifs (ex. statblock complet, capacités spéciales) au fur et à mesure que `pf1-data` enrichira son export, plutôt que de bloquer l'étape 6 en attendant une donnée plus riche. Chaque incrément fera l'objet de son propre livrable testable, sans re-brainstorming complet de la spec.
2. ~~**Mécanisme de synchronisation prod du clone `pf1-data`**~~ — **Tranché** : `git pull` planifié + rechargement à chaud (sans redémarrage du process), **fréquence une fois par jour**. Le `Refresh pipeline` (étape 1/2) doit implémenter un `IHostedService` avec un déclenchement périodique (ex. `PeriodicTimer` ou tâche planifiée quotidienne), qui vérifie le nouveau `HEAD` du dépôt, recharge les données en mémoire et purge les caches taggés (voir mécanique de cache). Pas besoin d'un mécanisme de webhook/push en v1 : le pull quotidien suffit au regard de la fréquence de mise à jour réelle de `pf1-data`.
3. ~~**Hébergement et CDN cible**~~ — **Tranché** : hébergement sur **VM auto-hébergée** (IIS ou reverse proxy devant Kestrel — à préciser en étape 1 selon les contraintes d'infra existantes), avec **Cloudflare** en frontal comme CDN/cache. Impacts concrets à prévoir pour l'implémentation :
   - configurer les en-têtes `Cache-Control` / `stale-while-revalidate` en sachant que Cloudflare les respecte nativement pour le cache de périphérie (« edge cache ») ;
   - prévoir une règle de **purge de cache Cloudflare** (API Cloudflare, purge par tag ou par URL) déclenchée par le `Refresh pipeline` après rechargement des données, pour éviter de servir du contenu périmé jusqu'à `max-age` ;
   - le reverse proxy devant Kestrel (IIS ou nginx selon l'infra) doit transmettre les en-têtes de cache sans les altérer ;
   - Cloudflare permettant aussi le filtrage de bots, envisager ses règles de bot management / rate limiting en complément du `robots.txt` (moins critique vu que le contenu est de toute façon caché en edge).
4. ~~**Segmentation exacte des index**~~ — **Tranché** : segmentation par **alphabet pour dons/sorts** et **par CR pour monstres** (proposition par défaut validée), avec une **taille de page cible de 50 éléments**. Si une tranche alphabétique dépasse sensiblement 50 éléments (ex. lettre très fréquente), prévoir une sous-pagination numérique **à l'intérieur** de cette tranche (ex. `/sorts/a?page=2`) plutôt que de renoncer à la segmentation par lettre — mais cette sous-pagination doit rester bornée et prévisible (pas de tri/filtre combiné en plus).
5. ~~**Design visuel**~~ — **Tranché** : pas de préférence de style imposée par le porteur de produit. **À définir plus tard avec des mockups**, avant l'étape 5, via le companion visuel de brainstorming. À cette occasion, proposer plusieurs références de sites existants sobres/rapides à titre d'inspiration (ex. sites de documentation technique, wikis minimalistes) plutôt que de partir d'une page blanche.
6. ~~**Fonctionnalités supprimées vs. différées**~~ — **Tranché** : suppression en v1 des recherches combinées de l'ancien site (attributs, classes, niveaux multiples pour les dons), **avec une note conservée pour réévaluation future** plutôt qu'une suppression définitive et irréversible. Cette note est conservée dans la section « À ne pas faire dans la v1 » ci-dessous, marquée comme réévaluable après la v1 plutôt que comme un renoncement permanent.
7. ~~**AOT / ReadyToRun**~~ — **Tranché** : reste **ouvert et différé à l'étape 2**, à tester techniquement par l'agent d'implémentation selon la compatibilité réelle avec Razor Pages et l'hébergement en VM auto-hébergée (IIS/Kestrel). Aucune préférence imposée d'avance entre ReadyToRun et Full AOT ; le choix doit être documenté avec sa justification (mesure de démarrage à l'appui) au moment de l'étape 2.
8. ~~**Devenir du format XML et d'un futur schéma JSON**~~ — **Tranché** : la nouvelle application reste sur le **format JSON**, pour la performance de désérialisation offerte par `System.Text.Json` (source generation, démarrage rapide sans réflexion). Le XML n'est plus consommé par cette application, indépendamment de ce que décidera le mainteneur de `pf1-data` sur son maintien. Si un schéma JSON formel est produit plus tard côté `pf1-data`, il pourra être ajouté en complément de la validation applicative déjà prévue (voir « Nature du dépôt » ci-dessus), sans remettre en cause ce choix de format.

## `[MAJ]` Traçabilité demande → livrables

| Demande initiale | Section(s) de la spec | Étape(s) |
|---|---|---|
| Migrer en .NET 10 | Cible technique / Stack | Étape 1 |
| Se baser sur le clone `pf1-data`, aussi utilisé en prod | Analyse détaillée du dépôt `pf1-data` / Source de vérité et cycle de mise à jour | Étape 1 |
| Optimiser le démarrage | Stack (System.Text.Json source gen, AOT/R2R) / Étape 2 | Étape 2 |
| Moderniser l'affichage, sobre et rapide | Simplicité et sobriété du front | Étape 5 |
| Page d'accueil dons/sorts trop coûteuse (liste tout) | Navigation et performance / routing définitif | Étape 3 |
| Résister au crawling bots/IA, cache CDN | Cache et crawl resistance / mécanique de cache concrète | Étape 4 |
| Navigation prévisible, anti-combinatoire, quitte à supprimer des features | Décisions de conception §1 (routing définitif) / À ne pas faire | Étape 3 |
| Gérer les monstres | Gestion des monstres (avec limitation documentée) | Étape 6 |
| Améliorer sorts et dons pour les nouveaux cas | Amélioration des dons et des sorts / schémas Feat, Spell détaillés | Étape 7 |

## Critères de succès globaux

Le projet est réussi si :

- la navigation est 10x plus légère que l’ancienne app sur les pages d’index,
- le site est compatible .NET 10,
- les données viennent du clone `pf1-data` en production,
- le démarrage est rapide,
- les pages stables sont cacheables via CDN,
- la navigation est simple et prévisible,
- les monstres sont intégrés,
- les dons et sorts gèrent correctement les cas complexes.

## À ne pas faire dans la v1

- ne pas introduire un moteur de recherche arbitraire à grande combinatoire,
- ne pas exposer des filtres “illimités” sans garde-fou,
- ne pas recréer une architecture d’édition web pour un contenu qui doit rester stable et source-based,
- ne pas ajouter de dépendances frontend lourdes,
- ne pas embarquer de copies de fichiers de données JSON dans le dépôt applicatif : la donnée vient uniquement du clone `pf1-data` externe, jamais d'une copie committée dans ce repo,
- ne pas consommer le format XML historique du dépôt `pf1-data` (décision actée : JSON uniquement, via `System.Text.Json`, cf. question ouverte n°8),
- `[MAJ]` filtres combinés avancés sur les dons (attributs, classes, niveaux multiples) : **supprimés en v1, mais réévaluables plus tard** — ne pas les considérer comme abandonnés définitivement (cf. question ouverte n°6, tranchée en ce sens) ; consigner cette suppression comme un choix de portée v1, pas comme un renoncement produit.

## Proposition de plan de mise en œuvre

La refonte doit avancer par livrables testables, avec un ordre clair :

1. migration technique .NET 10 et pipeline data source,
2. accélération startup + cache,
3. simplification navigation + indexation par sections,
4. modernisation visuelle + sobriété front,
5. intégration monstres,
6. robustesse dons/sorts,
7. hardening cache/CDN + bot protection,
8. stabilisation fonctionnelle avant production.

Ce plan évite de mélanger les problèmes de fond (source de données, architecture) avec les problèmes de surface (UI, navigation, performances). La priorité est de sécuriser la base de données et la structure, puis d’optimiser le front et la cacheabilité.
