using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;


namespace firstBot
{
    class Bot
    {
        static private DiscordClient dsClient;

        public Bot(string token, string prefix)
        {
            dsClient = CreateClient(token, prefix);
            HookEvents();
        }
        public async Task RunBot()
        {
            await dsClient.ConnectAsync();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Create an instance of discord client
        /// </summary>
        /// <param name="token"> bot token</param>
        /// <param name="prefix"> the prefix to use commands on</param>
        /// <returns></returns>
        static private DiscordClient CreateClient(string token, string prefix)
        {
            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,

                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            };

            return new DiscordClient(config);
        }

        /// <summary>
        /// Hook required events
        /// </summary>
        static private void HookEvents()
        {
            dsClient.Ready += async (e) =>
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Info, e.Client.CurrentApplication.Name, "Client is ready.", DateTime.Now);    
            };
        }
    }

    
}
