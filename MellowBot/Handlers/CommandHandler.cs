using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MellowBot.Handlers
{
    using Services;

    public class CommandHandler
    {
        private readonly NavigationService _navigationService;
        private readonly StateService _stateService;

        public CommandHandler(NavigationService navigationService, StateService stateService)
        {
            _navigationService = navigationService;
            _stateService = stateService;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await _navigationService.SendMainMenuAsync(update.Message.Chat.Id, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackData = update.CallbackQuery.Data;
                var chatId = update.CallbackQuery.Message.Chat.Id;
                var callbackQueryId = update.CallbackQuery.Id;

                switch (callbackData)
                {
                    case "navigate_buildings":
                        await _navigationService.SendBuildingsMenuAsync(chatId, cancellationToken);
                        break;
                    case "navigate_units":
                        await _navigationService.SendUnitsMenuAsync(chatId, cancellationToken);
                        break;
                    case "navigate_troops":
                        await _navigationService.SendTroopsMenuAsync(chatId, cancellationToken);
                        break;
                    case "barracks":
                    case "food_dispenser":
                    case "Infantry":
                    case "Cavalry":
                        await _navigationService.HandleButtonClickAsync(chatId, callbackData, cancellationToken,
                            callbackQueryId);
                        break;
                    default:
                        await _navigationService.SendMainMenuAsync(chatId, cancellationToken);
                        break;
                }
            }
        }
    }
}