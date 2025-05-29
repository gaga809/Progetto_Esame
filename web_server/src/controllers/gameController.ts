import { Request, Response } from "express";
import { RowDataPacket } from "mysql2";
import ms from "ms";
import dotenv from "dotenv";
import { v4 as uuidv4 } from "uuid";

// Local Imports
import Logger from "../utils/logger";
import SvlimeDatabase from "../db/mysql";
import { generateGameToken, JWTGAMEEXPIRESIN } from "../auth/jwt/token";

// Logger Setup
const logger = Logger.getInstance();
logger.setLevel("GAME_CON");

dotenv.config();

const MAXPLAYERS = parseInt(process.env.MAX_PLAYERS || "4", 10);
if (isNaN(MAXPLAYERS) || MAXPLAYERS <= 0) {
    logger.error("MAX_PLAYERS must be a positive integer.");
    throw new Error("MAX_PLAYERS must be a positive integer.");
}

const localGames: Map<string, any> = new Map();

export function addLocalGame(gameId: string, gameData: any) {
    const expiresAt = Date.now() + ms(JWTGAMEEXPIRESIN);
    localGames.set(gameId, { ...gameData, expiresAt });
}

export function getLocalGame(gameId: string) {
    const game = localGames.get(gameId);
    if (!game) return null;
    if (game.expiresAt && game.expiresAt < Date.now()) {
        localGames.delete(gameId);
        return null;
    }
    return game;
}

export function removeLocalGame(gameId: string) {
    localGames.delete(gameId);
}

// Clean games every hour
setInterval(cleanExpiredGames, 60 * 60 * 1000);

function cleanExpiredGames() {
    const now = Date.now();
    for (const [gameId, game] of localGames.entries()) {
        if (game.expiresAt && game.expiresAt < now) {
            localGames.delete(gameId);
        }
    }
}

export async function GetNewGameToken(
    req: Request,
    res: Response
): Promise<void> {
    try {
        const userId = res.locals.userId;

        const { users } = req.body;

        if (!users) {
            res.status(400).json({ message: "Users are required" });
            return;
        }

        if (!Array.isArray(users)) {
            res.status(400).json({ message: "Users must be an array" });
            return;
        }

        if (users.length === 0 || users.length > MAXPLAYERS) {
            res.status(400).json({
                message: `Invalid number of players. Must be between 1 and ${MAXPLAYERS}.`,
            });
            return;
        }

        let isThereOwner = false;
        users.forEach(async (id) => {
            if (isNaN(id)) {
                res.status(400).json({
                    message:
                        "Ids inside the users array must be a valid numeric id",
                });
                return;
            } else if (id == userId) {
                isThereOwner = true;
            } else {
                const db = await SvlimeDatabase.getInstance().getConnection();
                if (!db) {
                    res.status(500).json({
                        message: "Database connection failed",
                    });
                    return;
                }
                const results = await db.query<RowDataPacket[]>(
                    "SELECT id FROM Users WHERE id = ?;",
                    [id]
                );
                const [rows] = results;

                if (rows.length == 0) {
                    res.status(400).json({
                        message: `User with id ${id} does not exist`,
                    });
                    return;
                }
            }
        });

        if (!isThereOwner) {
            res.status(400).json({
                message: "The owner of the game must be inside the users array",
            });
            return;
        }

        // Generate a new UUID for the game
        const gameId = uuidv4();
        const gameToken = generateGameToken({ gameId, users, owner: userId });
        addLocalGame(gameId, { users, owner: userId });
        logger.info(
            `New game token generated for game ${gameId} with users ${users}`
        );

        res.status(201).json({
            message: "New game token generated successfully",
            gameId,
            gameToken,
        });
    } catch (error) {
        logger.error("Error during Game Token creation: " + error);
        res.status(500).json({ message: "Internal server error" });
    }
}

export async function SaveGameToDB(req: Request, res: Response): Promise<void> {
    try {
        const userId = res.locals.userId;
        const id = res.locals.gameId;
        const users = res.locals.users;

        const game = getLocalGame(id);

        // Check if the game exists
        if (!game) {
            res.status(400).json({ message: "Game not found" });
            return;
        }

        // Check if the user is the owner of the game
        if (game.owner !== userId) {
            res.status(403).json({ message: "Not the owner of this game" });
            return;
        }

        const { kills, wave } = req.body;

        if (!kills) {
            res.status(400).json({ message: "Kills are required" });
            return;
        }

        if (!wave) {
            res.status(400).json({ message: "Wave is required" });
            return;
        }

        if (!Array.isArray(kills)) {
            res.status(400).json({ message: "Kills must be an array" });
            return;
        }

        if (kills.length === 0 || kills.length > MAXPLAYERS) {
            res.status(400).json({
                message: `Invalid number of players. Must be between 1 and ${MAXPLAYERS}.`,
            });
            return;
        }

        if (kills.length !== users.length) {
            res.status(400).json({
                message: `Kills array must have the same number of players as the users array (${users.length})`,
            });
            return;
        }

        // Remove duplicates from the kills array
        const uniqueKills = Array.from(new Set(kills));

        // Check if the unique kills array has the same length as the kills array
        if (uniqueKills.length !== kills.length) {
            res.status(400).json({
                message: "Kills array must not contain duplicate values",
            });
            return;
        }

        // CHeck if all users inside the users array are part of the kills users
        game.users.forEach((id: number) => {
            const userKill = kills.find(
                (kill: { id: number; kills: number }) => kill.id === id
            );
            if (!userKill) {
                res.status(400).json({
                    message: `User with id ${id} is not part of the kills array`,
                });
                return;
            }
        });

        // save the game to the database

        const db = await SvlimeDatabase.getInstance().getConnection();
        if (!db) {
            res.status(500).json({ message: "Internal Server Error" });
            return;
        }

        try {
            db.beginTransaction();
            const results = await db.query<RowDataPacket[]>(
                "INSERT INTO Leaderboard VALUES (?, ?, ?);",
                [game.gameId, wave, Date.now()]
            );

            const [rows] = results;
            if (rows.length === 0) {
                res.status(500).json({
                    message: "Failed to save game to database",
                });
                return;
            }
            // Remove the game from local storage
            removeLocalGame(id);
            logger.info(`Game ${id} saved to database successfully`);
            // Respond with success
            res.status(201).json({
                message: "Game saved successfully",
            });

            // Add the players to the database
            for (let i = 0; i < users.length; i++) {
                const userId = users[i];
                const killCount = kills[i];

                await db.query<RowDataPacket[]>(
                    "INSERT INTO LeaderboardParticipants (leaderboard_id, user_id, kills) VALUES (?, ?, ?);",
                    [game.gameId, userId, killCount]
                );
            }

            db.commit();
        } catch (error) {
            logger.error("Error saving game to database: " + error);
            db.rollback();
            res.status(500).json({ message: "Internal server error" });
        }
    } catch (error) {
        logger.error("Error during Game Token creation: " + error);
        res.status(500).json({ message: "Internal server error" });
    }
}
