const express = require('express');
const http = require('http');
const WebSocket = require('ws');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

app.get('/api/estat', (req, res) => {
    res.json({ missatge: "El servidor de la API esta actiu i funcionant." });
});

wss.on('connection', (ws) => {
    console.log('Un nou jugador s\'ha connectat al WebSocket.');

    ws.on('message', (message) => {
        const text = message.toString();
        wss.clients.forEach(function each(client) {
            if (client !== ws && client.readyState === WebSocket.OPEN) {
                client.send(text);
            }
        });
    });

    ws.on('close', () => {
        console.log('Un jugador s\'ha desconnectat.');
    });
});

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
    console.log(`Servidor HTTP i WebSockets corrent al port ${PORT}`);
});