class GameService {
    constructor(gameRepository, userRepository) {
        this.gameRepository = gameRepository;
        this.userRepository = userRepository;
    }

    async createGame(playerId, maxPlayers = 2) {
        const user = await this.userRepository.findById(playerId);
        if (!user) {
            throw new Error('User not found');
        }

        const game = await this.gameRepository.create({
            players: [playerId],
            maxPlayers,
            status: 'waiting'
        });

        return game;
    }

    async joinGame(gameId, playerId) {
        const game = await this.gameRepository.findById(gameId);
        if (!game) {
            throw new Error('Game not found');
        }

        if (game.status !== 'waiting') {
            throw new Error('Game is not waiting for players');
        }

        if (game.players.some(p => p._id.toString() === playerId || p.toString() === playerId)) {
            throw new Error('Player already in game');
        }

        if (game.players.length >= game.maxPlayers) {
            throw new Error('Game is full');
        }

        const updatedGame = await this.gameRepository.addPlayer(gameId, playerId);
        
        if (updatedGame.players.length >= updatedGame.maxPlayers) {
            await this.gameRepository.updateStatus(gameId, 'playing');
        }

        return updatedGame;
    }
    
    async getGame(gameId) {
        return await this.gameRepository.findById(gameId);
    }
}

module.exports = GameService;
