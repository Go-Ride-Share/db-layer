## `DbAccessor` Function App API Definition

### /CreateUser
Request paylaod template:
```
{
    email: string, ex "test@email.com",
    password: string, ex "testPassword",
    name: string, ex "testName",
    bio: string, ex "testBio",
    phone: string, ex "4312245323",
    photo: string, ex "photo_encoding"
}
```

Response payload template:
```
{
    user_id: string, ex "new_user_guid"
}
```

### /VerifyLoginCredentials
Request paylaod template:
```
{
    email: string, ex "test@email.com",
    password: string, ex "testPassword"
}
```

Response payload template:
```
{
    user_id: string, ex "new_user_guid",
    photo: string, ex "photo_encoding"
}
```

### /GetUser
Request Header template:
```
{
    X-User-ID: string, ex "user_guid_to_get_user",
}
```

Response payload template:
```
{
    email: string, ex "test@email.com",
    name: string, ex "testName",
    bio: string, ex "testBio",
    phone: string, ex "4312245323",
    photo: string, ex "photo_encoding"
}
```

### /EditUser
Request Header template:
```
{
    X-User-ID: string, ex "user_guid_to_get_user",
}
```

Request paylaod template:
```
{
    // Note - pass null for the fields you don't wanna edit
    email: string, ex "test@email.com" or null,
    name: string, ex "testName" or null,
    bio: string, ex "testBio" or null,
    phone: string, ex "4312245323" or null,
    photo: string, ex "photo_encoding" or null
}
```

### /CreatePost
Request Header template:
```
{
    X-User-ID: string, ex "user_guid_to_get_user",
}
```

Request paylaod template:
```
{
    "name": "namename",
    "posterId": "user_guid",
    "description": "This is a dummy ride-share post.",
    "departureDate": "2024-10-09",
    "originLat": 40.712776,
    "originLng": -74.005974,
    "destinationLat": 34.052235,
    "destinationLng": -118.24368,
    "price": 25.00,
    "seatsAvailable": 2
}
```

Response payload template:
```
{
    postId: string, ex "new_post_guid",
}
```

### /GetPost
Request Header template:
```
{
    X-User-ID: string, ex "user_guid",
}
```

Request Query parameter template:
```
/GetPost?userId={user_guid}
```

Response paylaod template:
```
{
    "name": "namename",
    "posterId": "user_guid",
    "description": "This is a dummy ride-share post.",
    "departureDate": "2024-10-09",
    "originLat": 40.712776,
    "originLng": -74.005974,
    "destinationLat": 34.052235,
    "destinationLng": -118.24368,
    "price": 25.00,
    "seatsAvailable": 2
}
```

### /findrides/intercity
Request paylaod template:
```
{
    origin: string, ex "Winnipeg, MB, Canada"
    destination: string, ex "Winnipeg, MB, Canada"
    date: date, the date trip is leaving
    seatsNeeded: 1,
}
```

Response payload template:
```
TODO
```

### /findrides/intracity
Request payload template:
```
{
    city: string, ex "Winnipeg, MB, Canada"
    destination: float, coordinates of where the passnager wants to go
    departureMinDateTime: DateTime, this is the earliest the passanger is willing to depart
    departureMaxDateTime: DateTime, this is the latest the passanger is willing to depart
    date: string, the date trip is leaving
    seatsNeeded: 1,
}
```

Response payload template:
```
TODO
```
