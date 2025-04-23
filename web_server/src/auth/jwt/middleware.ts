import { verifyRefreshToken, verifyAccessToken } from "./token";
import { NextFunction, Request, Response } from "express";

export async function middlewareAccessToken(req: Request, res:Response, next:NextFunction) {
    try {
        const token = req.headers["authorization"]?.split(" ")[1];
        if (!token) {
            res.status(401).json({ message: "Access token is missing" });
            return;
        }
        const tokenPayload = verifyAccessToken(token);
        if(tokenPayload == null)
        {
            res.status(401).json({ message: "Invalid access token" });
            return;
        }

        res.locals.userId = tokenPayload.id;
        res.locals.token = token;
        next();
    } catch (error) {
        return;
    }
}

export async function middlewareRefreshToken(req: Request, res:Response, next:NextFunction) {
    try {
        const token = req.headers["authorization"]?.split(" ")[1];
        if (!token) {
            res.status(401).json({ message: "Refresh token is missing" });
            return;
        }
        const tokenPayload = verifyRefreshToken(token);
        if(tokenPayload == null)
        {
            res.status(401).json({ message: "Invalid access token" });
            return;
        }
        res.locals.userId = tokenPayload.id;
        res.locals.token = token;
        next();
    } catch (error) {
        return;
    }
}