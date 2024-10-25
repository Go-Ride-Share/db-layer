using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace MongoExample
{

    public static class GetMovies
    {

        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        [Function("GetMovies")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)

            string limit = req.Query["limit"];
            IMongoCollection<Movie> moviesCollection = client.GetDatabase("sample_mflix").GetCollection<Movie>("movies");

            string searchItem = req.Query["searchItem"];
            var filter = Builders<Movie>.Filter.And(
                Builders<Movie>.Filter.Gt("year", 2005),
                Builders<Movie>.Filter.Lt("year", 2010),
                Builders<Movie>.Filter.Text(searchItem)
            );

            var update = Builders<Movie>.Update.Set("streamingPlatforms", new BsonArray
            {
                new BsonDocument
                {
                    { "name", "Netflix" },
                    { "url", "https://www.netflix.com" },
                    { "subscriptionRequired", true }
                },
                new BsonDocument
                {
                    { "name", "Amazon Prime" },
                    { "url", "https://www.primevideo.com" },
                    { "subscriptionRequired", true }
                },
                new BsonDocument
                {
                    { "name", "Hulu" },
                    { "url", "https://www.hulu.com" },
                    { "subscriptionRequired", true }
                }
            });

            await moviesCollection.UpdateManyAsync(filter, update);

            var moviesToFind = moviesCollection.Find(filter);

            if(limit != null && Int32.Parse(limit) > 0) {
                moviesToFind.Limit(Int32.Parse(limit));
            }

            List<Movie> movies = moviesToFind.ToList();

            return new OkObjectResult(movies);

        }

    }

}