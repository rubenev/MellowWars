using Telegram.Bot;
using Telegram.Bot.Types;

namespace MellowBot.Services
{
    using Handlers;

    public class BotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly CommandHandler _commandHandler;
        private CancellationTokenSource _cancellationTokenSource;

        public BotService(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
            var stateService = new StateService();
            var navigationService = new NavigationService(_botClient, stateService);
            _commandHandler = new CommandHandler(navigationService, stateService);
        }

        public async Task StartAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                cancellationToken: _cancellationTokenSource.Token
            );

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"Bot {me.Username} is running...");
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            Console.WriteLine("Bot has stopped.");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            await _commandHandler.HandleUpdateAsync(update, cancellationToken);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}