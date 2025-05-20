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

router.post("/new", middlewareAccessToken, GetNewGameToken);

export default router;
