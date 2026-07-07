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

## 📊 Productivity Formulas

The system monitors growth and efficiency based on the following calculations:

*   **Daily To-Do Productivity**: 
    $$\text{Productivity \%} = \left( \frac{\text{Completed Tasks}}{\text{Total Staged Tasks}} \right) \times 100$$
*   **Weekly Growth Rate**: Compares the average productivity of the current 7 days against the previous 7-14 days:
    $$\text{Growth \%} = \left( \frac{\text{Current Week Avg} - \text{Past Week Avg}}{\text{Past Week Avg}} \right) \times 100$$

---

## 🛠️ Current Development Roadmap

Here is the developer todo list and tracking status for ongoing improvements:

### Completed (Done ✅)
- [x] Initialized MVC repository structure with Service-Repository pattern.
- [x] Dynamic Daily To-Do list generators.
- [x] Date tracking properties (`StartedOn`, `EndingOn`, `CompletedOn`).
- [x] User-based authentication via ASP.NET Core Identity.
- [x] Model validation error messages loaded through partial views.
- [x] Localdb connection setups and User Secrets separation.

### Active Priorities & Upcoming Features 🚀
- [ ] **Advanced Error Handling**: Introduce unified redirection/rendering filters for both API and MVC exception modes.
- [ ] **Sticky Notes & Timeline View**: Add rich content text formatting (WYSIWYG editor), pinned notes layout, and automatic timeline/roadmap views.
- [ ] **Google-Tasks Style Reoccurrence**: Expand recurring tasks logic for daily, weekly, or custom interval resets.
- [ ] **Admin Control Panel**: Add site settings (logo uploading, custom theme toggle, audit logs viewing) and user access controls (verifying, blocking accounts).
- [ ] **SignalR Notifications**: Implement real-time notifications for automated status changes (e.g., tasks moving to `Running` or `Ended`).
- [ ] **Social Logins**: Add OAuth2 configuration for Microsoft/Google accounts in the Identity system.

---

## 📝 Stored Procedures Index
The application relies on the following database stored procedures for specialized analytics and bulk updates:
- `usp_AddAndGetTodoFromTask.sql`
- `usp_AddTodoFromTask.sql`
- `usp_AddUpdateTaskWithGoals.sql`
- `usp_ChangeGoalStatus.sql`
- `usp_ChangeTaskStatus.sql`
- `usp_DashboardAnalyses.sql`
- `usp_GetAllGoalsWithStatus.sql`
- `usp_GetAllTasksWithStatus.sql`
- `usp_GetAllTasksWithStatusByGoalId.sql`
- `usp_GetGoalById.sql`
- `usp_GetTaskById.sql`
- `usp_TodoProgressAnalyses.sql`
- `usp_UpdateEndedTask.sql`