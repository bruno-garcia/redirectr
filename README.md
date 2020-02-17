# shortr

[![Travis](https://travis-ci.org/bruno-garcia/shortr.svg?branch=master)](https://travis-ci.org/bruno-garcia/shortr/builds/632765857)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/9i9o95vv30224bl6?svg=true)](https://ci.appveyor.com/project/bruno-garcia/shortr)
[![Tests](https://img.shields.io/appveyor/tests/bruno-garcia/shortr/master?compact_message)](https://ci.appveyor.com/project/bruno-garcia/shortr/branch/master/tests)
[![codecov](https://codecov.io/gh/bruno-garcia/shortr/branch/master/graph/badge.svg)](https://codecov.io/gh/bruno-garcia/shortr)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/9a5ba18bcceb4dcfbb9fffd2fcd2196d)](https://www.codacy.com/manual/bruno-garcia/shortr?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=bruno-garcia/shortr&amp;utm_campaign=Badge_Grade)

## A .NET Library for URL shortening and request redirection
This project can create short URLs (i.e: [https://nugt.net/s/FqJFMC2](https://nugt.net/s/FqJFMC2])) and redirect requests to the original URL.

You can see it in use [in the NuGetTrends source code](https://github.com/NuGetTrends/nuget-trends).

### How does it work
It offers an in-memory store, for testing. And a [Postgres extension package](https://github.com/bruno-garcia/shortr/tree/master/src/Shortr.Npgsql) 
and an [ASP.NET Core extension](https://github.com/bruno-garcia/shortr/tree/master/src/Shortr.AspNetCore) to plug it into the request pipeline. 
 
It also includes a [web app](https://github.com/bruno-garcia/shortr/tree/master/src/Shortr.Web) to demonstrate its use from a JavaScript client.

### Configuration
By default it only generates links to URLs sharing the same `BaseAddress` as configured for the application itself.
It's possible to change this behavior in different ways.

For example, allowing a direct URL to be created to any destination:

```csharp
var options = new ShortrOptions 
{
  AllowRedirectToAnyDomain = true
}
```

The other [options can be found here](https://github.com/bruno-garcia/shortr/blob/master/src/Shortr/ShortrOptions.cs).

### Run the Web project
Make sure you have [.NET Core 3](https://dot.net), `docker` and `docker-compose` to run `PostgreSQL`:

    1. Start postgres with `docker-compose up`
    2. Navigate to `src\Shortr.Web` and run `dotnet run`
    3. Browser `http://localhost:5000`
