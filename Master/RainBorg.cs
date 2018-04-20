﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RainBorg
{
    partial class RainBorg
    {
        // Initialization
        static void Main(string[] args)
        {
            // Vanity
            Console.WriteLine(Banner);

            // Resume if told to
            if (File.Exists(Constants.ResumeFile))
            {
                Console.WriteLine("{0} {1}    Resuming bot...", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");
                JObject Resuming = JObject.Parse(File.ReadAllText(Constants.ResumeFile));
                UserPools = Resuming["userPools"].ToObject<Dictionary<ulong, List<ulong>>>();
                Greylist = Resuming["greylist"].ToObject<List<ulong>>();
                UserMessages = Resuming["userMessages"].ToObject<Dictionary<ulong, UserMessage>>();
                File.Delete(Constants.ResumeFile);
            }

            // Create exit handler
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            // Run bot
            Start();
        }

        // Main loop
        public static Task Start()
        {
            // Begin bot process in its own thread
            new Thread(delegate ()
            {
                new RainBorg().RunBotAsync().GetAwaiter().GetResult();
            }).Start();

            // Begin timeout loop in its own thread
            new Thread(delegate ()
            {
                UserTimeout();
            }).Start();

            // Get console commands
            string command = "";
            while (command.ToLower() != "exit")
            {
                // Get command
                command = Console.ReadLine();

                if (command.ToLower().StartsWith("dotip"))
                {
                    Waiting = waitTime;
                    Console.WriteLine("Tip sent.");
                }
                else if (command.ToLower().StartsWith("reset"))
                {
                    foreach (KeyValuePair<ulong, List<ulong>> Entry in UserPools)
                        Entry.Value.Clear();
                    Greylist.Clear();
                    Console.WriteLine("Pools reset.");
                }
                else if (command.ToLower().StartsWith("loglevel"))
                {
                    logLevel = int.Parse(command.Substring(command.IndexOf(' ')));
                    Config.Save();
                    Console.WriteLine("Log level changed.");
                }
                else if (command.ToLower().StartsWith("say"))
                {
                    foreach (ulong Channel in StatusChannel)
                        (_client.GetChannel(Channel) as SocketTextChannel).SendMessageAsync(command.Substring(command.IndexOf(' ')));
                    Console.WriteLine("Sent message.");
                }
                else if (command.ToLower().StartsWith("addoperator"))
                {
                    if (!Operators.Contains(ulong.Parse(command.Substring(command.IndexOf(' ')))))
                        Operators.Add(ulong.Parse(command.Substring(command.IndexOf(' '))));
                    Console.WriteLine("Added operator.");
                }
                else if (command.ToLower().StartsWith("removeoperator"))
                {
                    if (Operators.Contains(ulong.Parse(command.Substring(command.IndexOf(' ')))))
                        Operators.Remove(ulong.Parse(command.Substring(command.IndexOf(' '))));
                    Console.WriteLine("Removed operator.");
                }
                else if (command.ToLower().StartsWith("restart"))
                {
                    Console.WriteLine("{0} {1}    Relaunching bot...", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");
                    Paused = true;
                    JObject Resuming = new JObject
                    {
                        ["userPools"] = JToken.FromObject(UserPools),
                        ["greylist"] = JToken.FromObject(Greylist),
                        ["userMessages"] = JObject.FromObject(UserMessages)
                    };
                    File.WriteAllText(Constants.ResumeFile, Resuming.ToString());
                    Process.Start("RelaunchUtility.exe", "RainBorg.exe");
                    ConsoleEventCallback(2);
                    Environment.Exit(0);
                }
            }

            // Completed, exit bot
            return Task.CompletedTask;
        }

        // Initiate bot
        public async Task RunBotAsync()
        {
            // Populate API variables
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            // Add event handlers
            _client.Log += Log;
            _client.Ready += Ready;

            // Load local files
            Console.WriteLine("{0} {1}    Loading config", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");
            await Config.Load();
            Console.WriteLine("{0} {1}    Loaded config", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");
            Console.WriteLine("{0} {1}    Loading stats", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");
            await Stats.Load();
            Console.WriteLine("{0} {1}    Loaded stats", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");

            // Register commands and start bot
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, Constants.BotToken);
            await _client.StartAsync();

            // Start tip cycle
            await DoTipAsync();

            // Rest infinitely
            await Task.Delay(-1);
        }

        // Ready event handler
        private Task Ready()
        {
            // Show start up message in all tippable channels
            if (Startup && entranceMessage != "")
            {
                _client.CurrentUser.ModifyAsync(m => { m.Username = _username; });
                foreach(ulong ChannelId in UserPools.Keys)
                    (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(entranceMessage);
                Startup = false;
            }

            // Completed
            return Task.CompletedTask;
        }

        // Log event handler
        private Task Log(LogMessage arg)
        {
            // Write message to console
            Console.WriteLine(arg);

            // Relaunch if disconnected
            if (arg.Message.Contains("Disconnected"))
            {
                Console.WriteLine("{0} {1}    Relaunching bot...", DateTime.Now.ToString("HH:mm:ss"), "RainBorg");
                Paused = true;
                JObject Resuming = new JObject
                {
                    ["userPools"] = JToken.FromObject(UserPools),
                    ["greylist"] = JToken.FromObject(Greylist),
                    ["userMessages"] = JObject.FromObject(UserMessages)
                };
                File.WriteAllText(Constants.ResumeFile, Resuming.ToString());
                Process.Start("RelaunchUtility.exe", "RainBorg.exe");
                ConsoleEventCallback(2);
                Environment.Exit(0);
            }

            // Completed
            return Task.CompletedTask;
        }

        // Register commands within API
        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += MessageReceivedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        // Message received
        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Get message and create a context
            var message = arg as SocketUserMessage;
            if (message == null) return;
            var context = new SocketCommandContext(_client, message);

            // Check if channel is a tippable channel
            if (UserPools.ContainsKey(message.Channel.Id) && !message.Author.IsBot)
            {
                // Check for spam
                await CheckForSpamAsync(message, out bool IsSpam);
                if (!IsSpam && !UserPools[message.Channel.Id].Contains(message.Author.Id))
                {
                    // Add user to tip pool
                    if (logLevel >= 1)
                        Console.WriteLine("{0} {1}      Adding {2} ({3}) to user pool on channel #{4}", DateTime.Now.ToString("HH:mm:ss"), "Tipper",
                            message.Author.Username, message.Author.Id, message.Channel);
                    UserPools[message.Channel.Id].Add(message.Author.Id);
                }

                // Remove users from pool if pool exceeds the threshold
                if (UserPools[message.Channel.Id].Count > userMax)
                    UserPools[message.Channel.Id].RemoveAt(0);
            }

            // Check if message is a commmand
            int argPos = 0;
            if (message.HasStringPrefix(Constants.BotPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute command and log errors to console
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        // Tip loop
        public static async Task DoTipAsync()
        {
            Start:
            // If client is connected
            if (_client.ConnectionState == ConnectionState.Connected)
            {
                // Create a randomizer
                Random r = new Random();

                try
                {
                    // Get balance
                    using (WebClient client = new WebClient())
                    {
                        string dl = client.DownloadString(Constants.BalanceURL);
                        JObject j = JObject.Parse(dl);
                        tipBalance = (double)j["balance"] / 100;
                    }

                    // Check tip balance against minimum tip
                    if (tipBalance - tipFee < tipMin)
                    {
                        // Log low balance message
                        Console.WriteLine("{0} {1}      {2}", DateTime.Now.ToString("HH:mm:ss"), "Tipper",
                            "Balance does not meet minimum tip threshold.");

                        // Check if bot should show a donation message
                        if (ShowDonation)
                        {
                            // Create message
                            var builder = new EmbedBuilder();
                            builder.WithTitle("UH OH");
                            builder.WithColor(Color.Green);
                            builder.Description = String.Format(tipBalanceError, String.Format("{0:n}", tipMin + tipFee - tipBalance));

                            // Cast message to all status channels
                            foreach (ulong u in StatusChannel)
                                await (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, builder);

                            // Reset donation message
                            ShowDonation = false;
                        }
                    }

                    // Grab eligible channels
                    List<ulong> Channels = EligibleChannels();

                    // No eligible channels
                    if (Channels.Count < 1)
                    {
                        Console.WriteLine("{0} {1}      {2}", DateTime.Now.ToString("HH:mm:ss"), "Tipper",
                            "No eligible tipping channels.");
                    }
                    else
                    {
                        // Roll until an eligible channel is chosen
                        ulong ChannelId = 0;
                        while (!Channels.Contains(ChannelId))
                            ChannelId = ChannelWeight[r.Next(0, ChannelWeight.Count)];

                        // Check user count
                        if (tipBalance - tipFee < tipMin && UserPools[ChannelId].Count < userMin)
                        {
                            Console.WriteLine("{0} {1}      {2}", DateTime.Now.ToString("HH:mm:ss"), "Tipper",
                                "Not enough users to meet threshold, will try again next tipping cycle.");
                        }

                        // Do a tip cycle
                        else if (tipBalance - tipFee >= tipMin && UserPools[ChannelId].Count >= userMin)
                        {
                            // Set tip amount
                            if (tipBalance - tipFee > tipMax)
                                tipAmount = tipMax / UserPools[ChannelId].Count;
                            else tipAmount = (tipBalance - tipFee) / UserPools[ChannelId].Count;

                            // Round tip amount down
                            tipAmount = Math.Floor(tipAmount * 100) / 100;

                            // Begin creating tip message
                            int userCount = 0;
                            double tipTotal = 0;
                            DateTime tipTime = DateTime.Now;
                            Console.WriteLine("{0} {1}      Sending tip of {2} to {3} users in channel #{4}", DateTime.Now.ToString("HH:mm:ss"), "Tipper",
                                tipAmount.ToString("F"), UserPools[ChannelId].Count, _client.GetChannel(ChannelId));
                            string m = ".tip " + tipAmount.ToString("F") + " ";

                            // Loop through user pool and add them to tip
                            for (int i = 0; i < UserPools[ChannelId].Count; i++)
                            {
                                // Make sure the message size is below the max discord message size
                                if ((m + _client.GetUser(UserPools[ChannelId][i]).Mention + " ").Length <= 2000)
                                {
                                    // Add a username mention
                                    m += _client.GetUser(UserPools[ChannelId][i]).Mention + " ";

                                    // Increment user count
                                    userCount++;

                                    // Add to tip total
                                    tipTotal += tipAmount;

                                    // Add tip to stats
                                    try
                                    {
                                        await Stats.Tip(tipTime, ChannelId, UserPools[ChannelId][i], tipAmount);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error adding tip to stat sheet: " + e.Message);
                                    }
                                }
                            }

                            // Send tip message to channel
                            await (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(m);

                            // Begin building status message
                            var builder = new EmbedBuilder();
                            builder.WithTitle("TUT TUT");
                            builder.ImageUrl = RaindanceImages[r.Next(0, RaindanceImages.Count)];
                            builder.Description = "Huzzah, " + tipTotal + " TRTL just rained on " + userCount +
                                " chatty turtle";
                            if (UserPools[ChannelId].Count > 1) builder.Description += "s";
                            builder.Description += " in #" + _client.GetChannel(ChannelId) + ", they ";
                            if (UserPools[ChannelId].Count > 1) builder.Description += "each ";
                            builder.Description += "got " + tipAmount + " TRTL!";
                            builder.WithColor(Color.Green);

                            // Send status message to all status channels
                            foreach (ulong u in StatusChannel)
                                await (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, builder);

                            // Clear user pool
                            UserPools[ChannelId].Clear();
                            Greylist.Clear();
                            ShowDonation = true;

                            // Update stat sheet
                            try
                            {
                                await Stats.Update();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error saving stat sheet: " + e.Message);
                            }
                        }
                    }
                }
                catch { }

                // Calculate wait time until next tip
                if (waitMin < waitMax)
                    waitTime = r.Next(waitMin, waitMax);
                else waitTime = 10 * 60 * 1000;
                waitNext = DateTime.Now.AddMilliseconds(waitTime).ToString("HH:mm:ss") + " " + _timezone;
                Console.WriteLine("{0} {1}      Next tip in {2} milliseconds ({3})", DateTime.Now.ToString("HH:mm:ss"), "Tipper", waitTime, waitNext);

                // Wait for X milliseconds
                Waiting = 0;
                while (Waiting < waitTime || Paused)
                {
                    await Task.Delay(500);
                    Waiting += 500;
                }
            }

            // Restart tip loop
            goto Start;
        }

        // Grab eligible channels
        private static List<ulong> EligibleChannels()
        {
            List<ulong> Output = new List<ulong>();
            foreach (KeyValuePair<ulong, List<ulong>> Entry in UserPools)
            {
                if (Entry.Value.Count >= userMin)
                {
                    Output.Add(Entry.Key);
                }
            }
            return Output;
        }

        // Remove expired users from userpools
        private static async Task UserTimeout()
        {
            while (true)
            {
                // Do not run loop while paused
                while (Paused) { }

                if (logLevel >= 3)
                    Console.WriteLine("{0} {1}     Running timeout loop", DateTime.Now.ToString("HH:mm:ss"), "Timeout");

                // Clone pools to iterate through (CPU-friendly work-around)
                Dictionary<ulong, List<ulong>> Temp = new Dictionary<ulong, List<ulong>>();
                foreach (KeyValuePair<ulong, List<ulong>> Pool in UserPools)
                    Temp.Add(Pool.Key, Pool.Value);

                // Iterate over all channel pools
                foreach (KeyValuePair<ulong, List<ulong>> Pool in Temp)
                {
                    // Iterate over users within pool
                    for (int i = 0; i < Pool.Value.Count; i++)
                    {
                        // Check if their last message was created beyond the timeout period
                        if ((DateTime.Now.ToUniversalTime() - DateTime.MinValue.ToUniversalTime()).TotalMilliseconds >= timeoutPeriod +
                            (UserMessages[Pool.Value[i]].CreatedAt.ToUniversalTime() - DateTime.MinValue.ToUniversalTime()).TotalMilliseconds)
                        {
                            if (logLevel >= 3)
                                Console.WriteLine("{0} {1}     Checking {2} against {3} on channel #{4}", DateTime.Now.ToString("HH:mm:ss"), "Timeout",
                                    (UserMessages[Pool.Value[i]].CreatedAt - DateTime.MinValue).TotalMilliseconds, (DateTime.Now - DateTime.MinValue).TotalMilliseconds, _client.GetChannel(Pool.Key));

                            // Remove user from channel's pool
                            if (logLevel >= 1)
                                Console.WriteLine("{0} {1}     Removed {2} ({3}) from user pool on channel #{4}", DateTime.Now.ToString("HH:mm:ss"), "Timeout",
                                    _client.GetUser(Pool.Value[i]), Pool.Value[i], _client.GetChannel(Pool.Key));
                            await RemoveUserAsync(_client.GetUser(Pool.Value[i]), Pool.Key);
                        }
                    }
                }

                // Wait
                await Task.Delay(1);
            }
        }

        // Remove a user from all user pools
        public static Task RemoveUserAsync(SocketUser User, ulong ChannelId)
        {
            // 0 = all channels
            if (ChannelId == 0)
                foreach (KeyValuePair<ulong, List<ulong>> Entry in UserPools)
                {
                    if (Entry.Value.Contains(User.Id))
                        Entry.Value.Remove(User.Id);
                }

            // Specific channel pool
            else if (UserPools.ContainsKey(ChannelId))
            {
                if (UserPools[ChannelId].Contains(User.Id))
                    UserPools[ChannelId].Remove(User.Id);
            }

            return Task.CompletedTask;
        }

        // On exit
        public static bool ConsoleEventCallback(int eventType)
        {
            // Exiting
            if (eventType == 2)
            {
                if (exitMessage != "") foreach (KeyValuePair<ulong, List<ulong>> Entry in UserPools)
                    (_client.GetChannel(Entry.Key) as SocketTextChannel).SendMessageAsync(exitMessage).GetAwaiter().GetResult();
                Config.Save().GetAwaiter().GetResult();
            }
            return false;
        }
    }
}
