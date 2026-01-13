# IntelliPM - Intelligent Project Management System

<div align="center">

![IntelliPM](https://img.shields.io/badge/IntelliPM-v2.9-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-18-blue.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.8-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

**An intelligent project management system combining traditional PM features with AI-powered agents for automated insights, risk detection, and sprint planning.**

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [API Reference](#-api-reference) • [Contributing](#-contributing)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Prerequisites](#-prerequisites)
- [Quick Start](#-quick-start)
- [Development Setup](#-development-setup)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Configuration](#-configuration)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

IntelliPM is a comprehensive project management platform designed for modern software development teams. It combines traditional project management capabilities with AI-powered insights, automated risk detection, and intelligent sprint planning.

### Key Highlights

- **Multi-Tenant Architecture**: Organization-based isolation for enterprise use
- **AI-Powered Agents**: Semantic Kernel-powered agents for project analysis and automation
- **Clean Architecture**: Maintainable, testable, and scalable codebase
- **CQRS Pattern**: Optimized read/write operations with read models
- **Real-time Updates**: Efficient data fetching and caching with TanStack Query
- **Type-Safe**: Full TypeScript coverage with strict mode enabled

---

## ✨ Features

### Core Project Management
- ✅ **Projects & Teams**: Multi-project support with team collaboration
- ✅ **Tasks & Backlog**: Task management with epic/feature/user story hierarchy
- ✅ **Sprints**: Sprint planning, tracking, and velocity metrics
- ✅ **Milestones**: Milestone tracking with statistics and timeline views
- ✅ **Releases**: Release management with quality gates and release notes
- ✅ **Task Dependencies**: Visual dependency graphs and dependency management
- ✅ **Defects**: Defect tracking and management

### AI & Automation
- 🤖 **AI Agents**: Product, Delivery, Manager, QA, and Business agents
- 🧠 **Risk Detection**: Automated project risk identification
- 📊 **Sprint Planning**: AI-assisted sprint planning recommendations
- 📈 **Project Insights**: Automated project health analysis
- 🎯 **Task Quality**: AI-powered task improvement suggestions

### Collaboration & Communication
- 💬 **Comments**: Threaded comments with @username mentions
- 📎 **File Attachments**: Drag-and-drop file uploads with validation
- 🔔 **Notifications**: Real-time notifications with user preferences
- 👥 **Team Management**: Role-based access control (RBAC)
- 📧 **Email Integration**: SMTP-based email notifications

### Analytics & Reporting
- 📊 **Metrics Dashboard**: Velocity, burndown, and distribution charts
- 📈 **Activity Feed**: Real-time activity tracking
- 🎯 **Insights**: Project insights and recommendations
- 📋 **Read Models**: Optimized CQRS read models for performance

### Administration
- ⚙️ **Global Settings**: Configurable system settings (General, Security, Email)
- 🚩 **Feature Flags**: Dynamic feature toggling
- 📝 **Audit Logs**: Comprehensive audit trail
- 🔐 **Permission Management**: Fine-grained permission control
- 🏥 **System Health**: Health monitoring and diagnostics
- 🤖 **AI Governance**: AI quota management and decision logging

### Internationalization
- 🌍 **Multi-Language Support**: English (en) and Français (fr)
- 🔄 **Dynamic Language Switching**: Change language at runtime
- 📱 **Language Toggle**: Easy language selection in UI
- 🔗 **Backend Sync**: Language preference synced with user settings
- 📅 **Locale-Aware Formatting**: Date and number formatting per locale

---

## 🛠 Technology Stack

### Backend
- **Framework**: .NET 8.0, ASP.NET Core Web API
- **ORM**: Entity Framework Core 8.0
- **Databases**: 
  - SQL Server (Transactional data)
  - PostgreSQL with pgvector (AI memory/embeddings)
- **Authentication**: JWT Bearer Tokens (HTTP-only cookies)
- **AI Framework**: Microsoft Semantic Kernel
- **LLM**: Ollama (Local LLM - llama3.2:3b)
- **Patterns**: CQRS, Repository, Unit of Work, MediatR
- **Logging**: Serilog (Console, File, Seq)
- **Monitoring**: Sentry (Error tracking & performance)
- **Validation**: FluentValidation

### Frontend
- **Framework**: React 18 with TypeScript (Strict Mode)
- **Build Tool**: Vite 5.4
- **UI Library**: shadcn/ui (Radix UI components)
- **Styling**: Tailwind CSS
- **State Management**: TanStack Query (React Query)
- **Routing**: React Router v6
- **Forms**: React Hook Form + Zod validation
- **Charts**: Recharts
- **Icons**: Lucide React
- **Notifications**: SweetAlert2

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Version Control**: Git
- **CI/CD**: Ready for GitHub Actions

---

## 🏗 Architecture

IntelliPM follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         API Layer (Controllers)         │
└─────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│      Application Layer (CQRS)           │
│   (Commands, Queries, Handlers, DTOs)   │
└─────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│        Domain Layer (Entities)         │
│   (Business Logic, Events, Value Objs) │
└─────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│    Infrastructure Layer (Data Access)   │
│  (Repositories, Services, External)    │
└─────────────────────────────────────────┘
```

### Design Patterns
- **CQRS**: Command Query Responsibility Segregation
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Mediator Pattern**: Decoupled request handling (MediatR)
- **Outbox Pattern**: Reliable event publishing
- **Feature Toggle Pattern**: Dynamic feature flags

---

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Node.js 18+** and **npm** ([Download](https://nodejs.org/))
- **SQL Server** (LocalDB or full instance)
- **PostgreSQL 14+** with pgvector extension
- **Ollama** ([Download](https://ollama.ai/))
- **Docker Desktop** (Optional, for containerized deployment)
- **Git**

---

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/intelliPM-V2.git
   cd intelliPM-V2
   ```

2. **Create environment file**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start all services**
   ```bash
   docker-compose up -d
   ```

4. **Initialize Ollama model**
   ```bash
   docker exec intellipm-v2-ollama ollama pull llama3.2:3b
   ```

5. **Access the application**
   - Frontend: http://localhost:3001
   - Backend API: http://localhost:5001
   - Swagger UI: http://localhost:5001/swagger

### Option 2: Local Development

#### Backend Setup

1. **Navigate to backend directory**
   ```bash
   cd backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure databases**
   - Create SQL Server database: `IntelliPM`
   - Create PostgreSQL database: `intellipm_vector`
   - Install pgvector extension in PostgreSQL:
     ```sql
     CREATE EXTENSION IF NOT EXISTS vector;
     ```

4. **Configure appsettings**
   - Update `IntelliPM.API/appsettings.Development.json` with your connection strings
   - Set up User Secrets for JWT secret key:
     ```bash
     dotnet user-secrets init --project IntelliPM.API
     dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-min-32-chars" --project IntelliPM.API
     ```

5. **Run migrations** (automatically applied on startup)
   ```bash
   dotnet ef database update --project IntelliPM.Infrastructure --startup-project IntelliPM.API --context AppDbContext
   ```

6. **Start Ollama**
   ```bash
   ollama serve
   ollama pull llama3.2:3b
   ```

7. **Run the backend**
   ```bash
   dotnet run --project IntelliPM.API
   ```

#### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure environment**
   - Create `.env.local`:
     ```env
     VITE_API_BASE_URL=http://localhost:5001
     ```

4. **Run the frontend**
   ```bash
   npm run dev
   ```

5. **Access the application**
   - Frontend: http://localhost:5173 (or port shown in terminal)
   - Backend API: http://localhost:5001

---

## 🌍 Supported Languages

IntelliPM supports multiple languages with dynamic language switching:

### Available Languages

- **English (en)** - Default language
- **Français (fr)** - French

### Changing Language

1. **Using Language Toggle**: Click the language icon (🌍) in the header/navigation bar
2. **Select Language**: Choose from the dropdown menu
3. **Automatic Sync**: Language preference is saved and synced with your user account

### Language Features

- ✅ **Dynamic Switching**: Change language without page reload
- ✅ **Backend Sync**: Language preference saved to user settings
- ✅ **Browser Detection**: Automatically detects browser language
- ✅ **Locale Formatting**: Dates and numbers formatted according to locale
- ✅ **18 Namespaces**: Translations organized by feature (common, projects, tasks, etc.)

### Translation Files

Translation files are located in:
```
frontend/public/locales/{language}/{namespace}.json
```

For more information, see:
- [i18n Documentation](docs/i18n.md)
- [Translation Guide](docs/TRANSLATION_GUIDE.md)

---

## 📁 Project Structure

```
intelliPM-V2/
├── backend/                          # Backend solution
│   ├── IntelliPM.Domain/            # Domain layer (39 entities, 23 events)
│   ├── IntelliPM.Application/       # Application layer (CQRS)
│   ├── IntelliPM.Infrastructure/    # Infrastructure layer
│   ├── IntelliPM.API/               # API layer (36 controllers)
│   └── IntelliPM.Tests/             # Test projects
│
├── frontend/                         # Frontend React application
│   ├── src/
│   │   ├── api/                     # API client modules (30 files)
│   │   ├── components/              # React components
│   │   │   ├── ui/                  # shadcn/ui components (51 components)
│   │   │   ├── projects/            # Project components
│   │   │   ├── tasks/               # Task components
│   │   │   ├── milestones/          # Milestone components (8 components)
│   │   │   ├── releases/            # Release components (18 components)
│   │   │   └── ...
│   │   ├── pages/                   # Page components
│   │   ├── contexts/                # React contexts
│   │   ├── hooks/                   # Custom hooks
│   │   └── lib/                     # Utility functions
│   └── package.json
│
├── docs/                             # Documentation
│   ├── IntelliPM Documentation/     # Comprehensive documentation
│   └── adr/                         # Architecture Decision Records
│
├── docker-compose.yml               # Docker Compose configuration
├── .gitignore                       # Git ignore rules
└── README.md                        # This file
```

---

## 📚 API Documentation

### Swagger UI

When running the backend, Swagger UI is available at:
- **Development**: http://localhost:5001/swagger
- **Production**: https://your-domain.com/swagger

### API Endpoints

The API follows RESTful conventions and is versioned:

- **Base URL**: `/api/v1/`
- **Admin Routes**: `/api/admin/`

#### Main Endpoints

| Resource | Endpoints | Description |
|----------|-----------|-------------|
| **Projects** | `GET/POST/PUT/DELETE /api/v1/Projects` | Project management |
| **Tasks** | `GET/POST/PUT/PATCH /api/v1/Tasks` | Task management |
| **Sprints** | `GET/POST/PUT/PATCH /api/v1/Sprints` | Sprint management |
| **Milestones** | `GET/POST/PUT/DELETE /api/v1/Milestones` | Milestone tracking |
| **Releases** | `GET/POST/PUT/DELETE /api/v1/Releases` | Release management |
| **Teams** | `GET/POST/PATCH /api/v1/Teams` | Team management |
| **Defects** | `GET/POST/PUT/DELETE /api/v1/Defects` | Defect tracking |
| **Comments** | `GET/POST/PUT/DELETE /api/v1/Comments` | Comment system |
| **Attachments** | `GET/POST/DELETE /api/v1/Attachments` | File attachments |
| **Notifications** | `GET/PATCH /api/v1/Notifications` | Notifications |
| **Auth** | `POST /api/v1/Auth/login` | Authentication |
| **Metrics** | `GET /api/v1/Metrics` | Project metrics |
| **AI Agents** | `POST /api/v1/projects/{id}/agents` | AI agent execution |

See [API Reference Documentation](docs/IntelliPM%20Documentation/IntelliPM_Backend.md#14-api-reference) for complete endpoint details.

---

## 🧪 Testing

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test              # Run tests in watch mode
npm run test:run      # Run tests once
npm run test:coverage # Generate coverage report
npm run test:ui       # Open test UI
```

### Test Coverage

- **Backend**: Unit tests, integration tests, API tests
- **Frontend**: Component tests with Vitest and React Testing Library

---

## 🚢 Deployment

### Docker Deployment

1. **Build images**
   ```bash
   docker-compose build
   ```

2. **Start services**
   ```bash
   docker-compose up -d
   ```

3. **Check logs**
   ```bash
   docker-compose logs -f
   ```

### Production Checklist

- [ ] Update connection strings
- [ ] Set JWT secret key (environment variable)
- [ ] Configure email settings
- [ ] Set Sentry DSN
- [ ] Configure CORS origins
- [ ] Enable HTTPS
- [ ] Set up logging aggregation
- [ ] Configure health check endpoints
- [ ] Set up database backups
- [ ] Review security headers
- [ ] **Build in Release mode**: `dotnet build -c Release` (excludes debug controllers)
- [ ] **Verify debug controllers excluded**: Runtime check in `Program.cs` confirms no test controllers in production

---

## ⚙️ Configuration

### Environment Variables

#### Backend

```env
# Database
ConnectionStrings__SqlServer=Server=...;Database=IntelliPM;...
ConnectionStrings__VectorDb=Host=...;Database=intellipm_vector;...

# JWT
Jwt__SecretKey=your-secret-key-min-32-characters
Jwt__Issuer=IntelliPM
Jwt__Audience=IntelliPM.API

# Ollama
Ollama__Endpoint=http://localhost:11434
Ollama__Model=llama3.2:3b

# Sentry
Sentry__Dsn=your-sentry-dsn
SENTRY_DSN=your-sentry-dsn

# Seq (Optional)
SEQ_URL=http://localhost:5341
SEQ_API_KEY=your-seq-api-key
```

#### Frontend

```env
VITE_API_BASE_URL=http://localhost:5001
```

### User Secrets (Development)

For sensitive configuration in development:

```bash
dotnet user-secrets init --project backend/IntelliPM.API
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key" --project backend/IntelliPM.API
```

---

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Make your changes**
4. **Write/update tests**
5. **Update documentation**
6. **Commit your changes** (`git commit -m 'Add amazing feature'`)
7. **Push to the branch** (`git push origin feature/amazing-feature`)
8. **Open a Pull Request**

### Code Standards

- Follow existing code style
- Add XML documentation comments
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass

---

## 📖 Documentation

Comprehensive documentation is available in the `docs/` directory:

### Core Documentation
- **[Backend Documentation](docs/IntelliPM%20Documentation/IntelliPM_Backend.md)**: Complete backend API and architecture guide
- **[Frontend Documentation](docs/IntelliPM%20Documentation/IntelliPM_Frontend.md)**: Frontend development guide
- **[Architecture Decision Records](docs/adr/)**: ADRs for key architectural decisions

### User Guides
- **[Roles and Permissions Guide](docs/RolesAndPermissions.md)**: Complete guide to all 8 roles, permissions, and access control
- **[Workflow Guide](docs/WorkflowGuide.md)**: Task, sprint, and release workflow documentation with role requirements
- **[AI Governance Guide](docs/AIGovernanceGuide.md)**: How AI decisions work, approval processes, and quota management

### Developer Resources
- **[Developer Onboarding Guide](docs/DeveloperOnboarding.md)**: Setup guide, architecture overview, and how to add permissions
- **[Permission Matrix](Docs/PermissionMatrix.md)**: Quick reference for role permissions
- **[Architecture Diagram](docs/ArchitectureDiagram.md)**: Permission flow and system architecture visualization

### Quick Reference

#### Roles Overview
- **Global Roles**: User, Admin, SuperAdmin (organization-wide)
- **Project Roles**: ProductOwner, ScrumMaster, Developer, Tester, Viewer, Manager (project-specific)

#### Common Permission Examples
- **As a Developer**: Can create tasks but cannot start sprints
- **As a ScrumMaster**: Can start sprints but cannot approve releases
- **As a Tester**: Can approve releases but cannot deploy them
- **As a ProductOwner**: Can manage projects but cannot start sprints (ScrumMaster exclusive)

---

## 🐛 Troubleshooting

### Common Issues

#### Database Connection Errors
- Verify connection strings in `appsettings.json`
- Check database server is running
- Verify firewall rules
- Check credentials

#### JWT Token Errors
- Ensure JWT secret key is at least 32 characters
- Check token expiration settings
- Verify cookie settings for CORS

#### Ollama Connection Errors
- Verify Ollama is running: `ollama serve`
- Check endpoint in configuration
- Verify model is available: `ollama list`

#### Frontend Build Errors
- Clear `node_modules` and reinstall: `rm -rf node_modules && npm install`
- Check Node.js version (18+ required)
- Verify environment variables

See [Troubleshooting Guide](docs/IntelliPM%20Documentation/IntelliPM_Backend.md#15-troubleshooting) for more details.

---

## 📊 Project Statistics

- **Backend Controllers**: 36 active controllers
- **Domain Entities**: 39 entities
- **Domain Events**: 23 events
- **API Endpoints**: 120+ endpoints
- **Frontend Components**: 200+ React components
- **Frontend Pages**: 20+ pages
- **Test Coverage**: Comprehensive test suite

---

## 🔐 Security

- **Authentication**: JWT tokens stored in HTTP-only cookies
- **Authorization**: Role-based access control (RBAC) with 8 distinct roles
- **Multi-Tenancy**: Organization-based data isolation
- **Input Validation**: FluentValidation on backend, Zod on frontend
- **Security Headers**: Comprehensive security headers middleware
- **CORS**: Configurable CORS policy
- **Rate Limiting**: Built-in rate limiting for API endpoints
- **Debug Controller Isolation**: All test/debug controllers are excluded from Release builds via `#if DEBUG` preprocessor directives

### Roles and Permissions

IntelliPM implements a comprehensive role-based access control system:

- **Global Roles** (3): User, Admin, SuperAdmin - Organization-wide permissions
- **Project Roles** (6): ProductOwner, ScrumMaster, Developer, Tester, Viewer, Manager - Project-specific permissions

**Key Features**:
- Exclusive permissions (e.g., only ScrumMaster can start sprints)
- Workflow-based role requirements (e.g., QA approval required for releases)
- Permission inheritance (ProductOwner inherits ScrumMaster permissions)
- Strict enforcement at API and UI levels

See [Roles and Permissions Guide](docs/RolesAndPermissions.md) for complete details.

### Development-Only Controllers

⚠️ **IMPORTANT**: The following controllers and endpoints are **DEBUG-ONLY** and are **completely excluded** from Release builds:

#### Debug Controllers (Excluded in Release Builds)

1. **`TestController`** (`/api/v1/Test`)
   - **Status**: Wrapped with `#if DEBUG` preprocessor directive
   - **Purpose**: Testing error tracking integrations (e.g., Sentry)
   - **Endpoints**:
     - `GET /api/v1/Test/sentry` - Test Sentry integration (throws exception)
   - **Build Behavior**: Controller code is completely removed in Release builds - compilation will fail if referenced elsewhere

#### Debug Endpoints in Production Controllers

2. **`HealthController`** - Debug-only endpoints:
   - `GET /api/v1/Health/smtp` - SMTP connection diagnostics (DEBUG-only)
   - `POST /api/v1/Health/smtp/send-test` - Send test email (DEBUG-only)
   - **Status**: Wrapped with `#if DEBUG` preprocessor directive
   - **Build Behavior**: These endpoints are completely removed in Release builds

#### Security Measures

- **Compile-time exclusion**: All debug controllers/endpoints use `#if DEBUG` preprocessor directives
- **Build configuration**: `.csproj` ensures DEBUG/RELEASE constants are properly set
- **Runtime verification**: `Program.cs` includes runtime checks in production to detect any debug controllers
- **Build failure protection**: Release builds will fail to compile if debug controllers are referenced

#### Verifying Debug Controller Exclusion

To verify debug controllers are excluded in Release builds:

```bash
# Build in Release mode
dotnet build -c Release

# The build should succeed, and TestController will not be included in the assembly
# Runtime check in Program.cs will also verify no test controllers are present
```

**Note**: Always build in Release mode for production deployments. Debug controllers are automatically excluded and cannot be accessed in production.

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/) and [React](https://react.dev/)
- UI components from [shadcn/ui](https://ui.shadcn.com/)
- Icons from [Lucide](https://lucide.dev/)
- AI powered by [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and [Ollama](https://ollama.ai/)

---

## 📞 Support

- **Documentation**: See `docs/IntelliPM Documentation/`
- **Issues**: [GitHub Issues](https://github.com/your-username/intelliPM-V2/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-username/intelliPM-V2/discussions)

---

<div align="center">

**Made with ❤️ by the IntelliPM Team**

[⬆ Back to Top](#intellipm---intelligent-project-management-system)

</div>

