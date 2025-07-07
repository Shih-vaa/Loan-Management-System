## Loan Management System

A web-based role-driven system to manage loan lead generation, team assignments, document verification, and commission tracking. Built using **ASP.NET Core MVC**, **Entity Framework Core**, and **MySQL**.
## Tech Stack

- 🔹 ASP.NET Core MVC (.NET 8/9)
- 🔹 Entity Framework Core (with Pomelo MySQL provider)
- 🔹 MySQL (backend DB)
- 🔹 Bootstrap 5 (for responsive UI)
- 🔹 Authentication: Cookie-based (Claims-based)
- 🔹 File Storage: Local server directory (uploads/)
## Completed Modules

### 🔐 Authentication & Authorization
- Role-based login using ASP.NET Core Identity-style claims
- User roles: Admin, Marketing, Office, Calling, Lead Generator
- Authorization gates for dashboard access and CRUD features

### 👥 User Roles & Dashboards
- Dynamic navbar and routing based on role
- Individual dashboards for Admin, Marketing, Calling, Office

### 🧾 Lead Management
- Add lead with dropdown of existing customers
- Assign lead to internal user (Admin only)
- Track status: `new`, `assigned`, `in_process`, `approved`, `rejected`, `disbursed`
- View leads with full audit trail (Lead Generator & Assignee)

### 👤 Customer Management
- Add/Edit/Delete customer profiles
- Role-restricted to Admin and Office team

### 📁 Lead Document Upload
- Upload document files for a lead
- Files stored in `/uploads`
- View, Download, Delete files
- Uploaded by assigned user or office team

### 👨‍👩‍👧‍👦 Team Management (In Progress)
- Create teams (Admin)
- Add/remove members
- Enforce same-role member restrictions
## Key Tables

- `users` – Stores user credentials and roles
- `customers` – Contains customer contact and address
- `leads` – Each record ties to a customer, status, generator, assignee
- `lead_documents` – Files uploaded per lead
- `teams`, `team_members` – Teams and assigned users

### Entity Relationships
- Lead ↔ Customer (many-to-one)
- Lead ↔ LeadGenerator (many-to-one)
- Lead ↔ AssignedUser (many-to-one)
- Lead ↔ Documents (one-to-many)
## UI Features

- Responsive layout using Bootstrap
- Razor Views with dynamic role-based controls
- NavBar items appear based on role
- Leads table with View/Edit buttons
- Upload panel in lead details
## Sample Users

| Email              | Password | Role        |
|-------------------|----------|-------------|
| admin@example.com | password | admin       |
| mark1@example.com | password | marketing   |
| office1@example.com | password | office    |


## Known Issues / Pending Work

- Commission calculation & reports module
- Team member UI CRUD screens
- Referral tracking for external lead generators
- File size/type validation for uploads
- Export to PDF/CSV

## Deployment

- Run using `dotnet run` or deploy to IIS/Azure
- Database: MySQL (import `loan_management_system.sql`)
- File storage: Local `/uploads` directory (ensure write permission)
