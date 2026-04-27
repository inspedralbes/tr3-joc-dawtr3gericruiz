# Foundations: Integració de CPU amb ML-Agents

## 1. Context i Definició del Problema
Actualment el projecte consisteix en un joc de lluita 2D en Unity (estil Smash Bros) amb dos personatges (Gojo i Sukuna). El joc ja disposa d'un sistema de control humà (teclat), un sistema de danys percentuals i knockback, i un sistema multijugador online. El problema és que no existeix cap manera de jugar offline (Solitari) on els rivals siguin controlats de manera autònoma.

## 2. Objectius del Projecte
- Implementar un agent d'Intel·ligència Artificial autònom (CPU) capaç de jugar contra un humà en els combats offline.
- Permetre el mode "Self-Play", on dues CPU poden enfrontar-se per entrenar la xarxa neuronal i millorar contínuament les seves habilitats.
- Assegurar que la incorporació d'aquesta IA sigui modular i no afecti l'experiència ni la lògica del multijugador existent.

## 3. Restriccions Tècniques
- S'ha d'utilitzar el framework oficial de **Unity ML-Agents (v1.1.0)** combinat amb un entorn virtual Python.
- L'arquitectura ha de reaprofitar el script existent `PlayerController.cs`. L'agent d'IA només pot interactuar subministrant "falses pulsacions de teclat" (inputs) a aquest controlador.
- El cicle de vida del combat (comprovar victòries, reiniciar vides, reaparèixer al mapa) no ha de paralitzar la simulació de ML-Agents durant el procés d'entrenament accelerat.
- Els canvis han de suportar els "cooldowns", les caigudes ràpides i les diferents habilitats actuals sense alterar la física pròpia de cada un d'aquests elements.
