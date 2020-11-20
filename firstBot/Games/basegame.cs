using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;

namespace firstBot.Games
{
    abstract class Basegame
    {
        public uint MaxPlayers { get; private set; }
        public List<DiscordUser> Players { get; set; }
        public async Task<(DiscordMessage, bool)> UserInput(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivityModule();

            var msg = await interactivity.WaitForMessageAsync(msg =>
            {
                if (msg.Channel == ctx.Channel && msg.Author == ctx.User)
                    return true;
                return false;
            }, TimeSpan.FromSeconds(30));

            if (msg == null)
            {
                await ctx.RespondAsync($"{ctx.User.Username}#{ctx.User.Discriminator} did not respond in time");
                return (null, false);
            }
            else
                return (msg.Message, true);
        }
        public abstract Task<bool> ValidInput();

    }
}
