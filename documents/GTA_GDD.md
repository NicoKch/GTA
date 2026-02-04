# Serious Game Design Document
## GTA - Gestion Transport Auto-élévateur

**Version :** 1.0  
**Date :** 04/02/2026  
**Auteur :** Nicolas  
**Formation :** Master 1 M2i - CCI Campus Strasbourg  
**Formateur :** Nicolas Lehmann

---

## Table des matières

1. [Titre et Résumé](#module-1--titre-et-résumé)
2. [Objectifs du Serious Game](#module-2--objectifs-du-serious-game)
3. [Public Cible](#module-3--public-cible)
4. [Gameplay et Mécaniques](#module-4--gameplay-et-mécaniques)
5. [Narration et Contexte](#module-5--narration-et-contexte)
6. [Contenus et Ressources](#module-6--contenus-et-ressources)
7. [Technologie et Plateformes](#module-7--technologie-et-plateformes)
8. [Système de Feedback](#module-8--système-de-feedback)
9. [Contraintes et Faisabilité](#module-9--contraintes-et-faisabilité)
10. [Plan de Lancement et Évaluation](#module-10--plan-de-lancement-et-évaluation)

---

## Module 1 : Titre et Résumé

### 1.1 Titre du Projet

**Nom du projet:** GTA - Gestion Transport Auto-élévateur

### 1.2 Résumé / Pitch

**GTA - Gestion Transport Auto-élévateur** est un Serious Game 2D en vue de dessus conçu pour former les candidats à la certification CACES (Certificat d'Aptitude à la Conduite En Sécurité) pour chariots élévateurs. Le joueur pilote un chariot élévateur dans un environnement logistique réaliste et doit transporter des palettes en respectant scrupuleusement les règles de sécurité authentiques.

Chaque infraction aux protocoles de sécurité entraîne des pénalités de points, tandis que les accidents provoquent l'échec immédiat de la mission. L'objectif est de créer un transfert direct des compétences acquises en jeu vers la pratique professionnelle réelle.

---

## Module 2 : Objectifs du Serious Game

### 2.1 Objectifs Pédagogiques

**Compétences techniques visées :**

| Compétence | Description | Intégration dans le jeu |
|------------|-------------|------------------------|
| Conduite sécurisée | Maîtriser les déplacements du chariot | Contrôles réalistes avec inertie et direction arrière |
| Gestion des charges | Lever, transporter et déposer des palettes | Système de fourches avec seuils d'attachement |
| Respect des vitesses | Adapter sa vitesse selon le contexte | Jauge de vitesse avec pénalités en cas de dépassement |
| Utilisation du klaxon | Signaler sa présence aux intersections | Touche dédiée, obligation aux carrefours |
| Vision adaptée | Circuler en marche arrière si charge bloque la vue | Système de cônes de vision avant/arrière |
| Connaissance des zones | Identifier zones de stockage, circulation, danger | Environnement annoté avec signalétique |

**Objectifs comportementaux :**
- Développer des réflexes de sécurité automatiques
- Intégrer la vérification systématique avant manœuvre
- Prioriser la sécurité sur la rapidité

### 2.2 Objectifs de Gameplay

**Engagement du joueur :**
- Missions variées avec objectifs clairs et chronométrés
- Système de score encourageant la précision et le respect des règles
- Progression débloquant de nouveaux environnements et défis
- Feedback immédiat sur chaque action

**Équilibre sérieux/ludique :**
- Les contraintes réglementaires deviennent des défis de gameplay
- La maîtrise des règles = meilleur score et progression
- Échec = opportunité d'apprentissage avec explication de l'erreur

---

## Module 3 : Public Cible

### 3.1 Profil des Utilisateurs

**Utilisateur principal : Candidat CACES**

| Caractéristique | Description |
|-----------------|-------------|
| Âge | 18-55 ans |
| Secteur | Logistique, industrie, grande distribution, BTP |
| Niveau technique | Variable (débutant à intermédiaire) |
| Expérience gaming | Faible à modérée |
| Motivation | Obtention certification, employabilité, mise à niveau |

**Utilisateur secondaire : Formateur CACES**
- Utilise le jeu comme outil pédagogique complémentaire
- Suit la progression des apprenants
- Personnalise les parcours selon les besoins

**Utilisateur tertiaire : Entreprise**
- Forme ses employés en continu
- Évalue les compétences avant attribution de poste
- Réduit les coûts de formation pratique

### 3.2 Contexte d'Utilisation

| Contexte | Description 
|----------|-------------
| Centre de formation | Sessions encadrées par formateur
| Auto-apprentissage | À domicile avant/après formation 
| Entreprise | Formation continue, révision
| Évaluation | Test de compétences 

---

## Module 4 : Gameplay et Mécaniques

### 4.1 Description du Gameplay

**Type de jeu :** Simulation 2D en vue de dessus (top-down)

**Concept central :** Le joueur contrôle un chariot élévateur et doit accomplir des missions de transport de palettes dans un entrepôt/zone logistique, en respectant les règles de sécurité CACES. Les violations entraînent des pénalités, les accidents causent l'échec immédiat.

**Boucle de gameplay principale :**

```
┌──────────────────────────────────────────────────────────────┐
│                    BOUCLE DE GAMEPLAY                        │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│   1. BRIEFING MISSION                                        │
│      ↓                                                       │
│   2. NAVIGATION vers palette                                 │
│      ↓                                                       │
│   3. POSITIONNEMENT précis                                   │
│      ↓                                                       │
│   4. PRISE DE CHARGE (lever fourches)                        │
│      ↓                                                       │
│   5. TRANSPORT (vision arrière si chargé)                    │
│      ↓                                                       │
│   6. DÉPÔT dans zone cible                                   │
│      ↓                                                       │
│   7. SCORING & FEEDBACK                                      │
│      ↓                                                       │
│   [Retour à 1 ou FIN MISSION]                                │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### 4.2 Mécaniques Principales

#### 4.2.1 Système de Conduite

**Contrôles (AZERTY) :**

| Touche | Action |
|--------|--------|
| Z | Accélérer (avant) |
| S | Reculer (arrière) |
| Q | Tourner roues à gauche |
| D | Tourner roues à droite |
| A | Monter fourches |
| E | Descendre fourches |
| Espace | Klaxon |
| Shift | Frein d'urgence |

**Physique réaliste :**
- Direction par roues arrière (géométrie Ackermann)
- Accélération et décélération progressives avec inertie
- Comportement différent à vide vs chargé
- Rayon de braquage réaliste

#### 4.2.2 Système de Fourches et Palettes

**Mécaniques implémentées :**

| État | Comportement |
|------|--------------|
| Fourches basses | Peuvent s'insérer sous palette |
| Fourches levées (seuil atteint) | Palette s'attache automatiquement |
| Fourches abaissées (sous seuil) | Palette se détache |
| Palette attachée | Suit le mouvement du chariot |

**Règles de sécurité intégrées :**
- Impossible de baisser les fourches au-dessus d'une palette non attachée
- Hauteur de transport recommandée (feedback visuel)
- Détection des deux fourches insérées avant levage

#### 4.2.3 Système de Vision

**Champs de vision dynamiques :**

L'utilisateur dispose de deux champs de vision, avant et arrière, qu'il peut alterner selon ses besoins. Certains objets bloquent la vision imposant une attention particulière aux consignes de sécurité.

![Image du système de vision](./images/Capture%20d’écran%202026-02-04%20à%2012.10.52.png "Système de vision")

#### 4.2.4 Système de Règles et Pénalités

**Infractions et pénalités :**

| Infraction | Points perdus | Feedback |
|------------|---------------|----------|
| Excès de vitesse | -10/sec | Jauge rouge + son |
| Charge trop haute en déplacement | -25 | Indicateur hauteur |
| Circulation sans vision adaptée | -30 | Avertissement écran |
| Collision légère | -100 | Impact visuel + son |
| **Accident grave** | **ÉCHEC MISSION** | **Écran game over** |

### 4.3 Progression

**Structure en niveaux :**

| Niveau | Nom | Objectif | Difficulté |
|--------|-----|----------|------------|
| 0 | Tutoriel | Apprendre les contrôles | ★☆☆☆☆ |
| 1 | Premiers pas | Transport simple A→B | ★★☆☆☆ |
| 2 | Entrepôt basique | 3 palettes, 3 zones | ★★☆☆☆ |
| 3 | Zones étroites | Manœuvres précises | ★★★☆☆ |
| 4 | Trafic | PNJ piétons, autres véhicules | ★★★☆☆ |

**Déblocage :**
- Compléter un niveau avec score minimum (60%)
- Bonus pour score parfait (100%)
- Rejouer pour améliorer son score

### 4.4 Gamification

**Système de points :**

| Action | Points |
|--------|--------|
| Palette livrée correctement | +100 |
| Bonus temps (livraison rapide) | +10 à +50 |
| Mission sans infraction | +200 (bonus) |
| Mission parfaite | +500 (bonus) |

**Badges et succès :**

| Badge | Condition |
|-------|-----------|
| 🏅 Première Livraison | Compléter le tutoriel |
| 🎯 Précision | 10 dépôts parfaits |
| ⚡ Efficace | Niveau terminé sous le temps cible |
| 🛡️ Zéro Infraction | Mission complète sans pénalité |
| 🏆 Maître Cariste | Tous les niveaux en score parfait |
| 👁️ Vision Pro | 50 transports en vision arrière |
| 📯 Klaxonneur | 100 signalements au klaxon |

**Classements :**
- Score par niveau
- Score global
- Temps de complétion
- Moins d'infractions

---

## Module 5 : Narration et Contexte

### 5.1 Synopsis

**Contexte général :**
Le joueur est un nouvel employé dans une entreprise de logistique. Pour être titularisé, il doit obtenir sa certification interne de cariste en démontrant sa maîtrise du chariot élévateur et des protocoles de sécurité.

**Progression narrative :**
1. **Jour 1 - Formation** : Tutoriel avec le chef d'équipe
2. **Semaine 1 - Probation** : Missions simples sous supervision
3. **Mois 1 - Autonomie** : Missions variées, responsabilités accrues
4. **Évaluation** : Examen de certification

**Conflits et enjeux :**
- Pression temporelle vs respect des règles
- Productivité vs sécurité (le jeu montre que sécurité = efficacité)
- Progression professionnelle liée aux compétences

### 5.2 Personnages

**Personnage principal : Le joueur**
- Rôle : Cariste en formation
- Évolution : Débutant → Certifié
- Pas de personnalisation (focus sur les compétences)

**PNJ (Personnages Non Joueurs) :**

| PNJ | Rôle | Fonction |
|-----|------|----------|
| Chef d'équipe | Tuteur | Explications, feedback |
| Piétons | Obstacles | Circulation dans l'entrepôt |
| Autres caristes | Trafic | Gestion du croisement |
| Responsable sécurité | Évaluateur | Rappels règles, scoring |

### 5.3 Univers

**Environnements :**

| Environnement | Description | Défis spécifiques |
|---------------|-------------|-------------------|
| Entrepôt formation | Petit, peu d'obstacles | Apprentissage |
| Entrepôt standard | Rayonnages, allées | Navigation |
| Zone de quai | Camions, rampes | Manœuvres |
| Usine | Machines, bruit | Vigilance accrue |

**Ambiance visuelle :**
- Style 2D épuré mais lisible
- Codes couleurs cohérents (zones, dangers)
- Signalétique réaliste (panneaux, marquages au sol)
- Éclairage adapté (zones claires, zones d'ombre)

**Ambiance sonore :**
- Moteur du chariot (réaliste)
- Klaxon distinctif
- Alarmes (excès vitesse, collision imminente)
- Ambiance entrepôt (fond sonore discret)

---

## Module 6 : Contenus et Ressources

### 6.1 Compétences à Mobiliser

**Compétences techniques CACES :**
- Prise et dépose de charges
- Circulation en allées et zones de stockage
- Signalisation (klaxon, gestes)

**Compétences transversales :**
- Anticipation des dangers
- Prise de décision rapide
- Gestion du stress (chrono)
- Attention aux détails

### 6.2 Contenus Pédagogiques

**Tutoriels intégrés :**
- Contrôles de base (mouvement, direction)
- Utilisation des fourches
- Règles de circulation
- Gestion de la visibilité

**Fiches de rappel (accessibles in-game) :**
- Règles de sécurité CACES
- Signification des panneaux
- Procédures d'urgence


### 6.3 Assets Nécessaires

**Graphismes 2D :**

| Asset | Description | Quantité |
|-------|-------------|----------|
| Chariot élévateur | Sprite animé multi-angles | 1 (+ variantes) |
| Palettes | Différentes charges | 3-4 types|
| Fourches | Animation levage | Inclus chariot |
| Environnements | Tilemaps entrepôt | 5 sets |
| PNJ | Piétons, autres véhicules | 4-6 |
| UI | Menus, HUD, feedback | Complet |
| Signalétique | Panneaux, marquages | 5-10 |

**Sons et musiques :**

| Audio | Description |
|-------|-------------|
| Moteur chariot | Loop réaliste (idle, accélération) |
| Klaxon | Distinctif, authentique |
| Collision | Impact selon gravité |
| UI | Clics, validations, erreurs |
| Ambiance | Fond d'entrepôt |
| Musique | Menu uniquement (discrète) |

**Animations :**
- Mouvement des fourches (levée/descente)
- Rotation du chariot
- Déplacement des palettes
- Feedback visuels (particules, flashes)

---

## Module 7 : Technologie et Plateformes

### 7.1 Moteur de Jeu

**Choix : Unity 6 LTS**

**Justification :**
- Excellente gestion du 2D (Sprite Renderer, Tilemaps)
- Input System moderne pour mapping AZERTY
- Physics 2D performant pour simulation
- Export multiplateforme natif
- Large communauté et documentation

### 7.2 Plateformes Cibles

| Plateforme | Priorité | Spécificités |
|------------|----------|--------------|
| **Android** | Principale | Touch controls adaptés, APK |
| PC (Windows) | Secondaire | Clavier AZERTY |
| WebGL | Optionnel | Navigateur, formation à distance |

### 7.3 Langues disponibles

Au démarrage : Français.
Futur mise à jour possible : Anglais.

## Module 8 : Système de Feedback

### 8.1 Feedback Immédiat

**Visuels :**

| Situation | Feedback |
|-----------|----------|
| Action correcte | Flash vert, particules ✓ |
| Infraction | Flash rouge, icône ⚠️ |
| Zone de dépôt atteinte | Zone s'illumine |
| Palette attachée | Indicateur visible |

**Sonores :**

| Situation | Son |
|-----------|-----|
| Palette prise | "Clic" métallique |
| Palette déposée | Son de validation |
| Infraction | Buzzer court |
| Collision | Impact proportionnel |
| Mission réussie | Jingle positif |

**Textuels :**
- Pop-ups explicatifs pour chaque infraction
- Conseils contextuels
- Compteur de points en temps réel

### 8.2 Feedback Différé

**Écran de fin de mission :**

```
┌─────────────────────────────────────────┐
│         MISSION TERMINÉE                │
├─────────────────────────────────────────┤
│                                         │
│  Palettes livrées : 3/3        ✓        │
│  Temps : 2:34 (Cible: 3:00)    ⭐       │
│  Infractions : 2               -60 pts  │
│    - Excès vitesse (x1)                 │
│    - Oubli klaxon (x1)                  │
│                                         │
│  SCORE TOTAL : 740 / 1000               │
│  Note : B (Bien)                        │
│                                         │
│  💡 Conseil : Pensez à klaxonner aux    │
│     intersections même sans piéton      │
│     visible.                            │
│                                         │
│  [Recommencer]  [Niveau suivant]        │
└─────────────────────────────────────────┘
```

**Rapports de progression :**
- Graphique d'évolution des scores
- Statistiques par type d'infraction
- Comparaison avec moyennes
- Recommandations personnalisées
---

## Module 9 : Contraintes et Faisabilité

### 9.1 Durée de Développement

**Planning prévisionnel :**

| Phase | Durée | Dates | Livrables |
|-------|-------|-------|-----------|
| Pré-production | 1 semaine | 13-16/01 | Game Concept |
| Production Core | 3 semaines | 17/01-04/02 | GDD, prototype jouable |
| Production Features | 4 semaines | 05/02-04/03 | Fonctionnalités complètes |
| Polish & Tests | 2 semaines | 05-17/03 | Build finale, documentation |
| **Total** | **~10 semaines** | **13/01-19/03** | |

**Jalons clés :**
- 16/01/2026 : Game Concept validé
- 04/02/2026 : GDD livré
- 17/03/2026 : Business Model + évaluation
- 19/03/2026 : Rendu final complet

### 9.2 Budget Estimé

**Estimation production commerciale :**

| Phase | Durée estimée | Collaborateur | Valeur | 
|-------|---------------|---------------------|--------|
| Conception | 115h      | Game Designer               | 4000€  |
| Développement Core | 300h      | Développeur Unity             | 12 000€  |
| Création Assets | 170h      | Graphiste 2D             | 5100€  |
| Sound Design | 70h      |  Sound Designer           | 2500€  |
| Expertise CACES | 40h      | Consultant         | 2400€  |
| Test & Debug | 50h      |     Testeur Q&A         | 1000€  |
| **Total heures** | **745h** | | **27 000€** |

### 9.3 Ressources Nécessaires

**Équipe projet commercial :**
- 1 Game Designer
- 1 Développeurs Unity
- 1 Graphiste 2D
- 1 Sound Designer (temps partiel)
- 1 Testeur Q&A
- 1 Expert CACES (consultant)

### 9.4 Contraintes Techniques

| Contrainte | Solution |
|------------|----------|
| Performance mobile | Optimisation sprites, LOD, pooling |
| Contrôles tactiles | UI adaptée, zones de touch larges |
| Taille application | Compression assets, chargement dynamique |
| Hors connexion | Sauvegarde locale, sync ultérieure |

### 9.5 Risques Identifiés

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Retard développement | Moyenne | Élevé | Priorisation features, MVP |
| Bugs critiques | Moyenne | Élevé | Tests continus, versioning |
| Contrôles non intuitifs | Moyenne | Moyen | Tests utilisateurs précoces |
| Contenu CACES incorrect | Faible | Élevé | Validation par expert |
| Performance insuffisante | Faible | Moyen | Profiling régulier |

---

## Module 10 : Plan de Lancement et Évaluation

### 10.1 Stratégie de Lancement

**Phase 1 : Bêta test (post-académique, optionnel)**
- Distribution à 10-20 testeurs
- Collecte de retours utilisateurs
- Itérations correctives

**Phase 2 : Lancement public (optionnel)**
- Publication Google Play Store
- Marketing ciblé (centres formation, entreprises)
- Support utilisateurs

### 10.2 Indicateurs d'Évaluation

**Métriques utilisateur :**
- Taux de complétion tutoriel > 90%
- Rétention J1 > 50%
- Score moyen progression > 10%/semaine
- Note store > 4/5

**Métriques pédagogiques :**
- Corrélation score jeu / réussite CACES
- Réduction erreurs communes
- Satisfaction apprenants

### 10.3 Plan de Mise à Jour

**V1.0 (Lancement)**
- 8 niveaux + tutoriel
- Système de score complet
- Feedback pédagogique

**V1.3 (Post-lancement)**
- Corrections bugs
- Ajustements équilibrage
- Nouvelles langues (EN)

**V2.0 (Évolution majeure)**
- Nouveaux environnements
- Mode multijoueur (compétition)
- Tableau de bord formateur

---

## Annexes

### A. Glossaire

| Terme | Définition |
|-------|------------|
| CACES | Certificat d'Aptitude à la Conduite En Sécurité |
| Cariste | Conducteur de chariot élévateur |
| Fourches | Éléments du chariot qui soulèvent les palettes |
| Mât | Structure verticale supportant les fourches |
| Palette | Support de manutention pour les marchandises |

### B. Références CACES

- Recommandation R489 (chariots de manutention)
- Code du travail - Article R4323-55 et suivants
- Norme NF EN ISO 3691-1

### C. Ressources Techniques

- Documentation Unity 6 : https://docs.unity3d.com
- Unity Input System : https://docs.unity3d.com/Packages/com.unity.inputsystem
- Physique 2D Unity : https://docs.unity3d.com/Manual/Physics2DReference.html

---

**Document rédigé dans le cadre du projet Serious Game**  
**Master 1 M2i - CCI Campus Strasbourg**  
**Année universitaire 2025-2026**
