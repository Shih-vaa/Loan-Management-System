# Loan Management System

A **production-grade**, role-driven web application to manage loan lead generation, team assignments, document verification, commission tracking, and notifications.  
Built using **ASP.NET Core MVC**, **Entity Framework Core**, and **MySQL**.

---

## 🚀 Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 8/9)
- **ORM:** Entity Framework Core (Pomelo MySQL Provider)
- **Database:** MySQL
- **Frontend:** Bootstrap 5 (responsive UI)
- **Authentication:** Cookie-based (Claims-based)
- **File Storage:** Local server directory (`/uploads`)
- **Charts/Reports:** Chart.js / Recharts
- **Real-time Features:** AJAX-based polling for notifications

---

## ✅ Completed Modules

### 🔐 Authentication & Authorization
- Claims-based role login
- User roles: `Admin`, `Marketing`, `Office`, `Calling`, `Lead Generator`
- Authorization gates for dashboards and CRUD operations

### 👥 User Roles & Dashboards
- Role-based dashboards with widgets
- Dynamic navbar & routing
- Separate insights for Admin, Marketing, Office, Calling

### 🧾 Lead Management
- Add new leads linked to customers
- Admin assigns leads to internal users
- Status workflow: `new → assigned → in_process → approved/rejected → disbursed`
- Audit trail for all lead actions
- Soft delete (GDPR-compliant anonymization)

### 👤 Customer Management
- Full CRUD (Add, Edit, Soft Delete)
- Passport photo support
- Restricted access (Admin + Office only)

### 📁 Lead Document Upload & Verification
- Upload files for each lead
- View, download, and delete options
- Document verification workflow:
  - Status (`pending`, `approved`, `rejected`)
  - Verifier assignment & timestamp
- Team-wise access restrictions

### 📢 Notifications Module
- Real-time polling for new notifications
- Dropdown on navbar
- Bulk actions: "Mark all as read"
- Role-based filters

### 👨‍👩‍👧‍👦 Team Management
- Admin creates teams
- Add/remove members with role restrictions
- Leads visible only within assigned team

### 💰 Commission Management
- Auto-generate commission on lead approval
- Status workflow: `pending → approved → paid`
- Role-based visibility (calling users see own commissions)
- Totals per status
- Notifications on update

### 📊 Dashboard Metrics & Analytics
- Widgets: Total Leads, Pending Verifications, Commission Summary, Documents Uploaded
- Recent Notifications panel
- Lead Status Pie Chart
- Team Performance Metrics:
  - Leads generated, assigned, verified, approved, commissions earned
- Drilldowns for each widget
- Export/Print options (CSV/PDF)

### 📧 Email Module
- Event-based email triggers:
  - Lead assignment
  - Password reset (OTP-based with resend support)
  - Commission updates
- Reusable HTML email templates
- Configurable SMTP (e.g., Gmail)

### 📜 Audit Logging
- Tracks login/logout, access denied
- Lead status changes & assignments
- Commission status changes
- Soft deletion events
- Logs IP, User-Agent, Role, Timestamp (IST)

---

## 🗄️ Key Tables

- `users` – User credentials, roles
- `customers` – Customer details & passport photo
- `leads` – Loan leads with generator, assignee, status
- `lead_documents` – Uploaded files with verification fields
- `teams`, `team_members` – Teams and members
- `notifications` – Role/user-based notifications
- `commissions` – Commission records
- `audit_logs` – System audit trail

---

## 🔗 Entity Relationships

- **Lead ↔ Customer:** many-to-one  
- **Lead ↔ LeadGenerator:** many-to-one  
- **Lead ↔ AssignedUser:** many-to-one  
- **Lead ↔ Documents:** one-to-many  
- **Team ↔ TeamMembers:** one-to-many  

---

## 🎨 UI Features

- Responsive layout (Bootstrap 5)
- Role-based dynamic navigation
- Lead table with filters & drilldowns
- Document upload panel
- Notification dropdown in navbar
- Charts & analytics on dashboard
- Export (CSV/PDF) options

---

## 👤 Sample Users

| Email              | Password | Role        |
|--------------------|----------|-------------|
| admin@example.com  | password | admin       |
| mark1@example.com  | password | marketing   |
| office1@example.com| password | office      |
| call1@example.com  | password | calling     |

---

## 🐞 Known Issues / Pending Work

- Referral tracking for external lead generators
- File size/type validation for uploads
- Advanced commission reports with filters
- Deployment CI/CD pipeline automation

---

## 🚀 Deployment

1. Clone repository & restore packages  
   ```bash
   dotnet restore
