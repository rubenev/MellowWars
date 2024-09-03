using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MellowBot.Services
{
    public class NavigationService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly StateService _stateService;

        public NavigationService(ITelegramBotClient botClient, StateService stateService)
        {
            _botClient = botClient;
            _stateService = stateService;
        }

        public async Task SendMainMenuAsync(long chatId, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Buildings", "navigate_buildings") },
                new[] { InlineKeyboardButton.WithCallbackData("Units", "navigate_units") },
                new[] { InlineKeyboardButton.WithCallbackData("Troops", "navigate_troops") }
            });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Main Menu:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        public async Task SendBuildingsMenuAsync(long chatId, CancellationToken cancellationToken)
        {
            var barracksState = _stateService.GetButtonState(chatId, "barracks");
            var foodDispenserState = _stateService.GetButtonState(chatId, "food_dispenser");

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData(barracksState.Text, barracksState.CallbackData) },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(foodDispenserState.Text, foodDispenserState.CallbackData)
                },
                new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "navigate_back") }
            });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Buildings Menu:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        public async Task SendUnitsMenuAsync(long chatId, CancellationToken cancellationToken)
        {
            var infantryCatState = _stateService.GetButtonState(chatId, "Infantry");
            var cavalryCatState = _stateService.GetButtonState(chatId, "Cavalry");

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"🐈‍⬛ {infantryCatState.Text}",
                        infantryCatState.CallbackData)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"🐈 {cavalryCatState.Text}", cavalryCatState.CallbackData)
                },
                new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "navigate_back") }
            });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Units Menu:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        public async Task SendTroopsMenuAsync(long chatId, CancellationToken cancellationToken)
        {
            var state = _stateService.GetUserState(chatId);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "navigate_back") }
            });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Current Troops Count:\n" +
                      $"Infantry Cats: {state.InfantryCatCount}\n" +
                      $"Cavalry Cats: {state.CavalryCatCount}",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        public async Task HandleButtonClickAsync(long chatId, string button, CancellationToken cancellationToken,
            string callbackQueryId)
        {
            try
            {
                // Acknowledge the callback query immediately
                await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQueryId,
                    text: $"Processing your request...",
                    showAlert: false,
                    cancellationToken: cancellationToken
                );

                var buttonKey = (chatId, button);

                // Check if the button is on a cooldown
                if (_stateService.IsButtonDisabled(buttonKey))
                {
                    var reactivationTime = _stateService.GetButtonReactivationTime(buttonKey);
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQueryId,
                        text: $"This button will be available at {reactivationTime:HH:mm:ss}.",
                        showAlert: false,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    // Process the button click and set a cooldown
                    _stateService.SetButtonCooldown(buttonKey, TimeSpan.FromSeconds(5));

                    // Send a second callback query acknowledgment if needed
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQueryId,
                        text: $"You clicked {button}. Button will be disabled for 5 minutes.",
                        showAlert: false,
                        cancellationToken: cancellationToken
                    );

                    switch (button)
                    {
                        // Update the respective menu
                        case "barracks":
                        case "food_dispenser":
                            await SendBuildingsMenuAsync(chatId, cancellationToken);
                            break;
                        case "Infantry":
                            _stateService.IncrementTroopCount(chatId, "Infantry");
                            await SendUnitsMenuAsync(chatId, cancellationToken);
                            break;
                        case "Cavalry":
                            _stateService.IncrementTroopCount(chatId, "Cavalry");
                            await SendUnitsMenuAsync(chatId, cancellationToken);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}