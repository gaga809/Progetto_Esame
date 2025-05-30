// src/routes/api/v1/game
import { Router } from "express";
import { GetLeaderboard } from "../../controllers/leaderboardController";

const router = Router();

/**
 * @swagger
 * /api/v1/leaderboard/get:
 *   post:
 *     summary: Get the leaderboard
 *     description: Retrieves a list of leaderboards, each with its participants, based on the specified number of players and pagination parameters.
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
 *                 description: (optional) The maximum number of leaderboard entries to return.
 *                 example: 10
 *               page:
 *                 type: integer
 *                 description: (optional) The page number for pagination.
 *                 example: 1
 *               numPlayers:
 *                 type: integer
 *                 description: (optional) Filter leaderboards that include exactly this number of players.
 *                 example: 2
 *     responses:
 *       201:
 *         description: Successfully retrieved the leaderboard list.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: ok
 *                 leaderboard:
 *                   type: array
 *                   items:
 *                     type: object
 *                     properties:
 *                       leaderboard_id:
 *                         type: integer
 *                         example: 1
 *                       waves_count:
 *                         type: integer
 *                         example: 10
 *                       game_date:
 *                         type: string
 *                         format: date-time
 *                         example: 2025-05-01T00:00:00.000Z
 *                       total_kills:
 *                         type: integer
 *                         example: 15
 *                       participants:
 *                         type: array
 *                         items:
 *                           type: object
 *                           properties:
 *                             user_id:
 *                               type: integer
 *                               example: 101
 *                             kills:
 *                               type: integer
 *                               example: 7
 *                             username:
 *                               type: string
 *                               example: alice
 *                             email:
 *                               type: string
 *                               example: alice@example.com
 *       500:
 *         description: Internal Server Error
 */

router.post("/get", GetLeaderboard);

export default router;
