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
        public const string CategoryTemplate = "*Категория: {0}*";
    }

    public class Notes
    {
        public const string EnterNoteTitle = "Введите название заметки:";
        public const string EnterNoteText = "Введите заметку:";
        public const string NoteCreated = "Заметка добавлена!";
        public const string NoteTemplate = "*{0}*\n\n{1}\n\n_Категория: {2}_";
    }

    public static class Users
    {
        public const string EnterName = "Введите имя:";
        public const string YouAreRegistered = "Вы зарегистрированны! Ваш код: {0}\n\nПродолжить - /start";
    }

    public static class Elements
    {
        public const string ArrowLeft = "⬅️";
        public const string ArrowRight = "➡️";
        public const string GoBack = "↩️";
        public const string AddCategory = "➕ Категорию";
        public const string AddNote = "➕ Заметку";
    }
}