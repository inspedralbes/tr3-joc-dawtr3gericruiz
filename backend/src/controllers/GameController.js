class GameController {
    constructor(gameService) {
        this.gameService = gameService;
    }

    async createGame(req, res) {
        try {
            const { maxPlayers } = req.body;
            
            const playerId = req.user ? req.user.id : req.body.playerId; 
            
            if (!playerId) return res.status(400).json({ error: 'Player ID required' });

            const game = await this.gameService.createGame(playerId, maxPlayers);
            res.status(201).json({ message: 'Game created', game });
        } catch (error) {
            res.status(400).json({ error: error.message });
        }
    }

    async joinGame(req, res) {
        try {
            const { gameId } = req.params;
            const playerId = req.user ? req.user.id : req.body.playerId;

            if (!playerId) return res.status(400).json({ error: 'Player ID required' });

            const game = await this.gameService.joinGame(gameId, playerId);
            res.status(200).json({ message: 'Joined game', game });
        } catch (error) {
            res.status(400).json({ error: error.message });
        }
    }
}

module.exports = GameController;
