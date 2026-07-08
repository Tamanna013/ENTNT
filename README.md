# FleetMind AI

FleetMind AI is an enterprise-grade, AI-powered maritime operations management platform built for modern shipping companies. It delivers comprehensive oversight of fleets, ships, crew, voyages, cargo, containers, maintenance, and fuel operations. Enhanced with integrated AI-assisted insights, natural language search, and automated reporting, FleetMind AI demonstrates production-quality full-stack engineering practices from end to end.

[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?logo=azuredevops)](#)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](#)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](#)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?logo=typescript)](#)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver)](#)
[![Azure](https://img.shields.io/badge/Azure-Hosted-0089D6?logo=microsoftazure)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## Key Features

### Authentication & Authorization
- **JWT with Refresh Tokens:** Secure authentication via short-lived JWTs and HttpOnly-cookie refresh token rotation.
- **Role-Based Access Control (RBAC):** Granular permission enforcement across five distinct roles (`SystemAdmin`, `FleetManager`, `Captain`, `CrewMember`, `PortAuthority`).
- **Security Posture:** Account lockout defenses, secure password hashing, and token revocation mechanisms.

### Fleet Operations
Full CRUD and workflow-status management across critical operational domains:
- **Fleets & Ships:** Hierarchical management of maritime assets.
- **Crew Management:** Certification tracking, assignment histories, and role definitions.
- **Voyage & Port Operations:** Departure/arrival tracking, port-of-call management, and automated delayed-voyage detection.
- **Cargo & Containers:** Load distributions, hazardous material compliance, and container manifests.
- **Maintenance & Fuel:** Overdue maintenance alerts, routine scheduling, and precise fuel consumption logging.
- **Incidents:** Prioritized incident reporting with file attachments and remediation workflows.

### AI-Powered Features
The platform features a provider-agnostic AI layer supporting both **Azure OpenAI** and **Google Gemini**, with graceful fallback to non-AI behavior when unconfigured:
1. **Chat Assistant:** Context-aware streaming assistant embedded directly in the UI.
2. **Voyage Summarization:** Automated post-voyage narrative generation.
3. **Incident Report Generation:** AI-assisted drafting of safety incidents based on unstructured notes.
4. **Maintenance Recommendations:** Predictive part replacement and scheduling suggestions.
5. **Cargo Risk Analysis:** Automated hazard compliance and compatibility checks.
6. **Analytics Insights:** Natural language interpretations of raw charting data.
7. **Natural Language Search:** Semantic search across entities without rigid SQL syntax.

### Analytics Dashboard
- **Data Visualization:** Real-time charting powered by Recharts (trend graphs, utilization metrics).
- **Drill-down Navigation:** Interactive filtering from macro fleet views down to specific ship voyages.
- **Exporting:** Scheduled background aggregation caching and PDF/Excel export endpoints.

### Notifications
- **Real-Time Polling:** Low-latency polling architecture for operational alerts.
- **User Preferences:** Granular, per-type opt-in/opt-out notification settings.

### Document Management
- **True Versioning:** Complete document lifecycle management with revision history, distinct from flat file attachments.

### Reporting & Exports
- **PDF Generation:** Pixel-perfect PDF rendering utilizing `QuestPDF`.
- **Reusable Exports:** A genuinely generic, reflection-based CSV/Excel export service for arbitrary data grids.

### Theming & Accessibility
- **Modern UI:** Tailwind CSS-powered interface supporting Dark, Light, and System themes.
- **Accessibility (a11y):** WCAG-conscious keyboard navigation, focus management, and screen-reader support.
- **Responsive Design:** Fully fluid mobile-first layouts ensuring usability for field crews on tablets and phones.

### Security Hardening
- **Defense in Depth:** Configured rate limiting, hardened security headers (HSTS, X-Content-Type-Options), reasoned CSRF/XSS posture, and strict upload content verification.

### Performance
- **Optimized Execution:** React route-based code splitting, resolved EF Core N+1 queries, global HTTP response compression, and evidence-based SQL indexing.

### Testing
- **Comprehensive Coverage:** Extensive backend unit/integration tests (xUnit), frontend component/hook tests (Vitest/Testing Library), and fully automated End-to-End browser tests (Playwright).

### DevOps
- **CI/CD Pipeline:** Fully automated Azure DevOps YAML pipeline.
- **Containerization:** Optional `docker-compose` orchestration for instant local development.
- **Cloud Native:** Ready for deployment to Azure App Service and Azure SQL Database.

---

## Tech Stack

| Frontend | Backend | AI | DevOps |
|---|---|---|---|
| React 18 | ASP.NET Core 9 | Azure OpenAI | Azure DevOps (CI/CD) |
| TypeScript | C# 13 | Google Gemini | Docker Compose |
| Tailwind CSS | Entity Framework Core | *Provider-Agnostic* | Azure App Service |
| React Router | SQL Server | | Azure SQL Database |
| TanStack Query | AutoMapper | | xUnit (Backend Tests) |
| Zustand | FluentValidation | | Vitest (Frontend Tests)|
| Recharts | Serilog | | Playwright (E2E Tests)|

---

## Architecture

FleetMind AI follows a clean, decoupled architecture:
- **Backend:** Employs the Repository + Unit of Work pattern, generic CRUD abstractions, and JWT bearer authentication. AI integration utilizes the decorator pattern for seamless provider swapping.
- **Frontend:** A Vite-powered React SPA heavily utilizing TanStack Query for server state management and Zustand for client state.
- **Data Flow:** [View the System Architecture Diagram](docs/architecture/system-architecture-diagram.md)

---

## Getting Started

### Option 1: Docker Compose (Recommended)
The fastest way to spin up the entire stack locally:
1. Copy the environment template:
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` to set a placeholder `DB_SA_PASSWORD` (e.g., `DevOnly!Passw0rd`).
3. Run the orchestration:
   ```bash
   docker-compose up --build
   ```
   *The backend will automatically apply database migrations on startup. The frontend is available at `http://localhost:5173`.*

### Option 2: Manual Setup
If you prefer running services directly on your host:
1. Set up a local SQL Server instance (Developer Edition or LocalDB). See [Database Setup Guide](docs/architecture/database-setup.md).
2. Start the backend:
   ```bash
   cd backend
   dotnet run --project FleetMind.Api
   ```
3. Start the frontend:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

### Running Tests
- **Backend Unit/Integration:** `cd backend && dotnet test`
- **Frontend Component:** `cd frontend && npm test`
- **End-to-End (Playwright):** `cd frontend && npm run test:e2e`

---

## Screenshots

Explore the visual design and functionality of FleetMind AI in the [Screenshots Gallery](docs/SCREENSHOTS.md).

---

## Project Structure

- `backend/` - The ASP.NET Core 9 Web API solution.
- `frontend/` - The React SPA built with Vite.
- `docs/` - Comprehensive architectural, deployment, and testing documentation.
- `load-tests/` - k6 performance testing scripts.
- `azure-pipelines.yml` - The automated CI/CD definition for Azure DevOps.

---

## Deployment & Production

FleetMind AI is explicitly designed for deployment to Azure App Service and Azure SQL Database. Secrets are managed exclusively via Azure Application Settings (or Key Vault), entirely isolated from source control.

For detailed provisioning instructions, consult the [Azure Deployment Guide](docs/deployment/azure-deployment-guide.md).

---

## Future Improvements

While this initial roadmap delivered the complete specification across ten phases and 150 milestones, see the [Future Improvements Roadmap](docs/architecture/future-improvements.md) for the next steps in maturing this enterprise platform (e.g., SignalR integration, multi-tenancy, and distributed Redis caching).

---

## License

This project is licensed under the [MIT License](LICENSE).
