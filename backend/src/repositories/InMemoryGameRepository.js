const GameRepository = require('./GameRepository');

class InMemoryGameRepository extends GameRepository {
    constructor() {
        super();
        this.games = [];
        this.currentId = 1;
    }

    async create(gameData) {
        const game = { _id: this.currentId.toString(), ...gameData, players: gameData.players || [], status: 'waiting', createdAt: new Date() };
        this.games.push(game);
        this.currentId++;
        return game;
    }

    async findById(id) {
        return this.games.find(g => g._id === id) || null;
    }

    async updateStatus(id, status) {
        const game = this.games.find(g => g._id === id);
        if (game) {
            game.status = status;
        }
        return game || null;
    }

    async addPlayer(gameId, playerId) {
        const game = this.games.find(g => g._id === gameId);
        if (game && !game.players.includes(playerId)) {
            game.players.push(playerId);
        }
        return game || null;
    }
}

module.exports = InMemoryGameRepository;
