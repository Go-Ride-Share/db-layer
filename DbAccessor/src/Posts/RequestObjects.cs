using System.Text.Json.Serialization;

namespace GoRideShare.posts
{
    public class PostDetails
    {
        [JsonPropertyName("postId")]
        public Guid? PostId { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("posterId")]
        public required Guid PosterId { get; set; }

        [JsonRequired]
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonRequired]
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("originName")]
        public string? OriginName { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLat")]
        public required float OriginLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLng")]
        public required float OriginLng { get; set; }

        [JsonPropertyName("destinationName")]
        public string? DestinationName { get; set; }

        [JsonRequired]        
        [JsonPropertyName("destinationLat")]
        public required float DestinationLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLng")]
        public required float DestinationLng { get; set; }

        [JsonRequired]
        [JsonPropertyName("price")]
        public required float Price { get; set; }

        [JsonRequired]
        [JsonPropertyName("seatsAvailable")]
        public required int SeatsAvailable { get; set; }

        [JsonPropertyName("seatsTaken")]
        public int? SeatsTaken { get; set; }

        [JsonRequired]
        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

        public PostDetails(){}

        public (bool, string) validate()
        {
            if (DepartureDate == "")
            {
                return (true, "DepartureDate cannot be empty");
            }
            if (Description == "")
            {
                return (true, "Description cannot be empty");
            }
            if (Name == "")
            {
                return (true, "Name cannot be empty");
            }
            if ( 90 < OriginLat || OriginLat < -90 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLng is Invalid");
            }
            if ( 180 < DestinationLng || DestinationLng < -180 )
            {
                return (true, "DestinationLng is Invalid");
            }
            
            return (false, "");
        }
    }

    public class  SearchCriteria
    {
        [JsonRequired]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLat")]
        public required float OriginLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLng")]
        public required float OriginLng { get; set; }

        [JsonRequired]        
        [JsonPropertyName("destinationLat")]
        public required float DestinationLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLng")]
        public required float DestinationLng { get; set; }

        [JsonRequired]
        [JsonPropertyName("price")]
        public float? Price { get; set; }

        [JsonRequired]
        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

        public SearchCriteria(){}

        public (bool, string) validate()
        {
            if (DepartureDate == "")
            {
                return (true, "DepartureDate cannot be empty");
            }

            //
            //  Parse into the correct Date Format
            //

            if ( 90 < OriginLat || OriginLat < -90 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLng is Invalid");
            }
            if ( 180 < DestinationLng || DestinationLng < -180 )
            {
                return (true, "DestinationLng is Invalid");
            }
            
            return (false, "");
        }
    }
}
