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
    photo: string, ex "testPhotoUrl"
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
