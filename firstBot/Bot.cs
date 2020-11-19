using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using firstBot.Commands;

namespace firstBot
{
    class Bot
    {
        static private DiscordClient dsClient;
        static private CommandsNextModule commands;
        
        public Bot(string token, string prefix)
        {
            dsClient = CreateClient(token);
            HookEvents();

            // setup bot for commands
            InitateCommands(prefix);
            RegisterCommands();

            // setup bot to listen
            InitiateInteractivity();

        }

        /// <summary>
        /// Register the commands to use
        /// </summary>
        static private void RegisterCommands()
        {
            commands.RegisterCommands<myCommands>();
            commands.RegisterCommands<privateCommands>();
        }

        /// <summary>
        /// Runs the bot asynchronously
        /// </summary>
        /// <returns></returns>
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
        static private DiscordClient CreateClient(string token)
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

            dsClient.ClientErrored += async (e) =>
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Error, e.Client.CurrentApplication.Name, e.Exception.Message, DateTime.Now);
            };
        }

        /// <summary>
        /// establishes commands
        /// </summary>
        /// <param name="prefix"> prefix to be used </param>
        static private void InitateCommands(string prefix)
        {
            var config = new CommandsNextConfiguration()
            {
                StringPrefix = prefix,
                CaseSensitive = false,
                EnableDms = false,
            };

            commands = dsClient.UseCommandsNext(config);
        }

        /// <summary>
        /// establishes Interactivity
        /// </summary>
        static private void InitiateInteractivity()
        {
            var config = new InteractivityConfiguration()
            {Timeout = TimeSpan.FromMinutes(2)};

            dsClient.UseInteractivity(config);
        }
    }


}
