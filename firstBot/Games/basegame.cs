using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;

namespace firstBot.Games
{
    abstract class Basegame
    {
        protected CommandContext ctx { get; private set; }
        protected Basegame(uint maxPlayers, CommandContext ctx) => (MaxPlayers, this.ctx) = (maxPlayers, ctx);
        protected uint MaxPlayers { get; private set; }
        protected List<DiscordUser> Players { get; set; } = new List<DiscordUser>();
        public async Task<List<DiscordUser>> AddPlayers(List<DiscordUser> Unavialable)
        {
            var UserRegex = new Regex(@"(<@\!?\d+?>)", RegexOptions.ECMAScript | RegexOptions.Compiled); 
            var usrConverter = new DiscordUserConverter();
            var msg = await ctx.RespondAsync("Adding players....");
            uint currPlayers = 0;
            AddPlayer(ctx.User);

            await AddMentionedPlayers(ctx.Message);
            
            while(currPlayers < MaxPlayers)
            {
                var interactivity = ctx.Client.GetInteractivityModule();
                await msg?.DeleteAsync();

                msg = await ctx.RespondAsync($"Not Enough players, {ctx.User.Mention} add more players by tagging them:");
                var newMsg = await interactivity.WaitForMessageAsync(msg =>
                {
                    if (msg.Channel == ctx.Channel && msg.Author == ctx.User)
                        return true;
                    return false;
                }, TimeSpan.FromMinutes(1));
                if (newMsg == null)
                {
                    await ctx.RespondAsync("Timed out, exiting game.");
                    break;
                }
                await AddMentionedPlayers(newMsg?.Message);
            }
            await msg?.DeleteAsync();
            
            return Players;

            async Task AddMentionedPlayers(DiscordMessage msg)
            {
                if (msg.MentionedUsers.Count != 0)
                {
                    while (currPlayers < MaxPlayers)
                    {
                        foreach (var usr in msg.MentionedUsers.Except(Unavialable))
                        {
                            if (usr == null || usr.IsBot)
                                continue;

                            AddPlayer(usr);
                            if (currPlayers == MaxPlayers)
                                break;
                        }
                        if (currPlayers == MaxPlayers)
                            break;
                        var matches = UserRegex.Matches(msg.Content);
                        foreach (var match in matches)
                        {
                            if (currPlayers == MaxPlayers)
                                break;
                            if (usrConverter.TryConvert(match.ToString(), ctx, out DiscordUser usr))
                                AddPlayer(usr);
                        }
                        break;
                    }
                }
                else
                {
                    await ctx.RespondAsync("Did not find any tagged members");
                }
            }
            void AddPlayer(DiscordUser usr)
            {
                Players.Add(usr);
                Unavialable.Add(usr);
                ++currPlayers;
            }
        }

    }
}
