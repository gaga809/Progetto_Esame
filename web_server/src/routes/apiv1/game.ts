// src/routes/api/v1/game
import { Router } from "express";

import { middlewareAccessToken, middlewareGameToken } from "../../auth/jwt/middleware";
import { GetNewGameToken, SaveGameToDB } from "../../controllers/gameController";

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

/**
 * @swagger
 * /api/v1/game/save:
 *   post:
 *     summary: Save game results to the database
 *     description: Saves the game session to the database if the game exists, the user is the owner, and the kills data is valid. Requires both a valid access token and game token.
 *     tags:
 *       - API v1
 *     security:
 *       - bearerAuth: []
 *       - gameToken: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - kills
 *               - wave
 *             properties:
 *               gameToken:
 *                 type: string
 *                 description: The game token for the current game session
 *                 example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *               kills:
 *                 type: array
 *                 description: Array of integers representing the number of kills by each user
 *                 items:
 *                   type: integer
 *                 example: [5, 10, 3]
 *               wave:
 *                 type: integer
 *                 description: The wave number reached in the game
 *                 example: 12
 *     responses:
 *       201:
 *         description: Game saved successfully.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Game saved successfully"
 *       400:
 *         description: Bad request. Missing or invalid fields, or authorization errors.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Kills are required"
 *       403:
 *         description: Forbidden. The user is not the owner of the game.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Not the owner of this game"
 *       500:
 *         description: Internal server error while saving the game.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Internal server error"
 */
router.post("/save", middlewareAccessToken, middlewareGameToken,  SaveGameToDB);

export default router;
