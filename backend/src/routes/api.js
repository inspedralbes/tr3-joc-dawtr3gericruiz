const express = require('express');

function setupRoutes(userController, gameController, resultController) {
    const router = express.Router();

    
    router.post('/users/register', (req, res) => userController.register(req, res));
    router.post('/users/login', (req, res) => userController.login(req, res));

    
    router.post('/games', (req, res) => gameController.createGame(req, res));
    router.post('/games/:gameId/join', (req, res) => gameController.joinGame(req, res));

    
    router.post('/results', (req, res) => resultController.saveResult(req, res));

    return router;
}

module.exports = setupRoutes;
