const ResultRepository = require('./ResultRepository');

class InMemoryResultRepository extends ResultRepository {
    constructor() {
        super();
        this.results = [];
        this.currentId = 1;
    }

    async create(resultData) {
        const result = { _id: this.currentId.toString(), ...resultData, createdAt: new Date() };
        this.results.push(result);
        this.currentId++;
        return result;
    }

    async findByGameId(gameId) {
        return this.results.find(r => r.gameId === gameId) || null;
    }

    async findByUserId(userId) {
        return this.results.filter(r => r.winnerId === userId || (r.losers && r.losers.includes(userId)));
    }
}

module.exports = InMemoryResultRepository;
