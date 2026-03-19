# Documentation Technique
## GTA — Gestion Transport Auto-élévateur

**Version :** 1.0
**Date de rendu :** 19/03/2026
**Auteur :** Nicolas Kirchhoffer
**Formation :** Master 1 M2i — CCI Campus Strasbourg
**Formateur :** Nicolas Lehmann
**Moteur :** Unity 6 LTS — C#
**Plateformes :** iOS / Android (principal), PC (secondaire)

---

## Table des matières

1. [Présentation du projet](#1-présentation-du-projet)
2. [Architecture générale](#2-architecture-générale)
3. [Structure du code](#3-structure-du-code)
4. [Systèmes techniques](#4-systèmes-techniques)
5. [Fonctionnalités implémentées](#5-fonctionnalités-implémentées)
6. [Flux de jeu](#6-flux-de-jeu)
7. [Configuration Unity](#7-configuration-unity)
8. [Difficultés rencontrées et solutions](#8-difficultés-rencontrées-et-solutions)
9. [Pistes d'amélioration](#9-pistes-damélioration)

---

## 1. Présentation du projet

**GTA — Gestion Transport Auto-élévateur** est un serious game 2D en vue de dessus (top-down) développé en 10 semaines dans le cadre du Master 1 M2i. L'objectif pédagogique est de préparer les candidats à la certification CACES (Certificat d'Aptitude à la Conduite En Sécurité) pour chariots élévateurs.

Le joueur pilote un chariot élévateur dans un environnement logistique et doit transporter des palettes jusqu'aux zones de dépôt en respectant les règles de sécurité réelles. Chaque infraction entraîne une pénalité de points ; toute collision provoque l'échec immédiat de la mission.

### Apport pédagogique

| Compétence CACES | Mécanique de jeu |
|------------------|-----------------|
| Conduite sécurisée | Physique Ackermann réaliste, inertie |
| Gestion des charges | Système de fourches avec seuils d'attachement |
| Respect des vitesses | Jauge de vitesse, pénalité automatique |
| Klaxon aux intersections | Touche dédiée, anti-spam intégré |
| Vision adaptée | Cônes avant/arrière, obligation de reculer avec charge |
| Connaissance des zones | Zones de dépôt délimitées, signalétique au sol |

---

## 2. Architecture générale

Le projet repose sur le **pattern Manager Singleton** : chaque sous-système est encapsulé dans un Manager accessible globalement via une propriété `Instance` statique.

### Diagramme d'architecture

```
                        ┌──────────────┐
                        │  GameManager │  ← Chef d'orchestre (DontDestroyOnLoad)
                        └──────┬───────┘
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
     ┌────────────────┐ ┌────────────┐ ┌───────────────┐
     │ MissionManager │ │ UIManager  │ │ SafetyManager │
     └────────────────┘ └────────────┘ └───────────────┘
              ▲                ▲                ▲
              │         Événements              │
     ┌────────┴────────────────┴────────────────┴──────┐
     │              Systèmes Gameplay                   │
     │  PlayerMovement · ForkController · HornController│
     │  VisionManager · NPCForkliftController           │
     └──────────────────────────────────────────────────┘
                        ▲
               ┌────────┴────────┐
               │  InputManager   │  (DontDestroyOnLoad)
               └─────────────────┘
```

### Règle DontDestroyOnLoad

Seuls **GameManager** et **InputManager** persistent entre les scènes. Tous les autres managers sont des objets de scène recréés à chaque chargement. GameManager se reconnecte à eux via l'événement `SceneManager.sceneLoaded`.

---

## 3. Structure du code

```
Assets/Scripts/
├── Data/
│   └── ForkliftConfig.cs          ScriptableObject de configuration du chariot
├── Events/
│   └── GameEvents.cs              Événements statiques centralisés
├── Gameplay/
│   ├── NPC/
│   │   ├── ForkliftWaypoint.cs    Point de navigation NPC
│   │   └── NPCForkliftController.cs  IA du chariot NPC
│   ├── Objects/
│   │   └── Pallet.cs              Logique des palettes
│   ├── Player/
│   │   ├── ForkController.cs      Mécanisme des fourches
│   │   ├── HornController.cs      Klaxon + effet visuel
│   │   ├── PlayerCollisionDetector.cs  Détection collisions fatales
│   │   └── PlayerMovement.cs      Physique de conduite
│   ├── Vision/
│   │   ├── VisionCone.cs          Génération dynamique du cône de vision
│   │   ├── VisionObstacle.cs      Objets bloquant la vision
│   │   └── VisionTarget.cs        Objets détectables (NPC, palettes)
│   └── Zones/
│       ├── DropZone.cs            Zone de dépôt palette
│       └── ForkZone.cs            Zone d'insertion des fourches
├── Managers/
│   ├── AudioManager.cs            Sons et musiques
│   ├── GameManager.cs             États du jeu, coordination
│   ├── InputManager.cs            Clavier + overrides mobiles
│   ├── MissionManager.cs          Score, livraisons, timer
│   ├── SafetyManager.cs           Règles CACES, violations
│   ├── UIManager.cs               HUD, panneaux, boutons
│   └── VisionManager.cs           Gestion des cônes de vision
├── UI/
│   ├── MainMenuController.cs      Contrôleur menu principal
│   ├── MobileInputController.cs   Boutons tactiles mobiles
│   ├── SceneEntryFader.cs         Déclencheur fade-in à l'entrée de scène
│   └── SceneFader.cs              Transitions de scènes par fondu
└── PlayerInputAction.cs           Actions d'entrée générées (New Input System)
```

**Total :** 26 scripts — environ 4 500 lignes de code C#

---

## 4. Systèmes techniques

### 4.1 Physique de conduite (Ackermann)

Le chariot utilise la **géométrie de direction Ackermann** (direction par les roues arrière, comme un vrai chariot élévateur). Le rayon de braquage est calculé dynamiquement :

```
turnRadius = wheelBase / Tan(wheelAngle)
angularVelocity = currentSpeed / turnRadius
```

Cette approche produit un comportement de conduite réaliste avec inertie, accélération progressive et différence de comportement à vide vs chargé.

**Fichier :** `Gameplay/Player/PlayerMovement.cs`

---

### 4.2 Système de fourches et palettes

Chaque palette possède deux **ForkZones** (gauche et droite). L'attachement se déclenche automatiquement quand :
- Les deux fourches sont insérées
- La hauteur dépasse le seuil d'attachement (0.15 m)

Lors de l'attachement, la physique de la palette passe en **Kinematic** et ses colliders sont désactivés pour éviter les conflits. La détachement se produit en dessous de 0.20 m.

```
ForkZone (×2 par palette)
    └── détecte tag "Fork"
    └── notifie Pallet
        └── Pallet notifie ForkController
            └── ForkController déclenche AttachToForks()
```

**Fichiers :** `Gameplay/Objects/Pallet.cs`, `Gameplay/Zones/ForkZone.cs`, `Gameplay/Player/ForkController.cs`

---

### 4.3 Système de vision (cônes dynamiques)

Deux cônes de vision (avant / arrière) sont générés par **raycasting** à chaque frame. Chaque rayon détecte les obstacles (murs, racks) et s'arrête au premier contact, produisant un mesh de vision précis.

Les objets dans le cône (NPC, palettes) ont un composant **VisionTarget** qui adapte leur visibilité :
- `HideCompletely` : invisible hors du cône
- `FadeAlpha` : transparent hors du cône
- `ShowOutline` : contour visible hors du cône

Le joueur peut basculer entre la vue avant et arrière (touche `E` ou bouton mobile). La vue avant se bloque automatiquement quand la charge dépasse 0.5 m de hauteur.

**Fichiers :** `Gameplay/Vision/VisionCone.cs`, `VisionTarget.cs`, `Managers/VisionManager.cs`

---

### 4.4 Système de sécurité CACES (SafetyManager)

Vérifié à chaque frame pendant l'état `Playing` :

| Règle | Condition de violation | Pénalité |
|-------|----------------------|----------|
| Excès de vitesse | Vitesse > 5 km/h chargé, 10 km/h à vide | −50 pts |
| Charge trop haute | Fourches > 0.3 m en avançant | −60 pts |
| Mauvaise vision | Vue avant avec charge obstruant (à finaliser) | −40 pts |
| Klaxon carrefour | Pas de klaxon à l'intersection (à finaliser) | −30 pts |

Un **cooldown de 2 secondes** par type de violation évite le spam de pénalités. Toute collision avec un mur ou un NPC déclenche un **game over immédiat**.

**Fichiers :** `Managers/SafetyManager.cs`, `Gameplay/Player/PlayerCollisionDetector.cs`

---

### 4.5 Klaxon avec effet visuel (HornController)

Le klaxon produit :
1. Un son (via AudioManager)
2. Un effet visuel d'anneau expansif (LineRenderer)
3. Un arrêt temporaire des NPC dans le rayon (3 secondes)

**Anti-spam :** 3 utilisations dans une fenêtre de 10 secondes → cooldown de 10 secondes.

**Fichier :** `Gameplay/Player/HornController.cs`

---

### 4.6 Transitions de scènes (SceneFader)

Singleton `DontDestroyOnLoad` qui crée dynamiquement un Canvas (sortingOrder 9999) avec un panel noir pour les fondus. Utilise `Time.unscaledDeltaTime` pour fonctionner même pendant les pauses. La factory `GetOrCreate()` garantit l'unicité sans nécessiter de préfab.

**Fichier :** `UI/SceneFader.cs`

---

### 4.7 Contrôles mobiles (MobileInputController)

Les boutons tactiles injectent des valeurs **override** dans l'InputManager via `SetMoveOverride()`, `SetRotateOverride()`, `SetLiftOverride()`. Quand un override est `null`, le clavier reprend la main. Sur `OnDisable()` (pause, game over), tous les états sont réinitialisés pour éviter les inputs bloqués.

**Fichier :** `UI/MobileInputController.cs`

---

### 4.8 IA des NPC (NPCForkliftController)

Navigation par **waypoints** avec la même géométrie Ackermann que le joueur. Chaque waypoint peut définir un temps d'attente et une limite de vitesse. Un raycasting frontal détecte les obstacles et déclenche un ralentissement proportionnel ou un arrêt d'urgence.

L'arrêt après klaxon (`HonkReaction()`) est géré indépendamment du système de navigation pour ne pas perturber la progression dans les waypoints.

**Fichiers :** `Gameplay/NPC/NPCForkliftController.cs`, `ForkliftWaypoint.cs`

---

## 5. Fonctionnalités implémentées

### Core gameplay ✅
- [x] Conduite réaliste (Ackermann, inertie, différentiel chargé/vide)
- [x] Levage et dépôt de palettes (double ForkZone, seuils automatiques)
- [x] Zones de dépôt avec détection et disparition de la palette
- [x] Compteur de livraisons et déclenchement mission accomplie

### Sécurité CACES ✅
- [x] Limite de vitesse chargé/vide
- [x] Pénalité charge trop haute en avancement
- [x] Game over sur collision (mur, NPC)
- [x] Klaxon avec anti-spam

### Vision ✅
- [x] Cônes de vision avant/arrière dynamiques
- [x] Occultation des objets hors du cône
- [x] Basculement manuel et automatique selon hauteur de charge
- [x] Blocage visuel par obstacle (VisionObstacle)

### Interface ✅
- [x] HUD : score, timer, livraisons, barre de progression, jauge vitesse, hauteur fourches
- [x] Menu principal avec transition par fondu
- [x] Menu pause (reprendre, menu principal, quitter)
- [x] Écran game over
- [x] Écran mission accomplie avec score final et bouton niveau suivant
- [x] Alertes de violation (popup 2 secondes)
- [x] Contrôles tactiles mobiles

### Architecture ✅
- [x] Pattern Manager Singleton sur tous les systèmes
- [x] Persistance correcte DontDestroyOnLoad (GameManager, InputManager uniquement)
- [x] Reconnexion automatique des managers au rechargement de scène
- [x] Transitions inter-scènes fluides (fade)
- [x] 2 niveaux jouables (Level 1, Level 2)

### Non implémenté (hors scope du livrable)
- [ ] Tutoriel interactif
- [ ] Sauvegarde de progression
- [ ] Système de badges
- [ ] Détection klaxon aux carrefours (infrastructure partielle dans SafetyManager)
- [ ] Tableau de bord formateur

---

## 6. Flux de jeu

```
Lancement
    └── MainMenu
        └── [Jouer] → fade → Level 1
            ├── GameManager.StartGame()
            │   ├── MissionManager.ResetMission() + StartMission()
            │   └── SafetyManager.ResetViolations()
            ├── Gameplay
            │   ├── Dépôt palette → DropZone.onPalletDropped
            │   │   └── MissionManager.HandlePalletDelivered()
            │   │       └── palletsDelivered >= targetDeliveries ?
            │   │           └── OUI → EndMission(true) → GameState.MissionComplete
            │   │               └── UIManager → MissionCompletePanel
            │   │                   ├── [Niveau suivant] → Level 2
            │   │                   └── [Quitter] → Application.Quit()
            │   ├── Collision fatale → GameManager.OnAccident()
            │   │   └── GameState.GameOver → UIManager → GameOverPanel
            │   └── Timer écoulé → EndMission(false) → GameState.GameOver
            └── Pause
                ├── GameState.Paused → UIManager → PausePanel
                ├── [Reprendre] → GameState.Playing
                └── [Menu Principal] → fade → MainMenu
```

---

## 7. Configuration Unity

### Scènes (Build Settings — dans l'ordre)
| Index | Scène | Rôle |
|-------|-------|------|
| 0 | MainMenu | Menu principal |
| 1 | Level 1 | Premier niveau |
| 2 | Level 2 | Deuxième niveau |

### Tags requis
- `Wall` — sur tous les murs
- `NPC` — sur les chariots NPC
- `Fork` — sur les fourches du chariot joueur

### Layers requis
- `ObstacleLayer` — murs et objets bloquant le raycasting de vision
- `VisionTarget` — objets détectables par les cônes

### Hiérarchie recommandée par scène de jeu
```
--- MANAGERS --- (DontDestroyOnLoad : GameManager, InputManager)
MissionManager
SafetyManager
AudioManager
VisionManager
UIManager
Canvas
    └── HUD
    └── PauseMenu (désactivé par défaut)
    └── GameOverPanel (désactivé par défaut)
    └── MissionCompletePanel (désactivé par défaut)
Chariot (joueur)
    └── PlayerMovement
    └── ForkController
    └── HornController
    └── PlayerCollisionDetector
    └── VisionCone (avant)
    └── VisionCone (arrière)
NPC
    └── NPCForkliftController
    └── Waypoints
Palettes
DropZones
```

---

## 8. Difficultés rencontrées et solutions

### Persistance DontDestroyOnLoad
**Problème :** GameManager persistait entre les scènes avec des références périmées vers UIManager, MissionManager, etc. (détruits au rechargement).
**Solution :** Abonnement à `SceneManager.sceneLoaded` dans GameManager. À chaque chargement d'une scène de jeu, les références sont retrouvées via `FindFirstObjectByType<>`.

### Détection de composants dans la hiérarchie
**Problème :** `GetComponent<VisionTarget>()` ne trouvait pas le composant si le Collider2D était sur un enfant différent du GameObject racine.
**Solution :** Remplacement par `GetComponentInParent<VisionTarget>()` avec fallback `GetComponentInChildren<VisionTarget>()`.

### Géométrie Ackermann et angle limite
**Problème :** `Mathf.Tan(90°)` tend vers l'infini, rendant la formule de rayon de braquage instable au-delà de 80°.
**Solution :** Le `maxWheelAngle` est maintenu en dessous de 80°. La réduction de `wheelBase` permet d'obtenir un rayon de braquage serré sans dépasser cette limite.

### Inputs mobiles bloqués
**Problème :** Lors d'un passage en pause ou game over pendant qu'un bouton tactile était maintenu, la valeur override restait active à la reprise.
**Solution :** `OnDisable()` et `OnDestroy()` dans MobileInputController réinitialisent tous les états et passent les overrides à `null`.

### Warning LoadTooHigh en marche arrière
**Problème :** La règle CACES de hauteur de charge se déclenchait aussi en marche arrière, alors que reculer avec une charge est la procédure correcte.
**Solution :** Remplacement de `Mathf.Abs(speed) > 0.1f` par `speed > 0.1f` (marche avant uniquement).

### Nom de fichier vs nom de classe (UIManager)
**Problème :** Le fichier `UiManager.cs` ne correspondait pas à la classe `UIManager`, empêchant Unity d'indexer le script dans l'Add Component.
**Solution :** Renommage du fichier dans le Project panel Unity (pas par le système de fichiers) pour que Unity régénère correctement le `.meta`.

---

## 9. Pistes d'amélioration

### Court terme
- **Tutoriel interactif** : Affichage contextuel des touches et règles à la première ouverture
- **Résumé de fin de mission détaillé** : Afficher les violations commises avec explication pédagogique
- **Détection du klaxon aux carrefours** : L'infrastructure existe dans SafetyManager, à finaliser avec des trigger zones

### Moyen terme
- **Sauvegarde de progression** : `PlayerPrefs` ou fichier JSON pour conserver le meilleur score par niveau
- **Système de notation** : Note A/B/C/D selon le score (comme prévu dans le GDD)
- **Piétons** : Ajout de NPC piétons avec `PedestrianDanger` violation active

### Long terme
- **Tableau de bord formateur** : Interface de suivi des apprenants avec statistiques d'erreurs
- **Mode examen** : Séquence de niveaux sans retry, score global pour simuler le CACES
- **Localisation** : Traduction anglaise du contenu

---

*Documentation rédigée dans le cadre du projet Serious Game — Master 1 M2i*
*CCI Campus Strasbourg — Année universitaire 2025-2026*
