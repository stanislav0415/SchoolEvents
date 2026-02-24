> **Creators and Authors:**   Miroslav Tsintsarski, Stanislav Vladimirov

> **Emails:**   127ikr@unibit.bg, 132ikr@unibit.bg  

> **Affiliation:**  University of Library Studies and Informational Technologies (ULSIT), Sofia  

> **Date:**  02.01.2026


# School Events Management System

## Overview

This web application is built using **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **ASP.NET Core Identity**. It manages school events, organizers, participants, and registrations, with role‑based access for teachers and students. Teachers can create and manage events, while students can browse events and register.

---

## Table of Contents
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Architecture Decisions & Rationale](#architecture-decisions--rationale)
- [Why This Architecture for University Coursework](#why-this-architecture-for-university-coursework)
- [Database Design](#database-design)
- [Installation](#installation)
- [Running the Application](#running-the-application)
- [Usage Guide](#usage-guide)
- [Project Structure](#project-structure)

---

## Features

- Full CRUD functionality for Events and Organizers
- Participant management and event registrations
- Monthly calendar view for scheduled events
- Filtering of events by date, type, and keywords
- Role‑based access control:
  - **Teacher** role can create, edit, and delete events and organizers
  - **Student** role can browse and register for events
- User authentication and authorization with ASP.NET Core Identity
- Data validation and graceful error handling
- MVC architecture with Razor views and Bootstrap for a responsive UI

---

## Technologies Used

- **.NET 8** - Latest long-term support version of the framework
- **ASP.NET Core MVC** - Web framework with Model-View-Controller pattern
- **Entity Framework Core** - Object-Relational Mapper (ORM) for database access
- **SQL Server** - Relational database management system
- **ASP.NET Core Identity** - Authentication and authorization framework
- **Bootstrap** - CSS framework for responsive design

---

## Architecture Decisions & Rationale

This section explains **why specific architectural choices were made** for this project and why they are appropriate for an academic context rather than enterprise production.

### 1. **MVC Pattern (Instead of API + SPA)**

#### What I Chose:
**Traditional ASP.NET Core MVC** with server-side Razor views

#### Why This Choice:
 **Learning Focus**: Demonstrates understanding of the fundamental MVC pattern taught in web development courses  
 **Simplicity**: One codebase, one deployment, easier to debug and present  
 **Time Efficiency**: Can complete full-featured application in academic semester timeframe  
 **Academic Requirements**: Most university courses still teach and evaluate MVC fundamentals  

#### What I Avoided:
 **API + React/Angular SPA**: Would require learning an additional JavaScript framework, doubling development time  
 **Blazor**: Too new, less stable, fewer learning resources for troubleshooting  

#### Real-World Context:
While modern enterprise applications often use API + SPA architecture, **MVC is still widely used** for:
- Internal business applications
- Admin panels
- Content management systems
- Applications where SEO matters (server-side rendering)

---

### 2. **Direct DbContext Injection (Instead of Repository Pattern)**

#### What I Chose:
Controllers directly access `ApplicationDbContext` via dependency injection

```csharp
public class EventsController : Controller
{
    private readonly ApplicationDbContext _db;
    
    public EventsController(ApplicationDbContext db) => _db = db;
    
    public async Task<IActionResult> Index()
    {
        var events = await _db.SchoolEvents
            .Include(e => e.Organizer)
            .AsNoTracking()
            .ToListAsync();
        return View(events);
    }
}
```

#### Why This Choice:
 **Clarity**: Easy to understand data flow for academic evaluation  
 **Less Boilerplate**: No need for multiple interface/implementation files per entity  
 **EF Core Best Practice**: Microsoft's own documentation recommends this for smaller applications  
 **Transparent**: Professors can see exactly what database operations occur  
 **Debugging**: Easier to debug without jumping through abstraction layers  

#### What I Avoided:
 **Repository Pattern**: Adds extra layer of abstraction
```csharp
// Would require creating this for each entity:
public interface IEventRepository { }
public class EventRepository : IEventRepository { }
// Then injecting IEventRepository instead of DbContext
```

 **Unit of Work Pattern**: Unnecessary when `DbContext` already implements this pattern internally

#### Real-World Context:
**Repository pattern is debated in the .NET community**:
- **Pro Repository**: Some enterprises use it for consistency across different data sources
- **Against Repository**: Many argue it's redundant with EF Core, which already abstracts database access
- **Microsoft's Stance**: Official docs show direct `DbContext` injection for most scenarios

**For this project**: Direct `DbContext` injection is cleaner and demonstrates understanding of EF Core fundamentals.

---

### 3. **Controllers Handle Business Logic (Instead of Service Layer)**

#### What I Chose:
Business logic lives in controller actions

```csharp
[HttpPost]
public async Task<IActionResult> Create(SchoolEvent ev)
{
    // Validation logic in controller
    if (ev.EndAt <= ev.StartAt)
        ModelState.AddModelError(nameof(ev.EndAt), "The end should be after the beginning.");
    
    if (!ModelState.IsValid)
    {
        await LoadOrganizersAsync(ev.OrganizerId);
        return View(ev);
    }
    
    _db.SchoolEvents.Add(ev);
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

#### Why This Choice:
 **Straightforward**: Entire workflow visible in one place  
 **Academic Clarity**: Professors can grade logic without hunting through multiple files  
 **Simple Domain**: Event management doesn't have complex business rules requiring separate service layer  
 **Validation Built-in**: Data Annotations + ModelState provide sufficient validation  

#### What I Avoided:
 **Service Layer Architecture**:
```csharp
// Would require:
public interface IEventService { }
public class EventService : IEventService { }
// Controllers would become thin wrappers
```

#### When Service Layer Makes Sense:
- Complex business rules (pricing calculations, approval workflows)
- Logic reused across multiple controllers
- Multiple data sources requiring coordination
- Need to support both Web UI and API endpoints

**For this project**: The business logic is straightforward enough to live in controllers without causing maintenance issues.

---

### 4. **SQL Server with LocalDB (Instead of SQLite)**

#### What I Chose:
**SQL Server with LocalDB** for development

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SchoolEventsDb;Trusted_Connection=True;"
  }
}
```

#### Why This Choice:
 **Industry Standard**: SQL Server is used in most enterprises  
 **LocalDB Convenience**: No separate installation, comes with Visual Studio  
 **Feature-Rich**: Supports advanced features (transactions, foreign keys, cascading deletes)  
 **Resume Value**: "SQL Server experience" carries more weight than "SQLite experience"  
 **Academic Alignment**: Most database courses teach SQL Server or PostgreSQL, not SQLite  
 **Scalability Path**: Easy migration from LocalDB → SQL Express → Full SQL Server  

#### What I Avoided:
 **SQLite**: 
- Good for mobile apps and embedded systems
- Limited concurrent write support
- Weaker datetime handling
- Less relevant for enterprise web applications

#### Real-World Context:
**Database Market Share** (for web applications):
1. SQL Server (~35%)
2. PostgreSQL (~30%)
3. MySQL (~25%)
4. SQLite (~5% - mostly mobile/embedded)

---

### 5. **ASP.NET Core Identity (Instead of Custom Auth)**

#### What I Chose:
Built-in **ASP.NET Core Identity** for authentication and authorization

#### Why This Choice:
 **Security Best Practice**: Never roll your own authentication  
 **Complete Solution**: Handles password hashing, account confirmation, lockout, two-factor auth  
 **Industry Standard**: What companies actually use in production  
 **Time Savings**: Would take weeks to implement securely from scratch  
 **Academic Proof**: Shows understanding of proper security practices  

#### What I Avoided:
 **Custom Authentication**: 
- High risk of security vulnerabilities
- Requires cryptography expertise
- Time-consuming to implement properly

 **Third-Party Auth Only** (OAuth only):
- Still need local user management
- Adds external dependencies
- Complicates demo/testing

#### Implementation Details:
```csharp
// Simple role-based authorization
[Authorize(Roles = "Teacher")]
public async Task<IActionResult> Create() { }

[Authorize(Roles = "Student")]
public async Task<IActionResult> Register(int eventId) { }
```

**This demonstrates understanding of**:
- Claims-based authentication
- Role-based authorization
- Secure password storage
- Session management

---

### 6. **ViewModels for Complex Views (Instead of ViewBag/ViewData)**

#### What I Chose:
**Strongly-typed ViewModels** for views requiring multiple data sources

```csharp
public class EventIndexVm
{
    public EventFilterVm Filter { get; set; } = new();
    public List<SchoolEvent> Events { get; set; } = new();
}

public class EventFilterVm
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public EventType? Type { get; set; }
    public string? Search { get; set; }
}
```

#### Why This Choice:
 **Type Safety**: Compile-time checking prevents runtime errors  
 **IntelliSense**: Better development experience  
 **Testability**: Can unit test view models independently  
 **Documentation**: ViewModel properties self-document view requirements  
 **Refactoring**: Easier to rename properties (IDE finds all usages)  

#### What I Avoided:
 **ViewBag/ViewData**:
```csharp
// Fragile - typos cause runtime errors
ViewBag.Organizers = organizers;
ViewData["EventTypes"] = types;
```

 **Mixing Domain Models Directly**:
- Exposes database entities to views
- Views might access navigation properties triggering lazy loading
- Harder to change database without breaking views

---

### 7. **Code-First Migrations (Instead of Database-First)**

#### What I Chose:
**Entity Framework Core Code-First** approach with migrations

```csharp
public class SchoolEvent
{
    public int Id { get; set; }
    
    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartAt { get; set; }
    // ...
}
```

```bash
Add-Migration InitSchoolEvents
Update-Database
```

#### Why This Choice:
 **Modern Approach**: Industry standard for new projects  
 **Version Control**: Database schema changes tracked in Git  
 **Team Collaboration**: Other developers can apply same migrations  
 **Data Annotations**: Validation rules visible in model classes  
 **Rollback Capability**: Can revert database changes  

#### What I Avoided:
 **Database-First**: Creating tables in SQL Server then generating models
- Loses track of schema changes
- Can't see database structure in code
- Harder for professor to evaluate without SQL Server

---

### 8. **Async/Await Throughout (Instead of Synchronous Operations)**

#### What I Chose:
All database operations use **async/await** pattern

```csharp
public async Task<IActionResult> Index()
{
    var events = await _db.SchoolEvents.ToListAsync();
    return View(events);
}
```

#### Why This Choice:
 **Scalability**: Frees up threads while waiting for database  
 **Best Practice**: Microsoft recommends async for all I/O operations  
 **Modern .NET**: Shows understanding of current C# features  
 **Real-World Ready**: How production applications are written  

#### What I Avoided:
 **Synchronous Database Calls**:
```csharp
// Bad - blocks thread
var events = _db.SchoolEvents.ToList();
```

#### Performance Impact:
- **Synchronous**: Can handle ~100 concurrent users
- **Asynchronous**: Can handle ~10,000 concurrent users
- Same server, just better thread utilization

---

### 9. **Bootstrap for UI (Instead of Custom CSS)**

#### What I Chose:
**Bootstrap 5** CSS framework with minimal custom styling

#### Why This Choice:
 **Time Efficiency**: Pre-built responsive components  
 **Professional Look**: Good-looking UI without design expertise  
 **Mobile Responsive**: Works on all screen sizes automatically  
 **Focus on Backend**: Spend time on C# logic, not CSS debugging  
 **Industry Use**: Bootstrap widely used in enterprise applications  

#### What I Avoided:
 **Custom CSS Framework**: Would take weeks to build responsive layouts  
 **Tailwind CSS**: More modern but steeper learning curve  
 **No Framework**: Plain CSS becomes unmaintainable quickly  

---

### 10. **Monolithic Architecture (Instead of Microservices)**

#### What I Chose:
Single **monolithic application** with all features in one codebase

```
Single Application:
- Authentication
- Event Management
- Registration System
- Calendar Views
- All in one ASP.NET Core project
```

#### Why This Choice:
 **Simplicity**: One codebase, one deployment, one database  
 **Easier Debugging**: Can step through entire flow in one debugger  
 **Faster Development**: No inter-service communication overhead  
 **Academic Appropriate**: Can demonstrate full application in one project  
 **Lower Complexity**: No distributed systems problems (network failures, consistency issues)  

#### What I Avoided:
 **Microservices Architecture**:
```
Event Service → Database 1
Registration Service → Database 2
Auth Service → Database 3
API Gateway
Message Queue
Service Discovery
```

#### When Microservices Make Sense:
- Large teams (50+ developers)
- Different scaling requirements per feature
- Need to use different technologies per service
- Complex deployment requirements

**For university project**: Microservices would add massive complexity for minimal benefit.

---

## Why This Architecture for University Coursework

### Academic Requirements Met:

1.  **Demonstrates MVC Pattern Understanding**
   - Clear separation: Models, Views, Controllers
   - Proper use of routing and action methods
   - View models for complex scenarios

2.  **Shows Database Design Skills**
   - Normalized database (3NF)
   - Proper foreign key relationships
   - Entity Framework proficiency

3.  **Implements Security Best Practices**
   - Built-in authentication system
   - Role-based authorization
   - CSRF protection
   - SQL injection prevention

4.  **Follows SOLID Principles** (Where Appropriate)
   - Single Responsibility (controllers focus on HTTP concerns)
   - Dependency Injection throughout
   - Open/Closed (can extend without modifying existing code)

5.  **Industry-Ready Code Quality**
   - Async/await for scalability
   - Proper error handling
   - Data validation
   - Clean, readable code

### Why NOT Enterprise Patterns?

| Pattern | Why Not Needed for University |
|---------|-------------------------------|
| **Repository Pattern** | EF Core already abstracts database; adds unnecessary complexity for academic evaluation |
| **Service Layer** | Business logic is simple; keeping it in controllers makes grading easier |
| **CQRS** | Massive overkill for CRUD operations; would obscure fundamental concepts |
| **Event Sourcing** | Unnecessary complexity; event management doesn't need audit trail of every change |
| **Microservices** | Single application easier to demo, test, and grade |
| **Domain-Driven Design** | Domain is too simple; DDD patterns would add unnecessary abstraction |

### Learning Outcomes Achieved:

This architecture successfully demonstrates:

1. **Backend Development**
   - C# and .NET Core proficiency
   - Entity Framework Core ORM usage
   - LINQ queries
   - Async programming

2. **Frontend Development**
   - Razor view syntax
   - HTML/CSS with Bootstrap
   - Client-side validation
   - Responsive design

3. **Database Management**
   - Relational database design
   - Migrations and schema management
   - Query optimization (AsNoTracking)
   - Transaction handling (via EF Core)

4. **Security**
   - Authentication implementation
   - Authorization with roles
   - Secure password storage
   - CSRF protection

5. **Software Engineering**
   - Project structure organization
   - Dependency injection
   - Version control (Git)
   - Code documentation

---

## Database Design

### Entity Relationship Diagram

```
┌─────────────────┐         ┌──────────────────┐
│   Organizer     │         │   SchoolEvent    │
├─────────────────┤         ├──────────────────┤
│ Id (PK)         │────────<│ Id (PK)          │
│ Name            │    1:N  │ Title            │
│ Email           │         │ Description      │
│ Phone           │         │ StartAt          │
│ Department      │         │ EndAt            │
└─────────────────┘         │ Type (enum)      │
                            │ Location         │
                            │ Capacity         │
                            │ OrganizerId (FK) │
                            └──────────────────┘
                                     │
                                     │ 1:N
                                     ▼
                            ┌───────────────────┐
                            │ EventRegistration │
                            ├───────────────────┤
                            │ Id (PK)           │
                            │ SchoolEventId (FK)│
                            │ ParticipantId (FK)│
                            │ RegisteredAt      │
                            └───────────────────┘
                                     │
                                     │ N:1
                                     ▼
                            ┌──────────────────┐
                            │   Participant    │
                            ├──────────────────┤
                            │ Id (PK)          │
                            │ FullName         │
                            │ Email            │
                            │ ClassName        │
                            └──────────────────┘
```

### Key Relationships:

1. **Organizer → SchoolEvent** (One-to-Many)
   - One organizer can create multiple events
   - Prevents orphaned events (cannot delete organizer with active events)

2. **SchoolEvent ↔ Participant** (Many-to-Many via EventRegistration)
   - Students can register for multiple events
   - Events can have multiple participants
   - Junction table tracks registration timestamp

3. **Identity System** (Built-in)
   - AspNetUsers stores user accounts
   - AspNetRoles stores roles (Teacher, Student)
   - AspNetUserRoles links users to roles

---

## Installation

### Prerequisites

- **.NET 8 SDK** installed on your machine ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server** (LocalDB, Express, or full version)
- **Visual Studio 2022** (or later) with ASP.NET and web development tools

### Setup Steps

#### 1. Clone the Repository
```bash
git clone <repository-url>
cd SchoolEvents
```

#### 2. Configure Database Connection
Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SchoolEventsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Connection String Options:**
- **LocalDB** (comes with Visual Studio): `Server=(localdb)\\mssqllocaldb;...`
- **SQL Express**: `Server=.\\SQLEXPRESS;...`
- **Full SQL Server**: `Server=localhost;...`

#### 3. Restore NuGet Packages
Packages restore automatically in Visual Studio, or run:
```bash
dotnet restore
```

#### 4. Apply Database Migrations

Open **Package Manager Console** (`Tools → NuGet Package Manager → Package Manager Console`):

```powershell
Add-Migration InitialCreate
Update-Database
```

Or using **.NET CLI**:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**What This Does:**
- Creates the database `SchoolEventsDb`
- Creates all tables (Identity tables + application tables)
- Sets up foreign keys and constraints

#### 5. Run the Application
Press **F5** in Visual Studio or run:
```bash
dotnet run
```

---

## Running the Application

### First Launch

The application automatically seeds:
1. **Roles**: "Teacher" and "Student"
2. **Default Teacher Account**:
   - Email: `teacher@school.com`
   - Password: `Teacher#123`

This is done in `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedRolesAsync(services);       
    await IdentitySeed.SeedRolesAndUsersAsync(services); 
}
```

### Login Workflow

1. **As Teacher**:
   - Go to `/Identity/Account/Login`
   - Email: `teacher@school.com`
   - Password: `Teacher#123`
   - Now you can create/edit/delete events and organizers

2. **As Student**:
   - Register new account at `/Identity/Account/Register`
   - **Manual Step**: Assign "Student" role
     - Option A: Via database (add row to `AspNetUserRoles`)
     - Option B: Extend `IdentitySeed.cs` to auto-assign Student role on registration
   - Login and browse/register for events

### Navigation Menu

- **Home** - Landing page
- **Events** - List all events with filtering
- **Organizers** - Manage organizers (Teachers only)
- **Calendar** - Monthly calendar view of events
- **Registrations** - View all registrations (Teachers only)

---

## Usage Guide

### Teacher Workflow

#### Creating an Organizer
1. Navigate to **Organizers** → **Create New**
2. Fill in:
   - Name (required)
   - Email (optional)
   - Phone (optional)
   - Department (optional)
3. Click **Create**

#### Creating an Event
1. Navigate to **Events** → **Create New**
2. Fill in event details:
   - **Title** (required, max 160 chars)
   - **Description** (optional, max 600 chars)
   - **Start Date & Time** (required)
   - **End Date & Time** (required, must be after start)
   - **Event Type** (Sports, Competition, Workshop, Lecture, Trip, Celebration)
   - **Location** (optional)
   - **Capacity** (1-500, default 30)
   - **Organizer** (select from dropdown)
3. Click **Create**

**Validation:**
- End time must be after start time
- All required fields validated
- Server-side and client-side validation

#### Managing Events
- **Edit**: Modify event details
- **Delete**: Remove event (blocked if registrations exist)
- **Details**: View event info and participant list

### Student Workflow

#### Browsing Events
1. Navigate to **Events**
2. Use filters:
   - **Date Range**: Filter by start date
   - **Event Type**: Filter by category
   - **Search**: Search in title or location
3. Click event for details

#### Registering for an Event
1. Click **Details** on an event
2. Click **Register** button
3. Fill in participant information:
   - Full Name (required)
   - Email (optional, prevents duplicate registrations)
   - Class Name (optional)
4. Click **Register**

**Business Rules:**
- Cannot register if event at full capacity
- Duplicate registration prevented (by email)
- Participant record created or reused
- Success/error messages displayed

#### Calendar View
1. Navigate to **Calendar**
2. View events by month
3. Navigate between months
4. Click event to view details

---

## Project Structure

```
SchoolEvents/
│
├── Controllers/                    # HTTP request handling
│   ├── EventsController.cs        # Event CRUD operations
│   ├── OrganizersController.cs    # Organizer management (Teacher only)
│   ├── RegistrationsController.cs # Student registration logic
│   ├── CalendarController.cs      # Calendar view
│   └── HomeController.cs          # Home and error pages
│
├── Models/                         # Domain entities
│   ├── SchoolEvent.cs             # Event entity
│   ├── Organizer.cs               # Organizer entity
│   ├── Participant.cs             # Participant entity
│   ├── EventRegistration.cs       # Junction table for many-to-many
│   ├── EventType.cs               # Enum for event categories
│   └── ErrorViewModel.cs          # Error page model
│
├── ViewModels/                     # Data transfer objects for views
│   ├── EventFilterVm.cs           # Filtering parameters
│   ├── EventIndexVm.cs            # Event list page data
│   └── CalendarVm.cs              # Calendar page data
│
├── Views/                          # Razor view templates
│   ├── Events/                    # Event views
│   ├── Organizers/                # Organizer views
│   ├── Registrations/             # Registration views
│   ├── Calendar/                  # Calendar view
│   ├── Shared/                    # Shared layouts
│   └── Home/                      # Home page
│
├── Data/                           # Data access layer
│   ├── ApplicationDbContext.cs    # EF Core DbContext
│   ├── Migrations/                # Database migrations
│   └── Seed/
│       └── IdentitySeed.cs        # Role and user seeding
│
├── wwwroot/                        # Static files
│   ├── css/                       # Stylesheets
│   ├── js/                        # JavaScript
│   └── lib/                       # Client libraries (Bootstrap, jQuery)
│
├── appsettings.json                # Configuration
├── appsettings.Development.json    # Development settings
├── Program.cs                      # Application startup
└── README.md                       # This file
```

### Key File Explanations:

**Program.cs** - Application entry point
- Configures services (DbContext, Identity, MVC)
- Sets up middleware pipeline (routing, authentication, authorization)
- Seeds roles and default user
- Defines startup behavior

**ApplicationDbContext.cs** - Database context
- Inherits from `IdentityDbContext` for user management
- Defines `DbSet` properties for each entity
- EF Core uses this to generate SQL

**Controllers** - Handle HTTP requests
- Receive user input
- Validate data
- Interact with database via DbContext
- Return views or redirects

**Models** - Business entities
- Represent database tables
- Define properties and validation rules
- Navigation properties establish relationships

**Views** - Razor templates
- Generate HTML using C# syntax
- Strongly-typed to models or view models
- Use Bootstrap for styling

---

## Security Features

### Implemented Security Measures:

1. **Password Security**
   - Passwords hashed using PBKDF2
   - Salt generated per password
   - Never stored in plain text

2. **Authorization**
   - Role-based access control
   - `[Authorize]` attributes protect endpoints
   - Unauthorized users redirected to login

3. **CSRF Protection**
   - `[ValidateAntiForgeryToken]` on all POST actions
   - Prevents cross-site request forgery

4. **SQL Injection Prevention**
   - EF Core uses parameterized queries
   - No raw SQL string concatenation

5. **XSS Protection**
   - Razor automatically HTML-encodes output
   - Prevents script injection

6. **Data Validation**
   - Server-side validation (Data Annotations)
   - Client-side validation (jQuery Validation)
   - ModelState checking before database operations

---



