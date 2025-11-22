import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

export default defineConfig(({ command }) => {
    // Initialize the HTTPS config as undefined by default (for production builds)
    let httpsConfig = undefined;

    // Only run certificate logic if we are in Development mode ('serve')
    if (command === 'serve') {
        const baseFolder =
            env.APPDATA !== undefined && env.APPDATA !== ''
                ? `${env.APPDATA}/ASP.NET/https`
                : `${env.HOME}/.aspnet/https`;

        const certificateName = "quippercodecompiler.client";
        const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
        const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

        // Ensure the folder exists
        if (!fs.existsSync(baseFolder)) {
            fs.mkdirSync(baseFolder, { recursive: true });
        }

        // Generate certs if missing (Requires .NET SDK, so only run locally)
        if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
            // We wrap this in a try-catch just in case dotnet is missing locally too
            try {
                const spawnResult = child_process.spawnSync('dotnet', [
                    'dev-certs',
                    'https',
                    '--export-path',
                    certFilePath,
                    '--format',
                    'Pem',
                    '--no-password',
                ], { stdio: 'inherit', });

                if (spawnResult.status !== 0) {
                    throw new Error("Could not create certificate.");
                }
            } catch (e) {
                console.warn("Warning: Could not create dev-certs via dotnet command. Skipping HTTPS configuration.");
            }
        }

        // If the files exist now, read them
        if (fs.existsSync(certFilePath) && fs.existsSync(keyFilePath)) {
            httpsConfig = {
                key: fs.readFileSync(keyFilePath),
                cert: fs.readFileSync(certFilePath),
            };
        }
    }

    //const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    //    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7024';

    return {
        plugins: [plugin()],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            }
        },
        server: {
            proxy: {
                '^/api': {
                    target: 'http://localhost:5000',
                    secure: false,
                    changeOrigin: true
                }
            },
            port: 3000,
            // This will be valid certs in Dev, and undefined in Prod/Build
            https: httpsConfig 
        }
    };
});