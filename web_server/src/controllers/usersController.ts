import { Request, Response } from "express";
import ms from "ms";
import dayjs from "dayjs";
import crypto from "crypto";

// Local Imports
import Logger from "../utils/logger";
import SvlimeDatabase from "../db/mysql";
import { RowDataPacket } from "mysql2";

// Logger Setup
const logger = Logger.getInstance();
logger.setLevel("USER_CON");

export async function GetMe(req: Request, res: Response): Promise<void> {
  try {
    const userId = res.locals.userId;

    let db = SvlimeDatabase.getInstance().getConnection();

    if (!db) {
      db = await SvlimeDatabase.getInstance().connect();
      if (!db) {
        res.status(500).json({ message: "Database connection failed" });
        return;
      }
    }
    const results = await db.query<RowDataPacket[]>(
      "SELECT id, username, isAdmin, email, creation_date, last_login, pfp FROM Users WHERE id = ?;",
      [userId]
    );

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
          email: string;
          creation_date: string;
          last_login: string;
          pfp: Blob;
        };
        logger.info(
          `Requested information for user ${user.username} (${user.id})`
        );

        res.status(201).json({
          message: "User information retrieved successfully",
          user: user,
        });
      } else {
        res.status(500).json({ message: "Database query failed" });
        return;
      }
    }
  } catch (error) {
    logger.error("Error during login: " + error);
    res.status(500).json({ message: "Internal server error" });
  }
}
