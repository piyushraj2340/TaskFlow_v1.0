# Task Monitoring Application (TMA)

A comprehensive task and productivity monitoring web application built with **ASP.NET Core MVC 8.0** and **Entity Framework Core**. TMA is designed to help users establish clear goals, break them down into actionable tasks, organize daily routines using a To-Do system, and visualize productivity analytics using stored procedures and dynamic calculations.

---

## 🚀 Key Features

*   **Goal & Objectives Management**: Establish high-level goals with customized target completion dates, track active vs. completed goals, and automatically track progress.
*   **Hierarchical Task Tracking**: Link tasks to specific goals or manage them independently. Features priority labels (Low, Medium, High), status updates, and automatic status transitions.
*   **Daily To-Do Lists**: Generate dynamic to-do instances from tracked tasks to plan daily routines.
*   **Productivity Analytics Dashboard**: View comprehensive progress stats including completed tasks count, success ratios, and weekly productivity growth calculations.
*   **Rich Notes & Sticky Notes System**: Attach formatted notes to tasks or goals. Features pinning, custom background colors, and timestamp tracking.
*   **Secure Authentication**: Built-in User Account management powered by **ASP.NET Core Identity** (supports password hashing, verified user checks, and roles).

---

## 🛠️ Tech Stack

### Backend
*   **Framework**: .NET 8.0 (ASP.NET Core MVC & Web API)
*   **Data Access**: Entity Framework Core 9.0 (Code-First) & Stored Procedures
*   **Authentication**: ASP.NET Core Identity (JWT Authentication & Application Cookies)
*   **Logging**: Serilog (structured file logging with automatic rotation)
*   **Object Mapping**: AutoMapper

### Frontend
*   **Styling**: Tailwind CSS & Custom CSS
*   **Interactions**: jQuery, AJAX
*   **Data Grid**: DataTables.net (for advanced client-side sorting and filtering)

### Database & Hosting
*   **Database Engine**: SQL Server (LocalDB for development, Azure SQL Database for production)
*   **CI/CD**: Azure DevOps Pipelines

---

## 📐 Architecture & Design Patterns

The project follows a modular **N-Tier Architecture** utilizing the **Repository-Service Pattern** for separation of concerns:

```
[Presentation Layer]  (MVC Controllers / Razor Views / API Controllers)
         │
         ▼
[Service Layer]       (Business Logic, Mapping, Emailing, Logging)
         │
         ▼
[Repository Layer]    (Data Access Layer, Stored Procedures, EF Queries)
         │
         ▼
[Data Source]         (SQL Server / Azure DB via ApplicationDbContext)
```

*   **Controllers**: Map incoming HTTP requests to corresponding DTOs (Data Transfer Objects) and ViewModels.
*   **Services**: Encapsulate business logic, invoke mappers, and write structured logs.
*   **Repositories**: Encapsulate queries and interact directly with the database.
*   **DTOs & ViewModels**: Used to decouple data models from views and prevent over-posting vulnerabilities.

---

## ⚙️ Setup & Installation

### Prerequisites
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) or SQLExpress
*   Visual Studio 2022 or VS Code

### Step 1: Clone the Repository
```bash
git clone https://github.com/piyushraj2340/TaskMonitorApp.git
cd TaskMonitorApp
```

### Step 2: Initialize Local User Secrets
To prevent committing database passwords and secret keys, the app uses **User Secrets**. Run the following commands to initialize and store your local connection credentials:

```bash
# Initialize User Secrets
dotnet user-secrets init

# Set your local database connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=TaskMonitoringApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

# Set a development JWT secret key
dotnet user-secrets set "Jwt:Secret" "YOUR_SUPER_SECRET_DEVELOPMENT_KEY_MUST_BE_AT_LEAST_256_BITS"
```

### Step 3: Run Database Migrations & Stored Procedures
TMA utilizes Entity Framework Core for schema management, alongside stored procedures for reporting.

1. Apply migrations to create the database schema:
   ```bash
   dotnet ef database update
   ```
2. Execute the database SQL scripts located in the `App_Data` / SQL directory to deploy the required stored procedures:
   *   `usp_AddTodoFromTask`
   *   `usp_AddUpdateTaskWithGoals`
   *   `usp_DashboardAnalyses`
   *   `usp_TodoProgressAnalyses`
   *   `usp_UpdateEndedTask`

### Step 4: Run the Application
```bash
dotnet run
```
Open `https://localhost:7198` (or the port specified in your `launchSettings.json`) in your browser.

---

## 🔒 Production Deployment Configuration

For security reasons, **never commit real credentials to `appsettings.json`**. When deploying to production (such as Azure App Service), configure your database connections and secrets as **Application Settings / Environment Variables** in the Azure portal:

| Config Key | Azure Setting Equivalent | Description |
| :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | Production Azure SQL Database connection string |
| `Jwt:Secret` | `Jwt__Secret` | Secure 256-bit token signature key |
| `Jwt:Issuer` | `Jwt__Issuer` | Token Issuer domain |
| `Jwt:Audience` | `Jwt__Audience` | Token Audience domain |

---