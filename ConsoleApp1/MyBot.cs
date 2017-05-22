//certain using directives have been commented out because they are part of a feature that is currently non functional and that I don't feel like fixing
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
using Discord;
using Discord.Commands;
using Discord.Audio;
//using NAudio;
//using NAudio.Wave;
//using NAudio.CoreAudioApi;
//using VideoLibrary;
//using EmergenceGuardian;
//using System.Diagnostics;
//using YoutubeExtractor;

namespace ConsoleApp1
{
    class ConfigVar
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    class MyBot
    {
        DiscordClient discord;
        string baseFilePath = @"C:\users\Seth Dolin\Desktop\PhysicsBot\";

        private static string WolframAlphaAppId { get; set; }
        private static string StartingTokens { get; set; }
        private static string GithubLink { get; set; }
        private static string TyperacerLink { get; set; }
        private static string OwnerUId { get; set; }
        private static string OwnerUsername { get; set; }
        private static string BotToken { get; set; }
        private static string PlayingMessage { get; set; }
        private static char PrefixChar { get; set; }

        public MyBot()
        {
            GetConfigValues();

            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = PrefixChar;
                x.AllowMentionPrefix = true;
            });

            discord.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            List<String> commandList = new List<String>();
            List<String> AdminCommandList = new List<string>();

            var commands = discord.GetService<CommandService>();
            Random rnd = new Random();

            commandList.Add("nickname");
            commands.CreateCommand("nickname")
                .Parameter("RequestedName", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("nickname", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("RequestedName"));

                    string desc = @"**Description:**
Sets the user's server wide nickname to the requested name

**Arguments:**
`RequestedName` - The name to be set as the user's nickname

**Restrictions:**
None";
                    if (e.GetArg("RequestedName").ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        await e.User.Edit(null, null, null, null, $"{e.GetArg("RequestedName")}");
                        await e.Channel.SendMessage("Done");
                    }
                });

            //admin only command so it isn't in the list
            AdminCommandList.Add("purge");
            commands.CreateCommand("purge")
                .Parameter("NumberOfMessages", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("purge", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("NumberOfMessages"));

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
                        if (e.User.ServerPermissions.Administrator || e.User.Id.ToString() == OwnerUId)
                        {
                            Message[] messagesToDelete;
                            messagesToDelete = await e.Channel.DownloadMessages(NumberOfMessages + 1);
                            await e.Channel.DeleteMessages(messagesToDelete);
                        }
                        else
                        {
                            LogCommandError(e.Command.Text, e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("NumberOfMessages"), "permission");
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
                .Alias("wolfram", "wolframalpha", "wa")
                .Parameter("RawInput", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("math", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("RawInput"));

                    string desc = @"**Description:**
Queries Wolfram Alpha with the given parameter and returns the response as a .gif file sent in chat

**Arguments:**
`RawInput` - The query to be sent to Wolfram Alpha

**Aliases:**
`wolfram`
`wolframalpha`
`wa`

**Restrictions:**
None";

                    if (e.GetArg("RawInput").ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string FormattedInput = SoftwareKobo.Net.WebUtility.UrlEncode($"{e.GetArg("RawInput")}");
                        string url = "https://api.wolframalpha.com/v1/simple?input=" + FormattedInput + "&appid=" + WolframAlphaAppId;
                        await e.Channel.SendMessage("Thinking...");
                        await e.Channel.SendIsTyping();
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(new Uri(url), baseFilePath + @"WolframOutput.gif");
                        }
                        Message[] MessagesToDelete = await e.Channel.DownloadMessages(1);
                        await e.Channel.DeleteMessages(MessagesToDelete);
                        await e.Channel.SendFile(baseFilePath + @"WolframOutput.gif");
                        await e.Channel.SendMessage("Click to enlarge");
                    }
                });

            commandList.Add("roulette");
            commands.CreateCommand("roulette")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("roulette", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Plays Russian Roulette

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param").ToLower() == "help")
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
                    LogCommand("slots", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("BetAmount"));

                    string desc = @"**Description:**
Plays slots

**Arguments:**
`BetAmount` - The amount that you wish to bet (integer)

**Restrictions:**
You must possess the amount of tokens that you wish to gamble
To find the number of tokens that you possess, use `!tokens`

The payouts are listed below:";

                    ulong BetAmount = 0;

                    if (e.GetArg("BetAmount").ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                        await e.Channel.SendFile(baseFilePath + @"SlotMachine\Payouts.png");
                    }
                    else if (ulong.TryParse(e.GetArg("BetAmount"), out BetAmount) || e.GetArg("BetAmount") == "max")
                    {
                        if (e.Channel.Id.ToString() == "308360449509031936")
                        {
                            ulong tokens = GetTokens(e.User.Id.ToString());
                            tokens /= 2;
                            SetTokens(e.User.Id.ToString(), tokens);
                            await e.Channel.SendMessage("Use the ***FUCKING*** slots channel for slots. For your infractions, your token count has been halved.");
                        }
                        else
                        {
                            //weighted random number generator to make payouts lower
                            string fileAddress = baseFilePath + @"SlotMachine\weight.txt";
                            double weight = double.Parse(File.ReadAllLines(fileAddress)[0]);
                            //set in the file listed above, not here
                            //higher values make it harder to get better rolls. 
                            //1 is default

                            double[] weights = new double[8];
                            double maxWeight = 0.0;
                            for (int i = 0; i < 8; i++)
                            {
                                weights[i] = (weight * (8 - i));
                                maxWeight += weights[i];
                            }

                            double num1 = Math.Pow(2, (WeightedSelector(weights, maxWeight, 0, rnd)));
                            double num2 = Math.Pow(2, (WeightedSelector(weights, maxWeight, 1, rnd)));
                            double num3 = Math.Pow(2, (WeightedSelector(weights, maxWeight, 2, rnd)));

                            string Id = e.User.Id.ToString();
                            string FileAddress = baseFilePath + @"SlotMachine\PlayerList.txt";
                            ulong currentTokens = GetTokens(Id);
                            ulong newTokens = 0;
                            if (e.GetArg("BetAmount") == "max")
                            {
                                BetAmount = currentTokens;
                            }

                            if (!File.ReadAllText(FileAddress).Contains(Id))
                            {
                                await e.Channel.SendMessage("You are not registered in the token database. Please use `!register` to add yourself to the database");
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
                                        img1 = baseFilePath + @"SlotMachine\Blank.png";
                                        break;

                                    case 4:
                                        img1 = baseFilePath + @"SlotMachine\RedBar.png";
                                        break;

                                    case 8:
                                        img1 = baseFilePath + @"SlotMachine\WhiteBar.png";
                                        break;

                                    case 16:
                                        img1 = baseFilePath + @"SlotMachine\BlueBar.png";
                                        break;

                                    case 32:
                                        img1 = baseFilePath + @"SlotMachine\Red7.png";
                                        break;

                                    case 64:
                                        img1 = baseFilePath + @"SlotMachine\White7.png";
                                        break;

                                    case 128:
                                        img1 = baseFilePath + @"SlotMachine\Blue7.png";
                                        break;

                                    case 256:
                                        img1 = baseFilePath + @"SlotMachine\Flag7.png";
                                        break;

                                    default:
                                        break;
                                }

                                switch (num2)
                                {
                                    case 512:
                                        img2 = baseFilePath + @"SlotMachine\Blank.png";
                                        break;

                                    case 1024:
                                        img2 = baseFilePath + @"SlotMachine\RedBar.png";
                                        break;

                                    case 2048:
                                        img2 = baseFilePath + @"SlotMachine\WhiteBar.png";
                                        break;

                                    case 4096:
                                        img2 = baseFilePath + @"SlotMachine\BlueBar.png";
                                        break;

                                    case 8192:
                                        img2 = baseFilePath + @"SlotMachine\Red7.png";
                                        break;

                                    case 16384:
                                        img2 = baseFilePath + @"SlotMachine\White7.png";
                                        break;

                                    case 32768:
                                        img2 = baseFilePath + @"SlotMachine\Blue7.png";
                                        break;

                                    case 65536:
                                        img2 = baseFilePath + @"SlotMachine\Flag7.png";
                                        break;

                                    default:
                                        break;
                                }

                                switch (num3)
                                {
                                    case 131072:
                                        img3 = baseFilePath + @"SlotMachine\Blank.png";
                                        break;

                                    case 262144:
                                        img3 = baseFilePath + @"SlotMachine\RedBar.png";
                                        break;

                                    case 524288:
                                        img3 = baseFilePath + @"SlotMachine\WhiteBar.png";
                                        break;

                                    case 1048576:
                                        img3 = baseFilePath + @"SlotMachine\BlueBar.png";
                                        break;

                                    case 2097152:
                                        img3 = baseFilePath + @"SlotMachine\Red7.png";
                                        break;

                                    case 4194304:
                                        img3 = baseFilePath + @"SlotMachine\White7.png";
                                        break;

                                    case 8388608:
                                        img3 = baseFilePath + @"SlotMachine\Blue7.png";
                                        break;

                                    case 16777216:
                                        img3 = baseFilePath + @"SlotMachine\Flag7.png";
                                        break;

                                    default:
                                        break;
                                }

                                //Begin the incredibly long and inefficient series of 'if' statements here
                                int SlotValue = System.Convert.ToInt32(num1) + System.Convert.ToInt32(num2) + System.Convert.ToInt32(num3);
                                if (SlotValue == 16843008)//Flag 7, Flag 7, Flag 7
                                {
                                    BetReturn = BetAmount * 1000;
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
                    LogCommand("id", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Gets the user id of the user that used the command

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param").ToLower() == "help")
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
                    LogCommand("register", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Registers the user in the token database

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param").ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string FileAddress = baseFilePath + @"SlotMachine\PlayerList.txt";
                        var oldLines = File.ReadAllLines(FileAddress);
                        int length = oldLines.Length;
                        string[] newLines = new string[length + 2];
                        for (int i = 0; i < length; i++)
                        {
                            newLines[i] = oldLines[i];
                        }
                        newLines[length] = e.User.Id.ToString();
                        newLines[length + 1] = StartingTokens;
                        File.WriteAllLines(FileAddress, newLines);
                        await e.Channel.SendMessage("User " + e.Server.GetUser(UInt64.Parse(e.GetArg("UserId"))).Nickname + " has been successfully added to the database");
                    }
                });

            //admin only command so it isn't in the list
            AdminCommandList.Add("adduser");
            commands.CreateCommand("adduser")
                .Parameter("UserId", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("adduser", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("UserId"));

                    string desc = @"**Description:**
Adds a user to the token database

**Arguments:**
`UserId` - The user id of the user to be added. User Id can be acquired by having the user type `!id` or through the use of Discord Developer Tools

**Restrictions:**
You must be an administrator on the server to use this command";
                    if (e.GetArg("UserId").ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.User.ServerPermissions.Administrator || e.User.Id.ToString() == OwnerUId)
                    {
                        string FileAddress = baseFilePath + @"SlotMachine\PlayerList.txt";
                        var oldLines = File.ReadAllLines(FileAddress);
                        int length = oldLines.Length;
                        string[] newLines = new string[length + 2];
                        for (int i = 0; i < length; i++)
                        {
                            newLines[i] = oldLines[i];
                        }
                        newLines[length] = e.GetArg("UserId");
                        newLines[length + 1] = StartingTokens;
                        File.WriteAllLines(FileAddress, newLines);
                        await e.Channel.SendMessage("User with id " + e.GetArg("UserId") + " has been successfully added to the database");
                    }
                    else
                    {
                        LogCommandError(e.Command.Text, e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("UserId"), "permission");
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            //admin only command so it isn't in the list
            AdminCommandList.Add("bonus");
            commands.CreateCommand("bonus")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("bonus", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Gives the specified amount of bonus tokens to the specified user

**Arguments:**
`UserId` - The id of the user to be given the bonus tokens. A user's Id can be found by using !id
`BonusTokens` - The amount of bonus tokens to be given to the user

**Restrictions:**
You must be an administrator on the server to use this command";
                    string param = e.GetArg("param").ToLower();
                    if (param == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.User.ServerPermissions.Administrator || e.User.Id.ToString() == OwnerUId)
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
                        LogCommandError(e.Command.Text, e.User.Name, e.Channel.Name, e.Server.Name, param, "permission");
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            //admin only command so it isn't in the list
            AdminCommandList.Add("take");
            commands.CreateCommand("take")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("take", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Takes the specified amount of tokens from the specified user

**Arguments:**
`UserId` - The id of the user to be given the bonus tokens. A user's Id can be found by using !id
`Tokens` - The amount of tokens to be taken from the user

**Restrictions:**
You must be an administrator on the server to use this command";
                    string param = e.GetArg("param").ToLower();
                    if (param == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else if (e.User.ServerPermissions.Administrator || e.User.Id.ToString() == OwnerUId)
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
                        LogCommandError(e.Command.Text, e.User.Name, e.Channel.Name, e.Server.Name, param, "permission");
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            commandList.Add("tokens");
            commands.CreateCommand("tokens")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("tokens", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Gets the token count for the user that uses it

**Arguments:**
None

**Restrictions:**
None";
                    if (e.GetArg("param").ToLower() == "help")
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
                    LogCommand("give", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Gives the specified user the specified amount of tokens from the bank of the user that used the command

**Arguments:**
`ReceiveId` - The user id of the user to receive the tokens
`Tokens` - The amount of tokens that you wish to give the user

**Restrictions:**
You must possess the amount of tokens that you wish to give";
                    string param = e.GetArg("param").ToLower();
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
                        if (GetTokens(e.User.Id.ToString()) < tokens)
                        {
                            await e.Channel.SendMessage("You do not have enough tokens to perform this action");
                        }
                        else
                        {
                            SetTokens(giveId, GetTokens(giveId) - tokens);
                            SetTokens(receiveId, GetTokens(receiveId) + tokens);
                            await e.Channel.SendMessage("user " + e.User.Name + " in channel " + e.Channel.Name + " in server " + e.Server.Name + " has successfully given user with user id " + receiveId + " " + string.Format("{0:n0}", tokens) + @" tokens.
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
                    LogCommand("chess", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
ASCII chess. Doesn't get much more complicated than that

**Arguments:**
`Action` - The action to take. Can be `create`, `move`, `display`, `delete`, or `checkmate`
`BoardName` - The name of the board which you want to edit or display. Set when the board is created
`Square1` - The location of the piece that you would like to move. Given in the format LetterNumber. e.g. A6. Not necessary for actions `create`, `display`, and `delete`
`Square2` - The location that you would like to move the selected piece to. Given in the format LetterNumber. e.g. A6. Not necessary for actions `create`, `display`, and `delete`

**Restrictions:**
None";
                    string arg = e.GetArg("param").ToLower();

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
                            CreateChessBoard(boardName);

                            await e.Channel.SendMessage("Board with name " + boardName + " successfully created");
                        }
                        else if (action == "display")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            await e.Channel.SendMessage("**Board: **" + boardName + @"
" + ChessBoardToString1(boardName, e.Server.Id.ToString()));
                            await e.Channel.SendMessage(ChessBoardToString2(boardName, e.Server.Id.ToString()));
                            await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                        }
                        else if (action == "move")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - (7 + spaceIndex1));
                            string square1 = arg.Substring(spaceIndex2 + 1, 2);
                            string square2 = arg.Substring(spaceIndex3 + 1, 2);
                            board = GetChessBoard(boardName);
                            int[] oldCoord = new int[2];
                            oldCoord = ParseChessLocation(square1);
                            int[] newCoord = new int[2];
                            newCoord = ParseChessLocation(square2);
                            int oldX = oldCoord[0];
                            int oldY = oldCoord[1];
                            int newX = newCoord[0];
                            int newY = newCoord[1];

                            if (ChessIsLegal(square1, square2, board[oldX, oldY], board, isWhitesTurn(boardName)))
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
                                    SaveChessBoard(boardName, board, !isWhitesTurn(boardName));
                                }
                                await e.Channel.SendMessage("**Board: **" + boardName + @"
" + ChessBoardToString1(boardName, e.Server.Id.ToString()));
                                await e.Channel.SendMessage(ChessBoardToString2(boardName, e.Server.Id.ToString()));
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
                            board = GetChessBoard(boardName);
                            int[] oldCoord = new int[2];
                            oldCoord = ParseChessLocation(square1);
                            int[] newCoord = new int[2];
                            newCoord = ParseChessLocation(square2);
                            int oldX = oldCoord[0];
                            int oldY = oldCoord[1];
                            int newX = newCoord[0];
                            int newY = newCoord[1];

                            if (ChessIsLegal(square1, square2, board[oldX, oldY], board, isWhitesTurn(boardName)))
                            {
                                if (board[newX, newY] == '5')
                                {
                                    await e.Channel.SendMessage(@"Black has won.
**Final Board: **" + boardName + @"
" + ChessBoardToString1(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage(ChessBoardToString2(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                                    DeleteChessBoard(boardName);
                                }
                                else if (board[newX, newY] == 'b')
                                {
                                    await e.Channel.SendMessage(@"White has won.
**Final Board: **" + boardName + @"
" + ChessBoardToString1(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage(ChessBoardToString2(boardName, e.Server.Id.ToString()));
                                    await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "white's" : "black's") + "` turn");
                                    DeleteChessBoard(boardName);
                                }
                            }
                        }
                        else if (action == "delete")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            DeleteChessBoard(boardName);
                            await e.Channel.SendMessage("Board with name " + boardName + " has been successfully deleted");
                        }
                        else
                        {
                            await e.Channel.SendMessage("One or more arguments are in the incorrect format. Please try again");
                        }
                    }
                });

            commandList.Add("lucas");
            commands.CreateCommand("lucas")
                .Parameter("termNumber", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("lucas", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("termNumber"));

                    string arg = e.GetArg("termNumber").ToLower();
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
                .Do(async (e) =>
                {
                    LogCommand("fibonacci", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("termNumber"));

                    string arg = e.GetArg("termNumber").ToLower();
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

            commandList.Add("shop");
            commands.CreateCommand("shop")
                .Parameter("choice", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("shop", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("choice"));

                    string fileAddress = baseFilePath + @"SlotMachine\Shop.txt";
                    var lines = File.ReadAllLines(fileAddress);
                    string arg = e.GetArg("choice");

                    string desc = @"**Description:**
Displays a shop that can be used to purchase things with the tokens that you earn on the slot machine

**Arguments:**
`ItemIndex` - The index of the item that you would like to purchase
    This can also be `prices` to display the prices of the items for sale

**Restrictions:**
You must possess an amount of tokens equal to or greater than the price of the option that you would like to purchase";
                    if (arg.ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }

                    else if (arg == "prices" || arg == null)
                    {
                        string prices = @"The prices of each item are as follows:
";
                        for (int i = 0; i < lines.Length - 1; i += 2)
                        {
                            prices += ((i / 2) + 1) + " :" + lines[i] + @":
    " + lines[i + 1] + @"
";
                        }
                        await e.Channel.SendMessage(prices);
                    }
                    else
                    {
                        ulong currentTokens = GetTokens(e.User.Id.ToString());
                        ulong newTokens = 0;
                        int spaceIndex = arg.IndexOf(' ');
                        short itemIndex = short.Parse(arg.Substring(0, spaceIndex));
                        if (itemIndex == 1)//TTS messages
                        {
                            ulong price = ulong.Parse(lines[(itemIndex * 2) - 1]);
                            if (currentTokens < price)
                            {
                                await e.Channel.SendMessage("You do not have enough tokens to purchase this item.");
                            }
                            else
                            {
                                var messages = await e.Channel.DownloadMessages(1);
                                await e.Channel.DeleteMessages(messages);
                                newTokens = currentTokens - price;
                                await e.Channel.SendTTSMessage(e.User.Name + " says" + arg.Substring(spaceIndex));
                                SetTokens(e.User.Id.ToString(), newTokens);
                            }
                        }
                    }
                });

            commandList.Add("hangman");
            commands.CreateCommand("hangman")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("hangman", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string arg = e.GetArg("param").ToLower();
                    string desc = @"**Description:**
Plays ASCII hangman

**Arguments:**
`Action` - The action that you would like to take. Can be `create`, `guess`, `display`, or `guessword`
`Guess` - The letter or word that you would like to guess. Only necessary for actions `guess` and `guessword`

**Restrictions:**
Action `create` can only be used by server administrators
Only one game can exist at a time on for each server

**Note:**
It is recommended that you create the game in a channel separate to where you intend to play it, that way players will be unable to see the secret word before the bot deletes it";
                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        string sId = e.Server.Id.ToString();
                        int spaceIndex1 = GetNthIndex(arg, ' ', 1);
                        //int spaceIndex2 = GetNthIndex(arg, ' ', 2);
                        //int spaceIndex3 = GetNthIndex(arg, ' ', 3);
                        string action;
                        if (spaceIndex1 == -1)
                        {
                            action = arg;
                        }
                        else
                        {
                            action = arg.Substring(0, spaceIndex1);
                        }
                        if (action == "create")
                        {
                            if (e.User.ServerPermissions.Administrator || e.User.Id.ToString() == OwnerUId)
                            {
                                await e.Channel.DeleteMessages(await e.Channel.DownloadMessages(1));

                                string word = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                                CreateHangman(sId, word);
                                await e.Channel.SendMessage("Hangman game successfully created");
                            }
                            else
                            {
                                await e.Channel.SendMessage("You do not have permission to use this command.");
                            }
                        }
                        else if (action == "display")
                        {
                            int numOfFails = GetHangmanFailCount(sId);
                            char[] guessed = GetHangmanGuessed(sId);
                            string answer = GetHangmanAnswer(sId);
                            string Hangman = @"**Hangman Game**
";
                            for (int i = 0; i < guessed.Length; i++)
                            {
                                if (guessed[i] == ' ')
                                {
                                    Hangman += "◯";
                                }
                                else
                                {
                                    Hangman += answer[i];
                                }
                            }
                            Hangman += @"
";

                            Hangman += DrawHangman(numOfFails);

                            await e.Channel.SendMessage(Hangman);
                        }
                        else if (action == "guess")
                        {
                            char letter = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1).ToUpper().ToCharArray()[0];
                            if (arg.Substring(spaceIndex1 + 1).Length != 1)
                            {
                                await e.Channel.SendMessage("Too many letters. To guess the whole word, use `!hangman guessword`");
                            }
                            else
                            {
                                char[] guessed = GetHangmanGuessed(sId);
                                string answer = GetHangmanAnswer(sId);
                                int numOfFails = GetHangmanFailCount(sId);
                                bool correct = false;
                                for (int i = 0; i < answer.Length; i++)
                                {
                                    if (answer[i] == letter)
                                    {
                                        correct = true;
                                        guessed[i] = letter;
                                    }
                                }

                                if (!correct)
                                {
                                    numOfFails++;
                                }

                                SetHangman(sId, answer, guessed, numOfFails);

                                string Hangman = @"**Hangman Game**
";
                                Hangman += e.User.Name + " has guessed " + letter + @"
";
                                for (int i = 0; i < guessed.Length; i++)
                                {
                                    if (guessed[i] == ' ')
                                    {
                                        Hangman += "◯";
                                    }
                                    else
                                    {
                                        Hangman += answer[i];
                                    }
                                }
                                Hangman += @"
";
                                Hangman += DrawHangman(numOfFails);
                                string guessedString = "";
                                for (int i = 0; i < guessed.Length; i++)
                                {
                                    guessedString += guessed[i];
                                }
                                if (guessedString == answer)
                                {
                                    int reward = GetHangmanReward(sId);
                                    SetTokens(e.User.Id.ToString(), GetTokens(e.User.Id.ToString()) + (ulong)reward);
                                    Hangman += @"
" + e.User.Name + @" has won and been awarded " + reward + @" tokens.
" + e.User.Name + " now has " + GetTokens(e.User.Id.ToString()) + " tokens.";

                                    DeleteHangman(sId);
                                }
                                else if (numOfFails == 6)
                                {
                                    Hangman += @"
You lose and the man has been hanged!";
                                    DeleteHangman(sId);
                                }

                                await e.Channel.SendMessage(Hangman);
                            }
                        }
                        else if (action == "guessword")
                        {
                            string word = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            string answer = GetHangmanAnswer(sId);
                            if (word.ToUpper() == answer)
                            {
                                string Hangman = @"**Hangman Game**
";
                                Hangman += e.User.Name + " has guessed " + word + @"
";
                                Hangman += answer + @"
";
                                Hangman += DrawHangman(GetHangmanFailCount(sId));
                                Hangman += @"
" + e.User.Name + " has won!";
                            }
                            else
                            {
                                char[] guessed = GetHangmanGuessed(sId);
                                int numOfFails = GetHangmanFailCount(sId);
                                numOfFails++;
                                string Hangman = @"**Hangman Game**
";
                                Hangman += e.User.Name + " has guessed " + word + @"
";
                                for (int i = 0; i < guessed.Length; i++)
                                {
                                    if (guessed[i] == ' ')
                                    {
                                        Hangman += "◯";
                                    }
                                    else
                                    {
                                        Hangman += answer[i];
                                    }
                                }
                                Hangman += @"
";
                                SetHangman(sId, answer, guessed, numOfFails);
                                Hangman += DrawHangman(numOfFails);
                            }
                        }

                    }
                });

            //commandList.Add("checkers");
            commands.CreateCommand("checkers")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("checkers", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
ASCII checkers. Doesn't get much more complicated than that

**Arguments:**
`Action` - The action to take. Can be `create`, `move`, `display`, or `delete`
`BoardName` - The name of the board which you want to edit or display. Set when the board is created
`Square1` - The location of the piece that you would like to move. Given in the format LetterNumber. e.g. A6. Not necessary for actions `create`, `display`, and `delete`
`Square2` - The location that you would like to move the selected piece to. Given in the format LetterNumber. e.g. A6. Not necessary for actions `create`, `display`, and `delete`

**Restrictions:**
None";
                    string arg = e.GetArg("param").ToLower();

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
                            CreateCheckersBoard(boardName);

                            await e.Channel.SendMessage("Checkers board with name " + boardName + " successfully created");
                        }
                        else if (action == "display")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            await e.Channel.SendMessage("**Board: **" + boardName + @"
" + CheckersBoardToString1(boardName, e.Server.Id.ToString()));
                            await e.Channel.SendMessage(CheckersBoardToString2(boardName, e.Server.Id.ToString()));
                            await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "red's" : "blue's") + "` turn");
                        }
                        else if (action == "move")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - (7 + spaceIndex1));
                            string square1 = arg.Substring(spaceIndex2 + 1, 2);
                            string square2 = arg.Substring(spaceIndex3 + 1, 2);
                            board = GetCheckersBoard(boardName);
                            int[] oldCoord = new int[2];
                            oldCoord = ParseChessLocation(square1);
                            int[] newCoord = new int[2];
                            newCoord = ParseChessLocation(square2);
                            int oldX = oldCoord[0];
                            int oldY = oldCoord[1];
                            int newX = newCoord[0];
                            int newY = newCoord[1];

                            if (CheckersIsLegal(square1, square2, board[oldX, oldY], board, isWhitesTurn(boardName)))
                            {
                                board[newX, newY] = board[oldX, oldY];
                                board[oldX, oldY] = '0';
                                SaveCheckersBoard(boardName, board, !isWhitesTurn(boardName));
                                await e.Channel.SendMessage("**Board: **" + boardName + @"
" + CheckersBoardToString1(boardName, e.Server.Id.ToString()));
                                await e.Channel.SendMessage(CheckersBoardToString2(boardName, e.Server.Id.ToString()));
                                await e.Channel.SendMessage("It is now `" + (isWhitesTurn(boardName) ? "red's" : "blue's") + "` turn");
                            }
                            else
                            {
                                await e.Channel.SendMessage("That move is illegal. Please try again");
                            }
                        }
                        else if (action == "delete")
                        {
                            string boardName = arg.Substring(spaceIndex1 + 1, arg.Length - spaceIndex1 - 1);
                            DeleteCheckersBoard(boardName);
                            await e.Channel.SendMessage("Board with name " + boardName + " has been successfully deleted");
                        }
                        else
                        {
                            await e.Channel.SendMessage("One or more arguments are in the incorrect format. Please try again");
                        }
                    }
                });

            commandList.Add("github");
            commands.CreateCommand("github")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("github", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));
                    string desc = @"**Description:**
Sends the link to the github page for the bot

**Arguments:**
None

**Restrictions:**
None";

                    if (e.GetArg("param").ToLower() == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        await e.Channel.SendMessage("The github for this bot can be found here: " + GithubLink);
                    }
                });

            commandList.Add("roll");
            commands.CreateCommand("roll")
                .Alias("dice")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string arg = e.GetArg("param").ToLower();
                    LogCommand("roll", e.User.Name, e.Channel.Name, e.Server.Name, arg);

                    string desc = @"**Description:**
Rolls the specified number and type of dice, and returns both the individual rolls and the total

**Arguments:**
`Dice` - The amount and type of dice to roll, given in the format *NumberOfDice*d*NumberOfFaces* e.g., 2d6 rolls 2 six sided dice
`OutputType` - The level of detail to output. Can be `concise` or `verbose`. Defaults to `concise`

**Aliases:**
`dice`

**Restrictions:**
None";
                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        int spaceIndex = arg.IndexOf(' ');
                        bool isVerbose = false;
                        if (spaceIndex != -1)
                        {
                            if (arg.Substring(spaceIndex).Contains("verbose"))
                            {
                                isVerbose = true;
                            }
                        }

                        int dIndex = arg.IndexOf('d');
                        int numOfDice = Int32.Parse(arg.Substring(0, dIndex));
                        int numOfFaces = Int32.Parse(arg.Substring(dIndex + 1, ((spaceIndex == -1) ? (arg.Length) : (spaceIndex)) - dIndex - 1));

                        string rolls = "Now rolling **" + arg + "**";
                        int roll;
                        int total = 0;
                        for (int i = 0; i < numOfDice; i++)
                        {
                            roll = rnd.Next(1, numOfFaces + 1);
                            if (isVerbose)
                            {
                                rolls += @"
**Roll " + (i + 1) + ":** " + roll;
                            }

                            total += roll;
                        }
                        rolls += @"

**Total:** " + total;

                        if (rolls.Length >= 2000 && isVerbose)
                        {
                            rolls = @"Due to the number of dice rolled, the output exceeded Discord's character limit. As a result, the output has been changed to concise.
Now rolling **" + arg + @"**
**Total:** " + total;
                        }
                        await e.Channel.SendMessage(rolls);
                    }
                });

            commandList.Add("coin");
            commands.CreateCommand("coin")
                .Alias("flip")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string arg = e.GetArg("param").ToLower();
                    LogCommand("coin", e.User.Name, e.Channel.Name, e.Server.Name, arg);

                    string desc = @"**Description:**
Flips the specified number of coins and returns the results of each flip, as well as the total occurences of heads and tails

**Arguments:**
`NumOfCoins` - The number of coins that you would like to flip
`TypeOfOutput` - The level of detail to output. Can be `concise` or `verbose`. Defaults to `verbose`

**Aliases:**
`flip`

**Restrictions:**
None";
                    if (arg == "help")
                    {
                        await e.Channel.SendMessage(desc);
                    }
                    else
                    {
                        int spaceIndex = arg.IndexOf(' ');
                        Console.WriteLine("spaceIndex = " + spaceIndex);
                        bool isVerbose = true;
                        if (spaceIndex != -1)
                        {
                            if (arg.Substring(spaceIndex).Contains("concise"))
                            {
                                Console.WriteLine("got to point 1");
                                isVerbose = false;
                            }
                        }
                        int coinsToFlip = Int32.Parse(arg.Substring(0, (spaceIndex - 1 <= 0) ? (arg.Length) : (spaceIndex)));
                        int heads = 0;
                        int tails = 0;
                        string flips;
                        Console.WriteLine("isVerbose = " + isVerbose);
                        if (isVerbose)
                        {
                            flips = "Now flipping **" + coinsToFlip + @"** coins:
```";
                        }
                        else
                        {
                            flips = "Now flipping **" + coinsToFlip + "** coins:";
                            Console.WriteLine("Got to point 2");
                        }
                        for (int i = 0; i < coinsToFlip; i++)
                        {
                            Console.WriteLine("Got to point 3. i = " + i);
                            int flip = rnd.Next(2);
                            if (isVerbose)
                            {
                                flips += (flip == 1) ? ("HEADS ") : ("TAILS ");
                            }

                            Console.WriteLine("");
                            if (flip == 1)
                            {
                                heads++;
                            }
                            else
                            {
                                tails++;
                            }
                        }
                        if (isVerbose)
                        {
                            flips += @"```
**Heads:** " + heads + @"
**Tails:** " + tails;
                        }
                        else
                        {
                            flips += @"
**Heads:** " + heads + @"
**Tails:** " + tails;
                        }

                        if (flips.Length >= 2000)
                        {
                            flips = @"Due to the number of dice rolled, the output exceeded Discord's character limit. As a result, the output has been changed to concise.
Now rolling **" + coinsToFlip + @"** coins:
**Heads:** " + heads + @"
**Tails:** " + tails;
                        }

                        await e.Channel.SendMessage(flips);
                    }
                });

            //Admin only command so it isn't in the list
            AdminCommandList.Add("refreshconfig");
            commands.CreateCommand("refreshconfig")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("refreshconfig", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Refreshes all variables to the values set in the config. Useful if you want to change something, but don't want to restart the bot entirely

**Arguments:**
None

**Restrictions:**
You must be the bot owner to use this command";
                    if (e.User.Id.ToString() == OwnerUId)
                    {
                        if (e.GetArg("param") == "help")
                        {
                            await e.Channel.SendMessage(desc);
                        }
                        else
                        {
                            GetConfigValues();
                            await e.Channel.SendMessage("Config values successfully refreshed");
                        }
                    }
                    else
                    {
                        LogCommandError(e.Command.Text, e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"), "permission");
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            //commandList.Add("typeracer");
            commands.CreateCommand("typeracer")
                .Parameter("param", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    LogCommand("typeracer", e.User.Name, e.Channel.Name, e.Server.Name, e.GetArg("param"));

                    string desc = @"**Description:**
Sends a link to Seth's typeracer scorecard

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
                        await e.Channel.SendMessage(TyperacerLink);
                    }
                });

            commands.CreateCommand("help")
                .Do(async (e) =>
                {
                    LogCommand("help", e.User.Name, e.Channel.Name, e.Server.Name, "NULL");

                    string list = @"**Available commands** (prefix with '" + PrefixChar + @"'):
";
                    commandList.Sort();
                    for (int i = 0; i < commandList.ToArray().Length; i++)
                    {
                        list += "`" + commandList[i] + @"`
";
                    }
                    list += @"To get help with a specific command, type
`" + PrefixChar + @"commandname help`
";
                    list += "If you need other help, or would like to report a bug, please message " + OwnerUsername;
                    await e.Channel.SendMessage(list);
                });

            commands.CreateCommand("adminhelp")
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.Administrator || e.User.Id.ToString() == OwnerUId)
                    {

                        LogCommand("adminhelp", e.User.Name, e.Channel.Name, e.Server.Name, "NULL");

                        AdminCommandList.AddRange(commandList);
                        string list = @"**Available commands** (prefix with '" + PrefixChar + @"'):
";
                        AdminCommandList.Sort();
                        for (int i = 0; i < AdminCommandList.ToArray().Length; i++)
                        {
                            list += "`" + AdminCommandList[i] + @"`
";
                        }
                        list += @"To get help with a specific command, type
`" + PrefixChar + @"commandname help`
";
                        list += "If you need other help, or would like to report a bug, please message " + OwnerUsername;
                        await e.Channel.SendMessage(list);
                    }
                    else
                    {
                        LogCommandError(e.Command.Text, e.User.Name, e.Channel.Name, e.Server.Name, "NULL", "permission");
                        await e.Channel.SendMessage("You do not have permission to use this command");
                    }
                });

            //command for testing stuff
            commands.CreateCommand("aoeuaoeu")
                .Do(async (e) =>
                {


                    await e.Channel.SendMessage("done");
                });

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect(BotToken, TokenType.Bot);
                discord.SetGame(PlayingMessage);
            });
        }

        private void GetConfigValues()
        {
            string logPath = baseFilePath + "Log.txt";
            var configLines = File.ReadAllLines(baseFilePath + "Config.txt");

            List<ConfigVar> configVars = new List<ConfigVar>();

            string[] validConfigVars =
            {
                "WolframAlphaAppId",
                "StartingTokens",
                "GithubLink",
                "TyperacerLink",
                "OwnerUId",
                "OwnerUsername",
                "BotToken",
                "PlayingMessage",
                "PrefixChar"
            };

            for (int i = 0; i < validConfigVars.Length; i++)
            {
                configVars.Add(new ConfigVar());
                configVars[i].name = validConfigVars[i];
            }

            for (int i = 0; i < configLines.Length; i++)
            {
                int colonIndex = configLines[i].IndexOf(':');
                int spaceIndex = configLines[i].IndexOf(' ');

                if (configLines[i].Length != 0)
                {
                    if (colonIndex != -1)
                    {
                        string name = configLines[i].Substring(0, colonIndex);
                        if (validConfigVars.Contains(name))
                        {
                            string value = "";
                            if (spaceIndex != -1)
                            {
                                value = configLines[i].Substring(colonIndex + 2, configLines[i].Length - colonIndex - 2);
                            }
                            else
                            {
                                value = configLines[i].Substring(colonIndex + 1, configLines[i].Length - colonIndex - 1);
                            }
                            configVars.FirstOrDefault(c => c.name == name).value = value;

                            LogEvent("Loading config variable " + name + " with value " + value);
                        }
                        else
                        {
                            LogEventError("Skipping config line [" + (i + 1) + "] because it does not contain a valid variable name", "config");
                        }
                    }
                    else
                    {
                        LogEventError("Skipping config line [" + (i + 1) + "] because it does not contain a valid separator", "config");
                    }
                }
                else
                {
                    LogEventError("Skipping config line [" + (i + 1) + "] because it is empty", "config");
                }
            }

            for (int i = 0; i < configVars.Count; i++)
            {
                if (configVars[i].value == null || configVars[i].value == "")
                {
                    LogEventError("Config variable [" + configVars[i].name + "] has not been given a value in the config", "config");
                }
            }

            WolframAlphaAppId = configVars.FirstOrDefault(c => c.name == "WolframAlphaAppId").value;
            StartingTokens = configVars.FirstOrDefault(c => c.name == "StartingTokens").value;
            GithubLink = configVars.FirstOrDefault(c => c.name == "GithubLink").value;
            TyperacerLink = configVars.FirstOrDefault(c => c.name == "TyperacerLink").value;
            OwnerUId = configVars.FirstOrDefault(c => c.name == "OwnerUId").value;
            OwnerUsername = configVars.FirstOrDefault(c => c.name == "OwnerUsername").value;
            BotToken = configVars.FirstOrDefault(c => c.name == "BotToken").value;
            PlayingMessage = configVars.FirstOrDefault(c => c.name == "PlayingMessage").value;
            PrefixChar = Char.Parse(configVars.FirstOrDefault(c => c.name == "PrefixChar").value);
        }

        private void LogEvent(string message)
        {
            string log = DateTime.Now.ToString() + ": " + message;
            Console.WriteLine(log);
            string fileAddress = baseFilePath + @"Log.txt";
            var lines = File.ReadAllLines(fileAddress);
            string[] newLines = new string[lines.Length + 1];
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[lines.Length] = log;
            File.WriteAllLines(fileAddress, newLines);
        }

        private void LogEventError(string message, string errorType)
        {
            string errorMessage = "";
            errorMessage = "~~" + errorType.ToUpper() + "_ERROR~~ ";
            string log = DateTime.Now.ToString() + ": " + errorMessage + message;
            Console.WriteLine(log);
            string fileAddress = baseFilePath + @"Log.txt";
            var lines = File.ReadAllLines(fileAddress);
            string[] newLines = new string[lines.Length + 1];
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[lines.Length] = log;
            File.WriteAllLines(fileAddress, newLines);
        }

        private void LogCommand(string command, string username, string channel, string server, string param)
        {
            string log;
            command = command.ToUpper();
            username = username.ToUpper();
            channel = channel.ToUpper();
            server = server.ToUpper();
            if (param == "" || param == null)
            {
                param = "NULL";
            }
            log = DateTime.Now.ToString() + ": Now executing command " + command + " for user " + username + " in channel " + channel + " in server " + server + " with parameter " + param;
            Console.WriteLine(log);
            string fileAddress = baseFilePath + @"Log.txt";
            var lines = File.ReadAllLines(fileAddress);
            string[] newLines = new string[lines.Length + 1];
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[lines.Length] = log;
            File.WriteAllLines(fileAddress, newLines);
        }

        private void LogCommandError(string command, string username, string channel, string server, string param, string errorType)
        {
            string log;
            string errorMessage = "";
            errorMessage = "~~" + errorType.ToUpper() + "_ERROR~~ ";
            command = command.ToUpper();
            username = username.ToUpper();
            channel = channel.ToUpper();
            server = server.ToUpper();
            if (param == "" || param == null)
            {
                param = "NULL";
            }
            log = DateTime.Now.ToString() + ": " + errorMessage + "Halting execution of command " + command + " for user " + username + " in channel " + channel + " in server " + server + " with parameter " + param;
            Console.WriteLine(log);
            string fileAddress = baseFilePath + @"Log.txt";
            var lines = File.ReadAllLines(fileAddress);
            string[] newLines = new string[lines.Length + 1];
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[lines.Length] = log;
            File.WriteAllLines(fileAddress, newLines);
        }

        private BigInteger[] CalculateFibonacci(int n)
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

        private void SaveFibonacciString(BigInteger[] terms)
        {
            string fileAddress = baseFilePath + @"Sequences\Fibonacci.txt";
            string[] lines = new string[terms.Length];
            for (int i = 0; i < terms.Length; i++)
            {
                lines[i] = terms[i].ToString();
            }

            File.WriteAllLines(fileAddress, lines);
        }

        private string GetFibonacci(int n)
        {
            string fileAddress = baseFilePath + @"Sequences\Fibonacci.txt";

            var lines = File.ReadAllLines(fileAddress);
            return lines[n - 1];
        }

        private BigInteger[] CalculateLucas(int n)
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

        private void SaveLucasString(BigInteger[] terms)
        {
            string fileAddress = baseFilePath + @"Sequences\Lucas.txt";
            string[] lines = new string[terms.Length];
            for (int i = 0; i < terms.Length; i++)
            {
                lines[i] = terms[i].ToString();
            }

            File.WriteAllLines(fileAddress, lines);
        }

        private string GetLucas(int n)
        {
            string fileAddress = baseFilePath + @"Sequences\Lucas.txt";

            var lines = File.ReadAllLines(fileAddress);
            return lines[n - 1];
        }

        private bool isWhitesTurn(string boardName)
        {
            string FileAddress = baseFilePath + @"Chess\Boards.txt";
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

        private int[] ParseChessLocation(string square)
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

        private bool ChessIsLegal(string square1, string square2, char piece, char[,] board, bool isWhitesTurn)
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
            oldCoord = ParseChessLocation(square1);
            int[] newCoord = new int[2];
            newCoord = ParseChessLocation(square2);
            int oldX = oldCoord[0];
            int oldY = oldCoord[1];
            int newX = newCoord[0];
            int newY = newCoord[1];
            int xChange = oldX - newX;
            int yChange = oldY - newY;
            int xChangeAbs = Math.Abs(xChange);
            int yChangeAbs = Math.Abs(yChange);
            bool ChessIsLegal = false;

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
                                ChessIsLegal = true;
                            }
                        }
                        return ChessIsLegal;
                    }

                    //rooks
                    else if (piece == '2' || piece == '8')
                    {
                        if (xChangeAbs == 0)
                        {
                            ChessIsLegal = true;
                            if (yChange > 0)
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX, oldY - i] != '0')
                                    {
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX, oldY + i] != '0')
                                    {
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                        }
                        else if (yChangeAbs == 0)
                        {
                            ChessIsLegal = true;
                            if (xChange > 0)
                            {
                                for (int i = 1; i < (xChangeAbs); i++)
                                {
                                    if (board[oldX - i, oldY] != '0')
                                    {
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (xChangeAbs); i++)
                                {
                                    if (board[oldX + i, oldY] != '0')
                                    {
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                        }
                        return ChessIsLegal;
                    }

                    //knights
                    else if (piece == '3' || piece == '9')
                    {
                        if ((yChangeAbs == 1 && xChangeAbs == 2) || (yChangeAbs == 2 && xChangeAbs == 1))
                        {
                            ChessIsLegal = true;
                        }
                        return ChessIsLegal;
                    }

                    //bishops
                    else if (piece == '4' || piece == 'a')
                    {
                        if (yChangeAbs == xChangeAbs)
                        {
                            ChessIsLegal = true;
                            if (yChange > 0)
                            {
                                if (xChange > 0)
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX - i, oldY - i] != '0')
                                        {
                                            ChessIsLegal = false;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX + i, oldY - i] != '0')
                                        {
                                            ChessIsLegal = false;
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
                                            ChessIsLegal = false;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i < (yChangeAbs); i++)
                                    {
                                        if (board[oldX + i, oldY + i] != '0')
                                        {
                                            ChessIsLegal = false;
                                        }
                                    }
                                }
                            }
                        }
                        return ChessIsLegal;
                    }

                    //kings
                    else if (piece == '5' || piece == 'b')
                    {
                        if (yChangeAbs <= 1 && xChangeAbs <= 1)
                        {
                            ChessIsLegal = true;
                        }
                        return ChessIsLegal;
                    }

                    //queens
                    else if (piece == '6' || piece == 'c')
                    {
                        //checks to see if the queen is moving in a cardinal direction, and returns false if she isn't
                        if ((xChangeAbs == 0 && yChangeAbs != 0) || (yChangeAbs == 0 && xChangeAbs != 0) || (xChangeAbs == yChangeAbs))
                        {
                            ChessIsLegal = true;
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
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX + i, oldY - i] != '0')
                                    {
                                        ChessIsLegal = false;
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
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < (yChangeAbs); i++)
                                {
                                    if (board[oldX + i, oldY + i] != '0')
                                    {
                                        ChessIsLegal = false;
                                    }
                                }
                            }
                        }
                        return ChessIsLegal;
                    }

                    //default, because Visual Studio won't let me compile because not all code paths return a value
                    else
                    {
                        return ChessIsLegal;
                    }
                }
                //Another default, because Visual Studio won't let me compile because not all code paths return a value
                else
                {
                    return ChessIsLegal;
                }
            }
            //Hooray for defaults and Visual Studio thinking I'm too dumb to know all the possible inputs into my own function
            else
            {
                return ChessIsLegal;
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

        private void CreateChessBoard(string boardName)
        {
            char[,] board = { { '8', '7', '0', '0', '0', '0', '1', '2' }, { '9', '7', '0', '0', '0', '0', '1', '3' }, { 'a', '7', '0', '0', '0', '0', '1', '4' }, { 'c', '7', '0', '0', '0', '0', '1', '6' }, { 'b', '7', '0', '0', '0', '0', '1', '5' }, { 'a', '7', '0', '0', '0', '0', '1', '4' }, { '9', '7', '0', '0', '0', '0', '1', '3' }, { '8', '7', '0', '0', '0', '0', '1', '2' } };

            string FileAddress = baseFilePath + @"Chess\Boards.txt";
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

        private void DeleteChessBoard(string boardName)
        {
            string FileAddress = baseFilePath + @"Chess\Boards.txt";
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

        private void SaveChessBoard(string boardName, char[,] board, bool isWhitesTurn)
        {
            string FileAddress = baseFilePath + @"Chess\Boards.txt";
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

        private char[,] GetChessBoard(string boardName)
        {
            char[,] board = new char[8, 8];
            string FileAddress = baseFilePath + @"Chess\Boards.txt";
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

        private string ConvertChessPieceToEmoji(char piece, string serverId)
        {
            //to get the array in proper notation, copy and paste the following line into the server
            //emojiCodes = new string[] { "\:Blank:", "\:WPawn:", "\:WRook:", "\:WKnight:", "\:WBishop:", "\:WKing:", "\:WQueen:", "\:BPawn:", "\:BRook:", "\:BKnight:", "\:BBishop:", "\:BKing:", "\:BQueen:"};
            //and paste the output into the corresponding if statement

            string[] emojiCodes = { };
            if (serverId == "293460412593209345")
            {
                emojiCodes = new string[] { "<:Blank:298949936475537430>", "<:WPawn:298949937155276800>", "<:WRook:298949937498947584>", "<:WKnight:298949937121591297>", "<:WBishop:298949937050288128>", "<:WKing:298949936836247584>", "<:WQueen:298949937398546433>", "<:BPawn:298949936362422275>", "<:BRook:298949936651698177>", "<:BKnight:298949936341319684>", "<:BBishop:298949936236724225>", "<:BKing:298949936676864030>", "<:BQueen:298949936949493760>" };
            }
            else if (serverId == "237688211420217344")
            {
                emojiCodes = new string[] { "<:Blank:298927119562571777>", "<:WPawn:298602840694325248>", "<:WRook:298602840706777088>", "<:WKnight:298602840824479754>", "<:WBishop:298602840304254978>", "<:WKing:298602840333615106>", "<:WQueen:298602840916623370>", "<:BPawn:298602840488804362>", "<:BRook:298602840249860102>", "<:BKnight:298602840295735306>", "<:BBishop:298602839805263875>", "<:BKing:298602840207917056>", "<:BQueen:298602840027561985>" };
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

        private string ChessBoardToString1(string boardName, string serverId)
        {
            char[,] board = GetChessBoard(boardName);
            string boardString = @"....A      B      C      D      E      F      G      H
--------------------------------------------";

            for (int i = 0; i < 4; i++)
            {
                boardString = boardString + @"
" + (i + 1) + "|";
                for (int j = 0; j < 8; j++)
                {
                    boardString = boardString + ConvertChessPieceToEmoji(board[j, i], serverId) + "|";
                }
                boardString = boardString + @"
--------------------------------------------";
            }

            return boardString;
        }

        private string ChessBoardToString2(string boardName, string serverId)
        {
            char[,] board = GetChessBoard(boardName);
            string boardString = @"";

            for (int i = 4; i < 8; i++)
            {
                boardString = boardString + @"
" + (i + 1) + "|";
                for (int j = 0; j < 8; j++)
                {
                    boardString = boardString + ConvertChessPieceToEmoji(board[j, i], serverId) + "|";
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
            string FileAddress = baseFilePath + @"SlotMachine\PlayerList.txt";
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
            string FileAddress = baseFilePath + @"SlotMachine\PlayerList.txt";
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

            var logLines = File.ReadAllLines(baseFilePath + @"SlotMachine\Log.txt");
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
            File.WriteAllLines(baseFilePath + @"SlotMachine\Log.txt", newLogLines);
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

        private int WeightedSelector(double[] weights, double maxWeight, int j, Random rnd)
        {
            double random;
            int selection = -1;
            double selectionStart = 0.0;
            double selectionEnd = 0.0;
            selectionEnd = weights[1];
            random = rnd.NextDouble() * maxWeight;
            //Console.WriteLine("random = " + (random / maxWeight));
            for (int i = 1; i < 9; i++)
            {
                if (random >= selectionStart && random < selectionEnd)
                {
                    selection = i;
                    break;
                }

                selectionEnd += weights[i];
                selectionStart += weights[i];
            }
            return (selection + (j * 8));
        }

        private string DrawHangman(int numOfFails)
        {
            string[] hangman = new string[7];
            hangman[0] = ". ┌─────┐";
            hangman[1] = ".┃...............┋";
            hangman[2] = ".┃...............┋";
            hangman[3] = ".┃";
            hangman[4] = ".┃";
            hangman[5] = ".┃";
            hangman[6] = @"/-\";

            if (numOfFails > 0)
            {
                hangman[3] = ".┃.............😲";
            }
            if (numOfFails > 1)
            {
                hangman[4] = ".┃............./";
            }
            if (numOfFails > 2)
            {
                hangman[4] = ".┃............./ |";
            }
            if (numOfFails > 3)
            {
                hangman[4] = ".┃............./ | \\";
            }
            if (numOfFails > 4)
            {
                hangman[5] = ".┃............../";
            }
            if (numOfFails > 5)
            {
                hangman[5] = ".┃............../ \\";
            }

            string result = hangman[0] + @"
" + hangman[1] + @"
" + hangman[2] + @"
" + hangman[3] + @"
" + hangman[4] + @"
" + hangman[5] + @"
" + hangman[6];

            return result;
        }

        private void CreateHangman(string name, string word, int reward = 10000)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            string[] newLines = new string[lines.Length + 2];

            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
            }

            newLines[lines.Length] = name + ":" + word.ToUpper() + ":" + reward + ":0";
            string guessed = "";
            for (int i = 0; i < word.Length; i++)
            {
                guessed += " ";
            }
            newLines[lines.Length + 1] = guessed;
            File.WriteAllLines(fileAddress, newLines);
        }

        private string GetHangmanAnswer(string name)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(name))
                {
                    lineNum = i;
                }
            }
            string result = lines[lineNum];
            int colonIndex1 = GetNthIndex(result, ':', 1);
            int colonIndex2 = GetNthIndex(result, ':', 2);
            result = result.Substring(colonIndex1 + 1, colonIndex2 - colonIndex1 - 1);

            return result;
        }

        private char[] GetHangmanGuessed(string name)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(name))
                {
                    lineNum = i;
                }
            }

            string s = lines[lineNum + 1];
            char[] guessed = new char[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                guessed[i] = s[i];
            }
            return guessed;
        }

        private int GetHangmanFailCount(string name)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(name))
                {
                    lineNum = i;
                }
            }

            string s = lines[lineNum];
            int result = Int32.Parse(s.Substring(s.Length - 1));
            return result;
        }

        private void SetHangman(string name, string word, char[] guessed, int numOfFails, int reward = 10000)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(name))
                {
                    lineNum = i;
                }
            }

            string guessedString = "";
            for (int i = 0; i < guessed.Length; i++)
            {
                guessedString += guessed[i];
            }
            lines[lineNum] = name + ":" + word.ToUpper() + ":" + reward + ":" + numOfFails;
            lines[lineNum + 1] = guessedString;

            File.WriteAllLines(fileAddress, lines);
        }

        private int GetHangmanReward(string name)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(name))
                {
                    lineNum = i;
                }
            }

            string s = lines[lineNum];
            int colonIndex1 = GetNthIndex(s, ':', 2);
            int colonIndex2 = GetNthIndex(s, ':', 3);

            string result = s.Substring(colonIndex1 + 1, colonIndex2 - colonIndex1 - 1);
            Console.WriteLine(result);
            return Int32.Parse(result);
        }

        private void DeleteHangman(string name)
        {
            string fileAddress = baseFilePath + @"Hangman\Games.txt";
            var lines = File.ReadAllLines(fileAddress);
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(name))
                {
                    lineNum = i;
                }
            }

            int length = lines.Length;
            string[] newLines = new string[length - 2];

            for (int i = 0; i < lineNum; i++)
            {
                newLines[i] = lines[i];
            }
            for (int i = lineNum; i < length - 2; i++)
            {
                newLines[i] = lines[i + 2];
            }

            File.WriteAllLines(fileAddress, newLines);
        }

        private bool CheckersIsLegal(string square1, string square2, char piece, char[,] board, bool isWhitesTurn)
        {
            /*pieces will be converted to and from integers as follows:
            0 = empty space
            1 = white regular
            2 = red regular
            3 = white king
            4 = red king
            */

            int[] oldCoord = new int[2];
            oldCoord = ParseChessLocation(square1);
            int[] newCoord = new int[2];
            newCoord = ParseChessLocation(square2);
            int oldX = oldCoord[0];
            int oldY = oldCoord[1];
            int newX = newCoord[0];
            int newY = newCoord[1];
            int xChange = oldX - newX;
            int yChange = oldY - newY;
            int xChangeAbs = Math.Abs(xChange);
            int yChangeAbs = Math.Abs(yChange);
            //used to determine if the piece should be moving up or down the board
            int k = (isWhitesTurn) ? (-1) : (1);

            if ((newX % 2 == 0 && newY % 2 == 0) || (newX % 2 == 1 && newY % 2 == 1))
            {
                return false;
            }
            else
            {
                if ((yChange == 1*k || yChange == 2*k) && (yChangeAbs == xChangeAbs))
                {
                    if (yChangeAbs == 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (board[oldX + (xChange / 2), oldY + (k)] != ((isWhitesTurn) ? ('2') : ('1')) || board[oldX + (xChange / 2), oldY + (k)] != ((isWhitesTurn) ? ('4') : ('3')))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (board[newX, newY] == '0')
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void CreateCheckersBoard(string boardName)
        {
            char[,] board = { { '0', '2', '0', '0', '0', '1', '0', '1' }, { '2', '0', '2', '0', '0', '0', '1', '0' }, { '0', '2', '0', '0', '0', '1', '0', '1' }, { '2', '0', '2', '0', '0', '0', '1', '0' }, { '0', '2', '0', '0', '0', '1', '0', '1' }, { '2', '0', '2', '0', '0', '0', '1', '0' }, { '0', '2', '0', '0', '0', '1', '0', '1' }, { '2', '0', '2', '0', '0', '0', '1', '0' } };

            string FileAddress = baseFilePath + @"Checkers\Boards.txt";
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

        private void DeleteCheckersBoard(string boardName)
        {
            string FileAddress = baseFilePath + @"Checkers\Boards.txt";
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

        private void SaveCheckersBoard(string boardName, char[,] board, bool isWhitesTurn)
        {
            string FileAddress = baseFilePath + @"Checkers\Boards.txt";
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

        private char[,] GetCheckersBoard(string boardName)
        {
            char[,] board = new char[8, 8];
            string FileAddress = baseFilePath + @"Checkers\Boards.txt";
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

        private string ConvertCheckersPieceToEmoji(char piece, string serverId)
        {
            //to get the array in proper notation, copy and paste the following line into the server
            //emojiCodes = new string[] { "\:Blank:", "\:RReg:", "\:BReg:", "\:RKing:", "\:BKing:"};
            //and paste the output into the corresponding if statement

            string[] emojiCodes = { };
            if (serverId == "308360449509031936")
            {
                emojiCodes = new string[] { "<:Blank:308361674568630277>", "<:RReg:314509024815087616>", "<:BReg:314509024676544512>", "<:RKing:314509024773013504>", "<:BKing:308361674413572097>" };
            }
            else if (serverId == "237688211420217344")
            {
                emojiCodes = new string[] { "<:Blank:298927119562571777>", "<:RReg:314509686453960705>", "<:BReg:314509686667870211>", "<:RKing:314509686600892417>", "<:BKing:298602840207917056>" };
            }
            //placeholder
            else if (serverId == "")
            {

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
                default:
                    return "";
            }
        }

        private string CheckersBoardToString1(string boardName, string serverId)
        {
            char[,] board = GetCheckersBoard(boardName);
            string boardString = @"....A      B      C      D      E      F      G      H
--------------------------------------------";

            for (int i = 0; i < 4; i++)
            {
                boardString = boardString + @"
" + (i + 1) + "|";
                for (int j = 0; j < 8; j++)
                {
                    boardString = boardString + ConvertCheckersPieceToEmoji(board[j, i], serverId) + "|";
                }
                boardString = boardString + @"
--------------------------------------------";
            }

            return boardString;
        }

        private string CheckersBoardToString2(string boardName, string serverId)
        {
            char[,] board = GetCheckersBoard(boardName);
            string boardString = @"";

            for (int i = 4; i < 8; i++)
            {
                boardString = boardString + @"
" + (i + 1) + "|";
                for (int j = 0; j < 8; j++)
                {
                    boardString = boardString + ConvertCheckersPieceToEmoji(board[j, i], serverId) + "|";
                }
                boardString = boardString + @"
--------------------------------------------";
            }

            return boardString;
        }
    }
}