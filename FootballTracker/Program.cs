using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Soccer365;
using Soccer365.Models;
using TelegramBot;

namespace FootballTracker
{
    class Program
    {
        async static Task Main(string[] args)
        {
            await TelegramAPI.Start();
        }
    }
}
