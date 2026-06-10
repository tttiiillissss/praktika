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
                        INSERT OR IGNORE INTO Users (VkId, Name, FirstMessageAt, Status)
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

        public static void SaveUserLanguage(long vkId, string language)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        UPDATE Users 
                        SET SelectedLanguage = @language
                        WHERE VkId = @vkId";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@vkId", vkId);
                        cmd.Parameters.AddWithValue("@language", language);
                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine($"✅ Язык {language} сохранён для {vkId}, строк обновлено: {rows}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения языка: {ex.Message}");
            }
        }

        public static string GetUserLanguage(long vkId)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT SelectedLanguage FROM Users WHERE VkId = @vkId";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@vkId", vkId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка получения языка: {ex.Message}");
                return null;
            }
        }

        public static void SaveUserLevel(long vkId, string level)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Users SET LanguageLevel = @level WHERE VkId = @vkId";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@vkId", vkId);
                        cmd.Parameters.AddWithValue("@level", level);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"✅ Уровень {level} сохранён для {vkId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения уровня: {ex.Message}");
            }
        }

        public static string GetUserLevel(long vkId)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT LanguageLevel FROM Users WHERE VkId = @vkId";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@vkId", vkId);
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка получения уровня: {ex.Message}");
                return null;
            }
        }
    }
}