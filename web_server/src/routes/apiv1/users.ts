// src/routes/api/v1/auth
import { Router } from "express";
import { middlewareAccessToken } from "../../auth/jwt/middleware";
import { GetMe } from "../../controllers/usersController";

const router = Router();

/**
 * @swagger
 * /api/v1/auth:
 *   get:
 *     summary: Root url for the Auth Endpoint
 *     description: Returns a message indicating that the API Auth Endpoint is working.
 *     tags:
 *       - API v1
 *     responses:
 *       200:
 *         description: A message indicating that the API Auth Endpoint is working.
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
    res.status(200).json({ message: "API v1 - Users" });
});

/**
 * @swagger
 * /api/v1/users/me:
 *   get:
 *     summary: Retrieve information about the authenticated user
 *     description: Returns the profile information of the authenticated user based on the user ID present in the request context.
 *     tags:
 *       - API v1
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       201:
 *         description: User information retrieved successfully.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "User information retrieved successfully"
 *                 user:
 *                   type: object
 *                   properties:
 *                     id:
 *                       type: integer
 *                       example: 1
 *                     username:
 *                       type: string
 *                       example: "johndoe"
 *                     isAdmin:
 *                       type: integer
 *                       example: 0
 *                     email:
 *                       type: string
 *                       example: "johndoe@example.com"
 *                     creation_date:
 *                       type: string
 *                       example: "2025-05-07T10:15:30.000Z"
 *                     last_login:
 *                       type: string
 *                       example: "2025-05-06T18:25:43.511Z"
 *                     pfp:
 *                       type: string
 *                       format: binary
 *       404:
 *         description: User not found.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "User not found"
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
router.get("/me", middlewareAccessToken, GetMe);

export default router;
