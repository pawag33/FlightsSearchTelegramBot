using FlightSearchAppCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using System.IO;
using FlightSearchAppRepository;
using FlightSerachAppEntities;

namespace FlightSearchAppConsoleStarter
{
    class Program
    {

        private static readonly TelegramBotClient Bot = new TelegramBotClient("your telegram bot key");

        private static FlightSearchParams flightSearchParams = new FlightSearchParams();

        public static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            bool messageProcessed = false;
            if (message.Text.StartsWith("/"))
            {
                messageProcessed = await ProcessCommandMessage(message.Text, message.Chat.Id);
            }
            else
            {
                messageProcessed = await ProcessSearchMessage(message.Text, message.Chat.Id);
            }
            if (messageProcessed == false)
            {
                const string usage = @"
                                            Usage:
                                            /flight_serach   - search a flight";

                try
                {

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        usage,
                        replyMarkup: new ReplyKeyboardRemove());
                }
                catch (Exception e)
                {

                }
            }
        }

        private static async Task<bool> ProcessSearchMessage(string msg, long chatId)
        {
            try
            {
                SearchFlight(flightSearchParams, chatId);
            }
            catch (Exception e)
            {

            }
            return true;
        }

        private static async Task<bool> ProcessSearchMessageOld(string msg, long chatId)
        {
            string responseMsg = string.Empty;
            switch (msg.ToLower())
            {

                // send custom keyboard
                case "ukraine":
                    // return true;
                    ReplyKeyboardMarkup ReplyKeyboard = new[]
                    {
                        new[] { "Kiev,Odesa,Lviv" }
                    };

                    await Bot.SendTextMessageAsync(
                       chatId,
                        "Choose Where to Flight",
                        replyMarkup: ReplyKeyboard);
                    return true;
                case "kiev,odesa,lviv":
                    responseMsg = "plese send outbound flight date (in format dd/mm/yyyy)";
                    await Bot.SendTextMessageAsync(
                       chatId,
                       responseMsg,
                       replyMarkup: new ReplyKeyboardRemove());
                    return true;
                default:
                    DateTime date;
                    try
                    {
                        date = DateTime.ParseExact(msg, "dd/MM/yyyy",
                                           System.Globalization.CultureInfo.InvariantCulture);
                        if (flightSearchParams.OutboundDate == default(DateTime))
                        {
                            flightSearchParams.OutboundDate = date;
                            responseMsg = "plese send inbound flight date (in format dd/mm/yyyy)";
                            await Bot.SendTextMessageAsync(
                               chatId,
                               responseMsg,
                               replyMarkup: new ReplyKeyboardRemove());

                        }
                        else
                        {
                            flightSearchParams.InboundDate = date;
                            flightSearchParams.DestinationPlace = DestinationPlace.UKRAINE_KIEV;
                            try
                            {
                                SearchFlight(flightSearchParams, chatId);
                                flightSearchParams = new FlightSearchParams();
                                responseMsg = "The search process is started";
                                await Bot.SendTextMessageAsync(
                                 chatId,
                                 responseMsg,
                                 ParseMode.Html,
                                 replyMarkup: new ReplyKeyboardRemove());
                            }
                            catch (Exception ex)
                            {
                                flightSearchParams = new FlightSearchParams();
                                responseMsg = "Flight api error";
                                await Bot.SendTextMessageAsync(
                                   chatId,
                                   responseMsg,
                                   replyMarkup: new ReplyKeyboardRemove());
                                return true;
                            }

                        }
                        return true;

                    }
                    catch (Exception e)
                    {
                        return false;
                    }
            }
        }

        private static void SearchFlight(FlightSearchParams flightSearchParams, long chatId)
        {
            Core core = new Core();
            List<DestinationPlace> destinationPlaces = new List<DestinationPlace>()
                { DestinationPlace.UKRAINE_KIEV, DestinationPlace.UKRAINE_LVIV, DestinationPlace.UKRAINE_ODESA };

            FlightsPresetSearchParams flightsPresetSearchParams = new FlightsPresetSearchParams();
            flightsPresetSearchParams.DatesRangeSearch = false;
            flightsPresetSearchParams.StartDateOfRange = new DateTime(2018, 11, 14);
            flightsPresetSearchParams.EndDateOfRange = new DateTime(2018, 11, 20);
            flightsPresetSearchParams.FlightSerachFilter = new FlightSerachFilter();
            flightsPresetSearchParams.DestinationPlaces = destinationPlaces;
            Task.Run(() => core.GetFlights(flightsPresetSearchParams, chatId, ProcessGetFlightsCallBack));

        }

        private static string GetDestinationPlaceName(DestinationPlace destinationPlace)
        {
            switch (destinationPlace)
            {
                case DestinationPlace.UKRAINE_KIEV:
                    return "Kiev";
                case DestinationPlace.UKRAINE_ODESA:
                    return "Odesa";
                case DestinationPlace.UKRAINE_LVIV:
                    return "Lviv";
                default:
                    throw new Exception("Unknown Destination Place");
            }
        }

        private static void ProcessGetFlightsCallBack(IEnumerable<Flight> flights, DestinationPlace destinationPlace, long chatId, bool lastResult)
        {
            string responseMsg = string.Empty;
            responseMsg += string.Format("Serach results for {0} \n",GetDestinationPlaceName(destinationPlace));
            if (flights.Any())
            {
                foreach (var flight in flights)
                {
                    responseMsg += flight.Price + "\n";
                    responseMsg += string.Format("<a href='{0}'>Link to book </a> \n", flight.linkToBookAgent);
                }
            }
            else
            {
                responseMsg += string.Format("No flights found for this destenation  \n");
            }
            if (lastResult)
            {
                responseMsg += string.Format("The search is completed \n");
            }
            else
            {
                responseMsg += string.Format("The search process not completed yet \n");

            }
            Bot.SendTextMessageAsync(
               chatId,
               responseMsg,
                ParseMode.Html,
               replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task<bool> ProcessCommandMessage(string msg, long chatId)
        {
            switch (msg.ToLower())
            {
                // send custom keyboard
                case "/flight_serach":
                    ReplyKeyboardMarkup ReplyKeyboard = new[]
                    {
                        new[] { "Ukraine" }
                    };

                    await Bot.SendTextMessageAsync(
                       chatId,
                        "Choose Where to Flight",
                        replyMarkup: ReplyKeyboard);
                    return true;
                default:
                    return false;
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                $"Received {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Received {callbackQuery.Data}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultLocation(
                    id: "1",
                    latitude: 40.7058316f,
                    longitude: -74.2581888f,
                    title: "New York")   // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 40.7058316f,
                            longitude: -74.2581888f)    // message if result is selected
                    },

                new InlineQueryResultLocation(
                    id: "2",
                    latitude: 13.1449577f,
                    longitude: 52.507629f,
                    title: "Berlin") // displayed result
                    {

                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 13.1449577f,
                            longitude: 52.507629f)   // message if result is selected
                    }
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQueryEventArgs.InlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}