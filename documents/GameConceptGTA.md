# GAME CONCEPT
## GTA - Gestion Transport Auto-élévateur

**Projet Serious Game - M2i Master 1**  
**CCI Campus Strasbourg**  
**Formateur : Nicolas Lehmann**  
**Date : 16/01/2026**

---

## Objectif principal

Former les futurs candidats à la certification CACES (Certificat d'Aptitude à la Conduite En Sécurité) pour les chariots élévateurs à travers une expérience ludique et immersive. Le jeu vise à :

- Enseigner les règles de sécurité réelles applicables à la conduite de chariots élévateurs
- Familiariser les apprenants avec les procédures de conduite et de manutention
- Préparer efficacement à l'examen pratique du CACES
- Sensibiliser aux risques professionnels liés à l'utilisation d'engins de manutention

---

## Public cible

- **Candidats au CACES** : Personnes en formation initiale souhaitant obtenir leur certification
- **Caristes en recyclage** : Professionnels devant renouveler leur certification (tous les 5 ans)
- **Étudiants en logistique** : Apprenants en cursus logistique, supply chain ou industrie
- **Entreprises** : Centres de formation et entreprises industrielles souhaitant former leurs employés
- **Tranche d'âge** : 18 ans et plus (âge minimum légal pour conduire un chariot élévateur)

---

## Résumé du concept

"GTA - Gestion Transport Auto-élévateur" est un serious game de simulation en vue top-down 2D où le joueur incarne un cariste dans un environnement d'entrepôt industriel. Le joueur doit accomplir des missions de manutention (déplacer des palettes, charger/décharger des camions, ranger des marchandises) tout en respectant scrupuleusement les règles de sécurité en vigueur.

Le jeu transforme les contraintes réglementaires en mécaniques de gameplay engageantes : les limitations de vitesse deviennent des défis de gestion du temps, les vérifications obligatoires de l'engin se transforment en checklist interactive au début de chaque mission, et les règles de circulation dans l'entrepôt créent des puzzles de navigation.

Chaque infraction aux règles de sécurité entraîne une pénalité de points, et tout accident grave provoque l'échec immédiat de la mission, reproduisant ainsi les conséquences réelles d'un comportement dangereux en milieu professionnel.

---

## Mécanismes de gameplay

### Boucle de gameplay principale

1. **Briefing** : Le joueur reçoit sa mission (objectifs, contraintes de temps, particularités)
2. **Vérification pré-opérationnelle** : Checklist interactive de l'état du chariot (freins, klaxon, fourches, etc.)
3. **Exécution** : Réalisation des tâches de manutention en respectant les règles
4. **Évaluation** : Score final basé sur le respect des règles, le temps et l'efficacité
5. **Débriefing** : Retour pédagogique sur les erreurs commises

### Règles de sécurité intégrées au gameplay

| Règle réelle | Mécanique de jeu |
|--------------|------------------|
| Limitation de vitesse | Jauge de vitesse avec zone verte/orange/rouge |
| Klaxon aux intersections | Bouton d'action obligatoire aux carrefours |
| Charge maximale | Indicateur de poids avec surcharge interdite |
| Sens de circulation | Flèches au sol, pénalité si non-respect |
| Visibilité arrière | Obligation de reculer avec charge haute |

### Système de scoring

- **Points de base** : Attribués pour chaque tâche accomplie
- **Pénalités** : Déduction pour chaque infraction aux règles de sécurité
- **Bonus temps** : Points supplémentaires si mission terminée dans le temps imparti
- **Échec critique** : Accident = fin de mission immédiate (collision, renversement de charge, blessure)

### Progression

- **Niveaux de difficulté** : Missions de plus en plus complexes (entrepôt simple → environnement multi-zones avec trafic)
- **Déblocage** : Nouveaux types de chariots et environnements selon la progression
- **Certification virtuelle** : Examen final simulant les conditions réelles du CACES

---

## Impact attendu

### Pour les apprenants

- **Mémorisation renforcée** : Les règles de sécurité sont intégrées par la pratique répétée et l'association action/conséquence
- **Réduction du stress** : Familiarisation avec les situations avant le passage en conditions réelles
- **Apprentissage par l'erreur** : Possibilité de faire des erreurs sans conséquences réelles, favorisant la compréhension des risques

### Pour les formateurs

- **Outil pédagogique complémentaire** : Support ludique en complément de la formation théorique
- **Suivi de progression** : Statistiques détaillées sur les performances et les erreurs récurrentes
- **Optimisation du temps** : Les apprenants arrivent mieux préparés pour la pratique sur engins réels

### Pour les entreprises

- **Réduction des accidents** : Personnel mieux sensibilisé aux risques
- **Économies** : Diminution des coûts liés aux accidents du travail et à la casse de matériel
- **Conformité** : Meilleure préparation aux audits de sécurité

### Indicateurs de succès

- Taux de réussite au CACES des utilisateurs du jeu vs. formation classique
- Réduction du nombre d'infractions constatées lors des évaluations pratiques
- Satisfaction des apprenants et des formateurs

---

## Principales fonctionnalités

### Fonctionnalités core

- **Simulation de conduite 2D top-down** : Contrôle réaliste du chariot élévateur avec gestion de l'inertie et des fourches
- **Système de collision** : Détection précise des contacts avec l'environnement, les palettes et les obstacles
- **Système de visibilité** : Intégration des angles mort, de l'obstruction de la visibilité par des objets et de différents angles de vision à utilisé selon la situation. 
- **Gestion des palettes** : Prise, transport et dépose de charges avec contraintes de poids et d'équilibre
- **Interface HUD** : Affichage en temps réel de la vitesse, du poids de la charge, du temps restant et du score

### Fonctionnalités pédagogiques

- **Tutoriel interactif** : Introduction progressive aux commandes et aux règles
- **Mode entraînement** : Pratique libre sans contrainte de temps ni de score
- **Mode examen** : Simulation des conditions réelles de passage du CACES
- **Feedback en temps réel** : Alertes visuelles et sonores en cas d'infraction

### Fonctionnalités techniques

- **Plateforme cible** : Android (mobile/tablette)
- **Moteur** : Unity 6 LTS
- **Sauvegarde de progression** : Stockage local et cloud des scores et de l'avancement
- **Multi-langues** : Interface en français (extensible)

### Fonctionnalités sociales (évolutions futures)

- **Classement** : Tableau des meilleurs scores par niveau
- **Mode multijoueur** : Missions coopératives dans le même entrepôt
- **Partage** : Export des résultats pour validation par le formateur

---



*Document rédigé dans le cadre du projet Serious Game - M2i Master 1*  
*CCI Campus Strasbourg - Année universitaire 2026*