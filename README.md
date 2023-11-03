# Blog.WebApi

This is a .NET 7 Minimal Api with Supabase.

Blog.WebApi is a API for blogs that use Supabase services for store data and files, with JWT Authentication.

## Getting Started with Supabase

First of all, you need a [Supabase](https://supabase.com/) account.

After create your Supabase's account you need to create a project that will hosts your API database and bucket. You can choose any name you like it, i used "WebApi".

You will need to get yours Supabase's credentials as "Url" and "Key" on "Settings -> Database -> API" and put in appsettings file.

At Supabase's SQL Editor execute "Tables.sql" commands to create your database's tables and "PostgresFunction.sql" to create the stored procedures.

At Supabase's Storage create a new public bucket named "WebApi.Multimidia" and give him all free-access policies.

With this settled we finish our Supabase configuration.

## Run Locally

```bash
dotnet restore
dotnet build
dotnet run
```

## Documentation

[Supabase - C# Client](https://supabase.com/docs/reference/csharp/v1/eq)

[Microsoft - Minimal Api](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0)
