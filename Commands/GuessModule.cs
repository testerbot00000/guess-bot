using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Rest;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Diagnostics;

public class GuessModule : ModuleBase
{
    private static int Number = -1;
    private static int Tries = 0;
    public static bool GameRunning = false;
    public static ulong ChannelID = 0;
    
    [Command("setnumber"), Summary("Set a number for the guess command")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task SetNumber([Summary("The number to set the guess to")] int number)
    {
        Number = number;
        await ReplyAsync("Guess number set to " + Number);
    }

    [Command("setchannel"), Summary("Set the specified game channel")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task SetChannel()
    {
        ChannelID = Context.Channel.Id;
        await ReplyAsync("Channel set.");
    }

    public static async Task GuessNumber(int number, ISocketMessageChannel channel, IUser user, IGuildChannel guildChannel, CommandContext context)
    {
        if (GameRunning)
        {
            if (number == Number)
            {
                GameRunning = false;
                
                Tries++;
                RestUserMessage msg = await channel.SendMessageAsync("Congratulations " + user.Mention + ", " + number + " is correct! It took " + Tries + " tries to guess it.");
                await msg.PinAsync();
                Number = -1;
                Tries = 0;

                var role = context.Guild.GetRole(context.Guild.Roles.FirstOrDefault(x => x.Name.Equals("Member")).Id);
                var channelNew = await context.Guild.GetChannelAsync(ChannelID);

                // If either the of the object does not exist, bail
                if (role == null || channelNew == null) return;

                // Fetches the previous overwrite and bail if one is found
                var previousOverwrite = channelNew.GetPermissionOverwrite(role);
                if (previousOverwrite.HasValue) await channelNew.RemovePermissionOverwriteAsync(role);

                // Creates a new OverwritePermissions with send message set to deny and pass it into the method
                await channelNew.AddPermissionOverwriteAsync(role,
                    new OverwritePermissions(sendMessages: PermValue.Deny));

            }
            else Tries++;
        }
    }

    [Command("start"), Summary("Start the game")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task StartGame()
    {
        if (!GameRunning)
        {
            if (ChannelID != 0)
            {
                if (Number != -1)
                {
                    GameRunning = true;

                    var role = Context.Guild.GetRole(Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals("Member")).Id);
                    var channelNew = await Context.Guild.GetChannelAsync(ChannelID);

                    // If either the of the object does not exist, bail
                    if (role == null || channelNew == null) return;

                    // Fetches the previous overwrite and bail if one is found
                    var previousOverwrite = channelNew.GetPermissionOverwrite(role);
                    if (previousOverwrite.HasValue) await channelNew.RemovePermissionOverwriteAsync(role);

                    // Creates a new OverwritePermissions with send message set to deny and pass it into the method
                    await channelNew.AddPermissionOverwriteAsync(role,
                        new OverwritePermissions(sendMessages: PermValue.Allow));
                    
                    await ReplyAsync("A game has started!");
                    await (await Context.Guild.GetTextChannelAsync(ChannelID)).SendMessageAsync("A game has started. Guess the number! No decimals");
                }
                else
                {
                    await ReplyAsync("A number has not been set yet. Set it with `g. setnumber <number>`");
                }
            }
            else
            {
                await ReplyAsync("A channel has not been set. Set the channel with `g. setchannel` in the correct channel.");
            }
        }
        else
        {
            await ReplyAsync("A game is already running");
        }
    }

    [Command("stop"), Summary("Stops the game")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task StopGame()
    {
        if (GameRunning)
        {
            GameRunning = false;

            var role = Context.Guild.GetRole(Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals("Member")).Id);
            var channelNew = await Context.Guild.GetChannelAsync(ChannelID);

            // If either the of the object does not exist, bail
            if (role == null || channelNew == null) return;

            // Fetches the previous overwrite and bail if one is found
            var previousOverwrite = channelNew.GetPermissionOverwrite(role);
            if (previousOverwrite.HasValue) await channelNew.RemovePermissionOverwriteAsync(role);

            // Creates a new OverwritePermissions with send message set to deny and pass it into the method
            await channelNew.AddPermissionOverwriteAsync(role,
                new OverwritePermissions(sendMessages: PermValue.Deny));

            await ReplyAsync("A game has been stoppoed");
        }
        else
        {
            await ReplyAsync("A game is not running");
        }
    }
}
