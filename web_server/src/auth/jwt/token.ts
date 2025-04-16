import jwt from "jsonwebtoken";
import ms, { StringValue } from "ms";
import Logger from "../../utils/logger";
import dotenv from "dotenv";

// Load environment variables from .env file
dotenv.config();

// Logger setup
const logger = Logger.getInstance();
logger.setLevel("JWT");

const JWTSECRET = process.env.JWT_SECRET;
if (!JWTSECRET) {
    logger.error("JWT_SECRET is not set in the environment variables.");
    throw new Error("JWT_SECRET is not set in the environment variables.");
}

const JWTEXPIRESIN = process.env.JWT_EXPIRATION as StringValue;
if (!JWTEXPIRESIN) {
    logger.error("JWT_EXPIRATION is not set in the environment variables.");
    throw new Error("JWT_EXPIRATION is not set in the environment variables.");
}

const JWTREFRESHSECRET = process.env.JWT_REFRESH_SECRET;
if (!JWTREFRESHSECRET) {
    logger.error("JWT_REFRESH_SECRET is not set in the environment variables.");
    throw new Error(
        "JWT_REFRESH_SECRET is not set in the environment variables."
    );
}

const JWTREFRESHEXPIRESIN = process.env.JWT_REFRESH_EXPIRATION as StringValue;
if (!JWTREFRESHEXPIRESIN) {
    logger.error(
        "JWT_REFRESH_EXPIRATION is not set in the environment variables."
    );
    throw new Error(
        "JWT_REFRESH_EXPIRATION is not set in the environment variables."
    );
}

/// <summary>
/// JWT Token Generation
/// </summary>
/// <param name="payload">The payload to include in the token</param>
/// <returns>The generated JWT token</returns>
/// <remarks>
/// This function generates a JWT token using the specified payload and secret.
/// The token will expire based on the expiration time set in the environment variables.
/// </remarks>
export const generateAccessToken = (payload: object) => {
    const seconds = ms(JWTEXPIRESIN) / 1000;
    return jwt.sign(payload, JWTSECRET, { expiresIn: seconds });
};

/// <summary>
/// JWT Refresh Token Generation
/// </summary>
/// <param name="payload">The payload to include in the token</param>
/// <returns>The generated JWT refresh token</returns>
/// <remarks>
/// This function generates a JWT refresh token using the specified payload and secret.
/// The refresh token will expire based on the expiration time set in the environment variables.
/// </remarks>
export const generateRefreshToken = (payload: object) => {
    const seconds = ms(JWTREFRESHEXPIRESIN) / 1000;
    return jwt.sign(payload, JWTREFRESHSECRET, {
        expiresIn: seconds,
    });
};

/// <summary>
/// JWT Token Verification
/// </summary>
/// <param name="token">The JWT token to verify</param>
/// <returns>The decoded token if verification is successful, null otherwise</returns>
/// <remarks>
/// This function verifies the provided JWT token using the secret.
/// If the token is valid, it returns the decoded token.
/// If the token is invalid or expired, it returns null.
/// </remarks>
export const verifyAccessToken = (token: string) => {
    try {
        return jwt.verify(token, JWTSECRET);
    } catch (err) {
        logger.error("JWT verification failed: " + err);
        return null;
    }
};

/// <summary>
/// JWT Refresh Token Verification
/// </summary>
/// <param name="token">The JWT refresh token to verify</param>
/// <returns>The decoded refresh token if verification is successful, null otherwise</returns>
/// <remarks>
/// This function verifies the provided JWT refresh token using the refresh secret.
/// If the refresh token is valid, it returns the decoded refresh token.
/// If the refresh token is invalid or expired, it returns null.
/// </remarks>
export const verifyRefreshToken = (token: string) => {
    try {
        return jwt.verify(token, JWTREFRESHSECRET);
    } catch (err) {
        logger.error("JWT refresh token verification failed: " + err);
        return null;
    }
};
