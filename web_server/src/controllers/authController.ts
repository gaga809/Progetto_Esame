import { Request, Response } from "express";

// Local Imports
import Logger from "../utils/logger";
import { generateAccessToken, generateRefreshToken } from "../auth/jwt/token";
import SvlimeDatabase from "../db/mysql";
import { RowDataPacket } from "mysql2";

// Logger Setup
const logger = Logger.getInstance();
logger.setLevel("AUTH_CON");

const PASSWORD_MIN_LENGTH = 6;
const USERNAME_MIN_LENGTH = 8;


export async function Login(req: Request, res: Response): Promise<void> {

}

export async function Register(req: Request, res: Response): Promise<void> {
  try {
    const { username, password, email } = req.body;

    if (!username || !email || !password) {
      res.status(400).json({ message: "Missing required fields" });
      return;
    }

    if(password.length < PASSWORD_MIN_LENGTH) {
        res.status(400).json({ message: "Password is too short (minimum " + PASSWORD_MIN_LENGTH + " characters)" });
        return;
    }

    if(!IsValidEmail(email)) {
        res.status(400).json({ message: "Invalid email format" });
        return;
    }

    if(username.length < USERNAME_MIN_LENGTH) {
        res.status(400).json({ message: "Username is too short (minimum " + USERNAME_MIN_LENGTH + " characters)" });
        return;
    }

    const db = SvlimeDatabase.getInstance().getConnection();

    if(!db){
        res.status(500).json({ message: "Database connection failed" });
        return;
    }

    const results = db
      ? await db.query<RowDataPacket[]>("SELECT * FROM Users WHERE email = ? OR username = ?", [
          email, username
        ])
      : [[]];

    const [rows] = results;

    if (rows.length > 0) {
      res.status(409).json({ message: "User with this email/username already exists" });
      return;
    } else {
      if (rows.length == 0) {
        await db.query(
          "INSERT INTO Users (username, email, password_hash) VALUES (?, ?, ?)",
          [username, email, password]
        );

        const userQueryResult = db
          ? await db.query<RowDataPacket[]>(
              "SELECT id, username, isAdmin FROM Users WHERE email = ?",
              [email]
            )
          : [[]];

        const [userRows] = userQueryResult;
        if (userRows.length == 0) {
          res.status(500).json({ message: "Couldn't create user" });
          return;
        }

        const user = userRows[0] as { id: number; username: string; isAdmin: number };
        logger.info(`Created new user '${user.username}' with id '${user.id}'. Admin = ${user.isAdmin == 1}`);

        const accessToken = generateAccessToken(user);
        const refreshToken = generateRefreshToken(user);

        res
          .status(201)
          .json({
            message: "User registered successfully",
            access_token: accessToken,
            refresh_token: refreshToken,
            type: "Bearer ",
          });
      } else {
        res.status(500).json({ message: "Database query failed" });
        return;
      }
    }
  } catch (error) {
    logger.error("Error during registration: " + error);
    res.status(500).json({ message: "Internal server error" });
  }
}

// Check if the email given is in valid format
export function IsValidEmail(email:string) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}