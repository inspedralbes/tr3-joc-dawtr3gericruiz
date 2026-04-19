class ResultService {
    constructor(resultRepository, gameRepository) {
        this.resultRepository = resultRepository;
        this.gameRepository = gameRepository;
    }

    async saveResult(gameId, winnerId, duration) {
        const game = await this.gameRepository.findById(gameId);
        if (!game) {
            throw new Error('Game not found');
        }

        if (game.status === 'finished') {
            throw new Error('Game already finished');
        }

        // Get losers
        const losers = game.players
            .filter(p => (p._id ? p._id.toString() : p.toString()) !== winnerId)
            .map(p => p._id || p);

        const result = await this.resultRepository.create({
            gameId,
            winnerId,
            losers,
            duration
        });

        await this.gameRepository.updateStatus(gameId, 'finished');

        return result;
    }

    async getResultsByUser(userId) {
        return await this.resultRepository.findByUserId(userId);
    }
}

module.exports = ResultService;
