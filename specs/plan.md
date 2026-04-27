# Plan: Estratègia d'Implementació

## Pas 1: Preparació del Projecte i Paquets
1. Instal·lar `com.unity.ml-agents` des del Package Manager.
2. Afegir el component `Behavior Parameters` als prefabs de Gojo i Sukuna, establint `Discrete Branches` i tipus d'observacions.
3. Crear i preparar un entorn de Python (`venv`) i instal·lar les llibreries compatibles (ex: PyTorch v2.2.2 i mlagents v1.1.0).

## Pas 2: Classe FighterAgent i Components Addicionals
1. Redactar el fitxer genèric `FighterAgent.cs`.
2. Afegir la recollida d'informació (`CollectObservations`).
3. Mapejar les dades que genera l'algorisme (`ActionBuffers`) als booleans i variables públiques d'input de `PlayerController.cs` mitjançant funcions d'avaluació en el mètode `OnActionReceived`.
4. Afegir la definició del càlcul de les recompenses (reward loop) durant l'`Update` monitoritzant la reducció i augment de `vidasActuales` i `porcentajeDaño`.

## Pas 3: Refactoredització d'Inputs al PlayerController
1. Modificar `RecogerInput()` en el `PlayerController` per desacoblar de manera segura els events de `UnityEngine.Input` i donar prioritat a les modificacions d'estat derivades del `FighterAgent`.
2. Afegeix condicions protectores contra col·lisions d'inputs, com prohibir l'ús del teclat si `FighterAgent.enabled` és cert.

## Pas 4: Alteracions del Cicle de Vida del Joc (GameManager)
1. Afegir un booleà `modoEntrenamiento` per bypass-ar la visualització de l'HUD de tancament.
2. Modificar el component `BlastZone` o la mateixa pèrdua de vida en el controlador perquè previngui desactivar el `gameObject` complet en mode d'entrenament, garantint la comunicació en xarxa de ML-Agents.
3. Codificar la lògica automàtica de restauració de danys, posicions de target i neteja d'estats a la funció de ML-Agents `OnEpisodeBegin()`.

## Pas 5: Parametrització i Iniciació (Entrenament)
1. Redactar l'arxiu YAML amb els hiperparàmetres òptims d'entrenament per a agents dinàmics tipus PPO.
2. Llançar l'entrenador des del CLI de Python assegurant els IDs del model.
3. Provar l'ajust manual del `self_play` en escena, modificant `GameManager` per inicialitzar un IA contra una IA.
