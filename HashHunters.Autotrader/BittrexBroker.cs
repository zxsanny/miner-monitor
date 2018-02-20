﻿using Bittrex.Net;
using Bittrex.Net.Objects;
using HashHunters.Autotrader.Core.DTO;
using HashHunters.Autotrader.Core.Interfaces;
using HashHunters.Autotrader.Entities;
using HashHunters.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HashHunters.Autotrader.Services
{
    public class BittrexBroker : IMarketBroker
    {
        IDatabase RedisDB;
        BittrexClient BittrexClient;
        ISecurityService SecurityService;

        public BittrexBroker(ISecurityService securityService)
        {
            SecurityService = securityService;
            var redis = ConnectionMultiplexer.Connect("localhost");
            RedisDB = redis.GetDatabase();
        }

        public async Task<List<CandleData>> Get(CurrencyPair market, CandleInterval candleInterval, DateTime from)
        {
            var res = await BittrexClient.GetCandlesAsync(market.ToBittrex(), candleInterval.ToBittrex());
            return res.Result.Select(c => c.FromBittrex()).ToList();
        }

        public void RunPollingMarketData()
        {
            var exchangeKey = SecurityService.GetKey(ExchangeEnum.Bittrex);
            BittrexClient = new BittrexClient(exchangeKey.ApiKey, exchangeKey.ApiSecret);

            foreach (var market in CurrencyPairConstants.MainCurrencyPairs)
            {
                foreach (var interval in Enum<CandleInterval>.GetValues())
                {
                    var task = Get(market, interval, DateTime.Now.AddMonths(-1));
                    task.Wait();
                    var res = task.Result;
                }
            }
        }

        private async Task<long> WriteCandles(CurrencyPair market, CandleInterval interval, List<CandleData> list)
        {
            var entries = list.Select(c => new SortedSetEntry(c.ToString(), c.Timestamp.ToUnixTime())).ToArray();
            return await RedisDB.SortedSetAddAsync($"{market}_{interval}", entries);
        }
    }

    public static class BittrexConverter
    {
        static Dictionary<CandleInterval, TickInterval> CandleDict = new Dictionary<CandleInterval, TickInterval>
        {
            { CandleInterval.M1, TickInterval.OneMinute },
            { CandleInterval.M5, TickInterval.FiveMinutes },
            { CandleInterval.M30, TickInterval.HalfHour },
            { CandleInterval.M60, TickInterval.OneHour },
            { CandleInterval.M1440, TickInterval.OneDay }
        };

        public static TickInterval ToBittrex(this CandleInterval interval) => CandleDict[interval];

        public static string ToBittrex(this CurrencyPair market)
        {
            var strs = market.ToString().Split('_');
            return $"{strs[1]}-{strs[0]}";
        }

        public static CandleData FromBittrex(this BittrexCandle bittrexCandle) 
            => new CandleData
            {
                Open = bittrexCandle.Open,
                High = bittrexCandle.High,
                Low = bittrexCandle.Low,
                Close = bittrexCandle.Close,
                Volume = bittrexCandle.Volume,
                Timestamp = bittrexCandle.Timestamp,
                BaseVolume = bittrexCandle.BaseVolume
            };
    }
}