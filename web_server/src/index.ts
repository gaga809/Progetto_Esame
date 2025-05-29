import express, { Response, Request, NextFunction } from "express";
import dotenv from "dotenv";
import swaggerJsdoc from "swagger-jsdoc";
import swaggerUi from "swagger-ui-express";
import fs from "fs";
import https from "https";
import http from "http";
import { execSync } from "child_process";

// Local Imports
import apiV1Router from "./routes/apiv1/apiv1";
import { middlewareCheckAdmin } from "./auth/jwt/middleware";
import Logger from "./utils/logger";
import SvlimeDatabase from "./db/mysql";

// Logger setup
const logger = Logger.getInstance();
logger.setLevel("INDEX");

// Load environment variables from .env file
dotenv.config();

// DB Setup
const db = SvlimeDatabase.getInstance();

// Swagger setup
const swaggerOptions = {
    definition: {
        openapi: "3.0.0",
        info: {
            title: "API Documentation",
            version: "1.0.0",
            description:
                "The documentation for the Svlime API. Endpoints are listed below.",
        },
        components: {
            securitySchemes: {
                BearerAuth: {
                    type: "http",
                    scheme: "bearer",
                    bearerFormat: "JWT",
                    description:
                        "Enter your bearer token in the format **Bearer {token}**",
                },
            },
        },
        security: [
            {
                BearerAuth: [],
            },
        ],
    },
    apis: ["./src/routes/apiv1/*.ts"],
};

const swaggerSpec = swaggerJsdoc(swaggerOptions);

// Environment variables
const HTTPPORT = process.env.HTTPPORT ? parseInt(process.env.HTTPPORT) : 80;
const HTTPSPORT = process.env.HTTPSPORT
    ? parseInt(process.env.HTTPSPORT)
    : 4433;
const CERT_FILE = process.env.CERT_FILE || "./cert.pem";
const KEY_FILE = process.env.KEY_FILE || "./key.pem";
const DEVELOPMENT = parseInt(process.env.DEVELOPMENT || "0");

// Check for certificate (HTTPS)

function hasOpenSSL() {
    try {
        execSync("openssl version", { stdio: "ignore" });
        return true;
    } catch (err) {
        return false;
    }
}

if (!fs.existsSync(CERT_FILE) || !fs.existsSync(KEY_FILE)) {
    logger.warn("Couldn't find a certificate for HTTPS.");
    if (hasOpenSSL()) {
        logger.info(
            "OpenSSL is available. Generating a self-signed certificate..."
        );
        try {
            execSync(
                `openssl req -x509 -newkey rsa:2048 -nodes -keyout ${KEY_FILE} -out ${CERT_FILE} -days 365 -subj "/CN=localhost"`
            );
            logger.info("Certificate generated successfully.");
        } catch (err: any) {
            logger.error("Error generating certificate: " + err.message);
            process.exit(1);
        }
    } else {
        logger.error(
            `OpenSSL is not available. Cannot generate a self-signed certificate. Missing ${CERT_FILE} and ${KEY_FILE}. Create them manually.`
        );
        process.exit(1);
    }
}

// Express Server
async function setup() {
    await db.connect();

    if (db.getConnection() != null) {
        logger.info("Starting server...");

        const app = express();

        app.use(express.json());

        // public folder for static files
        app.use(express.static("public"));

        if (DEVELOPMENT == 1) {
            logger.info("Development mode enabled. CORS is enabled.");
            logger.info(
                "SWAGGER UI Enabled at https://localhost:3000/api-docs"
            );
            // CORS setup
            app.use((req: Request, res: Response, next: NextFunction) => {
                res.header("Access-Control-Allow-Origin", "*");
                res.header(
                    "Access-Control-Allow-Headers",
                    "Origin, X-Requested-With, Content-Type, Accept"
                );
                next();
            });

            // Swagger UI setup
            app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));
        }

        // Show Main Route
        app.use("/api/v1", apiV1Router);

        https
            .createServer(
                {
                    key: fs.readFileSync(KEY_FILE),
                    cert: fs.readFileSync(CERT_FILE),
                },
                app
            )
            .listen(HTTPSPORT, () => {
                logger.info(
                    `Server HTTPS is running on https://localhost:${HTTPSPORT}`
                );
            });

        http.createServer((req, res) => {
            res.writeHead(HTTPPORT, {
                Location:
                    "https://" +
                    req.headers.host?.replace(/:\d+/, ":" + HTTPSPORT) +
                    req.url,
            });
            res.end();
        }).listen(301, () => {
            logger.info(
                `The HTTP is running on http://localhost:${HTTPPORT}. Redirecting to HTTPS server...`
            );
        });
    } else {
        return;
    }
}

if (require.main === module) {
    setup();
}
