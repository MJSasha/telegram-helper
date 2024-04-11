namespace TelegramHelper.Definitions;

public static class Messages
{
    public static class Base
    {
        public const string StartText = @"Привет, я бот для заметок. Выбери, чем мы с тобой сегодня будем заниматься.";
        public const string UnknownMessage = @"Такого не знаем...";
    }

    public static class Categories
    {
        public const string SelectCategory = "Выберите категорию:";
        public const string ViewNotes = "Посмотреть заметки";
    }

    public static class Elements
    {
        public const string ArrowLeft = "⬅️";
        public const string ArrowRight = "➡️";
    }
}