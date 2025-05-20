// src/routes/api/v1/game
import { Router } from "express";

import { middlewareAccessToken } from "../../auth/jwt/middleware";
import { GetNewGameToken } from "../../controllers/gameController";

const router = Router();

/**
 * @swagger
 * /api/v1/game:
 *   get:
 *     summary: Root url for the Game Endpoint
 *     description: Returns a message indicating that the API Game Endpoint is working.
 *     tags:
 *       - API v1
 *     responses:
 *       200:
 *         description: A message indicating that the API Game Endpoint is working.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Hello from the Svlime API v1 Game Endpoint!"
 */
router.get("/", (req, res)=>{
    res.status(200).json({ message: "API v1 - Game" });
});

/**
 * @swagger
 * /api/v1/game/new:
 *   post:
 *     summary: Generate a new game token
 *     description: Creates a new game session by validating the provided user IDs, checking their existence in the database, and generating a game token if all validations pass. The requesting user must be included in the users array.
 *     tags:
 *       - API v1
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - users
 *             properties:
 *               users:
 *                 type: array
 *                 description: Array of numeric user IDs participating in the game
 *                 items:
 *                   type: integer
 *                 example: [1, 2, 3]
 *     responses:
 *       201:
 *         description: New game token generated successfully.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "New game token generated successfully"
 *                 gameId:
 *                   type: string
 *                   example: "550e8400-e29b-41d4-a716-446655440000"
 *                 gameToken:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *       400:
 *         description: Bad request. Invalid or missing input fields.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Users are required"
 *       500:
 *         description: Internal server error.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Internal server error"
 */
router.post("/new", middlewareAccessToken, GetNewGameToken);

export default router;
