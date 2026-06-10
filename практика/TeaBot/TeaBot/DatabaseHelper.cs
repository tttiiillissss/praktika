using System;
using System.Data.SQLite;

namespace TeaBotFinal
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Data Source=tea_bot.db;Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            VkId INTEGER NOT NULL UNIQUE,
                            Name TEXT,
                            SelectedLanguage TEXT,
                            LanguageLevel TEXT,
                            FirstMessageAt TEXT,
                            Status TEXT
                        )";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("✅ База данных инициализирована");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка БД: {ex.Message}");
            }
        }

        public static void SaveUser(long vkId, string name)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT OR REPLACE INTO Users (VkId, Name, FirstMessageAt, Status)
                        VALUES (@vkId, @name, @date, 'лид')";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@vkId", vkId);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения пользователя: {ex.Message}");
            }
        }
    }
}