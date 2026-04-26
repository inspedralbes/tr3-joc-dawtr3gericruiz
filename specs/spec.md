# Spec: Comportament de l'Agent d'IA

## 1. Abstracció de l'Agent (FighterAgent)
El nucli de la funcionalitat s'encapsularà en una classe anomenada `FighterAgent` que heretarà de la classe bàsica `Agent` proporcionada pel paquet ML-Agents.

### 1.1 Sensors (Observacions)
L'agent recollirà de manera contínua la següent informació sobre l'estat del joc (14 valors per decisió):
- **Estat Propi:** Posició X/Y (absoluta al mapa), velocitat lineal 2D, vides restants, i dany percentual acumulat.
- **Estat del Rival:** Posició X/Y de l'enemic, velocitat lineal 2D, vides restants i dany percentual acumulat.

### 1.2 Actuadors (Accions Discretes)
L'agent emetrà ordres a través de 3 branques d'acció de la següent manera:
- **Branca 0 (Moviment Horitzontal):** 0 = Aturat, 1 = Moure a l'Esquerra, 2 = Moure a la Dreta.
- **Branca 1 (Moviment Vertical):** 0 = Res, 1 = Salt, 2 = Caiguda Ràpida (Fast Fall).
- **Branca 2 (Combate):** 0 = Cap, 1 = Jab, 2 = Inici Smash (K avall), 3 = Llançar Smash (K a dalt), 4 = Projectil (L), 5 = Recovery (Salt + L).

### 1.3 Sistema de Recompenses i Càstigs (Reinforcement Learning)
S'atorgaran senyals constants per entrenar la IA sota una política PPO:
- **Càstig Crític (-1.0f):** Sempre que les vides del propi agent baixin, considerant que ha estat un KO per caure de la pista.
- **Recompensa Crítica (+1.0f):** Sempre que s'elimini una vida a l'enemic.
- **Micro-càstig (-):** Quan augmenti el percentatge de dany propi, s'aplicarà una fracció negativa del dany per incentivar la fugida i la defensa.
- **Micro-recompensa (+):** Quan s'augmenti el dany del rival.

## 2. Modes d'Execució i Arquitectura Global
L'entorn permetrà un Mode Entrenament activable pel desenvolupador a través d'un booleà general.

**Si el Mode Entrenament està Activat:**
- Es força el control dual sobre els dos jugadors mitjançant IA (Habilitació del *Self-Play*).
- Quan les vides d'un jugador arriben a zero, s'ignora la interfície de Victòria, els menús i la pausa del temps (`Time.timeScale`). Es reseteja instantàniament l'Estat Complet del combat per al bucle ininterromput mitjançant `OnEpisodeBegin()`.

**Si el Mode Entrenament està Desactivat (Joc normal):**
- El Jugador 1 serà obligatòriament controlat pel teclat de l'humà i la seva instància d'Agent estarà desactivada.
- El Jugador 2 rebrà accions d'un model entrenat prèviament (`.onnx`) en mode "Inference Only", oferint una experiència de desafiament típica de jocs comercials.
