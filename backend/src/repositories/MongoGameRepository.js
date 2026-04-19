const GameRepository = require('./GameRepository');
const Game = require('../models/Game');

class MongoGameRepository extends GameRepository {
    async create(gameData) {
        const game = new Game(gameData);
        return await game.save();
    }

    async findById(id) {
        return await Game.findById(id).populate('players');
    }

    async updateStatus(id, status) {
        return await Game.findByIdAndUpdate(id, { status }, { new: true });
    }

    async addPlayer(gameId, playerId) {
        return await Game.findByIdAndUpdate(
            gameId,
            { $addToSet: { players: playerId } },
            { new: true }
        );
    }
}

module.exports = MongoGameRepository;
