import express from "express";
import dotenv from "dotenv";
import swaggerJsdoc from "swagger-jsdoc";
import swaggerUi from "swagger-ui-express";

// Local Imports
import apiV1Router from "./routes/apiv1/apiv1";
import logger from "./utils/logger";
import SvlimeDatabase from "./db/mysql";

// Logger setup
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
            version: "0.0.1",
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
const PORT = process.env.PORT || 3000;

// Express Server
async function setup() {
    await db.connect();

    if(db.getConnection() != null){
        logger.info("Starting server...");

        const app = express();

        app.use(express.json());
    
        // public folder for static files
        app.use(express.static("public"));
    
        // Swagger UI setup
        app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));
    
        // Show Main Route
        app.use("/api/v1", apiV1Router);
    
        app.listen(PORT, () => {
            logger.info(`Server is running on http://localhost:${PORT}`);
        });
    }else{
        return;
    }    
}

if (require.main === module) {
    setup();
}
