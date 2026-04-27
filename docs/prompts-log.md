# Registre de Prompts i Traçabilitat (Prompts Log)

Aquest document recull la traçabilitat cronològica de totes les sol·licituds (prompts), els errors que s'han detectat durant l'execució i com la IA els ha corregit mitjançant refinament de codi, seguint l'enfocament SDD de desenvolupament guiat per especificacions.

## 1. Generació de l'Especificació (OpenSpec)
**Prompt 1 (Context):**
> "Tinc part d'una guia per implementar ml-agents al meu projecte Unity. El meu projecte consisteix en un joc 2D de lluita estil Smash Bros amb 2 personatges (Gojo i Sukuna). Necessito adaptar aquesta guia a les meves necessitats. Vull que l'agent actui com a CPU controlant qualsevol dels personatges. Genera les fundacions i l'especificació bàsica segons OpenSpec."

**Prompt 2 (Comportament i condicions inicials):**
> "Quan esculls un personatge al menú, després has de poder escollir-ne un altre per a la CPU (fent que no sempre siguin els mateixos personatges en 1v1). També, vull que quan tiris al buit a la CPU, aquesta perdi vides i respawnegi bé. I vull assegurar-me que no hi hagi problemes amb els atacs i els cops."

**Prompt 3 (Pla d'acció de la IA):**
> "M'agradaria que generis el codi inicial de l'script FighterAgent per a què l'agent pugui enviar inputs al PlayerController, detectar quins jugadors són CPU i quins no, i configurar els mètodes relacionats amb ML-Agents."

---

## 2. Implementació i Refinament (Iteracions i Correcció d'Errors)

### Iteració 1: Implementació inicial i Problema d'Inputs
- **Error detectat:** Després d'aplicar el codi i fer funcionar les regles de xoc i respawn locals, l'usuari s'adona que el Jugador 1 (Gojo, controlat per l'Agent) no fa absolutament res, mentre que el Jugador 2 (Sukuna, Agent) sí que ataca.
- **Prompt:** *"Parece que gojo nunca ataca y sukuna es el unico que ataca (gojo es el player 1 (el que se supone que seria yo) y sukuna el player 2)"*
- **Anàlisi del problema:** Es va descobrir que el mètode `RecogerInput()` del `PlayerController` estava sobreescrivint a 0 els inputs enviats pel `FighterAgent` al Jugador 1 contínuament, donat que detectava Gojo internament com a "Jugador Local".
- **Solució / Canvi en codi:** S'afegeix la guarda a l'inici de l'extracció d'inputs humans:
  ```csharp
  FighterAgent agent = GetComponent<FighterAgent>();
  if (agent != null && agent.enabled) return;
  ```

### Iteració 2: Problema amb el Reseteig d'Entrenament (EndEpisode)
- **Error detectat:** L'agent era incapaç de restablir l'episodi i reaparèixer autònomament quan queia pel precipici amb 0 vides. Això bloquejava el flux general d'entrenament constant.
- **Prompt:** *"Les he puesto a entrenar y cuando se quedan sin vidas, no respawnean."*
- **Anàlisi del problema:** El `PlayerController` feia una crida a `gameObject.SetActive(false)` quan el recompte de vides arribava a 0, desactivant completament el component de IA i impedint l'execució de la funció `EndEpisode()` lligada al reseteig.
- **Solució / Canvi en codi:** S'altera l'estat final de mort local, protegint la desactivació sota la comprovació del flag de mode Entrenament.

### Iteració 3: Errors d'Entorn i Python (Timeouts)
- **Error detectat:** La consola de Python llençava repetidament l'error d'entorn `AttributeError: 'StrictVersion' object has no attribute 'version'` i Unity es congelava sobre el pas de formació número 20.000.
- **Prompt:** *"Parece que se me ha petado unity. [WARNING] Restarting worker[0] after 'The Unity environment took too long to respond..."*
- **Anàlisi del problema:** Aquest error no procedia del C# escrit sinó d'un canvi de focus asíncron al sistema operatiu, que causava la suspensió del procés d'Unity i posteriorment un Timeout en l'script de reconnexió de ML-Agents 1.1.0 en intentar reiniciar-lo automàticament.
- **Solució / Canvi en l'Arquitectura:** Es va instruir l'ús i marcació del setting `Run In Background` dins del tabulador *Player - Resolution and Presentation* de Unity per bloquejar qualsevol desconnexió inactiva, reprenent la formació en on es va pausar a l'escriptura usant l'argument CLI `--resume`.

### Iteració 4: Error de Coherència en Noms del Behavior
- **Error detectat:** A l'arrencar el terminal Python es tancava bruscament per falta de fitxers al JSON i als meta-configuradors interns indicant `The behavior name My Behavior has not been specified in the trainer configuration.`
- **Prompt:** *"Me ha dado estos errores de comportamiento... The behavior name My Behavior has not been specified..."*
- **Anàlisi del problema:** El document d'especificació previ descrivia un YAML que definia la IA com a `SimpleBehavior`, no obstant això per inèrcia nativa, el component de Unity ML-Agents Inspector autocompleta el nom a `My Behavior` de manera predeterminada.
- **Solució / Canvi en codi:** Edició remota asíncrona del fitxer `fighter.yaml` on s'altera `SimpleBehavior:` per `"My Behavior":` permetent concordança exacta de les *strings* sense exigir a l'estudiant tornar a editar valors a l'inspector visual d'Unity.
