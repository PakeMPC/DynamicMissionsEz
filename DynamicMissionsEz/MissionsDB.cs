using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using TShockAPI;
using TShockAPI.DB;

namespace DynamicMissionsEz
{
    public static class MissionsDB
    {
        public static IDbConnection Db;

        public static void Connect()
        {
            string sql = Path.Combine(TShock.SavePath, "DynamicMissions.sqlite");
            Db = new SqliteConnection($"Data Source={sql}");

            try
            {
                // Creación de tablas para SQLite
                Db.Query(@"
                    CREATE TABLE IF NOT EXISTS PendingRewards (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountName TEXT,
                        RewardString TEXT
                    );

                    CREATE TABLE IF NOT EXISTS PlacedBlocks (
                        X INTEGER,
                        Y INTEGER,
                        AccountName TEXT
                    );

                    CREATE TABLE IF NOT EXISTS ActiveMissions (
                        AccountName TEXT,
                        MissionName TEXT,
                        Progress INTEGER,
                        StartTime TEXT
                    );

                    CREATE TABLE IF NOT EXISTS GlobalMissions (
                        MissionName TEXT UNIQUE,
                        CompletedCount INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS PlayerMissionsCompleted (
                        AccountName TEXT,
                        MissionName TEXT,
                        CompletedCount INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS GuildRegions (
                        RegionName TEXT UNIQUE
                    );
                ");
                Db.Query("CREATE TABLE IF NOT EXISTS LifetimeMissionsCompleted (AccountName TEXT, MissionName TEXT)");

                TShock.Log.ConsoleInfo(Missionsi18n.GetString("LogDbLoaded"));
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogDbError", ex.Message));
            }
        }
    }
}
