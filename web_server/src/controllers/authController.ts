import { Request, Response } from "express";
import ms from "ms";
import dayjs from "dayjs";
import crypto from "crypto";

// Local Imports
import Logger from "../utils/logger";
import {
    generateAccessToken,
    generateRefreshToken,
    JWTEXPIRESIN,
    JWTREFRESHEXPIRESIN,
} from "../auth/jwt/token";
import SvlimeDatabase from "../db/mysql";
import { RowDataPacket } from "mysql2";

// Logger Setup
const logger = Logger.getInstance();
logger.setLevel("AUTH_CON");

const PASSWORD_MIN_LENGTH = 6;
const USERNAME_MIN_LENGTH = 3;

export async function Login(req: Request, res: Response): Promise<void> {
    try {
        const { username, password, email } = req.body;

        if ((!username && !email) || !password) {
            res.status(400).json({ message: "Missing required fields" });
            return;
        }

        let db = await SvlimeDatabase.getInstance().getConnection();

        if (!db) {
            db = await SvlimeDatabase.getInstance().connect();
            if (!db) {
                res.status(500).json({ message: "Database connection failed" });
                return;
            }
        }

        const hashed_password = crypto
            .createHash("sha256")
            .update(password)
            .digest("hex");

        let query, params;
        if (!email) {
            query =
                "SELECT id, username, isAdmin FROM Users WHERE username = ? AND password_hash = ?";
            params = [username, hashed_password];
        } else {
            query =
                "SELECT id, username, isAdmin FROM Users WHERE email = ? AND password_hash = ?";
            params = [email, hashed_password];
        }

        try {
            await db.beginTransaction();

            const results = await db.query<RowDataPacket[]>(query, params);

            const [rows] = results;

            if (rows.length == 0) {
                res.status(404).json({ message: "User not found" });
                return;
            } else {
                if (rows.length > 0) {
                    const [userRows] = results;

                    const user = userRows[0] as {
                        id: number;
                        username: string;
                        isAdmin: number;
                    };
                    logger.info(
                        `User '${user.username}' logged in with id '${
                            user.id
                        }'. Admin = ${user.isAdmin == 1}`
                    );

                    // Update last login in the db
                    await db.query(
                        "UPDATE Users SET last_login = NOW() WHERE id = ?",
                        [user.id]
                    );

                    const accessToken = generateAccessToken(user);
                    const refreshToken = generateRefreshToken(user);

                    // Session Data
                    const agent = req.headers["user-agent"] || "Unknown";
                    const ip = req.ip || req.socket.remoteAddress || "Unknown";
                    const createdAt = dayjs().format("YYYY-MM-DD HH:mm:ss");
                    logger.info("Created at: " + createdAt);
                    const expiresAt = dayjs()
                        .add(ms(JWTREFRESHEXPIRESIN), "ms")
                        .format("YYYY-MM-DD HH:mm:ss");
                    logger.info("Expires at: " + expiresAt);
                    logger.info("Agent: " + agent);
                    logger.info("IP: " + ip);

                    await db.commit();

                    await SaveSession(
                        user.id,
                        refreshToken,
                        agent,
                        ip,
                        createdAt,
                        expiresAt
                    );

                    res.status(201).json({
                        message: "User logged in successfully",
                        access_token: accessToken,
                        refresh_token: refreshToken,
                        type: "Bearer ",
                        id: user.id,
                        username: user.username,
                        isAdmin: user.isAdmin == 1,
                    });
                } else {
                    await db.rollback();
                    res.status(500).json({ message: "Database query failed" });
                    return;
                }
            }
        } catch (error) {
            logger.error("Error during login: " + error);
            res.status(500).json({ message: "Internal server error" });

            await db.rollback();
        }
    } catch (error) {
        logger.error("Error during login: " + error);
        res.status(500).json({ message: "Internal server error" });
    }
}

export async function Register(req: Request, res: Response): Promise<void> {
    try {
        const { username, password, email } = req.body;

        if (!username || !email || !password) {
            res.status(400).json({ message: "Missing required fields" });
            return;
        }

        const pswCheck = IsValidPassword(password);
        if (!pswCheck.check) {
            res.status(400).json({ message: pswCheck.message });
            return;
        }

        if (!IsValidEmail(email)) {
            res.status(400).json({ message: "Invalid email format" });
            return;
        }

        if (username.length < USERNAME_MIN_LENGTH) {
            res.status(400).json({
                message:
                    "Username is too short (minimum " +
                    USERNAME_MIN_LENGTH +
                    " characters)",
            });
            return;
        }

        let db = await SvlimeDatabase.getInstance().getConnection();

        const hashed_password = crypto
            .createHash("sha256")
            .update(password)
            .digest("hex");

        if (!db) {
            db = await SvlimeDatabase.getInstance().connect();
            if (!db) {
                res.status(500).json({ message: "Database connection failed" });
                return;
            }
        }

        try {
            await db.beginTransaction();

            const results = await db.query<RowDataPacket[]>(
                "SELECT * FROM Users WHERE email = ? OR username = ?",
                [email, username]
            );

            const [rows] = results;

            if (rows.length > 0) {
                await db.rollback();
                res.status(409).json({
                    message: "User with this email/username already exists",
                });
                return;
            } else {
                if (rows.length == 0) {
                    await db.query(
                        "INSERT INTO Users (username, email, password_hash) VALUES (?, ?, ?)",
                        [username, email, hashed_password]
                    );

                    const userQueryResult = db
                        ? await db.query<RowDataPacket[]>(
                              "SELECT id, username, isAdmin FROM Users WHERE email = ?",
                              [email]
                          )
                        : [[]];

                    const [userRows] = userQueryResult;
                    if (userRows.length == 0) {
                        await db.rollback();

                        logger.info("Couldn't create user.");
                        res.status(500).json({
                            message: "Internal server error",
                        });
                        return;
                    }

                    const user = userRows[0] as {
                        id: number;
                        username: string;
                        isAdmin: number;
                    };
                    logger.info(
                        `Created new user '${user.username}' with id '${
                            user.id
                        }'. Admin = ${user.isAdmin == 1}`
                    );

                    const accessToken = generateAccessToken(user);
                    const refreshToken = generateRefreshToken(user);

                    // Session Data
                    const agent = req.headers["user-agent"] || "Unknown";
                    const ip = req.ip || req.socket.remoteAddress || "Unknown";
                    const createdAt = dayjs().format("YYYY-MM-DD HH:mm:ss");
                    logger.info("Created at: " + createdAt);
                    const expiresAt = dayjs()
                        .add(ms(JWTREFRESHEXPIRESIN), "ms")
                        .format("YYYY-MM-DD HH:mm:ss");
                    logger.info("Expires at: " + expiresAt);
                    logger.info("Agent: " + agent);
                    logger.info("IP: " + ip);
                    await db.commit();

                    await SaveSession(
                        user.id,
                        refreshToken,
                        agent,
                        ip,
                        createdAt,
                        expiresAt
                    );

                    res.status(201).json({
                        message: "User registered successfully",
                        access_token: accessToken,
                        refresh_token: refreshToken,
                        type: "Bearer ",
                    });
                } else {
                    await db.rollback();
                    res.status(500).json({ message: "Database query failed" });
                    return;
                }
            }
        } catch (error) {
            await db.rollback();
            logger.error("Error during registration: " + error);
            res.status(500).json({ message: "Internal server error" });
        }
    } catch (error) {
        logger.error("Error during registration: " + error);
        res.status(500).json({ message: "Internal server error" });
    }
}

export async function GetNewAccessToken(
    req: Request,
    res: Response
): Promise<void> {
    try {
        const refreshToken = res.locals.token;
        const userId = res.locals.userId;
        let db = await SvlimeDatabase.getInstance().getConnection();

        if (!db) {
            db = await SvlimeDatabase.getInstance().connect();
            if (!db) {
                res.status(500).json({ message: "Database connection failed" });
                return;
            }
        }

        // Get if is valid session

        try {
            await db.beginTransaction();

            const sessionQuery = db
                ? await db.query<RowDataPacket[]>(
                      "SELECT session_id FROM UserSessions WHERE user_id = ? AND refresh_token = ? AND ip_address = ?",
                      [userId, refreshToken, req.socket.remoteAddress]
                  )
                : [[]];

            const [sessionRows] = sessionQuery;
            if (sessionRows.length == 0) {
                db.rollback();
                res.status(401).json({
                    message: "Refresh token is not of a valid session.",
                });
                return;
            }

            const userQueryResult = db
                ? await db.query<RowDataPacket[]>(
                      "SELECT id, username, isAdmin FROM Users WHERE id = ?",
                      [userId]
                  )
                : [[]];

            const [userRows] = userQueryResult;
            if (userRows.length == 0) {
                await db.rollback();
                res.status(500).json({ message: "Couldn't fetch user" });
                return;
            }

            const user = userRows[0] as {
                id: number;
                username: string;
                isAdmin: number;
            };

            const access_token = generateAccessToken(user);

            res.status(201).json({
                message:
                    "Session is still valid. Here is the new access token!",
                access_token: access_token,
                type: "Bearer ",
            });

            await db.commit();
        } catch (error) {
            await db.rollback();
            logger.error(
                "Error during the retrieval of a new access token: " + error
            );
            res.status(500).json({ message: "Internal server error" });
        }
    } catch (error) {
        logger.error(
            "Error during the retrieval of a new access token: " + error
        );
        res.status(500).json({ message: "Internal server error" });
    }
}

/* OTHER FUNCTIONS */

// Check if the email given is in valid format
export function IsValidEmail(email: string) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

export function IsValidPassword(password: string) {
    // Check if the password has at least one special character, is at least PASSWORD_MIN_LENGTH long,
    // and has at least one number and one uppercase character
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasUppercase = /[A-Z]/.test(password);

    if (!hasSpecialChar)
        return {
            check: false,
            message: "Password must contain at least one special character",
        };
    if (!hasNumber)
        return {
            check: false,
            message: "Password must contain at least one number",
        };
    if (!hasUppercase)
        return {
            check: false,
            message: "Password must contain at least one uppercase character",
        };

    if (password.length < PASSWORD_MIN_LENGTH)
        return {
            check: false,
            message:
                "Password is too short (minimum " +
                PASSWORD_MIN_LENGTH +
                " characters)",
        };

    return { check: true };
}

export async function SaveSession(
    user_id: number,
    refresh_token: string,
    agent: string,
    ip: string,
    created_at: string,
    expires_at: string
) {
    const db = await SvlimeDatabase.getInstance().getConnection();

    if (!db) {
        logger.error("SaveSession: No DB connection");
        return;
    }

    try {
        await db.beginTransaction();
        await db.query(
            "INSERT INTO UserSessions (user_id, refresh_token, user_agent, ip_address, created_at, expires_at) VALUES (?, ?, ?, ?, ?, ?)",
            [user_id, refresh_token, agent, ip, created_at, expires_at]
        );
        logger.info(`Session saved for user ${user_id}`);
        await db.commit();
    } catch (err: any) {
        await db.rollback();
        logger.error("SaveSession DB error: " + err.message);
    }
}
