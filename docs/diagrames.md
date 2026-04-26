# Diagrames del Projecte

Aquest document recull els principals diagrames que defineixen l'arquitectura i el funcionament del videojoc multijugador.

## 1. Diagrama de Casos d'Ús

Aquest diagrama mostra les interaccions principals del jugador amb el sistema, incloent l'autenticació, la gestió de partides i el joc mateix.

```mermaid
usecaseDiagram
    actor Jugador as "Jugador"
    
    package "Sistema Multijugador" {
        usecase UC1 as "Identificar-se / Login"
        usecase UC2 as "Configurar paràmetres bàsics"
        usecase UC3 as "Crear nova partida"
        usecase UC4 as "Unir-se a partida existent"
        usecase UC5 as "Jugar partida (Temps real/Torns)"
        usecase UC6 as "Consultar resultats finals"
    }

    Jugador --> UC1
    Jugador --> UC2
    Jugador --> UC3
    Jugador --> UC4
    Jugador --> UC5
    Jugador --> UC6
    
    UC3 ..> UC1 : include
    UC4 ..> UC1 : include
    UC5 ..> UC3 : extends
    UC5 ..> UC4 : extends
```

## 2. Diagrama de Seqüència (Connexió, Creació de Partida i Joc amb WebSockets)

Aquest diagrama il·lustra el procés de creació i unió a una partida, equivalent al procés de "reserva" d'una sala i el desenvolupament del joc, fent ús de peticions HTTP i WebSockets/Socket.IO per a la comunicació en temps real.

```mermaid
sequenceDiagram
    participant C1 as Client 1 (Host)
    participant C2 as Client 2 (Jugador)
    participant API as API HTTP (Express)
    participant WS as Servidor WebSockets
    participant DB as Base de Dades

    %% Login i Creació
    C1->>API: POST /api/login (Credencials)
    API-->>C1: Token JWT + Dades Usuari
    
    C1->>API: POST /api/games/create (Configuració)
    API->>DB: Guarda Partida (Repository)
    DB-->>API: Partida Creada
    API-->>C1: GameID

    %% Connexió WebSocket i Unió
    C1->>WS: Connecta WS (Token)
    WS-->>C1: Connexió establerta
    C1->>WS: Emite "joinRoom" (GameID)
    
    C2->>API: POST /api/login (Credencials)
    API-->>C2: Token JWT + Dades Usuari
    
    C2->>API: GET /api/games/available
    API-->>C2: Llista de partides (inclou GameID)
    
    C2->>WS: Connecta WS (Token)
    WS-->>C2: Connexió establerta
    C2->>WS: Emite "joinRoom" (GameID)
    
    WS-->>C1: "playerJoined" (Dades C2)
    WS-->>C2: "roomState" (Estat actual)

    %% Desenvolupament de la partida
    Note over C1, WS: Comença la partida
    C1->>WS: Emite "startGame"
    WS-->>C1: "gameStarted"
    WS-->>C2: "gameStarted"
    
    C2->>WS: Emite "playerAction" (ex: moure, atacar)
    WS-->>C1: "updateState" (Nova acció de C2)
    C1->>WS: Emite "syncState" (El Host valida l'estat)
    WS-->>C2: "updateState" (Estat validat)
    
    %% Finalització
    C1->>WS: Emite "endGame" (Resultats parcials)
    WS-->>C1: "gameOver" (Puntuacions)
    WS-->>C2: "gameOver" (Puntuacions)
    
    WS->>API: Intern: Desa Resultats Finals
    API->>DB: Guarda Resultat (ResultRepository)
```

## 3. Diagrama Entitat-Relació

Estructura de les dades persistents de l'aplicació, gestionades mitjançant el patró Repository.

```mermaid
erDiagram
    USER ||--o{ GAME_PARTICIPANT : "participa en"
    GAME ||--o{ GAME_PARTICIPANT : "conté"
    GAME ||--|| RESULT : "genera"
    USER ||--o{ RESULT : "obté"

    USER {
        string id PK
        string username
        string password_hash
        datetime created_at
    }

    GAME {
        string id PK
        string host_id FK
        string status "waiting, playing, finished"
        int max_players
        datetime created_at
    }

    GAME_PARTICIPANT {
        string game_id FK
        string user_id FK
        datetime joined_at
    }

    RESULT {
        string id PK
        string game_id FK
        string winner_id FK
        int score
        int duration_seconds
    }
```

## 4. Diagrama de Microserveis

Arquitectura del backend en Node.js, destacant la separació de responsabilitats i l'ús del proxy invers.

```mermaid
graph TD
    Client[Unity Client] --> |HTTP / WebSockets| Proxy[API Gateway / Proxy Invers]
    
    Proxy --> |Rutes /api/users| AuthService[Servei d'Usuaris i Auth]
    Proxy --> |Rutes /api/games| GameService[Servei de Partides i API]
    Proxy --> |Connexió WS| WSService[Servei WebSockets en Temps Real]
    
    AuthService --> UserRepository[User Repository]
    GameService --> GameRepository[Game Repository]
    GameService --> ResultRepository[Result Repository]
    WSService -.-> |Comunica fi de partida| GameService
    
    UserRepository --> DB[(Base de Dades Compartida)]
    GameRepository --> DB
    ResultRepository --> DB
    
    subgraph Capa de Persistència (Patró Repository)
        UserRepository
        GameRepository
        ResultRepository
    end
    
    subgraph Microserveis Node.js
        AuthService
        GameService
        WSService
    end
```
