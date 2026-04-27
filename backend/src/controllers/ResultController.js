class ResultController {
    constructor(resultService) {
        this.resultService = resultService;
    }

    async saveResult(req, res) {
        try {
            const { gameId, winnerId, duration } = req.body;
            const result = await this.resultService.saveResult(gameId, winnerId, duration);
            res.status(201).json({ message: 'Result saved', result });
        } catch (error) {
            res.status(400).json({ error: error.message });
        }
    }
}

module.exports = ResultController;
