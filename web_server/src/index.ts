import express from "express";
import dotenv from "dotenv";
import apiV1Router from "./routes/apiv1/apiv1";

// Load environment variables from .env file
dotenv.config();

// Environment variables
const PORT = process.env.PORT || 3000;

// Express Server
function setup() {
    const app = express();

    app.use(express.json());

    app.use("/api/v1", apiV1Router);

    app.listen(PORT, () => {
        console.log("Server avviato su http://localhost:3000");
    });

    app.get("/", (req, res) => {
        res.send("Hello World!");
    });
}

if (require.main === module) {
    setup();
}
