# MegaProject

Test assignment for ASP.NET (C#) Developer position.

## 📌 Overview

Web application for managing projects and employees with full CRUD functionality.  
Implements a 5-step wizard for project creation, file uploads, filtering, and sorting.

## 🚀 Features

### Backend
- CRUD for Projects and Employees
- Many-to-many relationship (Projects ↔ Employees)
- Assign project manager
- Filtering (date range, priority, search)
- Sorting by columns
- File upload with unique filenames
- Automatic database migrations

### Frontend
- Multi-step wizard:
    1. Basic info (name, dates, priority)
    2. Companies
    3. Project manager selection
    4. Team members selection
    5. File upload (drag & drop)
- AJAX employee search
- Dynamic UI with JavaScript
- Upload progress bar

## 🏗 Architecture

- 3-layer architecture:
    - Data Access Layer (EF Core)
    - Business Logic Layer
    - Presentation Layer (MVC + Razor)
- Database: SQLite

## 🛠 Technologies

- .NET 10+
- ASP.NET Core MVC
- Entity Framework Core (Code First)
- JavaScript (AJAX, drag & drop)
- Bootstrap

## ▶️ Getting Started

### Download Releases
 - Install .Net 10
 - Launch MegaProject.exe from the [downloaded release](https://github.com/ManFizz/Project-manager/releases/latest)
