# Manual d'Instal·lació i Configuració

Aquest document descriu els passos necessaris per instal·lar, configurar i executar el projecte multijugador (Backend en Node.js i Frontend en Unity).

## 1. Requisits Previs

Abans de començar, assegura't de tenir instal·lat el següent programari:

- **Node.js** (versió 18 o superior) i **npm** (gestor de paquets).
- **Unity Hub** i **Unity Editor** (versió 2022.3 LTS o superior recomanada).
- Una base de dades compatible per a la persistència (ex. **MySQL**, **PostgreSQL** o **MongoDB**). Aquest projecte inclou implementacions per a la persistència.
- **Git** per a clonar el repositori.

## 2. Clonació del Repositori

Primer, clona el repositori del projecte al teu ordinador:

```bash
git clone <URL_DEL_REPOSITORI>
cd tr3-joc-dawtr3gericruiz
```

## 3. Configuració del Backend (Node.js)

El backend utilitza una arquitectura de microserveis i està construït amb Express i WebSockets.

### 3.1. Instal·lació de dependències

Navega a la carpeta del backend i instal·la les dependències necessàries:

```bash
cd backend
npm install
```

### 3.2. Configuració de l'entorn

Crea un fitxer `.env` a l'arrel de la carpeta `backend`. Pots utilitzar un fitxer `.env.example` com a plantilla si existeix. Afegeix-hi les variables d'entorn necessàries, per exemple:

```env
PORT=3000
WS_PORT=3001
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=admin
DB_NAME=joc_multijugador
JWT_SECRET=la_teva_clau_secreta_super_segura
```

### 3.3. Base de Dades

1. Crea la base de dades al teu gestor (ex. MySQL).
2. Executa l'script SQL inicial (si fas servir SQL) per crear les taules necessàries. Aquest script es troba a `backend/scripts/init.sql` (o similar).
3. Si utilitzes la implementació *InMemory* per a proves, no cal configurar cap base de dades externa.

### 3.4. Execució del Servidor

Per iniciar el servidor en entorn de desenvolupament:

```bash
npm run dev
```

O en mode producció:

```bash
npm start
```

El servidor de l'API HTTP i el servidor de WebSockets estaran actius en els ports configurats.

## 4. Configuració del Frontend (Unity)

### 4.1. Obrir el projecte

1. Obre **Unity Hub**.
2. Fes clic a **Open** i selecciona la carpeta `unity-client` del repositori clonat.
3. Espera que Unity importi tots els *assets* i configuri el projecte.

### 4.2. Configuració de la connexió

Per defecte, el client de Unity s'ha de connectar al backend local. Cal revisar els scripts o objectes de configuració (ex. `NetworkManager`, `GameManager` o constants de connexió) per assegurar que apunten a les adreces correctes:

- **API HTTP URL:** `http://localhost:3000/api`
- **WebSocket URL:** `ws://localhost:3001`

*(Nota: Si el backend està en un servidor extern, cal modificar aquestes URLs a les de l'entorn de producció).*

### 4.3. Execució i Compilació

- **Desenvolupament:** Prem el botó **Play** a l'editor de Unity per provar el joc. Pots obrir diverses instàncies o utilitzar el *ParrelSync* per provar el multijugador localment.
- **Compilació (Build):** Ve a `File > Build Settings`. Selecciona la plataforma objectiu (PC, WebGL, etc.) i fes clic a **Build**. Això generarà els fitxers executables del joc.

## 5. Resolució de Problemes Comuns

- **Error de connexió a la BD:** Verifica que el servei de la base de dades està actiu i que les credencials de l'arxiu `.env` són correctes.
- **El client Unity no es connecta al servidor:** Assegura't que el backend està executant-se abans d'iniciar el client de Unity i comprova que no hi ha cap tallafocs bloquejant els ports 3000 o 3001.
- **Errors de WebSockets:** Obre la consola del navegador (si és WebGL) o la consola de Unity per revisar si hi ha errors de xarxa o CORS.
