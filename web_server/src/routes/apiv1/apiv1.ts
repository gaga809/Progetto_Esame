// src/routes/apiv1/apiv1.ts
import { Router } from "express";

const router = Router();

/**
 * @swagger
 * /api/v1:
 *   get:
 *     summary: Get the API version
 *     description: Returns a message indicating that the API is working.
 *     responses:
 *       200:
 *         description: A message indicating that the API is working.
 */
router.get("/", (req, res) => {
    res.send("API v1 is working!");
});

export default router;
