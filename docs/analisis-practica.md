# Anàlisi de la Pràctica - IA amb ML-Agents (SDD)

## 1. Explicació de la funcionalitat
S'ha desenvolupat la integració d'una CPU autònoma per a un joc de lluita 2D existent (tipus Smash Bros) utilitzant el framework Unity ML-Agents. Aquesta funcionalitat dota el joc de la capacitat d'entrenar personatges per jugar mitjançant Reinforcement Learning (Self-Play). S'ha dissenyat un sistema híbrid i respectuós amb el codi *legacy* que permet alternar de forma nativa entre "Mode Joc" (on el jugador humà s'enfronta a la IA en mode inferència pura) i "Mode Entrenament" (on dos agents IA s'enfronten a velocitat accelerada en un bucle tancat i infinit per generar aprenentatge seqüencial continu).

## 2. Procés seguit amb la IA (Spec-Driven Development)
S'ha seguit la metodologia imposada per OpenSpec de manera lineal però amb retroalimentació constant. Primer es va establir la base de context global a l'arxiu `foundations.md` fixant l'existència de restriccions fermes referents a codi preexistent, tals com sistemes de hitboxes acoblats o controls hard-coded en classe única. Un cop analitzat l'ecosistema, el comportament teòric del component `FighterAgent` es va traslladar a l'arxiu de descripcions abstractes `spec.md`. Finalment, un darrer fitxer estratègic denominat `plan.md` va dividir aquesta execució global complexa cap a passos accionables d'infiltració asíncrona, codificant i aplicant hiperparàmetres concrets en un model i iterant a posteriori.

## 3. Principals problemes trobats i decisions preses
Durant la realització de la integració han florit alguns inconvenients estructurals no previstos directament:
- **Sobreescriptura Perifèrica d'Inputs:** El motor local continuava dictant que Gojo com a *Player 1* devia obtenir els inputs globals de tecles per defecte (que reportaven un valor 0 constant si no es premia el teclat). La decisió imposada va consistir a trencar el disseny asíncron, fixant sentències exclusores condicionalitzades on, si un controlador posseïa l'etiqueta funcional `FighterAgent.enabled`, la funció de `RecogerInput()` cancel·lava la seva execució mitjançant un retorn directe (return), permetent que la Xarxa Neuronal prengués tot el pes de l'entrada.
- **Ruptura del bucle pel Destroy Visual:** Quan una classe abstracta local informava d'una mort als mètodes (`PerderVida()`), Unity desactiva íntegrament l'objecte pare via `SetActive(false)`. La conseqüència d'això es manifestava com la mort de la mateixa IA. Per solucionar aquesta anomalia crònica, el plan es va subvertir modificant el GameManager i el PlayerController perquè reconeguessin d'entrada el flag global `modoEntrenamiento`, que neutralitza la instrucció *SetActive*, oferint oxigen suficient a l'agent com perquè ell mateix invoqui la restauració de l'episodi virtual (`EndEpisode();`).
- **Pèrdues d'Execució d'entorn i TimeOuts:** Ocasionalment el CLI patia crashes i desconnexions bruscades per bugs nadius (`StrictVersion`) derivats de pauses internes per minimització del client. La decisió presa pel seu arranjament era forçar opcions internes del motor (`Run In Background`).

## 4. Valoració crítica real
**L’agent ha seguit realment la especificació?**
En trets generals l'agent d'IA programador (model iterador) va respectar estrictament els contorns descrits en l'especificació d'abstraccions per a l'entrada del `FighterAgent`, així com els mapes de configuracions `yaml`. No obstant això, certs factors col·laterals inherents a l'arquitectura de l'usuari van quedar obviats al moment de planificació.

**Quantes iteracions han estat necessàries?**
Van ser requerides aproximadament un marge d'entre 4 a 5 interaccions d'escriptura per acabar obtenint un comportament ferm i pur en execució ininterrompuda.

**On falla més la IA (interpretació, execució, coherència)?**
On ha mostrat més debilitat l'aplicatiu autònom de la IA és en mantenir **coherència global amb la totalitat de l'arquitectura font**, sovint perdent de vista la imatge àmplia, tals com assumir configuracions per defecte o pressuposar de qui seria propietat l'execució en cadascun dels mètodes subjacents. Si bé l'agent es desenvolupava exquisidament i de manera excel·lent generant seccions lògiques singulars (com ara l'arquitectura pura d'algorisme `CollectObservations()`), els xocs amb mecanismes estructurats (tals com `gameObject.SetActive()`) no hi varen formar part de cap previsió i es van haver de solucionar amb pegats successius post-implementació en l'estructura del codi general.

**Has hagut de modificar la especificació o només els prompts?**
Principalment l'eina utilitzada ha sigut introduir alteracions sota el mateix mètode empíric mitjançant prompts de redireccionament puntual (modificar comportaments d'arxius preexistents on abans no s'havia previst). L'especificació fundacional va mantenir correctament l'estat pur del teoricisme (IA en bucle), el que va patir veritables modificacions van ésser les injeccions locals necessàries mitjançant les redireccions d'error del prompt en viu.
