const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');

class UserService {
    constructor(userRepository) {
        this.userRepository = userRepository;
    }

    async register(username, password) {
        if (!username || !password) {
            throw new Error('Username and password are required');
        }
        
        const existingUser = await this.userRepository.findByUsername(username);
        if (existingUser) {
            throw new Error('Username already exists');
        }

        const hashedPassword = await bcrypt.hash(password, 10);
        const user = await this.userRepository.create({ username, password: hashedPassword });
        
        return {
            id: user._id,
            username: user.username
        };
    }

    async login(username, password) {
        const user = await this.userRepository.findByUsername(username);
        if (!user) {
            throw new Error('Invalid credentials');
        }

        const isValid = await bcrypt.compare(password, user.password);
        if (!isValid) {
            throw new Error('Invalid credentials');
        }

        const token = jwt.sign({ id: user._id, username: user.username }, process.env.JWT_SECRET || 'supersecret', { expiresIn: '24h' });
        
        return {
            token,
            user: { id: user._id, username: user.username }
        };
    }
}

module.exports = UserService;
