using System;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace TeaBotFinal
{
    class Program
    {
        static string token = "vk1.a.1FobaNuXZzvkO24OGb819d_ihWbXjrLc_8S05xT5zsZYZHdc7yJut8raIiewblOy-6muFMIxMpno0XODXed3Zr5A7I9UgYbDRp3qu1stduhMHMjxwh0jnkcsKq0QFfh9Cpm7YB7raqj66zf9iq2t_FVWmL7JY_IGLMbSomT1FxZHGGaw5lStXXX4sLbtQR74l9h46mYC0AdBppdwPlXW8g";
        static ulong groupId = 239455819;

        static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║        🤖 TEA БОТ ЗАПУЩЕН 🤖         ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("✅ Токен загружен");
            Console.WriteLine($"✅ ID группы: {groupId}");
            Console.WriteLine();
            Console.WriteLine("📋 ДОСТУПНЫЕ КОМАНДЫ:");
            Console.WriteLine("   • привет / начать");
            Console.WriteLine("   • прайс");
            Console.WriteLine("   • помощь");
            Console.WriteLine("   • акции");
            Console.WriteLine("   • пока");
            Console.WriteLine();
            Console.WriteLine("⏳ Ожидание сообщений...");
            Console.WriteLine();

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

                    // Убираем https:// из server, если оно там есть
                    if (server.StartsWith("https://"))
                        server = server.Substring(8);
                    if (server.StartsWith("http://"))
                        server = server.Substring(7);

                    Console.WriteLine($"✅ Подключено к серверу, ts={ts}");
                    Console.WriteLine("🎧 Бот слушает сообщения...");

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
                                            string answer = GetAnswer(text);
                                            SendMessage(userId, answer);
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

        static string GetAnswer(string text)
        {
            if (text == "прайс" || text == "price")
            {
                return "💰 ПРАЙС-ЛИСТ TEA 💰\n\n" +
                       "🇬🇧 Английский язык:\n" +
                       "   • Индивидуально — 1500₽/час\n" +
                       "   • В паре — 1000₽/час\n" +
                       "   • Группа (4-6 чел) — 800₽/час\n\n" +
                       "🇨🇳 Китайский язык:\n" +
                       "   • Индивидуально — 1700₽/час\n" +
                       "   • Группа — 1000₽/час\n\n" +
                       "🎁 АКЦИЯ: Скидка 20% на первый месяц!\n\n" +
                       "📞 По вопросам записи напишите ПОМОЩЬ";
            }
            else if (text == "привет" || text == "начать" || text == "start")
            {
                return "🌟 Добро пожаловать в TEA!\n\n" +
                       "Я бот школы иностранных языков TEA.\n\n" +
                       "📋 Доступные команды:\n" +
                       "   • ПРАЙС — посмотреть цены\n" +
                       "   • ПОМОЩЬ — контакты\n" +
                       "   • АКЦИИ — скидки\n" +
                       "   • ПОКА — завершить диалог\n\n" +
                       "Напишите нужную команду!";
            }
            else if (text == "помощь" || text == "help")
            {
                return "📞 КОНТАКТЫ TEA:\n\n" +
                       "• Телефон: +7 (999) 123-45-67\n" +
                       "• Email: tea@school.ru\n" +
                       "• Адрес: ул. Ленина, д. 10\n\n" +
                       "🕐 Режим работы: Пн-Сб с 10:00 до 20:00\n\n" +
                       "Напишите ПРАЙС чтобы узнать стоимость занятий.";
            }
            else if (text == "акции")
            {
                return "🎁 ТЕКУЩИЕ АКЦИИ TEA 🎁\n\n" +
                       "1️⃣ Скидка 20% на первый месяц обучения\n" +
                       "2️⃣ Приведи друга — получи урок в подарок\n" +
                       "3️⃣ Оплата за месяц — один урок бесплатно";
            }
            else if (text == "пока" || text == "до свидания")
            {
                return "👋 До свидания! Будем рады видеть вас снова в TEA!";
            }
            else
            {
                return "❓ Я не понял ваш запрос.\n\n" +
                       "Пожалуйста, используйте команды:\n" +
                       "   • ПРИВЕТ — приветствие\n" +
                       "   • ПРАЙС — стоимость занятий\n" +
                       "   • ПОМОЩЬ — контакты\n" +
                       "   • АКЦИИ — текущие скидки\n" +
                       "   • ПОКА — завершить диалог";
            }
        }

        static void SendMessage(long userId, string text)
        {
            try
            {
                string url = $"https://api.vk.com/method/messages.send?peer_id={userId}&message={Uri.EscapeDataString(text)}&random_id={new Random().Next()}&access_token={token}&v=5.131&group_id={groupId}";
                string response = Get(url);
                Console.WriteLine($"✅ Ответ отправлен");

                if (response.Contains("\"error\""))
                {
                    Console.WriteLine($"⚠️ Ошибка API: {response}");
                }
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
    }
}