const UserRepository = require('./UserRepository');

class InMemoryUserRepository extends UserRepository {
    constructor() {
        super();
        this.users = [];
        this.currentId = 1;
    }

    async create(userData) {
        const user = { _id: this.currentId.toString(), ...userData, createdAt: new Date() };
        this.users.push(user);
        this.currentId++;
        return user;
    }

    async findById(id) {
        return this.users.find(u => u._id === id) || null;
    }

    async findByUsername(username) {
        return this.users.find(u => u.username === username) || null;
    }
}

module.exports = InMemoryUserRepository;
