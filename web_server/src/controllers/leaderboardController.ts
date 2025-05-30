import { Request, Response } from "express";
import { RowDataPacket } from "mysql2";
import dotenv from "dotenv";

// Local Imports
import Logger from "../utils/logger";
import SvlimeDatabase from "../db/mysql";

// Logger Setup
const logger = Logger.getInstance();
logger.setLevel("LEADERBOARD_CON");

dotenv.config();

export async function GetLeaderboard(
    req: Request,
    res: Response
): Promise<void> {
    try {
        let { limit, page, numPlayers } = req.body;

        if (!limit || isNaN(limit) || limit <= 0) {
            limit = 10;
        }

        if (!page || isNaN(page) || page <= 0) {
            page = 1;
        }

        if (!numPlayers || isNaN(numPlayers) || numPlayers <= 0) {
            numPlayers = 1;
        }

        const db = await SvlimeDatabase.getInstance().getConnection();
        if (!db) {
            res.status(500).json({ message: "Internal Server Error" });
            return;
        }

        const offset = (page - 1) * limit;
        const [rows] = await db.query<RowDataPacket[]>(
            `
            SELECT 
              l.id AS leaderboard_id,
              l.waves_count,
              l.game_date,
              lp.user_id,
              lp.kills,
              COALESCE(u.username, au.username) AS username,
              COALESCE(u.email, au.email) AS email,
              totals.total_kills
            FROM Leaderboard l
            JOIN LeaderboardParticipants lp ON l.id = lp.leaderboard_id
            LEFT JOIN Users u ON lp.user_id = u.id
            LEFT JOIN ArchivedUsers au ON lp.user_id = au.id
            JOIN (
              SELECT leaderboard_id, SUM(kills) AS total_kills
              FROM LeaderboardParticipants
              GROUP BY leaderboard_id
            ) totals ON l.id = totals.leaderboard_id
            WHERE l.id IN (
              SELECT l2.id
              FROM Leaderboard l2
              JOIN LeaderboardParticipants lp2 ON l2.id = lp2.leaderboard_id
              GROUP BY l2.id
              HAVING COUNT(lp2.user_id) = ?
            )
            ORDER BY l.waves_count DESC, totals.total_kills DESC;
            `,
            [numPlayers]
        );

        const leaderboardMap = new Map<number, any>();

        rows.forEach((row) => {
            const {
                leaderboard_id,
                waves_count,
                game_date,
                total_kills,
                user_id,
                kills,
                username,
                email,
            } = row;

            if (!leaderboardMap.has(leaderboard_id)) {
                leaderboardMap.set(leaderboard_id, {
                    leaderboard_id,
                    waves_count,
                    game_date,
                    total_kills,
                    participants: [],
                });
            }

            leaderboardMap.get(leaderboard_id).participants.push({
                user_id,
                kills,
                username,
                email,
            });
        });

        // Dopo aver popolato leaderboardMap
        const allLeaderboards = Array.from(leaderboardMap.values());

        // Calcola offset come numero di leaderboard da saltare
        const paginatedLeaderboards = allLeaderboards.slice(
            (page - 1) * limit,
            page * limit
        );

        res.status(201).json({
            message: "ok",
            leaderboard: paginatedLeaderboards,
        });
    } catch (error) {
        logger.error("Error during Leadeboard retrieving: " + error);
        res.status(500).json({ message: "Internal server error" });
    }
}
