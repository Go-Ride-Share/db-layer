using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DbLayer
{
    public class PollMovies
    {
        private MongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<BsonDocument> _moviesCollection;

        public PollMovies(string connectionString, string dbName)
        {
            Console.WriteLine("Initializing MongoClient...");
            try
            {
                _client = new MongoClient(connectionString);
                _database = _client.GetDatabase(dbName);
                _moviesCollection = _database.GetCollection<BsonDocument>("movies");
                Console.WriteLine("MongoClient initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to MongoDB: " + ex.Message);
                throw;
            }
        }

        public string PollMoviesByAuthor(string author)
        {
            Console.WriteLine("Polling movies for author: " + author);
            try
            {
                var filter = CreateAuthorFilter(author);
                var moviesList = RetrieveMoviesByFilter(filter);
                
                if (moviesList == null || moviesList.Count == 0)
                {
                    Console.WriteLine("No movies found for author: " + author);
                    return "[]";
                }

                List<string> movieStreamingList = new List<string>();
                foreach (var movie in moviesList)
                {
                    string movieId = movie.GetValue("_id").ToString();
                    var title = movie.GetValue("title").AsString;
                    
                    List<string> platforms = GetPlatformsByMovie(movieId);
                    if (platforms.Count > 0)
                    {
                        string movieJson = GenerateMoviePlatformJson(movieId, title, platforms);
                        movieStreamingList.Add(movieJson);
                    }
                }
                Console.WriteLine("Polling complete for author: " + author);
                return "[" + string.Join(",", movieStreamingList) + "]";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error polling movies: " + ex.Message);
                return "[]";
            }
        }

        private FilterDefinition<BsonDocument> CreateAuthorFilter(string author)
        {
            Console.WriteLine("Creating filter for author: " + author);
            var builder = Builders<BsonDocument>.Filter;
            return builder.Eq("author", author);
        }

        private List<BsonDocument> RetrieveMoviesByFilter(FilterDefinition<BsonDocument> filter)
        {
            Console.WriteLine("Retrieving movies from database...");
            try
            {
                var moviesList = _moviesCollection.Find(filter).ToList();
                LogRetrievedMovies(moviesList);
                return moviesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving movies: " + ex.Message);
                return new List<BsonDocument>();
            }
        }

        private void LogRetrievedMovies(List<BsonDocument> moviesList)
        {
            Console.WriteLine("Movies retrieved: " + moviesList.Count);
            foreach (var movie in moviesList)
            {
                Console.WriteLine("Movie ID: " + movie.GetValue("_id") + ", Title: " + movie.GetValue("title"));
            }
        }

        private List<string> GetPlatformsByMovie(string movieId)
        {
            Console.WriteLine("Fetching platforms for movie ID: " + movieId);
            try
            {
                var platformsList = new List<string>();
                
                var streamingPlatforms = DefineAvailablePlatforms();

                foreach (var platform in streamingPlatforms)
                {
                    if (IsPlatformAvailable(platform))
                    {
                        platformsList.Add(JsonSerializer.Serialize(platform));
                    }
                }

                Console.WriteLine("Platforms found for movie ID: " + movieId);
                return platformsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving platforms: " + ex.Message);
                return new List<string>();
            }
        }

        private List<dynamic> DefineAvailablePlatforms()
        {
            Console.WriteLine("Defining available platforms...");
            return new List<dynamic>
            {
                new { Name = "Netflix", Country = "USA", Link = "https://netflix.com" },
                new { Name = "Hulu", Country = "USA", Link = "https://hulu.com" },
                new { Name = "Amazon Prime", Country = "Global", Link = "https://primevideo.com" },
                new { Name = "Disney+", Country = "Global", Link = "https://disneyplus.com" },
                new { Name = "Apple TV+", Country = "Global", Link = "https://tv.apple.com" }
            };
        }

        private bool IsPlatformAvailable(dynamic platform)
        {
            Random random = new Random();
            return random.Next(0, 2) == 1;
        }

        private string GenerateMoviePlatformJson(string movieId, string title, List<string> platforms)
        {
            Console.WriteLine("Generating JSON for movie ID: " + movieId);
            string platformArray = "[" + string.Join(",", platforms) + "]";
            return $"{{ \"movieId\": \"{movieId}\", \"title\": \"{title}\", \"platforms\": {platformArray} }}";
        }

        public string ConvertMovieListToJson(List<string> movieList)
        {
            Console.WriteLine("Converting movie list to JSON...");
            return "[" + string.Join(",", movieList) + "]";
        }

        public void DisplayAuthorMovies(string author)
        {
            string moviesJson = PollMoviesByAuthor(author);
            Console.WriteLine("Movies JSON for author " + author + ":");
            Console.WriteLine(moviesJson);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter MongoDB connection string: ");
            string connectionString = Console.ReadLine();
            Console.Write("Enter database name: ");
            string dbName = Console.ReadLine();

            PollMovies pollMovies = new PollMovies(connectionString, dbName);

            while (true)
            {
                Console.Write("Enter author (or 'exit' to quit): ");
                string author = Console.ReadLine();
                if (author.ToLower() == "exit")
                {
                    Console.WriteLine("Exiting...");
                    break;
                }

                pollMovies.DisplayAuthorMovies(author);
                Console.WriteLine("Operation complete for author: " + author);
            }
        }
    }
}
