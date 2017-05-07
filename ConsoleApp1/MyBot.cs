using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Commands;
using Discord.Audio;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using VideoLibrary;
using EmergenceGuardian;
using System.Diagnostics;
using YoutubeExtractor;

namespace ConsoleApp1
{
    class MyBot
    {
        DiscordClient discord;

        public MyBot()
        {
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            discord.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            List<String> commandList = new List<String>();

            var commands = discord.GetService<CommandService>();
            Random rnd = new Random();

            //deprecated so it isn't in the list
            //commandList.Add("whiteboard");
            commands.CreateCommand("whiteboard")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Sends the link to the whiteboard web app

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        await e.Channel.SendMessage("The whiteboard app can be found at https://awwapp.com/");
                    }
                });

            //deprecated so it isn't in the list
            //commandList.Add("verify");
            commands.CreateCommand("verify")
                .Parameter("RequestedName", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Sets the user's server wide nickname to the requested name

**Arguments:**
`RequestedName` - The name to be set as the user's nickname

**Restrictions:**
Can only be used in the verification channel";
                    if (e.GetArg("RequestedName") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        if (e.Channel.Name == "get_verified")
                        {
                            await e.User.Edit(null, null, null, null, $"{e.GetArg("RequestedName")}");
                        }
                        else
                        {
                            await e.Channel.SendMessage("This is not the verification channel");
                        }
                    }
                });

            //admin only command so it isn't in the list
            //commandList.Add("purge");
            commands.CreateCommand("purge")
                .Parameter("NumberOfMessages", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Purges the specified number of messages from the channel

**Arguments:**
`NumberOfMessages` - The number of messages to be deleted, must be an integer. Defaults to 99 for rate limit reasons

**Restrictions:**
Can only be used by users with administrator permissions on the server";

                    int NumberOfMessages = 0;
                    if (e.GetArg("NumberOfMessages") == "")
                    {
                        NumberOfMessages = 99;
                    }
                    if (e.GetArg("NumberOfMessages") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (Int32.TryParse(e.GetArg("NumberOfMessages"), out NumberOfMessages) || e.GetArg("NumberOfMessages") == "")
                    {
                        if (e.User.ServerPermissions.Administrator == true)
                        {
                            Message[] messagesToDelete;
                            messagesToDelete = await e.Channel.DownloadMessages(NumberOfMessages + 1);
                            await e.Channel.DeleteMessages(messagesToDelete);
                        }
                        else
                        {
                            await e.Channel.SendMessage("You do not have permission to use this command");
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage("That's not a valid number");
                    }
                });

            commandList.Add("math");
            commands.CreateCommand("math")
                .Parameter("RawInput", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Queries Wolfram Alpha with the given parameter and returns the response as a .gif file sent in chat

**Arguments:**
`RawInput` - The query to be sent to Wolfram Alpha

**Restrictions:**
None";

                    if (e.GetArg("RawInput") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string FormattedInput = SoftwareKobo.Net.WebUtility.UrlEncode($"{e.GetArg("RawInput")}");
                        string url = "https://api.wolframalpha.com/v1/simple?input=" + FormattedInput + "&appid=Q4G43K-6Y3AXL983V";
                        await e.Channel.SendMessage("Thinking...");
                        await e.Channel.SendIsTyping();
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(new Uri(url), @"C:\users\Seth Dolin\Desktop\PhysicsBot\WolframOutput.gif");
                        }
                        Message[] MessagesToDelete = await e.Channel.DownloadMessages(1);
                        await e.Channel.DeleteMessages(MessagesToDelete);
                        await e.Channel.SendFile(@"C:\users\Seth Dolin\Desktop\PhysicsBot\WolframOutput.gif");
                        await e.Channel.SendMessage("Click to enlarge");
                    }
                });

            commandList.Add("roulette");
            commands.CreateCommand("roulette")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Plays Russian Roulette

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        if (rnd.Next(1, 7) == 6)
                        {
                            await e.Channel.SendMessage(@":boom::gun:
***BANG!!***
" + e.User.Name + " has died!");
                        }
                        else
                        {
                            await e.Channel.SendMessage(@":sweat_smile::gun:
*click*
" + e.User.Name + " has survived");
                        }
                    }
                });

            commandList.Add("slots");
            commands.CreateCommand("slots")
                .Parameter("BetAmount", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Plays slots

**Arguments:**
`BetAmount` - The amount that you wish to bet (integer)

**Restrictions:**
You must possess the amount of tokens that you wish to gamble
To find the number of tokens that you possess, use `!tokens`

The payouts are shown below. If you believe that the payouts are being calculated incorrectly, please let me know.";

                    ulong BetAmount = 0;

                    if (e.GetArg("BetAmount") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                        await e.Channel.SendFile(@"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Payouts.png");
                    }
                    else if (ulong.TryParse(e.GetArg("BetAmount"), out BetAmount))
                    {
                        if (e.Channel.Id.ToString() == "308360449509031936")
                        {
                            ulong tokens = GetTokens(e.User.Id.ToString());
                            tokens /= 2;
                            SetTokens(e.User.Id.ToString(), tokens);
                            await e.Channel.SendMessage("Use the ***FUCKING*** slots channel for slots. For your infractions, your token count has been halved.");
                        }
                        else {
                            double num1 = Math.Pow(2, rnd.Next(1, 9));
                            double num2 = Math.Pow(2, rnd.Next(9, 17));
                            double num3 = Math.Pow(2, rnd.Next(17, 25));

                            string Id = e.User.Id.ToString();
                            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\PlayerList.txt";
                            ulong currentTokens = GetTokens(Id);
                            ulong newTokens = 0;
                            if (!File.ReadAllText(FileAddress).Contains(Id))
                            {
                                await e.Channel.SendMessage("You are not registered in the token database. Please contact a moderator to have them add you and give you starting tokens");
                            }
                            else if (BetAmount > currentTokens)
                            {
                                await e.Channel.SendMessage("You do not have enough tokens to perform this bet");
                            }
                            else
                            {
                                string img1 = "";
                                string img2 = "";
                                string img3 = "";
                                ulong BetReturn = 0;

                                switch (num1)
                                {
                                    case 2:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Blank.png";
                                        break;

                                    case 4:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\RedBar.png";
                                        break;

                                    case 8:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\WhiteBar.png";
                                        break;

                                    case 16:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\BlueBar.png";
                                        break;

                                    case 32:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Red7.png";
                                        break;

                                    case 64:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\White7.png";
                                        break;

                                    case 128:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Blue7.png";
                                        break;

                                    case 256:
                                        img1 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Flag7.png";
                                        break;

                                    default:
                                        break;
                                }

                                switch (num2)
                                {
                                    case 512:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Blank.png";
                                        break;

                                    case 1024:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\RedBar.png";
                                        break;

                                    case 2048:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\WhiteBar.png";
                                        break;

                                    case 4096:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\BlueBar.png";
                                        break;

                                    case 8192:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Red7.png";
                                        break;

                                    case 16384:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\White7.png";
                                        break;

                                    case 32768:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Blue7.png";
                                        break;

                                    case 65536:
                                        img2 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Flag7.png";
                                        break;

                                    default:
                                        break;
                                }

                                switch (num3)
                                {
                                    case 131072:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Blank.png";
                                        break;

                                    case 262144:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\RedBar.png";
                                        break;

                                    case 524288:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\WhiteBar.png";
                                        break;

                                    case 1048576:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\BlueBar.png";
                                        break;

                                    case 2097152:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Red7.png";
                                        break;

                                    case 4194304:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\White7.png";
                                        break;

                                    case 8388608:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Blue7.png";
                                        break;

                                    case 16777216:
                                        img3 = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Flag7.png";
                                        break;

                                    default:
                                        break;
                                }

                                //Begin the incredibly long and inefficient series of 'if' statements here
                                int SlotValue = System.Convert.ToInt32(num1) + System.Convert.ToInt32(num2) + System.Convert.ToInt32(num3);
                                if (SlotValue == 16843008)//Flag 7, Flag 7, Flag 7
                                {
                                    BetReturn = BetAmount * 4000;
                                }
                                else if (SlotValue == 8405024)//Red 7, White 7, Blue 7
                                {
                                    BetReturn = BetAmount * 400;
                                }
                                else if (SlotValue == 2105376)//Red 7, Red 7, Red 7
                                {
                                    BetReturn = BetAmount * 300;
                                }
                                else if (SlotValue == 4210752)//White 7, White 7, White 7
                                {
                                    BetReturn = BetAmount * 200;
                                }
                                else if (SlotValue == 8421504)//Blue 7, Blue 7, Blue 7
                                {
                                    BetReturn = BetAmount * 100;
                                }
                                else if ((num1 == 32 || num1 == 64 || num1 == 128 || num1 == 256) && (num2 == 8192 || num2 == 16384 || num2 == 32768 || num2 == 65536) && (num3 == 2097152 || num3 == 4194304 || num3 == 8388608 || num3 == 16777216))//Any 7, Any 7, Any 7
                                {
                                    BetReturn = BetAmount * 50;
                                }
                                else if (SlotValue == 1050628)//Red Bar, White Bar, Blue Bar
                                {
                                    BetReturn = BetAmount * 50;
                                }
                                else if (SlotValue == 1052688)//Blue Bar, Blue Bar, Blue Bar
                                {
                                    BetReturn = BetAmount * 40;
                                }
                                else if (SlotValue == 526344)//White Bar, White Bar, White Bar
                                {
                                    BetReturn = BetAmount * 20;
                                }
                                else if ((num1 == 4 || num1 == 32) && (num2 == 2048 || num2 == 16384) && (num3 == 1048576 || num3 == 8388608))//Any Red, Any White, Any Blue
                                {
                                    BetReturn = BetAmount * 20;
                                }
                                else if (SlotValue == 263172)//Red Bar, Red Bar, Red Bar
                                {
                                    BetReturn = BetAmount * 10;
                                }
                                else if ((num1 == 4 || num1 == 8 || num1 == 16) && (num2 == 1024 || num2 == 2048 || num2 == 4096) && (num3 == 252144 || num3 == 524288 || num3 == 1048576))//Any Bar, Any Bar, Any Bar
                                {
                                    BetReturn = BetAmount * 5;
                                }
                                else if ((num1 == 256 && (num2 == 65536 || num3 == 16777216)) || (num2 == 65536 && (num1 == 256 || num3 == 16777216)))//Contains 2 Flag 7's
                                {
                                    BetReturn = BetAmount * 5;
                                }
                                else if ((num1 == 4 || num1 == 32) && (num2 == 1024 || num2 == 8192) && (num3 == 262144 || num3 == 2097152))//Any Red, Any Red, Any Red
                                {
                                    BetReturn = BetAmount * 2;
                                }
                                else if ((num1 == 8 || num1 == 64) && (num2 == 2048 || num2 == 16384) && (num3 == 524288 || num3 == 4194304))//Any White, Any White, Any White
                                {
                                    BetReturn = BetAmount * 2;
                                }
                                else if ((num1 == 4 || num1 == 32) && (num2 == 1024 || num2 == 8192) && (num3 == 1048576 || num3 == 8388608))//Any Blue, Any Blue, Any Blue
                                {
                                    BetReturn = BetAmount * 2;
                                }
                                else if (num1 == 256 || num2 == 65536 || num3 == 16777216)//Contains 1 Flag 7
                                {
                                    BetReturn = BetAmount * 2;
                                }
                                else if (SlotValue == 131586)//Blank, Blank, Blank
                                {
                                    BetReturn = BetAmount;
                                }
                                else
                                {
                                    BetReturn = 0;
                                }
                                //End the incredibly long and inefficient series of 'if' statements here

                                newTokens = currentTokens - BetAmount + BetReturn;
                                await e.Channel.SendFile(img1);
                                await e.Channel.SendFile(img2);
                                await e.Channel.SendFile(img3);

                                if (BetReturn > 0)
                                {
                                    await e.Channel.SendMessage(e.User.Name + " has won " + string.Format("{0:n0}", BetReturn) + @" tokens
" + e.User.Name + " now has " + string.Format("{0:n0}", newTokens) + " tokens");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(e.User.Name + " has lost " + string.Format("{0:n0}", BetAmount) + @" tokens
" + e.User.Name + " now has " + string.Format("{0:n0}", newTokens) + " tokens");
                                }

                                //Save new token count
                                SetTokens(e.User.Id.ToString(), newTokens);
                            }
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage("That is not a valid integer");
                    }
                });

            commandList.Add("id");
            commands.CreateCommand("id")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Gets the user id of the user that used the command

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.GetArg("param") == "")
                    {
                        await e.Channel.SendMessage("Your user id is " + e.User.Id);
                    }
                    else
                    {
                        //need to figure out how to get user id of a user with a specific nickname and put it here
                    }
                });

            commandList.Add("register");
            commands.CreateCommand("register")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Registers the user in the token database

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\PlayerList.txt";
                        var oldLines = File.ReadAllLines(FileAddress);
                        int length = oldLines.Length;
                        string[] newLines = new string[length + 2];
                        for (int i = 0; i < length; i++)
                        {
                            newLines[i] = oldLines[i];
                        }
                        newLines[length] = e.User.Id.ToString();
                        newLines[length + 1] = "500";
                        File.WriteAllLines(FileAddress, newLines);
                        await e.Channel.SendMessage("User " + e.Server.GetUser(UInt64.Parse(e.GetArg("UserId"))).Nickname + " has been successfully added to the database");
                    }
                });

            //admin only command so it isn't in the list
            //commandList.Add("adduser");
            commands.CreateCommand("adduser")
                .Parameter("UserId", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Adds a user to the token database

**Arguments:**
`UserId` - The user id of the user to be added. User Id can be acquired by having the user type `!id`

**Restrictions:**
You must be an administrator on the server to use this command";
                    if (e.GetArg("UserId") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.User.ServerPermissions.Administrator == true)
                    {
                        await e.Channel.SendMessage("Got to p2");
                        string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\PlayerList.txt";
                        var oldLines = File.ReadAllLines(FileAddress);
                        int length = oldLines.Length;
                        string[] newLines = new string[length + 2];
                        for (int i = 0; i < length; i++)
                        {
                            newLines[i] = oldLines[i];
                        }
                        newLines[length] = e.GetArg("UserId");
                        newLines[length + 1] = "0";
                        File.WriteAllLines(FileAddress, newLines);
                        await e.Channel.SendMessage("User with id " + e.GetArg("UserId") + " has been successfully added to the database");
                    }
                    else
                    {
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            //admin only command so it isn't in the list
            //commandList.Add("bonus");
            commands.CreateCommand("bonus")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Gives the specified amount of bonus tokens to the specified user

**Arguments:**
`UserId` - The id of the user to be given the bonus tokens. A user's Id can be found by using !id
`BonusTokens` - The amount of bonus tokens to be given to the user

**Restrictions:**
You must be an administrator on the server to use this command";
                    string param = e.GetArg("param");
                    if (param == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.User.ServerPermissions.Administrator == true)
                    {
                        int spaceIndex = param.IndexOf(" ");
                        string userId = param.Substring(0, spaceIndex);
                        ulong bonusTokens = ulong.Parse(param.Substring(spaceIndex, param.Length - spaceIndex));
                        ulong currentTokens = GetTokens(userId);
                        ulong newTokens = currentTokens + bonusTokens;
                        SetTokens(userId, newTokens);
                        await e.Channel.SendMessage("User " + e.Server.GetUser(UInt64.Parse(userId)).Name + " has been given " + string.Format("{0:n0}", bonusTokens) + @" bonus tokens.
" + e.Server.GetUser(UInt64.Parse(userId)).Name + " now has " + string.Format("{0:n0}", newTokens) + " tokens");
                    }
                    else
                    {
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            //admin only command so it isn't in the list
            //commandList.Add("take");
            commands.CreateCommand("take")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Takes the specified amount of tokens from the specified user

**Arguments:**
`UserId` - The id of the user to be given the bonus tokens. A user's Id can be found by using !id
`Tokens` - The amount of tokens to be taken from the user

**Restrictions:**
You must be an administrator on the server to use this command";
                    string param = e.GetArg("param");
                    if (param == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.User.ServerPermissions.Administrator == true)
                    {
                        int spaceIndex = param.IndexOf(" ");
                        string userId = param.Substring(0, spaceIndex);
                        ulong bonusTokens = ulong.Parse(param.Substring(spaceIndex, param.Length - spaceIndex));
                        ulong currentTokens = GetTokens(userId);
                        ulong newTokens = currentTokens - bonusTokens;
                        SetTokens(userId, newTokens);
                        await e.Channel.SendMessage("User " + e.Server.GetUser(UInt64.Parse(userId)).Name + " has had " + string.Format("{0:n0}", bonusTokens) + @" tokens taken from them.
" + e.Server.GetUser(UInt64.Parse(userId)).Name + " now has " + string.Format("{0:n0}", newTokens) + " tokens");
                    }
                    else
                    {
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            commandList.Add("tokens");
            commands.CreateCommand("tokens")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Gets the token count for the user that uses it

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param") == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        ulong tokens = GetTokens(e.User.Id.ToString());

                        await e.Channel.SendMessage("You have " + string.Format("{0:n0}", tokens) + " tokens");
                    }
                });

            commandList.Add("give");
            commands.CreateCommand("give")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Gives the specified user the specified amount of tokens from the bank of the user that used the command

**Arguments:**
`ReceiveId` - The user id of the user to receive the tokens
`Tokens` - The amount of tokens that you wish to give the user

**Restrictions:**
You must possess the amount of tokens that you wish to give";
                    string param = e.GetArg("param");
                    if (param == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        int spaceIndex = param.IndexOf(" ");
                        string receiveId = param.Substring(0, spaceIndex);
                        string giveId = e.User.Id.ToString();
                        ulong tokens = ulong.Parse(param.Substring(spaceIndex, param.Length - spaceIndex));
                        if (GetTokens(e.User.Id.ToString()) < tokens) {
                            await e.Channel.SendMessage("You do not have enough tokens to perform this action");
                        }
                        else
                        {
                            SetTokens(giveId, GetTokens(giveId) - tokens);
                            SetTokens(receiveId, GetTokens(receiveId) + tokens);
                            await e.Channel.SendMessage("User " + e.User.Name + " has successfully given user with user id " + receiveId + " " + string.Format("{0:n0}", tokens) + @" tokens.
" + e.User.Name + " now has " + string.Format("{0:n0}", GetTokens(giveId)) + @" tokens
User with user id " + receiveId + " now has " + string.Format("{0:n0}", GetTokens(receiveId)) + " tokens");
                        }
                    }
                });

            commandList.Add("chess");
            commands.CreateCommand("chess")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string desc = @"**Description:**
Makes an ASCII chess board which can be edited or displayed using this command

**Arguments:**
`Action` - The action to take. Can be `create`, `move`, `display`, `delete`, or `checkmate`
`BoardName` - The name of the board which you want to edit or display. Set when the board is created
`Square1` - The location of the piece that you would like to move. Given in the format LetterNumber. e.g. A6. Not necessary for actions `create`, `display`, and `delete`
`Square2` - The location that you would like to move the selected piece to. Given in the format LetterNumber. e.g. A6. Not necessary for actions `create`, `display`, and `delete`

**Restrictions:**
None";
                    string arg = e.GetArg("param");

                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        int spaceIndex1 = GetNthIndex(arg, ' ', 1);
                        int spaceIndex2 = GetNthIndex(arg, ' ', 2);
                        int spaceIndex3 = GetNthIndex(arg, ' ', 3);
                        string action = arg.Substring(0, spaceIndex1);
                        char[,] board = new char[8, 8];

                        if (action == "create")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            CreateBoard(boardName);

                            await e.Channel.SendMessage("Board with name " + boardName + " successfully created");
                        }
                        else if (action == "display")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            await e.Channel.SendMessage("**Board: **" + boardName + @"
" + BoardToString1(boardName, e.Server.Id.ToString()));
                            await e.Channel.SendMessage(BoardToString2(boardName, e.Server.Id.ToString()));
                            await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                        }
                        else if (action == "move")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - (7 + spaceIndex1));
                            string square1 = arg.Substring(spaceIndex2 + 1, 2);
                            string square2 = arg.Substring(spaceIndex3 + 1, 2);
                            board = GetBoard(boardName);
                            int[] oldCoord = new int[2];
                            oldCoord = parseChessLocation(square1);
                            int[] newCoord = new int[2];
                            newCoord = parseChessLocation(square2);
                            int oldX = oldCoord[0];
                            int oldY = oldCoord[1];
                            int newX = newCoord[0];
                            int newY = newCoord[1];

                            if (isLegal(square1, square2, board[oldX, oldY], board, isWhitesTurn(boardName)))
                            {
                                if (board[newX, newY] == '5')
                                {
                                    await e.Channel.SendMessage("White is in check. You must move out of check this turn. If you cannot move out of check, type `!chess checkmate boardname (location of piece that is placing you in check) (location of king)` to confirm the checkmate, and end the game.");
                                }
                                else if (board[newX, newY] == 'b')
                                {
                                    await e.Channel.SendMessage("Black is in check. You must move out of check this turn. If you cannot move out of check, type `!chess checkmate boardname (location of piece that is placing you in check) (location of king)` to confirm the checkmate, and end the game.");
                                }
                                else
                                {
                                    board[newX, newY] = board[oldX, oldY];
                                    board[oldX, oldY] = '0';
                                    SaveBoard(boardName, board, !isWhitesTurn(boardName));
                                }
                                await e.Channel.SendMessage("**Board: **" + boardName + @"
" + BoardToString1(boardName, e.Server.Id.ToString()));
                                await e.Channel.SendMessage(BoardToString2(boardName, e.Server.Id.ToString()));
                                await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                            }
                            else
                            {
                                await e.Channel.SendMessage("That move is illegal. Please try again");
                            }
                        }
                        else if (action == "checkmate")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - (7 + spaceIndex1));
                            string square1 = arg.Substring(spaceIndex2 + 1, 2);
                            string square2 = arg.Substring(spaceIndex3 + 1, 2);
                            board = GetBoard(boardName);
                            int[] oldCoord = new int[2];
                            oldCoord = parseChessLocation(square1);
                            int[] newCoord = new int[2];
                            newCoord = parseChessLocation(square2);
                            int oldX = oldCoord[0];
                            int oldY = oldCoord[1];
                            int newX = newCoord[0];
                            int newY = newCoord[1];

                            if (isLegal(square1, square2, board[oldX, oldY], board, isWhitesTurn(boardName)))
                            {
                                if (board[newX, newY] == '5')
                                {
                                    await e.Channel.SendMessage(@"Black has won.
**Final Board: **" + boardName + @"
" + BoardToString1(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage(BoardToString2(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                                    DeleteBoard(boardName);
                                }
                                else if (board[newX, newY] == 'b')
                                {
                                    await e.Channel.SendMessage(@"White has won.
**Final Board: **" + boardName + @"
" + BoardToString1(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage(BoardToString2(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                                    DeleteBoard(boardName);
                                }
                            }
                        }
                        else if (action == "delete")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            DeleteBoard(boardName);
                            await e.Channel.SendMessage("Board with name " + boardName + " has been successfully deleted");
                        }
                        /*else if (action == "gamble")
                        {
                            await e.Channel.SendMessage("got to point 1");
                            string boardName = arg.Substring(spaceIndex1 + 1, spaceIndex3 - spaceIndex2);
                            await e.Channel.SendMessage(boardName);
                            //SetBets(boardName, betAmount, e.User.Id);
                            
                        }*/
                        else
                        {
                            await e.Channel.SendMessage("One or more arguments are in the incorrect format. Please try again");
                        }
                    }
                });

            commandList.Add("serverid");
            commands.CreateCommand("serverid")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage(e.Server.Id.ToString());
                });

            commandList.Add("lucas");
            commands.CreateCommand("lucas")
                .Parameter("termNumber", ParameterType.Unparsed)
                .Do(async (e) => {
                    string arg = e.GetArg("termNumber");
                    string desc = @"**Description:**
Returns the requested term from the Lucas Sequence

**Arguments:**
`TermNumber` - The index of the term that you would like to request

**Restrictions:**
Due to operator overflow errors in the calculation of the sequence, `TermNumber` must be less than 3225";
                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string term = GetLucas(Int32.Parse(arg));
                        await e.Channel.SendMessage("The " + arg + "th term of the Lucas Sequence is " + term);
                    }
                });

            commandList.Add("fibonacci");
            commands.CreateCommand("fibonacci")
                .Parameter("termNumber", ParameterType.Unparsed)
                .Do(async (e) => {
                    string arg = e.GetArg("termNumber");
                    string desc = @"**Description:**
Returns the requested term from the Fibonacci Sequence

**Arguments:**
`TermNumber` - The index of the term that you would like to request

**Restrictions:**
Due to operator overflow errors in the calculation of the sequence, `TermNumber` must be less than 3225";
                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string term = GetFibonacci(Int32.Parse(arg));
                        await e.Channel.SendMessage("The " + arg + "th term of the Fibonacci Sequence is " + term);
                    }
                });

            //Currently broken for message amounts over 100. Can't seem to get it to loop properly
            commands.CreateCommand("getmessages")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    if (e.User.Id == 193399026748620800)
                    {
                        int numOfMessages = Int32.Parse(e.GetArg("param"));
                        //deletes the message containing the command to download from the channel
                        Message[] messages = new Message[numOfMessages];
                        var message = await e.Channel.DownloadMessages(2);
                        messages[1] = message[1];
                        await e.Channel.DeleteMessages(message);
                        string[] lines = new string[numOfMessages];

                        for (int i = 0; i < (numOfMessages / 100); i++)
                        {
                            Console.WriteLine(messages[(messages[0] == null) ? (i * 100 - 1) : (1)].RawText);
                            messages.Concat(await e.Channel.DownloadMessages(100, messages[(messages[0] == null) ? (i * 100 - 1) : (1)].Id));
                        }

                        for (int j = 0; j < numOfMessages; j++)
                        {
                            lines[j] = messages[j].RawText;
                        }
                        string fileAddress = @"C:\users\Seth Dolin\Desktop\DiscordNeuralNetwork\Messages.txt";


                        File.WriteAllLines(fileAddress, lines);
                    }
                });

            //commandList.Add("play");
            //Currently broken. Need to figure out how to get the audio downloader to extract the audio properly, then it should work fine from there
            commands.CreateCommand("play")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string arg = e.GetArg("param");
                    string desc = @"**Description:**
Plays the audio from a given YouTube video

**Arguments:**
`Link` - The link to the YouTube video whose audio you wish to play

**Restrictions:**
None";
                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        //when you fix this, remember to uncomment the commandList.Add thing at the top
                        string filePath = @"C:\users\Seth Dolin\Desktop\Physicsbot\Music\Song.mp3";
                        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(arg);
                        VideoInfo video = videoInfos.First();
                        var audioDownloader = new AudioDownloader(video, filePath);
                        await e.Channel.SendMessage("Got to point 1");
                        audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
                        audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);
                        await e.Channel.SendMessage("Got to point 2");
                        //completes the download just fine, can't seem to extract for some reason
                        audioDownloader.Execute();

                        var voiceChannel = e.User.VoiceChannel;
                        await discord.GetService<AudioService>().Join(voiceChannel);
                        var _vClient = e.Server.GetAudioClient();
                        await _vClient.Join(voiceChannel);

                        var channelCount = discord.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                        var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                        using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                        using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                        {
                            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                            int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                            byte[] buffer = new byte[blockSize];
                            int byteCount;

                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                            {
                                if (byteCount < blockSize)
                                {
                                    // Incomplete Frame
                                    for (int i = byteCount; i < blockSize; i++)
                                        buffer[i] = 0;
                                }
                                _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                            }
                        }

                        System.Threading.Thread.Sleep(5000);
                        await discord.GetService<AudioService>().Leave(e.Server);
                    }
                });
                
            commands.CreateCommand("help")
                .Do(async (e) =>
                {
                    string list = @"**Available commands** (prefix with '!'): :BRook:
";
                    for(int i = 0; i < commandList.ToArray().Length; i++)
                    {
                        list += "`" + commandList[i] + @"`
";
                    }
                        await e.Channel.SendMessage(list);
                });

			discord.ExecuteAndWait(async () =>
			{
				await discord.Connect("Mjk4NTk0NDQyNjAyODcyODM2.C8RnWg.QmRSfJ0atEkcwruNqFwqDwJJb_w", TokenType.Bot);
			});
		}

        private static BigInteger[] CalculateFibonacci(int n)
        {
            Console.WriteLine("Got to point 1");
            BigInteger[] terms = new BigInteger[n];
            terms[0] = 1;
            terms[1] = 1;
            for (int i = 2; i < n; i++)
            {
                terms[i] = (terms[i - 1] + terms[i - 2]);
            }
            return terms;
        }

        private static void SaveFibonacciString(BigInteger[] terms)
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Sequences\Fibonacci.txt";
            string[] lines = new string[terms.Length];
            for (int i = 0; i < terms.Length; i++)
            {
                lines[i] = terms[i].ToString();
            }

            File.WriteAllLines(fileAddress, lines);
        }

        private string GetFibonacci(int n)
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Sequences\Fibonacci.txt";

            var lines = File.ReadAllLines(fileAddress);
            return lines[n - 1];
        }

        private static BigInteger[] CalculateLucas(int n)
        {
            Console.WriteLine("Got to point 1");
            BigInteger[] terms = new BigInteger[n];
            terms[0] = 1;
            terms[1] = 3;
            for (int i = 2; i < n; i++)
            {
                terms[i] = (terms[i - 1] + terms[i - 2]);
            }
            return terms;
        }

        private static void SaveLucasString(BigInteger[] terms)
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Sequences\Lucas.txt";
            string[] lines = new string[terms.Length];
            for (int i = 0; i < terms.Length; i++)
            {
                lines[i] = terms[i].ToString();
            }

            File.WriteAllLines(fileAddress, lines);
        }

        private string GetLucas(int n)
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Sequences\Lucas.txt";

            var lines = File.ReadAllLines(fileAddress);
            return lines[n - 1];
        }

        private bool isWhitesTurn(string boardName)
        {
            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Chess\Boards.txt";
            StreamReader sr = new StreamReader(FileAddress);
            string line = "";
            int lineNumber = 1;

            line = sr.ReadLine();
            while (!line.Contains(boardName))
            {
                line = sr.ReadLine();
                lineNumber++;
            }
            sr.Close();

            if (line.Contains(":1"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int[] parseChessLocation(string square)
        {
            int row = Int32.Parse(square.Substring(1, 1)) - 1;
            int column = 0;
            int[] result = new int[2];
            switch (square.Substring(0, 1))
            {
                case "a":
                    column = 0;
                    break;
                case "b":
                    column = 1;
                    break;
                case "c":
                    column = 2;
                    break;
                case "d":
                    column = 3;
                    break;
                case "e":
                    column = 4;
                    break;
                case "f":
                    column = 5;
                    break;
                case "g":
                    column = 6;
                    break;
                case "h":
                    column = 7;
                    break;
            }

            result[0] = column;
            result[1] = row;
            return result;
        }

        private bool isLegal(string square1, string square2, char piece, char[,] board, bool isWhitesTurn)
        {
            /*pieces will be converted to and from integers as follows:
            0 = empty space
            1 = white pawn
            2 = white rook
            3 = white knight
            4 = white bishop
            5 = white king
            6 = white queen
            7 = black pawn
            8 = black rook
            9 = black knight
            10/A = black bishop
            11/B = black king
            12/C = black queen
            */

            int[] oldCoord = new int[2];
            oldCoord = parseChessLocation(square1);
            int[] newCoord = new int[2];
            newCoord = parseChessLocation(square2);
            int oldX = oldCoord[0];
            int oldY = oldCoord[1];
            int newX = newCoord[0];
            int newY = newCoord[1];
            int xChange = oldX - newX;
            int yChange = oldY - newY;
            int xChangeAbs = Math.Abs(xChange);
            int yChangeAbs = Math.Abs(yChange);
            bool isLegal = false;

            if ((isWhitesTurn && isWhiteOrBlank(board[oldX, oldY]) && board[oldX, oldY] != '0') || (!isWhitesTurn && isBlackOrBlank(board[oldX, oldY]) && board[oldX, oldY] != '0'))
            {
                //checks if the final square is blank or of the opposing team. If it is, it continues with the legality checker. Otherwise it returns illegal
                if (((piece == '1' || piece == '2' || piece == '3' || piece == '4' || piece == '5' || piece == '6') && (isBlackOrBlank(board[newX, newY]))) || (piece == '7' || piece == '8' || piece == '9' || piece == 'a' || piece == 'b' || piece == 'c') && (isWhiteOrBlank(board[newX, newY])))
                {
                    //pawns
                    if (piece == '1' || piece == '7')
                    {
                        if (yChange == ((piece == '1') ? 1 : -1) || (yChange == ((piece == '1') ? 2 : -2) && oldY == ((piece == '1') ? 6 : 1)))
                        {
                            if (xChangeAbs == 0 || (board[newX, newY] != '0' && xChangeAbs == 1 && yChangeAbs == 1))
                            {
                                isLegal = true;
                            }
                        }
                        return isLegal;
                    }

                    //rooks
                    else if (piece == '2' || piece == '8')
                    {
                        if (xChangeAbs == 0)
                        {
                            isLegal = true;
                            if (yChange > 0)
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX, oldY - i] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX, oldY + i] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                        }
                        else if (yChangeAbs == 0)
                        {
                            isLegal = true;
                            if (xChange > 0)
                            {
                                for (int i = 1; i < (xChangeAbs); i++)
                                {
                                    if (board[oldX - i, oldY] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (xChangeAbs); i++)
                                {
                                    if (board[oldX + i, oldY] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                        }
                        return isLegal;
                    }

                    //knights
                    else if (piece == '3' || piece == '9')
                    {
                        if ((yChangeAbs == 1 && xChangeAbs == 2) || (yChangeAbs == 2 && xChangeAbs == 1))
                        {
                            isLegal = true;
                        }
                        return isLegal;
                    }

                    //bishops
                    else if (piece == '4' || piece == 'a')
                    {
                        if (yChangeAbs == xChangeAbs)
                        {
                            isLegal = true;
                            if (yChange > 0)
                            {
                                if (xChange > 0)
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX - i, oldY - i] != '0')
                                        {
                                            isLegal = false;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX + i, oldY - i] != '0')
                                        {
                                            isLegal = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (xChange > 0)
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX - i, oldY + i] != '0')
                                        {
                                            isLegal = false;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX + i, oldY + i] != '0')
                                        {
                                            isLegal = false;
                                        }
                                    }
                                }
                            }
                        }
                        return isLegal;
                    }

                    //kings
                    else if (piece == '5' || piece == 'b')
                    {
                        if (yChangeAbs <= 1 && xChangeAbs <= 1)
                        {
                            isLegal = true;
                        }
                        return isLegal;
                    }

                    //queens
                    else if (piece == '6' || piece == 'c')
                    {
                        //checks to see if the queen is moving in a cardinal direction, and returns false if she isn't
                        if ((xChangeAbs == 0 && yChangeAbs != 0) || (yChangeAbs == 0 && xChangeAbs != 0) || (xChangeAbs == yChangeAbs))
                        {
                            isLegal = true;
                        }
                        else
                        {
                            return false;
                        }
                        if (yChange > 0)
                        {
                            if (xChange > 0)
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX - i, oldY - i] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX + i, oldY - i] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (xChange > 0)
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX - i, oldY + i] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX + i, oldY + i] != '0')
                                    {
                                        isLegal = false;
                                    }
                                }
                            }
                        }
                        return isLegal;
                    }

                    //default, because Visual Studio won't let me compile because not all code paths return a value
                    else
                    {
                        return isLegal;
                    }
                }
                //Another default, because Visual Studio won't let me compile because not all code paths return a value
                else
                {
                    return isLegal;
                }
            }
            //Hooray for defaults and Visual Studio thinking I'm too dumb to know all the possible inputs into my own function
            else
            {
                return isLegal;
            }
        }

        private bool isWhiteOrBlank(char piece)
        {
            if (piece == '0' || piece == '1' || piece == '2' || piece == '3' || piece == '4' || piece == '5' || piece == '6')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isBlackOrBlank(char piece)
        {
            if (piece == '0' || piece == '7' || piece == '8' || piece == '9' || piece == 'a' || piece == 'b' || piece == 'c')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CreateBoard(string boardName)
        {
            char[,] board = { {'8', '7', '0', '0', '0', '0', '1', '2'}, { '9', '7', '0', '0', '0', '0', '1', '3' }, { 'a', '7', '0', '0', '0', '0', '1', '4' }, { 'c', '7', '0', '0', '0', '0', '1', '6' }, { 'b', '7', '0', '0', '0', '0', '1', '5' }, { 'a', '7', '0', '0', '0', '0', '1', '4' }, { '9', '7', '0', '0', '0', '0', '1', '3' }, { '8', '7', '0', '0', '0', '0', '1', '2' }};

            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Chess\Boards.txt";
            var lines = File.ReadAllLines(FileAddress);
            int length = lines.Length;
            string[] newLines = new string[length + 9];

            for (int i = 0; i < length; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[length] = boardName + ":1";

            for (int i = 1; i < 9; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    newLines[length + i] = newLines[length + i] + Char.ToString(board[j, i - 1]);
                }
            }

            File.WriteAllLines(FileAddress, newLines);
        }

        private void DeleteBoard(string boardName)
        {
            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Chess\Boards.txt";
            StreamReader sr = new StreamReader(FileAddress);
            string line = "";
            int lineNumber = 0;

            line = sr.ReadLine();
            while (!line.Contains(boardName))
            {
                line = sr.ReadLine();
                lineNumber++;
            }
            sr.Close();

            var lines = File.ReadAllLines(FileAddress);
            int length = lines.Length;
            string[] newLines = new string[length - 9];

            for (int i = 0; i < lineNumber; i++)
            {
                newLines[i] = lines[i];
            }
            for (int i = lineNumber; i < length - 9; i++)
            {
                newLines[i] = lines[i + 9];
            }

            File.WriteAllLines(FileAddress, newLines);
        }

        private void SaveBoard(string boardName, char[,] board, bool isWhitesTurn)
        {
            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Chess\Boards.txt";
            StreamReader sr = new StreamReader(FileAddress);
            string line = "";
            int lineNumber = 1;

            line = sr.ReadLine();
            while (!line.Contains(boardName))
            {
                line = sr.ReadLine();
                lineNumber++;
            }
            sr.Close();

            var lines = File.ReadAllLines(FileAddress);
            lines[lineNumber - 1] = boardName + ":" + (isWhitesTurn ? "1" : "0");

            for (int i = 0; i < 8; i++)
            {
                lines[lineNumber + i] = "";
                for (int j = 0; j < 8; j++)
                {
                    lines[lineNumber + i] = lines[lineNumber + i] + board[j, i].ToString();
                }
            }

            File.WriteAllLines(FileAddress, lines);
        }

        private char[,] GetBoard(string boardName)
        {
            char[,] board = new char[8,8];
            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\Chess\Boards.txt";
            StreamReader sr = new StreamReader(FileAddress);
            string line = "";
            int lineNumber = 1;

            line = sr.ReadLine();
            while (!line.Contains(boardName))
            {
                line = sr.ReadLine();
                lineNumber++;
            }
            sr.Close();

            var lines = File.ReadAllLines(FileAddress);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[j, i] = char.Parse(lines[lineNumber + i].Substring(j, 1));
                }
            }

            return board;
        }

        private string ConvertPieceToEmoji(char piece, string serverId)
        {
            //to get the array in proper notation, copy and paste the following line into the server
            //emojiCodes = new string[] { "\:Blank:", "\:WPawn:", "\:WRook:", "\:WKnight:", "\:WBishop:", "\:WKing:", "\:WQueen:", "\:BPawn:", "\:BRook:", "\:BKnight:", "\:BBishop:", "\:BKing:", "\:BQueen:"};
            //and paste the output into the corresponding if statement

            string[] emojiCodes = { };
            if (serverId == "293460412593209345")
            {
                emojiCodes = new string[] { "<:Blank:298949936475537430>", "<:WPawn:298949937155276800>", "<:WRook:298949937498947584>", "<:WKnight:298949937121591297>", "<:WBishop:298949937050288128>", "<:WKing:298949936836247584>", "<:WQueen:298949937398546433>", "<:BPawn:298949936362422275>", "<:BRook:298949936651698177>", "<:BKnight:298949936341319684>", "<:BBishop:298949936236724225>", "<:BKing:298949936676864030>", "<:BQueen:298949936949493760>"};
            }
            else if (serverId == "237688211420217344")
            {
                emojiCodes = new string[] { "<:Blank:298927119562571777>", "<:WPawn:298602840694325248>", "<:WRook:298602840706777088>", "<:WKnight:298602840824479754>", "<:WBishop:298602840304254978>", "<:WKing:298602840333615106>", "<:WQueen:298602840916623370>", "<:BPawn:298602840488804362>", "<:BRook:298602840249860102>", "<:BKnight:298602840295735306>", "<:BBishop:298602839805263875>", "<:BKing:298602840207917056>", "<:BQueen:298602840027561985>"};
            }
            else if (serverId == "308360449509031936")
            {
                emojiCodes = new string[] { "<:Blank:308361674568630277>", "<:WPawn:308361675139186698>", "<:WRook:308361674975477762>", "<:WKnight:308361675206033428>", "<:WBishop:308361674899849216>", "<:WKing:308361674979803137>", "<:WQueen:308361675029872641>", "<:BPawn:308361674610442241>", "<:BRook:308361674816094208>", "<:BKnight:308361674535075841>", "<:BBishop:308361674191142942>", "<:BKing:308361674413572097>", "<:BQueen:308361674405052418>" };
            }
            switch (piece)
            {
                case '0':
                    return emojiCodes[0];
                case '1':
                    return emojiCodes[1];
                case '2':
                    return emojiCodes[2];
                case '3':
                    return emojiCodes[3];
                case '4':
                    return emojiCodes[4];
                case '5':
                    return emojiCodes[5];
                case '6':
                    return emojiCodes[6];
                case '7':
                    return emojiCodes[7];
                case '8':
                    return emojiCodes[8];
                case '9':
                    return emojiCodes[9];
                case 'a':
                    return emojiCodes[10];
                case 'b':
                    return emojiCodes[11];
                case 'c':
                    return emojiCodes[12];
                default:
                    return "";
            }
        }

        private string BoardToString1(string boardName, string serverId)
        {
            char[,] board = GetBoard(boardName);
            string boardString = @"....A      B      C      D      E      F      G      H
--------------------------------------------";

            for (int i = 0; i < 4; i++)
            {
                boardString = boardString + @"
" + (i + 1) + "|";
                for (int j = 0; j < 8; j++)
                {
                    boardString = boardString + ConvertPieceToEmoji(board[j, i], serverId) + "|";
                }
                boardString = boardString + @"
--------------------------------------------";
            }

            return boardString;
        }

        private string BoardToString2(string boardName, string serverId)
        {
            char[,] board = GetBoard(boardName);
            string boardString = @"";

            for (int i = 4; i < 8; i++)
            {
                boardString = boardString + @"
" + (i + 1) + "|";
                for (int j = 0; j < 8; j++)
                {
                    boardString = boardString + ConvertPieceToEmoji(board[j, i], serverId) + "|";
                }
                boardString = boardString + @"
--------------------------------------------";
            }

            return boardString;
        }

        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void Log(object sender, LogMessageEventArgs e)
		{
			Console.WriteLine(e.Message);
		}

        private ulong GetTokens(string userId)
        {
            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\PlayerList.txt";
            StreamReader sr = new StreamReader(FileAddress);

            ulong currentTokens = 0;
            int lineNumber = 1;
            string line = "";
            string Id = userId;

            line = sr.ReadLine();
            while (line != Id)
            {
                line = sr.ReadLine();
                lineNumber++;
            }
            line = sr.ReadLine();
            currentTokens = ulong.Parse(line);
            sr.Close();
            
            return currentTokens;
        }

        private void SetTokens(string userId, ulong newTokens)
        {
            string FileAddress = @"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\PlayerList.txt";
            var lines = File.ReadAllLines(FileAddress);
            StreamReader sr = new StreamReader(FileAddress);
            int lineNumber = 1;
            string line = "";
            string Id = userId;
            ulong oldTokens = GetTokens(Id);

            line = sr.ReadLine();
            while (line != Id)
            {
                line = sr.ReadLine();
                lineNumber++;
            }
            line = sr.ReadLine();
            sr.Close();

            lines[lineNumber] = newTokens.ToString();
            File.WriteAllLines(FileAddress, lines);

            var logLines = File.ReadAllLines(@"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Log.txt");
            int length = logLines.Length;
            string[] newLogLines = new string[length + 1];
            for (int i = 0; i < length; i++)
            {
                newLogLines[i] = logLines[i];
            }
            DateTime localDate = DateTime.Now;
            if (newTokens > oldTokens)
            {
                newLogLines[length] = Id.ToString() + ":" + GetTokens(Id) + ":" + "+" + (newTokens - oldTokens) + ":" + localDate.ToString();
            }
            else
            {
                newLogLines[length] = Id.ToString() + ":" + GetTokens(Id) + ":" + "-" + (oldTokens - newTokens) + ":" + localDate.ToString();
            }
            File.WriteAllLines(@"C:\users\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Log.txt", newLogLines);
        }

        private bool isRegistered(string userId)
        {
            string FileAddress = @"C:\user\Seth Dolin\Desktop\PhysicsBot\SlotMachine\Playerlist.txt";

            var lines = File.ReadAllLines(FileAddress);
            if (lines.Contains<string>(userId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}