using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

class Program
{
    private DiscordSocketClient _client;

    static async Task Main(string[] args)
        => await new Program().MainAsync();

    private string token = Environment.GetEnvironmentVariable("BOT_TOKEN");

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMessages |
                GatewayIntents.MessageContent
        });

        _client.Log += Log;
        _client.MessageReceived += MessageReceived;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }

    private async Task MessageReceived(SocketMessage message)
    {
        try
        {
            // 본인이 보낸 메시지 예외처리
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            // 메시지 검사
            if(MessageInspect(message.Content))
                return;

            await MessageWrite(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private bool MessageInspect(string content)
    {
        if (!content.Contains("he's in "))
            return true;

        if (!content.Contains("Selling:"))
            return true;

        if (!content.Contains("Terul's Maw Shop:"))
            return true;

        if (!content.Contains("Video:"))
            return true;

        return false;
    }

    private async Task MessageWrite(SocketMessage message)
    {
        DateTime koreaTime =
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "Korea Standard Time"
                );

        // 내용 정리
        string content = message.Content;

        string location = content.Split("he's in ", 2)[1];
        location = location.Split(". ")[0].Trim();

        if (char.IsLower(location[0]))
        {
            location =
                char.ToUpper(location[0]) +
                location.Substring(1);
        }

        string strangerSection =
            content.Split("Selling:")[1]
           .Split("\n\n")[0]
           .Trim();

        string terulSection =
            content.Split("Terul's Maw Shop:")[1]
           .Split("\n\n")[0]
           .Trim();

        var match = Regex.Match(
            content,
            @"Video:\s*(https?:\/\/\S+)");

        string videoUrl = match.Groups[1].Value;

        var embed = new EmbedBuilder()
            .WithTitle($"🛒  {koreaTime:yyyy년 MM월 dd일} 히든 상점")
            .AddField(
            "🗺️  위치",
            $"```{location}```")
            .AddField(
            "🎩 스트레인저",
            $"```{strangerSection}```")
            .AddField(
            "🌊  테룰 상점",
            $"```{terulSection}```")
            .WithColor(Color.DarkBlue)
            .Build();

        await message.Channel.SendMessageAsync(
            embed: embed
            );

        await message.Channel.SendMessageAsync(
            text: videoUrl
            );
    }
}