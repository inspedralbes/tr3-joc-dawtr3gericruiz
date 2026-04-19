const mongoose = require('mongoose');

const GameSchema = new mongoose.Schema({
    _id: { type: String },
    status: { type: String, enum: ['waiting', 'playing', 'finished'], default: 'waiting' },
    players: [{ type: mongoose.Schema.Types.ObjectId, ref: 'User' }],
    maxPlayers: { type: Number, default: 2 },
    createdAt: { type: Date, default: Date.now }
});

module.exports = mongoose.model('Game', GameSchema);
