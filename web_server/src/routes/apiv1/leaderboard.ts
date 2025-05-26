// src/routes/api/v1/game
import { Router } from "express";
import { GetLeaderboard } from "../../controllers/leaderboardController";

const router = Router();

/**
 * @swagger
 * /api/v1/leaderboard/get:
 *   post:
 *     summary: Get the leaderboard
 *     description: Retrieves the leaderboard based on the specified parameters.
 *     tags:
 *       - API v1
 *     requestBody:
 *       required: false
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               limit:
 *                 type: integer
 *                 description: (optional) The maximum number of entries to return.
 *                 example: 10
 *               page:
 *                 type: integer
 *                 description: (optional) The page number for pagination.
 *                 example: 1
 *               numPlayers:
 *                 type: integer
 *                 description: (optional) The number of players to filter by.
 *                 example: 1
 *     responses:
 *       200:
 *         description: Successfully retrieved the leaderboard.
 *       500:
 *         description: Internal Server Error
 */
router.post("/get",  GetLeaderboard);

export default router;
