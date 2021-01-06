using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PMDividendDataLoader.Models;
using PMUnifiedAPI.Models;
using Serilog;

namespace PMDividendDataLoader
{
    public class Program
    {
        public static MongoClient Client = null;
        public static string AlphaVantageKey = string.Empty;
        public static IConfigurationRoot Configuration;
        public static string MongoConnectionString = string.Empty;
        
        public static void Main(string[] args)
        {
            SetupServices();
            SetupLogger();
            CreateMongoConnection();
            var apiKeyAndSymbols = FetchSymbolsAndApiKeyFromSqlServer();
            AlphaVantageKey = apiKeyAndSymbols.Item2;
            var symbols = apiKeyAndSymbols.Item1;
            var result = GetDividendDataAndInsertToMongoDb(symbols);
            Log.Information($"Inserted or updated {result} dividend(s)");
        }

        public static int GetDividendDataAndInsertToMongoDb(List<string> symbols)
        {
            int recordsInsertedOrUpdated = 0;
            try
            {
                var db = Client.GetDatabase("PseudoMarketsDB");
                var collection = db.GetCollection<BsonDocument>("DividendData");
                var replaceOptions = new ReplaceOptions()
                {
                    IsUpsert = true
                };

                var dividends = AlphaVantageClient.GetDividendData(symbols, AlphaVantageKey).GetAwaiter().GetResult();
                foreach (DividendData dividend in dividends)
                {
                    if (dividend.dividendPerShare > 0)
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("symbol", dividend.symbol);
                        var dividendAsBson = dividend.ToBsonDocument();

                        collection.ReplaceOne(filter, dividendAsBson, replaceOptions);

                        recordsInsertedOrUpdated++;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, $"{nameof(GetDividendDataAndInsertToMongoDb)}");
            }

            return recordsInsertedOrUpdated;
        }

        public static Tuple<List<string>, string> FetchSymbolsAndApiKeyFromSqlServer()
        {
            List<string> symbols = new List<string>();
            string apiKey = string.Empty;
            try
            {
                using (var db = new PseudoMarketsDbContext())
                {
                    symbols = db.Positions.Select(x => x.Symbol).Distinct().ToList();
                    apiKey = db.ApiKeys.Where(x => x.ProviderName == "AV").Select(x => x.ApiKey).FirstOrDefault();
                }

                if (symbols.Any())
                {
                    Log.Information("Fetched symbols from Positions table in SQL Server");
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, $"{nameof(FetchSymbolsAndApiKeyFromSqlServer)}");
            }

            return new Tuple<List<string>, string>(symbols, apiKey);
        }

        public static void SetupServices()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
        }

        public static void SetupLogger()
        {
            // Setup the Serilog logger
            string logFileName = "PMDividendDataLoader-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFileName)
                .CreateLogger();
            Log.Information("Starting PMDividendDataLoader");
        }

        private static ILoggerFactory SetupSerilog(ILoggerFactory factory)
        {
            factory.AddSerilog(dispose: true);
            return factory;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Inject Serilog
            var factory = SetupSerilog(new LoggerFactory());
            serviceCollection.AddSingleton(factory);

            serviceCollection.AddLogging();

            // Setup our application config
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            serviceCollection.AddSingleton<IConfigurationRoot>(Configuration);

            serviceCollection.AddTransient<Program>();

            MongoConnectionString = Configuration.GetConnectionString("MongoDb");
        }

        public static void CreateMongoConnection()
        {
            try
            {
                Client = new MongoClient(MongoConnectionString);

                Log.Information("Connected to Mongo DB");
            }
            catch (Exception e)
            {
                Log.Fatal(e, $"{nameof(CreateMongoConnection)}");
            }
        }
    }
}
