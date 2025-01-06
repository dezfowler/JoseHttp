import http from 'node:http';
import fetch from 'node-fetch';
import express from 'express';
import { createTerminus } from '@godaddy/terminus';
import bodyParser from 'body-parser'; 
import jose from 'node-jose';
import { Buffer } from 'node:buffer';

// Read configuration from environment variables
const config = {
    environment: process.env.NODE_ENV || 'development',
    httpPort: process.env['PORT'] ?? 8080
};

console.log(`config: ${JSON.stringify(config)}`);

// Setup express app
const app = express();

app.use(bodyParser.json());

app.post('/client/sig', async (req, res) => {
    try {
        const clientReq = req.body;
        console.log(`request content: ${JSON.stringify(clientReq)}`);
        const signingKey = await jose.JWK.asKey(JSON.parse(clientReq.signingKey));
        console.log(`signingKey: ${JSON.stringify(signingKey)}`);
        const jws = await jose.JWS.createSign({ format: 'compact', fields: { alg: clientReq.algo } }, signingKey) 
                                  .update(Buffer.from(clientReq.payload), 'utf8')
                                  .final();
    
        const clientResponse = await fetch(clientReq.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/jose' },
            body: jws });
                 
        res.send(await clientResponse.arrayBuffer()); 
    }
    catch (error) { 
        res.status(500).send(`Error signing the content: ${error.message}`);
    }    
});

// Start a server
function startServer(server, port) {
    if (server) {
        const serverType = 'HTTP';

        // Create the health check endpoint
        createTerminus(server, {
            signal: 'SIGINT',
            healthChecks: {
                '/health': () => { },
                '/alive': () => { }
            },
            onSignal: async () => {
                console.log('server is starting cleanup');
                console.log('closing Redis connection');
                await cache.disconnect();
            },
            onShutdown: () => console.log('cleanup finished, server is shutting down')
        });

        // Start the server
        server.listen(port, () => {
            console.log(`${serverType} listening on ${JSON.stringify(server.address())}`);
        });
    }
}

const httpServer = http.createServer(app);
startServer(httpServer, config.httpPort);
