import { Request, Response } from "express";
import bcrypt from "bcrypt";

// Local Imports
import Logger from "../utils/logger";
import { generateAccessToken } from "../auth/jwt/token";
import SvlimeDatabase from "../db/mysql";
import { RowDataPacket } from "mysql2";

// Logger Setup
const logger = Logger.getInstance();
logger.setLevel("AUTH_CON");

// DB Setup
const db = SvlimeDatabase.getInstance().getConnection();

export async function Login(req: Request, res: Response): Promise<void> {

}

export async function Register(req: Request, res: Response): Promise<void> {
    try {
        const { username, password, email } = req.body;

        if (!username || !password || !email) {
            res.status(400).json({ message: "Missing required fields" });
            return;
        }

        const result = await db?.query<RowDataPacket[]>("SELECT * FROM users WHERE email = ?", [email]);
        console.dir(result);
        /*
        if (!result) {
            res.status(500).json({ message: "Database query failed" });
            return;
        }

        const users = result[0].
        const [existingUser] = result[0];
        if (existingUser.length > 0) {
            res.status(409).json({ message: "User already exists" });
            return;
        }

        const hashedPassword = await bcrypt.hash(password, 10);

        await db?.query("INSERT INTO users (username, email, password) VALUES (?, ?, ?)", [
            username,
            email,
            hashedPassword,
        ]);

        res.status(201).json({ message: "User registered successfully" });*/
    } catch (error) {
        logger.error("Error during registration: " + error);
        res.status(500).json({ message: "Internal server error" });
    }
}