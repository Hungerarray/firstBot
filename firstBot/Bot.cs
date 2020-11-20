using System;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
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
            InitateCommands(prefix);

            // setup bot for commands
            RegisterCommands();

            // setup bot to listen
            InitiateInteractivity();

            HookEvents(prefix);
        }

        /// <summary>
        /// Register the commands to use
        /// </summary>
        static private void RegisterCommands()
        {
            dsClient.DebugLogger.LogMessage(LogLevel.Info, "setup", "Registering commands", DateTime.Now);
            commands.RegisterCommands<myCommands>();
            commands.RegisterCommands<privateCommands>();
        }

        /// <summary>
        /// Runs the bot asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task RunBot()
        {
            dsClient.DebugLogger.LogMessage(LogLevel.Info, "setup", "Starting bot", DateTime.Now);
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
        static private void HookEvents(string prefix)
        {
            dsClient.DebugLogger.LogMessage(LogLevel.Info, "Setup" , "adding events", DateTime.Now);
            dsClient.Ready += (e) =>
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Info, e.Client.CurrentApplication.Name, "Client is ready.", DateTime.Now);
                return Task.CompletedTask;
            };

            dsClient.ClientErrored += (e) =>
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Error, e.Client.CurrentApplication.Name, e.Exception.Message, DateTime.Now);
                return Task.CompletedTask;
            };

            commands.CommandExecuted += (e) =>
            {
                e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, e.Context.Client.CurrentApplication.Name,
                    $"{e.Context.User.Username}#{e.Context.User.Discriminator}, executed {e.Command.Name} successfully, in {e.Context.Channel.Name}.",
                    DateTime.Now);
                return Task.CompletedTask;
            };

            commands.CommandErrored += async (e) =>
            {
                var user = e.Context.User;
                e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, e.Context.Client.CurrentApplication.Name,
                    $"{user.Username}#{user.Discriminator}, tried to execute {e.Command?.Name ?? "<unknown_Command>"}"
                    + $" in {e.Context.Channel.Name} and failed with {e.Exception.GetType()} with {e.Exception.Message}",DateTime.Now);

                
                DiscordEmbedBuilder embd = null;

                var ex = e.Exception;
                while (ex is AggregateException)
                    ex = ex.InnerException;

                dsClient.DebugLogger.LogMessage(LogLevel.Error, dsClient.CurrentApplication.Name, $"{ex.GetType()}: {ex.Message}", DateTime.Now);
                switch (ex)
                {
                    case CommandNotFoundException _: 
                        break;
                    case ChecksFailedException cfe:
                        if (cfe.FailedChecks.Any(x => x is RequirePermissionsAttribute || x is RequireUserPermissionsAttribute || x is RequireOwnerAttribute || x is RequireRolesAttributeAttribute))
                        {
                            embd = new DiscordEmbedBuilder()
                            {
                                Title = "Premission Denied!",
                                Description = ":octagonal_sign: You lack necessary permission to execute this task",
                                Color = new DiscordColor(0xFF0000)
                            };
                        }
                        break;
                    default:
                        embd = new DiscordEmbedBuilder()
                        {
                            Title = "A problem occured while executing the command",
                            Description = $"{Formatter.InlineCode(e.Command.QualifiedName)} threw an exception: {ex.GetType()}: {ex.Message}"
                             + $" Try {Formatter.InlineCode(prefix + "help")} ",
                            Color = new DiscordColor(0xFF0000)
                        };
                        break;
                }

                if (embd != null)
                    await e.Context.RespondAsync(embed: embd);
            };
        }

        /// <summary>
        /// establishes commands
        /// </summary>
        /// <param name="prefix"> prefix to be used </param>
        static private void InitateCommands(string prefix)
        {
            dsClient.DebugLogger.LogMessage(LogLevel.Info, "setup", "setting up commandsNext", DateTime.Now);
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
            dsClient.DebugLogger.LogMessage(LogLevel.Info, "setup", "setting up InteractivityModule", DateTime.Now);
            var config = new InteractivityConfiguration()
            {Timeout = TimeSpan.FromMinutes(2)};

            dsClient.UseInteractivity(config);
        }
        
        
    }


}
