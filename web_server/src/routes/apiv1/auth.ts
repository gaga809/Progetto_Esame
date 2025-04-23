// src/routes/api/v1/auth
import { Router } from "express";
import { middlewareRefreshToken } from "../../auth/jwt/middleware";
import { Login, Register, GetNewAccessToken} from "../../controllers/authController"
import Logger from "../../utils/logger";
const logger = Logger.getInstance();

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
    res.status(200).json({ message: "API v1 - Auth" });
});


/**
 * @swagger
 * /api/v1/auth/register:
 *   post:
 *     summary: Register a new user
 *     description: Registers a new user by validating the input fields (username, email, password), checking for existing records, and storing the new user in the database. Returns access and refresh tokens upon success.
 *     tags:
 *       - API v1
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - username
 *               - email
 *               - password
 *             properties:
 *               username:
 *                 type: string
 *                 example: "john_doe"
 *               email:
 *                 type: string
 *                 example: "john.doe@example.com"
 *               password:
 *                 type: string
 *                 example: "secure_password_123"
 *     responses:
 *       201:
 *         description: User registered successfully.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "User registered successfully"
 *                 access_token:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *                 refresh_token:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *                 type:
 *                   type: string
 *                   example: "Bearer"
 *       400:
 *         description: Bad request. Missing or invalid fields.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Missing required fields"
 *       409:
 *         description: Conflict. User already exists.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "User with this email/username already exists"
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
router.post("/register", Register)

/**
 * @swagger
 * /api/v1/auth/login:
 *   post:
 *     summary: Authenticate a user
 *     description: Authenticates a user using either email or username along with a password. Returns JWT access and refresh tokens upon successful login.
 *     tags:
 *       - API v1
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - password
 *             properties:
 *               username:
 *                 type: string
 *                 example: "john_doe"
 *               email:
 *                 type: string
 *                 example: "john.doe@example.com"
 *               password:
 *                 type: string
 *                 example: "secure_password_123"
 *             oneOf:
 *               - required: [username, password]
 *               - required: [email, password]
 *     responses:
 *       201:
 *         description: User logged in successfully.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "User logged in successfully"
 *                 access_token:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *                 refresh_token:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *                 type:
 *                   type: string
 *                   example: "Bearer"
 *       400:
 *         description: Missing required fields.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Missing required fields"
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
router.post("/login", Login);

/**
 * @swagger
 * /api/v1/auth/refresh:
 *   get:
 *     summary: Get new access token using refresh token
 *     description: Returns a new access token if the provided refresh token is valid and belongs to an active session.
 *     tags:
 *       - API v1
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       201:
 *         description: New access token issued successfully.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Session is still valid. Here is the new access token!"
 *                 access_token:
 *                   type: string
 *                   example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
 *                 type:
 *                   type: string
 *                   example: "Bearer"
 *       401:
 *         description: Invalid or missing refresh token.
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 message:
 *                   type: string
 *                   example: "Refresh token is not of a valid session."
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
router.get("/refresh", middlewareRefreshToken, GetNewAccessToken);

router.get("/refresh", middlewareRefreshToken, GetNewAccessToken);

export default router;
