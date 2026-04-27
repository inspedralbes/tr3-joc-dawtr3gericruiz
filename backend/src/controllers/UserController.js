class UserController {
    constructor(userService) {
        this.userService = userService;
    }

    async register(req, res) {
        try {
            const { username, password } = req.body;
            const user = await this.userService.register(username, password);
            res.status(201).json({ message: 'User registered successfully', user });
        } catch (error) {
            res.status(400).json({ error: error.message });
        }
    }

    async login(req, res) {
        try {
            const { username, password } = req.body;
            const data = await this.userService.login(username, password);
            res.status(200).json(data);
        } catch (error) {
            res.status(401).json({ error: error.message });
        }
    }
}

module.exports = UserController;
