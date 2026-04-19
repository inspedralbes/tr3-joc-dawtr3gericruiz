// Dependency Injection Container
const MongoUserRepository = require('./repositories/MongoUserRepository');
const MongoGameRepository = require('./repositories/MongoGameRepository');
const MongoResultRepository = require('./repositories/MongoResultRepository');

const UserService = require('./services/UserService');
const GameService = require('./services/GameService');
const ResultService = require('./services/ResultService');

const UserController = require('./controllers/UserController');
const GameController = require('./controllers/GameController');
const ResultController = require('./controllers/ResultController');

function createContainer() {
    // 1. Instantiate Repositories
    const userRepository = new MongoUserRepository();
    const gameRepository = new MongoGameRepository();
    const resultRepository = new MongoResultRepository();

    // 2. Instantiate Services
    const userService = new UserService(userRepository);
    const gameService = new GameService(gameRepository, userRepository);
    const resultService = new ResultService(resultRepository, gameRepository);

    // 3. Instantiate Controllers
    const userController = new UserController(userService);
    const gameController = new GameController(gameService);
    const resultController = new ResultController(resultService);

    return {
        userController,
        gameController,
        resultController
    };
}

module.exports = createContainer;
