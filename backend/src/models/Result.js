const mongoose = require('mongoose');

const ResultSchema = new mongoose.Schema({
    gameId: { type: String, ref: 'Game', required: true },
    winnerId: { type: mongoose.Schema.Types.ObjectId, ref: 'User' },
    losers: [{ type: mongoose.Schema.Types.ObjectId, ref: 'User' }],
    duration: { type: Number, default: 0 }, 
    createdAt: { type: Date, default: Date.now }
});

module.exports = mongoose.model('Result', ResultSchema);
