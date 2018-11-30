# Cerith
Library for wrapping MongoDb with an API using ASP.Net Core 2.1

## Installation

In Visual studio, create a ASP.Net Core Web Application.
Select API as type.

Install the Nuget Cerith package.
You can find the nuget package [here](https://www.nuget.org/packages/Cerith/1.0.0).


1. Add a configuration file to your project

Create a new json file. The file should have following layout:
```javascript
{
  "MongoConnectionString": "{{connectionstring}}",
  "Collections": [
    {
      "DatabaseName": "MyDatabase",
      "Name": "MyCollectionAttributes", //The collection name
      "IdName": "SpecialIdName", //optional: when the default collection id name is not '_id'
      "Route": "/api/mycollection/attributes/", //optional: when not using default routing
      "AccessType": "Admin" // ReadOnly or Admin, ReadOnly is the default
    },
    {
      "DatabaseName": "MyDatabase",
      "Name": "MyReadOnlyCollection",
    }
  ]
}
```
When Route is not set, it defaults to `/api/{collectionname}/`.
When IdName is not set, it defaults to `_id`.
When AccessType is not set, it defaults to `ReadOnly`.


2. Add middleware in the Startup.cs

Assume the filename of the previous step was `cerith.json'.

Add the configuration file to the ConfigurationBuilder in the constructor of the Startup.cs
```
public Startup(IHostingEnvironment env)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        ...
        .AddJsonFile("cerith.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"cerith.{env.EnvironmentName}.json", optional: true)
        ...
    Configuration = builder.Build();
}
```

Add using statement in the Startup.cs.
```
using Cerith;
```
Add the Cerith middelware configuration in the ConfigureServices;
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    services.AddCerith(Configuration);
}
```
Call the app.UseCerith(). <b>Do this before the app.UseMvc()</b>.
```
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    //IMPORTANT: place this before the UseMvc to prevent MVC to consume the request!
    app.UseCerith();

    app.UseMvc();
}
```

You're done!

## Examples

### Setting
MongoDb has one database "MyDatabase" and 2 collections MyCollectionAttributes and MyReadOnlyCollection.
The Cerith is setup as shown above.

#### Get

There are several ways to get to objects. 
Assume we want to get object with id = 1 from the MyCollectionAttributes.

* Via Short notation
```javascript
GET http://localhost:1234/api/mycollection/attributes/1
```
This will return a single object in the response.

* Via Query parameter (_id is the default Mongo id name)
```javascript
GET http://localhost:1234/api/mycollection/attributes?_id=1
```
This will return an array with only one item.

You can use any available property from the document, also nested.
If, for instance, the object has a nested property moreInfo.type, you can use this in your query as such.
```javascript
GET http://localhost:1234/api/mycollection/attributes?moreInfo.type=helpfull
```
This will return all documents with the property moreInfo.type set to 'helpfull'

* Via Mongo filter syntax parameter
```javascript
GET http://localhost:1234/api/mycollection/attributes?filter={'_id', '1'}
```
This will return an array with only one item.

Full Mongo filter syntax is supported. See [documentation](https://docs.mongodb.com/manual/reference/method/db.collection.find/) for more information.


#### Update a document

Although Mongo allows to update a single field in a document, Cerith does always a document replacement.

It is the responsibility of the user to handle concurrency.

To update a document use
```javascript
PUT http://localhost:1234/api/mycollection/attributes/1
```
The document is in the body of the request.

When the document is not found in the collection a HttpStatus 409 Conflict will be returned.

#### Insert a document

To insert a document use
```javascript
POST http://localhost:1234/api/mycollection/attributes
```
The document is in the body of the request.

When the document is already in the collection a HttpStatus 409 Conflict will be returned.