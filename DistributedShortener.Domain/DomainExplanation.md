````
DistributedShortener.Domain
│
├── Aggregates/
│   └── ShortLink.cs              ← el aggregate
│
├── ValueObjects/
│   ├── ShortCode.cs              ← value object
│   └── OriginalUrl.cs            ← value object
│
├── Events/
│   └── LinkCreatedEvent.cs       ← domain event
│
└── Exceptions/
└── InvalidShortCodeException.cs
└── InvalidUrlException.cs
````

Domain validations goes inside each ValueObject.
FluentValidation acts in the Application layer validating commands
and queries.

***Aggregate***
Group of domain objects acting as a unit. Comes from DDD
The root (AggregateRoot) is the entry point. In this case, the entry point is
ShortLink.cs

IMPORTANT: Aggregates are created through a Factory method (builder pattern).

```csharp
var link = ShortLink.Create(originalUrl, createdBy)
```

