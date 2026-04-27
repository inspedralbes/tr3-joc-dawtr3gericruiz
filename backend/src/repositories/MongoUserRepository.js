const UserRepository = require('./UserRepository');
const User = require('../models/User');

class MongoUserRepository extends UserRepository {
    async create(userData) {
        const user = new User(userData);
        return await user.save();
    }

    async findById(id) {
        return await User.findById(id);
    }

    async findByUsername(username) {
        return await User.findOne({ username });
    }
}

module.exports = MongoUserRepository;
