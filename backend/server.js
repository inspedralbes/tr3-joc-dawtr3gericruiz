const express = require('express');
const http = require('http');
const WebSocket = require('ws');
const cors = require('cors');
const mongoose = require('mongoose');
require('dotenv').config();

const createContainer = require('./src/container');
const setupRoutes = require('./src/routes/api');

const app = express();
app.use(cors());
app.use(express.json());


const MONGODB_URI = process.env.MONGODB_URI || 'mongodb://localhost:27017/smashbros';
mongoose.connect(MONGODB_URI)
    .then(() => console.log('Connectat a MongoDB'))
    .catch(err => console.error('Error connectant a MongoDB:', err));


const container = createContainer();
const apiRoutes = setupRoutes(container.userController, container.gameController, container.resultController);
app.use('/api', apiRoutes);

app.get('/api/estat', (req, res) => {
    res.json({ missatge: "El servidor de la API esta actiu i funcionant." });
});


const server = http.createServer(app);
const wss = new WebSocket.Server({ server });


const rooms = new Map();

wss.on('connection', (ws) => {
    console.log('Un nou jugador s\'ha connectat al WebSocket.');
    let currentRoom = null;

    ws.on('message', (message) => {
        const text = message.toString();
        
        try {
            const data = JSON.parse(text);
            
            
            if (data.tipo === 'join_room') {
                const gameId = data.gameId;
                if (!gameId) return;

                
                if (!rooms.has(gameId)) {
                    rooms.set(gameId, new Set());
                }

                rooms.get(gameId).add(ws);
                currentRoom = gameId;
                console.log(`Jugador unit a la sala: ${gameId}`);
                return; 
            }
        } catch (e) {
            
        }

        
        if (currentRoom && rooms.has(currentRoom)) {
            const roomClients = rooms.get(currentRoom);
            roomClients.forEach(function each(client) {
                if (client !== ws && client.readyState === WebSocket.OPEN) {
                    client.send(text);
                }
            });
        }
    });

    ws.on('close', () => {
        console.log('Un jugador s\'ha desconnectat.');
        if (currentRoom && rooms.has(currentRoom)) {
            const roomClients = rooms.get(currentRoom);
            roomClients.delete(ws);
            if (roomClients.size === 0) {
                rooms.delete(currentRoom); 
                console.log(`Sala ${currentRoom} eliminada per inActivitat.`);
            }
        }
    });
});

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
    console.log(`Servidor HTTP i WebSockets corrent al port ${PORT}`);
});