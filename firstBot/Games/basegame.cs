using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;

namespace firstBot.Games
{
    abstract class Basegame
    {
        protected CommandContext ctx { get; private set; }
        protected Basegame(uint maxPlayers, CommandContext ctx) => MaxPlayers = maxPlayers;
        protected uint MaxPlayers { get; private set; }
        protected List<DiscordUser> Players { get; set; } = new List<DiscordUser>();
        protected async Task<(DiscordMessage, bool)> UserInput()
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

        protected async Task AddPlayers(DiscordMessage msg)
        {
            msg = await msg.ModifyAsync("Adding players....");
            Players.Add(ctx.User);
            uint currPlayers = 1;

            await AddMentionedPlayers(ctx.Message);
            
            while(currPlayers < MaxPlayers)
            {
                var interactivity = ctx.Client.GetInteractivityModule();
                msg = await msg.ModifyAsync($"Not Enough players, {ctx.User.Mention} add more players by tagging them:");
                var newMsg = await interactivity.WaitForMessageAsync(msg =>
                {
                    if (msg.Channel == ctx.Channel && msg.Author == ctx.User)
                        return true;
                    return false;
                }, TimeSpan.FromMinutes(1));
                await AddMentionedPlayers(newMsg?.Message);
            }

            async Task AddMentionedPlayers(DiscordMessage msg)
            {
                if (msg.MentionedUsers.Count != 0)
                {
                    while (currPlayers < MaxPlayers)
                    {
                        foreach (var usr in msg.MentionedUsers)
                        {
                            if (usr == null)
                                break;

                            Players.Add(usr);
                            ++currPlayers;
                            if (currPlayers == MaxPlayers)
                                break;
                        }
                    }
                }
                else
                {
                    await ctx.RespondAsync("Did not find any tagged members");
                }
            }

            await msg.DeleteAsync();
        }

    }
}
