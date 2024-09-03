using MellowBot.Services;
using Telegram.Bot;

class Program
{
    private static ITelegramBotClient botClient;
    private static readonly string telegram_token = "TELEGRAM_TOKEN";

    static async Task Main(string[] args)
    {
        var botService = new BotService(telegram_token);
        await botService.StartAsync();

        Console.WriteLine("Bot is running... Press Enter to exit.");
        Console.ReadLine();

        botService.Stop();
    }
}