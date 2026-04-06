# VGC College - Multi-Branch Student & Course Management System

ASP.NET Core MVC application for Acme Global College (VGC).

## How to Run
```bash
cd src/VgcCollege.Web
dotnet run
```

Navigate to `https://localhost:5001` (or the port shown in terminal).

## How to Run Tests
```bash
dotnet test
```

## Seeded Demo Accounts

| Role    | Email              | Password     |
|---------|--------------------|--------------|
| Admin   | admin@vgc.ie       | Admin123!    |
| Faculty | faculty@vgc.ie     | Faculty123!  |
| Student | student1@vgc.ie    | Student123!  |
| Student | student2@vgc.ie    | Student123!  |

## Design Decisions

- SQLite for simplicity (no SQL Server setup needed)
- ASP.NET Core Identity for authentication + role-based authorization
- Server-side RBAC enforcement on all controllers (not just UI hiding)
- Seed data: 3 branches, multiple courses, faculty assignments, enrolments, and sample results
