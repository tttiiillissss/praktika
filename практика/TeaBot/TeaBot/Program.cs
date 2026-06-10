using System;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;

namespace TeaBotFinal
{
    class Program
    {
        static string token = "vk1.a.Po_6P3j_XoRdiFpVIGlSqk_9qRteM-tJ5nRF5w4NYg_CMhgesGQFS-747uxPn0qpY87C0fOV80TclfKmcjUOi2zwuecUMua0QU69zbj0MoIb10gTi0ydkz5dyS1CAe-VhUexyH5uepVEA-Uioq30dp8bB_D4Iz2ltxMWva72p0KKwr6oLMg3QQQ8NDUb9G3qaKD8X1n5YKKrzXfxCN2z0A";
        static ulong groupId = 239482439;

        static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║            TEA БОТ ЗАПУЩЕН           ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("✅ Токен загружен");
            Console.WriteLine($"✅ ID группы: {groupId}");
            Console.WriteLine();

            DatabaseHelper.InitializeDatabase();

            RunBot();
        }

        static void RunBot()
        {
            while (true)
            {
                try
                {
                    string url = $"https://api.vk.com/method/groups.getLongPollServer?group_id={groupId}&access_token={token}&v=5.131";
                    string json = Get(url);

                    JObject obj = JObject.Parse(json);

                    if (obj["error"] != null)
                    {
                        Console.WriteLine($"❌ Ошибка ВК: {obj["error"]}");
                        Thread.Sleep(10000);
                        continue;
                    }

                    JObject resp = obj["response"] as JObject;
                    if (resp == null)
                    {
                        Console.WriteLine("❌ Ошибка: response = null");
                        Thread.Sleep(10000);
                        continue;
                    }

                    string server = resp["server"]?.ToString();
                    string key = resp["key"]?.ToString();
                    string ts = resp["ts"]?.ToString();

                    if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(key))
                    {
                        Console.WriteLine("❌ Ошибка: server или key пустые");
                        Thread.Sleep(10000);
                        continue;
                    }

                    if (server.StartsWith("https://"))
                        server = server.Substring(8);
                    if (server.StartsWith("http://"))
                        server = server.Substring(7);

                    Console.WriteLine($"✅ Подключено к серверу, ts={ts}");

                    while (true)
                    {
                        string poll = $"https://{server}?act=a_check&key={key}&ts={ts}&wait=25";
                        string updates = Get(poll);
                        JObject up = JObject.Parse(updates);

                        if (up["ts"] != null) ts = up["ts"].ToString();

                        if (up["updates"] != null && up["updates"].HasValues)
                        {
                            foreach (var ev in up["updates"])
                            {
                                if (ev["type"]?.ToString() == "message_new")
                                {
                                    var msg = ev["object"]?["message"] ?? ev["object"];
                                    if (msg != null)
                                    {
                                        long userId = msg["from_id"]?.Value<long>() ?? msg["user_id"]?.Value<long>() ?? 0;
                                        string text = msg["text"]?.ToString()?.ToLower() ?? "";

                                        if (userId != 0 && !string.IsNullOrEmpty(text))
                                        {
                                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Пользователь {userId}: {text}");

                                            DatabaseHelper.SaveUser(userId, "пользователь");

                                            string answer = GetAnswer(userId, text);
                                            SendMessage(userId, answer, GetKeyboard());
                                        }
                                    }
                                }
                            }
                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        static string GetAnswer(long userId, string text)
        {
            // Нормализуем текст
            text = NormalizeText(text);

            // Проверяем, проходит ли пользователь тест
            if (LanguageTest.IsUserTesting(userId))
            {
                return LanguageTest.ProcessAnswer(userId, text);
            }

            // Команды
            if (text == "привет" || text == "начать" || text == "start")
            {
                string userLang = DatabaseHelper.GetUserLanguage(userId);
                string userLevel = DatabaseHelper.GetUserLevel(userId);
                string info = "";
                if (!string.IsNullOrEmpty(userLang)) info += $"\n📍 Ваш язык: {userLang}";
                if (!string.IsNullOrEmpty(userLevel)) info += $"\n🎯 Ваш уровень: {userLevel}";
                return "🌟 Добро пожаловать в TEA!\n\nЯ бот школы иностранных языков TEA.\n\nНажмите на кнопки ниже!" + info;
            }
            else if (text == "прайс" || text == "price")
            {
                return "💰 ПРАЙС-ЛИСТ TEA 💰\n\nАнглийский язык:\n• Индивидуально — 1500₽/час\n• В паре — 1000₽/час\n• Группа — 800₽/час\n\nКитайский язык:\n• Индивидуально — 1700₽/час\n• Группа — 1000₽/час";
            }
            else if (text == "акции")
            {
                return "🎁 АКЦИИ TEA 🎁\n\n1️⃣ Скидка 20% на первый месяц\n2️⃣ Приведи друга — урок в подарок\n3️⃣ Оплата за месяц — урок бесплатно";
            }
            else if (text == "помощь")
            {
                return "📞 КОНТАКТЫ TEA:\n\n• Телефон: +7 (999) 123-45-67\n• Email: tea@school.ru\n• Адрес: ул. Ленина, д. 10\n\n🕐 Режим работы: Пн-Сб с 10:00 до 20:00";
            }
            else if (text == "человек" || text == "менеджер")
            {
                return "👋 Сейчас я соединю вас с менеджером TEA.\n\nОпишите вопрос, менеджер скоро ответит.\n\n📌 +7 (999) 123-45-67";
            }
            else if (text == "выбрать язык" || text == "язык")
            {
                return "🗣 ВЫБЕРИТЕ ЯЗЫК:\n\n🇬🇧 Английский\n🇨🇳 Китайский\n🇯🇵 Японский\n🇰🇷 Корейский\n🇪🇸 Испанский\n🇫🇷 Французский\n🇩🇪 Немецкий\n🇮🇹 Итальянский\n🇹🇷 Турецкий\n\nНапишите название языка";
            }
            else if (text == "тест" || text == "пройти тест")
            {
                string userLang = DatabaseHelper.GetUserLanguage(userId);
                if (string.IsNullOrEmpty(userLang))
                {
                    return "❓ Сначала выберите язык! Напишите ВЫБРАТЬ ЯЗЫК";
                }
                return LanguageTest.StartTest(userId);
            }
            else if (text == "пока" || text == "до свидания")
            {
                return "👋 До свидания! Будем рады видеть вас снова!";
            }
            else
            {
                // Обработка выбора языка
                string selectedLang = null;

                if (text.Contains("итальянский") || text == "итальянский" || text == "итал")
                    selectedLang = "итальянский";
                else if (text.Contains("английский") || text == "английский" || text == "англ")
                    selectedLang = "английский";
                else if (text.Contains("китайский") || text == "китайский" || text == "кит")
                    selectedLang = "китайский";
                else if (text.Contains("японский") || text == "японский" || text == "яп")
                    selectedLang = "японский";
                else if (text.Contains("корейский") || text == "корейский" || text == "кор")
                    selectedLang = "корейский";
                else if (text.Contains("испанский") || text == "испанский" || text == "исп")
                    selectedLang = "испанский";
                else if (text.Contains("французский") || text == "французский" || text == "франц")
                    selectedLang = "французский";
                else if (text.Contains("немецкий") || text == "немецкий" || text == "нем")
                    selectedLang = "немецкий";
                else if (text.Contains("турецкий") || text == "турецкий" || text == "тур")
                    selectedLang = "турецкий";

                if (selectedLang != null)
                {
                    DatabaseHelper.SaveUserLanguage(userId, selectedLang);
                    return $"✅ Вы выбрали {selectedLang} язык!\n\nТеперь напишите ТЕСТ";
                }

                return "❓ Я не понял. Используйте кнопки меню:\n• ПРИВЕТ\n• ПРАЙС\n• АКЦИИ\n• ПОМОЩЬ\n• ВЫБРАТЬ ЯЗЫК\n• ТЕСТ\n• ЧЕЛОВЕК\n• ПОКА";
            }
        }

        static void SendMessage(long userId, string text, string keyboard = null)
        {
            try
            {
                string url = $"https://api.vk.com/method/messages.send?user_id={userId}&message={Uri.EscapeDataString(text)}&random_id={new Random().Next()}&access_token={token}&v=5.131&group_id={groupId}";
                if (!string.IsNullOrEmpty(keyboard))
                {
                    url += $"&keyboard={Uri.EscapeDataString(keyboard)}";
                }
                string response = Get(url);
                Console.WriteLine($"✅ Ответ отправлен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка отправки: {ex.Message}");
            }
        }

        static string Get(string url)
        {
            using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
            {
                return wc.DownloadString(url);
            }
        }

        static string GetKeyboard()
        {
            return @"
            {
                ""one_time"": false,
                ""buttons"": [
                    [
                        { ""action"": { ""type"": ""text"", ""label"": ""🌟 Привет"" }, ""color"": ""positive"" },
                        { ""action"": { ""type"": ""text"", ""label"": ""💰 Прайс"" }, ""color"": ""primary"" }
                    ],
                    [
                        { ""action"": { ""type"": ""text"", ""label"": ""🎁 Акции"" }, ""color"": ""primary"" },
                        { ""action"": { ""type"": ""text"", ""label"": ""📞 Помощь"" }, ""color"": ""secondary"" }
                    ],
                    [
                        { ""action"": { ""type"": ""text"", ""label"": ""🗣 Выбрать язык"" }, ""color"": ""primary"" },
                        { ""action"": { ""type"": ""text"", ""label"": ""📝 Тест"" }, ""color"": ""primary"" }
                    ],
                    [
                        { ""action"": { ""type"": ""text"", ""label"": ""👨‍💼 Позвать человека"" }, ""color"": ""negative"" },
                        { ""action"": { ""type"": ""text"", ""label"": ""👋 Пока"" }, ""color"": ""secondary"" }
                    ]
                ]
            }";
        }

        static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string result = text.ToLower().Trim();
            string[] emojis = { "🌟", "💰", "🎁", "📞", "🗣", "📝", "👨‍💼", "👋", "✅", "❓", "📍", "🎯", "✔", "🇬🇧", "🇨🇳", "🇯🇵", "🇰🇷", "🇪🇸", "🇫🇷", "🇩🇪", "🇮🇹", "🇹🇷" };
            foreach (string emoji in emojis)
            {
                result = result.Replace(emoji, "");
            }
            return result.Trim();
        }
    }
}