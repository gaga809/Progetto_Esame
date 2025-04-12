// src/routes/apiv1/apiv1.ts
import { Router } from "express";

const router = Router();

/**
 * @swagger
 * /api/v1:
 *   get:
 *     summary: Get the API version
 *     description: Returns a message indicating that the API is working.
 *     tags:
 *       - API v1
 *     responses:
 *       200:
 *         description: A message indicating that the API is working.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Hello from the Svlime API v1!"
 */
router.get("/", (req, res) => {
    res.json({ message: "Hello from the Svlime API v1!" });
});

export default router;
