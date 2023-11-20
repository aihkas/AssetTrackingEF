# Asset Tracking Application

This Asset Tracking Application is a simple console application to manage and track company assets such as laptops and mobile phones. The application is built using C#.NET with Entity Framework Core for data access.

## Prerequisites

- .NET 7.0 SDK
- Docker (if not running on Windows or if you prefer containerized SQL Server)
- Entity Framework Core CLI tools

## Setup and Configuration

### Database Configuration

The application is configured to use SQL Server by default. If you're running on Windows and have SQL Server installed, you can use LocalDB.

For non-Windows users or those who prefer to use Docker, SQL Server can be run as a Docker container. Follow the instructions below to set up SQL Server in Docker:

1. Pull the SQL Server Docker image:

   ```
   docker pull mcr.microsoft.com/mssql/server:2019-latest
   ```

Run the SQL Server container:

```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Yourpassword1" -p 1433:1433 --name sql_server_demo -d mcr.microsoft.com/mssql/server:2019-latest
```

Replace `Yourpassword1` with a strong password of your choice.

Update the connection string in AssetContext class to match the Docker SQL Server configuration if you're not using the default:

```
optionsBuilder.UseSqlServer(@"Server=localhost,1433;Database=AssetTrackingDb;User ID=SA;Password=Yourpassword1");
```
Add Encrypt=False;TrustServerCertificate=True to the connection string if you encounter any SSL/TLS related issues.

### Running Migrations

Before running the application, you need to apply the database migrations. Make sure you are in the root directory of the project and run the following commands:


```dotnet ef migrations add InitialCreate
dotnet ef database update
```


### Running the Application
To run the application, use the following command in the terminal:

```
dotnet run
```
This will start the application, and you will be able to interact with it via the console.

### Features

Add and track laptops and mobile phones as assets.
Store assets with details such as model name, purchase date, and price.
Sort and view assets by office location, purchase date, and asset type.
Highlight assets that are nearing the end of their lifecycle.
Reporting Issues
If you encounter any issues or have suggestions, please open an issue in the repository.

### Contributions

Contributions to this project are welcome. Please fork the repository and submit a pull request with your changes.

### License

This project is licensed under the MIT License - see the LICENSE file for details.
