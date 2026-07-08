# Production Configuration Checklist

Before considering the FleetMind AI application "production-ready," a deployer MUST verify that every setting below is securely injected via Azure App Service Application Settings (or Azure Key Vault). 

**Never** commit these to `appsettings.json` or `appsettings.Production.json`.

### Core Connectivity
- [ ] `ConnectionStrings__DefaultConnection` - Set to the real Azure SQL Database connection string (must include `Encrypt=True` and omit `TrustServerCertificate=True` in production).

### Security & Authentication
- [ ] `Jwt__SigningKey` - Set to a securely generated, >= 32 character cryptographic key. MUST be completely different from any value ever used locally.
- [ ] `Cors__AllowedOrigin` - Set to the exact, public HTTPS URL of the deployed frontend SPA (e.g., `https://calm-sea-12345.azurestaticapps.net`). Omit trailing slashes.

### AI Integration (If Enabled)
- [ ] `AiProvider__Provider` - Set to `AzureOpenAI` or `OpenAI` instead of `None`.
- [ ] `AiProvider__ApiKey` - Set to the genuine provider secret key.
- [ ] `AiProvider__Endpoint` - (Required for AzureOpenAI) Set to the Azure OpenAI resource endpoint.

### Feature Configuration
- [ ] `FileStorage__LocalStoragePath` - (Optional) Verify or override the file upload directory if mapping to an Azure Storage mount. For robust production, consider transitioning from Local Storage to an Azure Blob Storage provider.

### Rate Limiting & Account Lockout
- [ ] *(Optional)* Validate if default limits defined in `appsettings.json` (`RateLimiting__GeneralApiPermitLimit` = 100, `AccountLockout__MaxFailedAttempts` = 5) are suitable for your production traffic expectations. Adjust via Application Settings if necessary.
