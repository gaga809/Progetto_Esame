// src/routes/api/v1/auth
import { Router } from "express";
import { Login, Register} from "../../controllers/authController"

const router = Router();

/**
 * @swagger
 * /api/v1/auth:
 *   get:
 *     summary: Get the API v1 Auth Endpoint
 *     description: Returns a message indicating that the API v1 Auth endpoint is working.
 *     tags:
 *       - API v1
 *     responses:
 *       200:
 *         description: A message indicating that the API v1 Auth endpoint is working.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Hello from the Svlime API v1 Auth Endpoint!"
 */
router.get("/", (req, res) => {
    res.json({ message: "Hello from the Svlime API v1 Auth Endpoint!" });
});

/**
 * @swagger
 * /api/v1/auth/login:
 *   get:
 *     summary: Generate an access token
 *     description: Generates and returns an access token for authentication.
 *     tags:
 *       - API v1
 *     responses:
 *       200:
 *         description: A generated access token.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 token:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 */
router.get("/login", Login);

export default router;
