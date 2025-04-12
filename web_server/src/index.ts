import express from "express";
import dotenv from "dotenv";
import apiV1Router from "./routes/apiv1/apiv1";
import logger from "./utils/logger";

// Logger setup
logger.setLevel("INDEX");

// Load environment variables from .env file
dotenv.config();

// Environment variables
const PORT = process.env.PORT || 3000;

// Express Server
function setup() {
    const app = express();

    app.use(express.json());

    app.use("/api/v1", apiV1Router);

    app.get("/", (req, res) => {
        res.send("Hello World!");
    });

    app.listen(PORT, () => {
        logger.info(`Server is running on http://localhost:${PORT}`);
    });
}

if (require.main === module) {
    logger.info("Starting server...");
    setup();
}
