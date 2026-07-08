# System Architecture

```mermaid
graph TB
    %% Definitions
    subgraph Azure Hosting Environment
        
        %% Frontend Boundary
        subgraph Frontend
            SPA[React SPA<br/>Vite / TypeScript]
        end
        
        %% Backend Boundary
        subgraph Backend [ASP.NET Core 9 Web API]
            API[Controllers & Services]
            
            subgraph Background Services
                BgVoyage[Delayed Voyage Check]
                BgMaintenance[Overdue Maintenance]
                BgCert[Expiring Certification]
                BgCache[Analytics Cache Warmup]
            end
        end
        
        %% Database
        DB[(SQL Server /<br/>Azure SQL Database)]
        
    end
    
    %% External Services
    AI[External AI Provider<br/>Azure OpenAI / Google Gemini]
    
    %% Connections
    SPA -- "HTTPS / REST" --> API
    API -- "Entity Framework Core" --> DB
    BgVoyage -. "Reads/Updates" .-> DB
    BgMaintenance -. "Reads/Updates" .-> DB
    BgCert -. "Reads/Updates" .-> DB
    BgCache -. "Reads/Updates" .-> DB
    
    API -- "Provider-Agnostic HTTP" --> AI
```

## Architecture Notes
- **Frontend**: A React Single Page Application utilizing TanStack Query for robust server-state synchronization and caching, with Zustand handling localized client state.
- **Backend**: An ASP.NET Core 9 Web API utilizing the Repository and Unit of Work patterns to abstract Entity Framework Core interactions.
- **AI Integration**: The AI provider integration is heavily abstracted using the Decorator pattern. If API keys are missing, it gracefully degrades via a `NullAiProvider` rather than faulting the application.
- **Background Services**: Built-in `IHostedService` implementations run alongside the API to process time-sensitive domain logic (e.g., flagging delayed voyages) without requiring external cron jobs.
