# 🗳️ VotingSystem Claude

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Blazor](https://img.shields.io/badge/Framework-Blazor%20Server-purple.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A robust, enterprise-grade Digital Voting System built with **.NET 10** and **Blazor Interactive Server**. This system provides a secure, transparent, and user-friendly platform for managing school elections, featuring real-time analytics, automated voter code generation, and comprehensive student management.

![Dashboard Mockup](file:///C:/Users/ZAK%20I.T.%20WORLD/.gemini/antigravity/brain/c61bfdf4-dc6a-4621-89e0-6353c0d7e418/voting_system_dashboard_mockup_1778258073002.png)

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
- **Database**: Entity Framework Core (SQL Server / SQLite)
- **Reporting**: EPPlus (Excel), CsvHelper (CSV), iText7 (PDF)
- **Logging**: Serilog with Console, File, and Seq sinks
- **Styling**: Bootstrap 5 + Custom Modern CSS

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (or use SQLite for development)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/VotingSystem_Claude.git
   ```
2. Navigate to the project directory:
   ```bash
   cd VotingSystem_Claude/VotingSystem_Claude
   ```
3. Update `appsettings.json` with your connection string.
4. Run the application:
   ```bash
   dotnet run
   ```

### Default Credentials (Development Mode)
- **Username**: `admin`
- **Password**: ``
*Note: Credentials are reset automatically in Development mode for ease of access.*

## 📁 Project Structure

```text
VotingSystem_Claude/
├── Components/         # Blazor Components & Pages
├── Data/               # DB Context & Migrations
├── Models/             # Entity Models
├── Services/           # Business Logic & Interfaces
├── Middleware/         # Custom Security & Audit Logic
└── wwwroot/            # Static Assets (JS, CSS, Images)
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](file:///g:/From%20Desktop/Console_WinUI3_Blazor_MAUI/VotingSystem_Claude/LICENSE.txt) file for details.