const ResultRepository = require('./ResultRepository');
const Result = require('../models/Result');

class MongoResultRepository extends ResultRepository {
    async create(resultData) {
        const result = new Result(resultData);
        return await result.save();
    }

    async findByGameId(gameId) {
        return await Result.findOne({ gameId }).populate('winnerId losers');
    }

    async findByUserId(userId) {
        return await Result.find({ $or: [{ winnerId: userId }, { losers: userId }] });
    }
}

module.exports = MongoResultRepository;
