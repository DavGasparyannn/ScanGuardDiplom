﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using MGOBankApp.BLL.Services;

namespace ScanGuard.TelegramBot.Service
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly TGUserService _userService; // BLL для работы с пользователями

        public BotService(ITelegramBotClient botClient, TGUserService userService)
        {
            _botClient = botClient;
            _userService = userService;
        }

        public async Task StartAsync()
        {
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // Получаем все обновления
            };

            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message.Type == MessageType.Text)
                {
                    var text = message.Text;
                    if (text == "/start")
                    {
                        await client.SendMessage(message.Chat.Id, "Hello !!!\r\nIf you want to connect your ScanGuard account to the bot, then use the command \r\n<b>/link [Your tg token]</b>\r\n You can get it in the Get Token section✅", cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else if (text == "/link")
                    {
                        await client.SendMessage(message.Chat.Id, "Please, use\r\n<b>/link [Your tg token]</b>", cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else if (text!.StartsWith("/link"))
                    {
                        var tgToken = text.Split(" ")[1];

                        /*var user = new TGUserEntity
                        {
                            ChatId = message.Chat.Id,
                            Token = token
                        };
                        using var context = new ApplicationDbContext();
                        context.TGUserEntities.Add(user);
                        await context.SaveChangesAsync(token);*/
                        await client.SendMessage(message.Chat.Id, "You have successfully linked your account", cancellationToken: token);
                    }


                    await client.SendMessage(message.Chat.Id, "Hello, world!", cancellationToken: token);
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
