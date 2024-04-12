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
        public const string ViewNotes = "📒 Заметки";
        public const string EnterCategoryName = "Введите категорию:";
        public const string CategoryCreated = "Категория добавлена!";
    }

    public static class Elements
    {
        public const string ArrowLeft = "⬅️";
        public const string ArrowRight = "➡️";
        public const string GoBack = "↩️";
    }
}