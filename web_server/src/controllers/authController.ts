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
const USERNAME_MIN_LENGTH = 3;

export async function Login(req: Request, res: Response): Promise<void> {
  try {
    const { username, password, email } = req.body;

    if ((!username && !email) || !password) {
      res.status(400).json({ message: "Missing required fields" });
      return;
    }

    let db = SvlimeDatabase.getInstance().getConnection();

    if (!db) {
      db = await SvlimeDatabase.getInstance().connect();
      if (!db) {
        res.status(500).json({ message: "Database connection failed" });
        return;
      }
    }

    let query, params;
    if (!email) {
      query =
        "SELECT id, username, isAdmin FROM Users WHERE username = ? AND password_hash = ?";
      params = [username, password];
    } else {
      query =
        "SELECT is, username, isAdmin FROM Users WHERE email = ? AND password_hash = ?";
      params = [email, password];
    }

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
          `User '${user.username}' logged in with id '${user.id}'. Admin = ${
            user.isAdmin == 1
          }`
        );

        // Update last login in the db
        await db.query("UPDATE Users SET last_login = NOW() WHERE id = ?", [
          user.id,
        ]);

        const accessToken = generateAccessToken(user);
        const refreshToken = generateRefreshToken(user);

        res.status(201).json({
          message: "User logged in successfully",
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

    let db = SvlimeDatabase.getInstance().getConnection();

    if (!db) {
      db = await SvlimeDatabase.getInstance().connect();
      if (!db) {
        res.status(500).json({ message: "Database connection failed" });
        return;
      }
    }

    const results = await db.query<RowDataPacket[]>(
      "SELECT * FROM Users WHERE email = ? OR username = ?",
      [email, username]
    );

    const [rows] = results;

    if (rows.length > 0) {
      res
        .status(409)
        .json({ message: "User with this email/username already exists" });
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

        const user = userRows[0] as {
          id: number;
          username: string;
          isAdmin: number;
        };
        logger.info(
          `Created new user '${user.username}' with id '${user.id}'. Admin = ${
            user.isAdmin == 1
          }`
        );

        const accessToken = generateAccessToken(user);
        const refreshToken = generateRefreshToken(user);

        res.status(201).json({
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
