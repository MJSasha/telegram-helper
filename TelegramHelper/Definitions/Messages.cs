namespace TelegramHelper.Definitions;

public static class Messages
{
    public static class Base
    {
        public const string StartText = @"Привет! 🌟 Я - твой личный бот для заметок. Выбери, чем мы с тобой сегодня будем заниматься.";
        public const string UnknownMessage = @"Ой, что-то пошло не так... Я не знаю, что ответить. 😕";
        public const string Canceled = @"Отмененно, продолжим работу /start!";
        public const string Deleted = "Успешно удалено, продолжим работу /start!";
    }

    public static class Categories
    {
        public const string SelectCategory = "Выбери категорию: 📚";
        public const string ViewNotes = "📒 Просмотреть заметки";
        public const string EnterCategoryName = "Назови новую категорию (/cancel для отмены):";
        public const string CategoryCreated = "Категория успешно добавлена! 🎉";
        public const string CategoryTemplate = "*Категория: {0}*";
    }

    public class Notes
    {
        public const string EnterNoteTitle = "Введи название заметки (/cancel для отмены):";
        public const string EnterNoteText = "Теперь введи текст заметки (/cancel для отмены):";
        public const string NoteCreated = "Заметка успешно добавлена! 📝";
        public const string NoteTemplate = "*{0}*\n\n{1}\n\n_Категория: {2}_";
        public const string CategoryTitleTemplate = "*Категория: {0}*\n\nЗдесь ты можешь посмотреть заметки из этой категории.";
    }

    public static class Users
    {
        public const string EnterName = "Как к тебе обращаться? 🤔";
        public const string YouAreRegistered = "Ты успешно зарегистрирован! Твой уникальный код: {0}\n\nДавай начнем! Нажми /start";
        public const string YouAlreadyRegistered = "Ты уже зарегистрирован...";
    }

    public static class Elements
    {
        public const string ArrowLeft = "⬅️ Назад";
        public const string ArrowRight = "Вперёд ➡️";
        public const string GoBack = "↩️ Назад";
        public const string AddCategory = "➕ Добавить категорию";
        public const string AddNote = "➕ Добавить заметку";
        public const string Delete = "❌ Удалить";
    }

    public static class Commands
    {
        public const string Start = "/start";
        public const string Reg = "/reg";
        public const string Cancel = "/cancel";
    }
}