import fs from "fs";
import path from "path";
import dayjs from "dayjs";

const LOGDIR = process.env.LOGS_DIR || "../../logs";
const LOGFILE = process.env.LOGS_FILE_NAME || "web_server.log";
const LOGDATEFORMAT = process.env.LOGS_DATE_FORMAT || "DD-MM-YYYY_HH-mm-ss";

class Logger {
    private static instance: Logger | null = null;
    private static logFilePath: string;
    private level: string;

    private constructor(level: string = "main") {
        this.level = level;
    }

    /// <summary>
    /// Gets the singleton instance of the Logger class.
    /// </summary>
    /// <returns>The singleton instance of the Logger class.</returns>
    static getInstance(): Logger {
        if (!Logger.instance) {
            Logger.instance = new Logger();
            Logger.logFilePath = Logger.instance.getLogFilePath();
        }
        return Logger.instance;
    }

    /// <summary>
    /// Gets the path to the log file.
    /// </summary>
    /// <returns>The path to the log file.</returns>
    private getLogFilePath(): string {
        const logDir = path.resolve(__dirname, LOGDIR);

        // Verify if the directory exists, if not create it
        // with recursive option to create all directories in the path
        if (!fs.existsSync(logDir)) {
            fs.mkdirSync(logDir, { recursive: true });
        }

        // If the log file already exists, rename it with a timestamp
        // to avoid overwriting the previous log file
        const logFile = path.join(logDir, LOGFILE);
        if (fs.existsSync(logFile)) {
            const archiveName = `${dayjs().format("YYYY-MM-DD_HH-mm-ss")}.log`;
            const archivePath = path.join(logDir, archiveName);
            fs.renameSync(logFile, archivePath);
        }

        return logFile;
    }

    /// <summary>
    /// Logs a message to the log file with a timestamp and log level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="type">The type of log (INFO, WARN, ERROR).</param>
    /// <remarks>
    /// This method appends the log message to the log file.
    /// The log message includes a timestamp and the log level.
    /// </remarks>
    private log(message: string, type: string): void {
        const timestamp = dayjs().format(LOGDATEFORMAT);
        const logMessage = `${timestamp} - {${this.level.toUpperCase()}} [${type}]: ${message}`;
        fs.appendFileSync(Logger.logFilePath, logMessage + "\n");

        // Log to console as well
        if (type === "ERROR") {
            console.error(logMessage);
        } else if (type === "WARN") {
            console.warn(logMessage);
        } else {
            console.log(logMessage);
        }
    }

    /// <summary>
    /// Logs an info message to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public info(message: string): void {
        this.log(message, "INFO");
    }

    /// <summary>
    /// Logs a warning message to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public warn(message: string): void {
        this.log(message, "WARN");
    }

    /// <summary>
    /// Logs an error message to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public error(message: string): void {
        this.log(message, "ERROR");
    }

    /// <summary>
    /// Set the "level" or "context" dynamically. This changes the "location" from where the log is coming.
    /// </summary>
    public setLevel(level: string): void {
        this.level = level;
    }
}

const logger = Logger.getInstance();
logger.setLevel("logger");
logger.info("Logger initialized");
export default logger;
