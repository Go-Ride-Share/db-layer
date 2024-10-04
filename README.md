## `DbAccessor` Function App API Definition

### /CreateUser
Request paylaod template:
```
{
    email: "test@email.com",
    password: "hashed_password_here",
    name: "testName",
    bio: "testBio",
    phone: "4312245323",
    photo: "testPhotoUrl"
}
```

### /VerifyLoginCredentials
Request paylaod template:
```
{
    email: "test@email.com",
    password: "hashed_password_here"
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
