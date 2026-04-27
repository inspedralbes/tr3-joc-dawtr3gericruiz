# Documentació del Patró Repository

Aquest document detalla l'arquitectura d'accés a dades utilitzada en el backend del projecte, basada en el patró de disseny **Repository**, segons els requeriments demanats.

## 1. Separació Repository / Service / Controller

Per garantir un codi net, escalable i fàcil de testejar, s'ha implementat una separació estricta de responsabilitats en tres capes principals:

### Controller (Capa de Presentació / Rutes HTTP)
Els Controladors són els responsables exclusius de gestionar les peticions i respostes HTTP (o esdeveniments WebSocket).
- **Responsabilitats:** Rebre la petició (Request), extreure'n els paràmetres i el cos (Body), cridar al Servei corresponent i retornar la resposta HTTP adequada (Status Codes com 200, 201, 400, 404, etc.) juntament amb les dades en format JSON.
- **Restriccions:** No contenen regles de negoci ni lògica de base de dades.

### Service (Capa de Lògica de Negoci)
Els Serveis centralitzen totes les regles i el funcionament de l'aplicació.
- **Responsabilitats:** Validar l'estat del joc, calcular puntuacions, aplicar les regles de negoci de creació i unió de partides, i orquestrar les operacions necessàries.
- **Relació amb el Repository:** El Service utilitza instàncies del Repository a través d'una interfície comuna (o classes base), sense importar-li quina base de dades s'està utilitzant per sota.

### Repository (Capa d'Accés a Dades)
Els Repositoris aïllen completament l'aplicació de la infraestructura de la base de dades.
- **Responsabilitats:** Són l'únic lloc on s'executen consultes SQL, comandes MongoDB o operacions en memòria. Realitzen les operacions CRUD (Create, Read, Update, Delete) i mapegen les dades de la base de dades a objectes o entitats del domini.
- **Beneficis:** Si en el futur cal canviar de MySQL a PostgreSQL, o de MongoDB a MariaDB, només caldrà reescriure la capa del Repository. La lògica de negoci (Service) i l'API (Controller) restaran inalterades.

---

## 2. Implementacions del Patró Repository

D'acord amb els requisits tècnics, s'han dissenyat com a mínim dues implementacions per a cadascun dels Repositoris principals (User, Game, Result): la implementació per a entorns de Producció/Desenvolupament i la implementació per a Testing (InMemory).

### 2.1. Implementació amb Base de Dades Real (ex. SQL / MongoDB)
**Justificació:** És la implementació principal que es connecta al motor de base de dades real, assegurant la persistència persistent a llarg termini, garantint que si el servidor es reinicia no es perden els usuaris, les partides o els resultats històrics.
Aquesta implementació fa ús de paquets com `mysql2`, `pg` o `mongoose` per executar la persistència física. Totes les contrasenyes s'emmagatzemen aplicant tècniques de *hashing* per garantir la seguretat de l'usuari.

### 2.2. Implementació InMemory (per a Testing)
**Justificació:** Durant la fase de desenvolupament, i especialment per realitzar els **Tests Unitaris**, dependre d'una base de dades real ralenteix els tests, requereix configuracions complexes d'entorn i pot causar col·lisions de dades entre tests executats en paral·lel.
Per solucionar-ho, s'ha creat una implementació `InMemory` que guarda les dades en estructures de dades del mateix node (per exemple, `Arrays` o `Maps` en memòria). 
- **Avantatges:** Permet executar els tests de manera aïllada i extremadament ràpida. Verifica que el Servei (lògica de negoci) funciona correctament assumint que la persistència respon com caldria.

### 2.3 Exemple d'ús de la interfície

Com que treballem amb Node.js, s'implementa utilitzant Classes o funcions que respecten un mateix contracte:

```javascript
// Interfície lògica que han de complir els repositoris
class IUserRepository {
  async findById(id) { throw new Error("Not implemented"); }
  async create(user) { throw new Error("Not implemented"); }
}

// 1. Implementació DB Real
class UserMySQLRepository extends IUserRepository {
  async create(user) { /* INSERT INTO users ... */ }
}

// 2. Implementació InMemory
class UserInMemoryRepository extends IUserRepository {
  constructor() { super(); this.users = []; }
  async create(user) { this.users.push(user); return user; }
}
```

D'aquesta manera, quan instanciem el Service, li injectem el repositori que vulguem segons l'entorn (`process.env.NODE_ENV === 'test' ? new UserInMemoryRepository() : new UserMySQLRepository()`). Aquesta tècnica d'**Injecció de Dependències** afavoreix el principi d'inversió de dependències de SOLID.
