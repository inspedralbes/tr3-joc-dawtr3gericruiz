const express = require('express');

function setupRoutes(userController, gameController, resultController) {
    const router = express.Router();

    // User Routes
    router.post('/users/register', (req, res) => userController.register(req, res));
    router.post('/users/login', (req, res) => userController.login(req, res));

    // Game Routes
    router.post('/games', (req, res) => gameController.createGame(req, res));
    router.post('/games/:gameId/join', (req, res) => gameController.joinGame(req, res));

    // Result Routes
    router.post('/results', (req, res) => resultController.saveResult(req, res));

    return router;
}

module.exports = setupRoutes;
