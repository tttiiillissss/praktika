using System;
using System.Collections.Generic;

namespace TeaBotFinal
{
    public static class LanguageTest
    {
        private static Dictionary<long, TestState> _userTests = new Dictionary<long, TestState>();

        private static List<Question> _questions = new List<Question>
        {
            new Question { Text = "1/5 Как будет «Привет» на английском?", Options = new[] { "Hello", "Goodbye", "Thanks", "Please" }, Correct = 0 },
            new Question { Text = "2/5 Как переводится «Кошка»?", Options = new[] { "Dog", "Cat", "Bird", "Fish" }, Correct = 1 },
            new Question { Text = "3/5 Выберите правильный вариант: «She ___ to school every day»", Options = new[] { "go", "goes", "going", "went" }, Correct = 1 },
            new Question { Text = "4/5 Что означает слово «Beautiful»?", Options = new[] { "Красивый", "Большой", "Быстрый", "Грустный" }, Correct = 0 },
            new Question { Text = "5/5 Как переводится «Несмотря на»?", Options = new[] { "Despite", "Because", "However", "Therefore" }, Correct = 0 }
        };

        public static string StartTest(long userId)
        {
            _userTests[userId] = new TestState { CurrentQuestion = 0, Score = 0 };
            return GetCurrentQuestion(userId);
        }

        private static string GetCurrentQuestion(long userId)
        {
            if (!_userTests.ContainsKey(userId)) return null;
            var state = _userTests[userId];
            if (state.CurrentQuestion >= _questions.Count) return FinishTest(userId);
            var q = _questions[state.CurrentQuestion];
            return $"{q.Text}\n\n1️⃣ {q.Options[0]}\n2️⃣ {q.Options[1]}\n3️⃣ {q.Options[2]}\n4️⃣ {q.Options[3]}\n\nНапишите номер ответа (1-4):";
        }

        public static string ProcessAnswer(long userId, string answerText)
        {
            if (!_userTests.ContainsKey(userId)) return null;
            var state = _userTests[userId];
            if (state.CurrentQuestion >= _questions.Count) return FinishTest(userId);

            if (int.TryParse(answerText, out int answerIndex) && answerIndex >= 1 && answerIndex <= 4)
            {
                var q = _questions[state.CurrentQuestion];
                if (answerIndex - 1 == q.Correct) state.Score++;
                state.CurrentQuestion++;
                return state.CurrentQuestion >= _questions.Count ? FinishTest(userId) : GetCurrentQuestion(userId);
            }
            return "❓ Пожалуйста, введите номер ответа (1, 2, 3 или 4):";
        }

        private static string FinishTest(long userId)
        {
            var state = _userTests[userId];
            int score = state.Score;
            string level = score >= 4 ? "B2" : score >= 3 ? "B1" : score >= 2 ? "A2" : "A1";
            DatabaseHelper.SaveUserLevel(userId, level);
            _userTests.Remove(userId);
            return $"✅ ТЕСТ ПРОЙДЕН!\n\nВаш результат: {score} из 5\n🎯 Ваш уровень: {level}";
        }

        public static bool IsUserTesting(long userId) => _userTests.ContainsKey(userId);
    }

    public class TestState { public int CurrentQuestion { get; set; } public int Score { get; set; } }
    public class Question { public string Text { get; set; } public string[] Options { get; set; } public int Correct { get; set; } }
}