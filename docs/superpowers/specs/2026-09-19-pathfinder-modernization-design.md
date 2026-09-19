# Spécification de modernisation du site Pathfinder FR DB

## Contexte et domaine

Le projet actuel est un site de référence historique pour la base de données Pathfinder 1e, répondant à l’URL https://db.pathfinder-fr.org/. Il a été construit comme un petit moteur de navigation sur des données extraites de la communauté Pathfinder-fr.org, avec des listes de dons et de sorts publiées sous forme HTML générée côté serveur.

Le site est fonctionnel, mais il a été conçu pour un contexte de données statiques et un front web legacy. L’analyse du site et du dépôt montre des points de friction majeurs :

- le site charge et affiche la totalité des dons et des sorts dans les pages d’index, ce qui produit un volume réseau excessif et un rendu lourd ;
- la navigation est très ouverte et combinatoire ;
- les données sont traitées via des fichiers XML embarqués et une logique monolithique ASP.NET MVC 4 ;
- les pages ne sont pas construites pour la mise en cache CDN ni pour la résistance aux crawlers IA/bots ;
- le domaine est incomplet avec le besoin d’ajouter explicitement les monstres et de mieux couvrir les cas nouveaux sur dons et sorts.

Le nouveau projet doit moderniser l’application, la migrer vers .NET 10, et s’appuyer sur le dépôt source de données `D:\code\perso\pf\pf1-data` (clon Git du repo GitHub `pathfinder-fr/pf1-data`). Ce clone doit être la source de vérité du contenu produit et mis à jour régulièrement, et il sera également la source utilisée en production.

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

## Cible technique

### Stack

- ASP.NET Core 10 / .NET 10
- Razor Pages ou ASP.NET Core MVC selon le besoin d’UI server-side
- HTML/CSS minimal, sans dépendances lourdes de frontend
- pipeline de génération de données depuis `pf1-data`
- cache HTTP / CDN / ETag / `Cache-Control`
- sortie statique ou quasi-statique pour les pages à forte lecture

### Source de vérité

Le dépôt `pf1-data` est considéré comme le référentiel de production des données. Il contient des exports structurés (`feats.json`, `spells.json`, `monsters.json`, etc.) et est utilisé comme source d’actualisation de contenu. Le projet web ne doit pas conduire de maintenance ad hoc dans des XML embarqués.

### Architecture proposée

- `Data source` : clone de `pf1-data` en production
- `Import / normalization` : service de conversion des fichiers JSON/XML vers des modèles internes normalisés
- `Content model` : entités Feat, Spell, Monster, source, references, prerequisites, etc.
- `Runtime app` : site web ASP.NET Core 10 qui expose des routes déterministes
- `Cache layer` : CDN + cache HTTP + cache mémoire pour les listes indexées et les détails
- `Build pipeline` : génération de données et validation des schémas avant publication

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

### 1. Limiter les dimensions de navigation

La navigation ne doit plus proposer une combinatoire illimitée. Concrètement, l’application doit exposer des chemins lisibles et prévisibles, pas un moteur de recherche paramétré infini.

Exemples de routes attendues :

- `/dons` : page d’audit / index général avec sections déterministes
- `/dons/a-c` : tranche alphabétique
- `/dons/combats` : sous-section par type
- `/sorts` : index général
- `/sorts/a-c` : tranche alphabétique
- `/sorts/niveau/1` : filtre stable et public
- `/monstres/cr/1` : filtre stable et public

Pas de route générique type :

- `/search?type=x&source=y&class=z&...` explosant les combinaisons

### 2. Préférer le server-rendered / pre-rendered

Le site ne doit pas dépendre d’une application frontend lourde et dynamique. Un rendu côté serveur propre, avec CSS minimal et HTML simple, est meilleur pour la vitesse, la cacheabilité et la simplicité.

### 3. Insérer un pipeline de données explicite

Le projet doit traiter le contenu comme un pipeline :

- import depuis `pf1-data`,
- validation des données,
- génération des index / pages / caches,
- publication du site.

### 4. Préparer la cacheabilité CDN

Les pages qui sont des références stables doivent être publiées comme contenu quasi statique. Cela couvre :

- fiches détaillées,
- index par lettre / section,
- listes de sources ou types connus,
- résultats de répertoires déterministes.

## Livrables par étapes

### Étape 1 — Fondations du nouveau système

Livrable testable : le projet est migré vers .NET 10 et la source des données est connectée au dépôt `pf1-data`.

À livrer :

- projet ASP.NET Core 10 fonctionnel,
- modèle de données internes pour dons, sorts, monstres et sources,
- pipeline de chargement depuis le clone `pf1-data`,
- validation schéma des exports JSON/XML,
- preuve que les données de production ne sont plus embarquées dans le repo web.

Validation :

- le site démarre sans dépendre des anciens XML du dépôt legacy,
- la donnée est chargée depuis le clone `pf1-data` au build/déploiement,
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

### Étape 6 — Monstres et contenu avancé

Livrable testable : le domaine `Monster` est pleinement intégré à la base de données et à la navigation.

À livrer :

- page d’index monstres,
- pages de détail,
- filtres CR / type / environnement / climat / source,
- support de génération des données depuis `pf1-data`.

Validation :

- les monstres sont visibles et navigables,
- les propriétés métier sont cohérentes et représentées, 
- les pages sont compatibles avec la stratégie de cache et de navigation simplifiée.

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
- ne pas réintroduire le modèle XML embarqué dans le front.

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
