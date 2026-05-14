# 🗳️ VotingSystem Railway

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Blazor](https://img.shields.io/badge/Framework-Blazor%20Server-purple.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791.svg)](https://www.postgresql.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A robust, enterprise-grade Digital Voting System built with **.NET 10** and **Blazor Interactive Server**, optimized for cloud hosting on **Railway**. This system provides a secure, transparent, and user-friendly platform for managing school elections, featuring real-time analytics, automated voter code generation, and comprehensive student management.

## ✨ Key Features

### 🏛️ Election Management
- **Automated Scheduling**: Elections automatically transition between *Upcoming*, *Active*, and *Ended* based on configured start and end times.
- **Dynamic Positions**: Define multiple positions per election with custom constraints (e.g., maximum selectable options).
- **Candidate Profiles**: Rich profiles for candidates including photos, classes, and manifesto summaries.

### 👥 Student & Voter Management
- **Bulk Import/Export**: Effortlessly manage large student databases using **Excel (.xlsx)** or **CSV** files.
- **Secure Voter Codes**: Unique, cryptographically secure codes are automatically generated for each registered student.
- **Voter Tracking**: Real-time monitoring of voter turnout and status.

### 📊 Real-Time Analytics
- **Live Statistics**: Monitor election progress with dynamic bar charts and turnout percentages.
- **Interactive Results**: Detailed breakdown of results per position and candidate once an election ends.
- **Exportable Reports**: Generate detailed reports in Excel and CSV formats.

### 🛡️ Security & Reliability
- **Advanced Authentication**: Secure administrator and voter portals with ASP.NET Core Identity.
- **Security Headers**: Comprehensive CSP, HSTS, and X-Frame-Options protection.
- **Audit Logging**: Every sensitive action is logged for transparency and accountability.
- **Rate Limiting**: Protection against brute-force and DDoS attacks.
- **Health Monitoring**: Integrated health checks for database and system services.

## 🛠️ Technology Stack

- **Frontend**: Blazor Interactive Server (Modern Glassmorphism UI)
- **Backend**: .NET 10 Web API / Services
- **Database**: PostgreSQL with Entity Framework Core
- **Hosting**: Optimized for Railway
- **Reporting**: EPPlus (Excel), CsvHelper (CSV), iText7 (PDF)
- **Logging**: Serilog with Console, File, and Seq sinks
- **Styling**: Bootstrap 5 + Custom Modern CSS

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/zakitworld/VotingSystem-Railway.git
   ```
2. Navigate to the project directory:
   ```bash
   cd VotingSystem-Railway/VotingSystem_Claude
   ```
3. Update `appsettings.json` with your PostgreSQL connection string.
4. Run the application:
   ```bash
   dotnet run
   ```

## ☁️ Railway Deployment

This project is pre-configured for Railway deployment.

1. **Create a New Project** on Railway.
2. **Add a PostgreSQL Database** to your project.
3. **Connect your GitHub Repo** (this repository).
4. **Environment Variables**: Railway will automatically provide connection details. Ensure the app uses the `DATABASE_URL` or `ConnectionStrings__DefaultConnection` provided by Railway.
5. The application will automatically apply migrations and create the necessary tables on the first startup.

### Default Credentials (Development Mode)
- **Username**: `admin`
- **Password**: `Admin@123456` (or as configured in `SeedData:AdminPassword`)
*Note: Credentials are reset automatically in Development mode for ease of access.*

## 📁 Project Structure

```text
VotingSystem_Claude/
├── Components/         # Blazor Components & Pages
├── Data/               # DB Context & PostgreSQL Migrations
├── Models/             # Entity Models
├── Services/           # Business Logic & Interfaces
├── Middleware/         # Custom Security & Audit Logic
└── wwwroot/            # Static Assets (JS, CSS, Images)
```

## 📄 License

This project is licensed under the MIT License.