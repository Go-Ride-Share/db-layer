using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
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
        {

            string limit = req.Query["limit"];
            IMongoCollection<Movie> moviesCollection = client.GetDatabase("sample_mflix").GetCollection<Movie>("movies");

            BsonDocument filter = new BsonDocument{
                {
                    "year", new BsonDocument{
                        { "$gt", 2005 },
                        { "$lt", 2010 }
                    }
                }
            };

            var moviesToFind = moviesCollection.Find(filter);

            if(limit != null && Int32.Parse(limit) > 0) {
                moviesToFind.Limit(Int32.Parse(limit));
            }

            List<Movie> movies = moviesToFind.ToList();

            return new OkObjectResult(movies);

        }

    }

}