using TelegramHelper.Definitions;
using TelegramHelper.Infrastructure.Interfaces;
using TelegramHelper.Utils;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers;

public class CategoriesController : BotController
{
    private readonly IInlineButtonsGenerationService _buttonsGenerationService;
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(IInlineButtonsGenerationService buttonsGenerationService, ICategoriesService categoriesService)
    {
        _buttonsGenerationService = buttonsGenerationService;
        _categoriesService = categoriesService;
    }

    [Callback(nameof(DisplayCategories))]
    public async Task DisplayCategories()
    {
        var categories = await _categoriesService.GetCategories(0, 5);
        for (var i = 0; i < categories.Count; i += 2)
        {
            if (i + 1 <= categories.Count - 1)
            {
                _buttonsGenerationService.SetInlineButtons(categories[i].GetCategoryButton(), categories[i + 1].GetCategoryButton());
            }
            else
            {
                _buttonsGenerationService.SetInlineButtons(categories[i].GetCategoryButton());
            }
        }

        await Client.SendMdTextMessage(Update.GetChatId(),
            Messages.Categories.SelectCategory,
            replyMarkup: _buttonsGenerationService.GetButtons()
        );
    }
}