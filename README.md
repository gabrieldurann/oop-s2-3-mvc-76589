# VGC College — Multi-Branch Student & Course Management System

ASP.NET Core MVC application for Acme Global College (VGC), managing student registration, attendance tracking, and academic progress across three branches in Ireland.

## How to Run
```bash
cd src/VgcCollege.Web
dotnet run
```

Navigate to the URL shown in the terminal (typically `https://localhost:5001`).

The database is created and seeded automatically on first run using SQLite.

## How to Run Tests
```bash
dotnet test
```

10 xUnit tests covering enrolment rules, visibility rules, grade validation, attendance calculations, and authorization filtering.

## Seeded Demo Accounts

| Role    | Email              | Password     |
|---------|--------------------|--------------|
| Admin   | admin@vgc.ie       | Admin123!    |
| Faculty | faculty@vgc.ie     | Faculty123!  |
| Student | student1@vgc.ie    | Student123!  |
| Student | student2@vgc.ie    | Student123!  |

## Seeded Data

- 3 branches (Dublin, Cork, Galway)
- 4 courses across branches
- Faculty assigned to Computer Science and Business Studies
- 2 students enrolled with attendance records, assignment results, and exam results
- 1 released exam (Midterm) and 1 provisional exam (Final) to demonstrate visibility rules

## Design Decisions

- **SQLite** for simplicity — no SQL Server setup needed, works on all platforms and in CI
- **ASP.NET Core Identity** for authentication with role-based authorization (Admin, Faculty, Student)
- **Server-side RBAC enforcement** on all controllers — not just UI hiding but query-level filtering:
  - Admin: full access to all data
  - Faculty: can only view/manage students and grades for their assigned courses
  - Student: can only view their own data; exam results hidden until released
- **Cascade deletes** configured so deleting a faculty member only removes their course assignments, not the courses or grades
- **EF Core InMemory provider** for deterministic unit tests
- **Friendly error pages** for 404, access denied, and server errors — no raw exceptions exposed

## Project Structure
```
src/VgcCollege.Web/          — MVC application
tests/VgcCollege.Tests/      — xUnit test project
.github/workflows/ci.yml     — CI workflow (build + test + coverage)
```
