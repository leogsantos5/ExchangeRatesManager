# 📊 Exchange Rates Manager API

An ASP .NET Core 8.0 Web API for managing foreign exchange rates with automatic rate fetching, database storage, and RabbitMQ event messaging.

### ✍🏻 **Prerequisites** 
Before running the project, ensure you have the following installed:
- **[Docker Desktop](https://www.docker.com/get-started/)** (Required for running SQL Server & RabbitMQ containers) 🐋
- **[Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** (Cloning & analyzing code. API testing with Swagger) 👓
- **[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** (Required to run the API)
---

## 🖥️ **Setup Instructions**
Follow these steps to set up and run the project:

### ** Open a Terminal or a Command Prompt on your machine to run the following commands: **

### ** Start SQL Server Container (Database) **
```sh
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=testPassw0rd!" -e "MSSQL_PID=Express" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```

### ** Start RabbitMQ Publisher Container (Message Qeueuing) **
```sh
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

### ** Open Visual Studio 2022 and clone this repository **

### ** Open Package Manager Console inside it and apply migrations: **
```sh
dotnet ef database update --project ExchangeRatesManager.Infrastructure --startup-project ExchangeRatesManager.WebApi
```
You are all setup now!

Note: To properly test RabbitMQ's Message Qeueing behaviour, read comments on ExchangeRateConsumer.cs file.
---

### 🛠️ **Technolgies and libraries used: **

- ASP.NET CORE 8 / .NET 8
- Entity Framework Core (ORM to manipulate data from and to the DB)
- Microsoft SQL Server Management Studio 2020 (Testing CRUD fucntionality on DB)
- Postman (AlphaVantage public API testing)
- Refit (AlphaVantage public API calls)
- AutoMapper (Mapping from domain entities to view models)
- FluentValidation (JSON data validation on API's endpoints)
- Serilog (Logging to file easily)
- MediatR (CQRS pattern)
- xUnit (Unit Testing)
- Moq (Mocking for Unit Tests)
- Swagger (API Testing)
---

### 🏛️ **Why choose a Clean Architecture with DDD, CQRS & Mediator Pattern?

For this project, I followed Clean Architecture principles combined with CQRS (Command Query Responsibility Segregation), MediatR and a Domain Driven Design. At my current job, I develop small backend microservices used and integrated into larger web apps, used by thousands of clients. Because of that, all our solutions employ those same principles and patterns. These choices were made to ensure scalability, maintainability, and separation of concerns. 

🛀🏻 1. Clean Architecture – Separation of Concerns
I structured the solution using Clean Architecture to achieve:

✅ Independent business logic – The core logic is not dependent on external frameworks.
✅ Separation of concerns – Divides the application into Domain, Application, Infrastructure, and Presentation (Controllers only, for backend) layers.
✅ Easier testing – Business logic can be tested without worrying about controllers or external dependencies.

📈 2. CQRS (Command Query Responsibility Segregation) – Better Performance & Maintainability
CQRS splits read (queries) and write (commands) operations, leading to:

✅ Improved scalability – Read operations can be optimized separately from writes.
✅ Simplified logic – Each request type has a dedicated handler, making it easier to maintain.

📨 3. MediatR (Mediator Pattern) – Decoupling & Flexibility
I used MediatR to implement CQRS, which helps by:

✅ Decoupling components – Controllers don’t directly depend on services they use mediators instead.
✅ Better organization – Keeps controllers thin by moving logic into separate handlers.
✅ Scalability – Easier to extend the system with additional behaviors like logging or validation without modifying existing code.

🧱 4. Why Use DDD?

✅ Encapsulation of Business Rules – Business logic lives inside domain entities, not scattered across services.
✅ High Maintainability – Changes to business logic happen in one place (domain layer).
✅ Separation of Concerns – Each layer has a distinct responsibility, improving code organization.

---

END!
