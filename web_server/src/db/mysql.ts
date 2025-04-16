import mysql from "mysql2/promise";
import Logger from "../utils/logger";

class SvlimeDatabase {
  private static instance: SvlimeDatabase;
  private connection: mysql.Connection | null = null;
  l: Logger = Logger.getInstance();

  private constructor() {
    this.l.setLevel("MYSQLDB");
  }

  public static getInstance(): SvlimeDatabase {
    if (!SvlimeDatabase.instance) {
      SvlimeDatabase.instance = new SvlimeDatabase();
    }
    return SvlimeDatabase.instance;
  }

  public async connect(): Promise<mysql.Connection> {
    if (!this.connection) {
      try {
        const DBHOST = process.env.DB_HOST || "localhost";
        const DBPORT =
          process.env.DB_PORT != null ? parseInt(process.env.DB_PORT) : 3306;
        const DBUSER = process.env.DB_USER || "admin";
        const DBPASSWORD = process.env.DB_PASSWORD || "password";
        const DBNAME = process.env.DB_SCHEMA || "svlime";

        this.connection = await mysql.createConnection({
          host: DBHOST,
          port: DBPORT,
          user: DBUSER,
          password: DBPASSWORD,
          database: DBNAME,
        });

        this.l.info(
          `Connected to MySQL database at ${DBHOST}:${DBPORT} as ${DBUSER}`
        );
      } catch (error: any) {
        this.l.error(`Couldn't connect to MySQL Database: ${error.code}`);
        throw new Error(error.code);
      }
    }
    return this.connection;
  }

  public getConnection(): mysql.Connection | null {
    return this.connection;
  }

  public async close(): Promise<void> {
    if (this.connection) {
      await this.connection.end();
      this.l.info("Disconnected from MySQL database");
    }
  }
}

export default SvlimeDatabase;
