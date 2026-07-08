# Azure Deployment Guide (App Service & Azure SQL Database)

This complete, step-by-step runbook provides actionable instructions for deploying the FleetMind AI application to Microsoft Azure. 

## Prerequisites

- An active Azure subscription.
- The [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed on your local machine.
- Verify installation and log in:
  ```bash
  az --version
  az login
  ```

---

## 1. Provision Azure SQL Database

The backend API strictly depends on the database, so the database must be provisioned first.

```bash
# Create a Resource Group
az group create --name fleetmind-prod-rg --location eastus

# Create an Azure SQL Server (replace with a globally unique name and strong password)
az sql server create \
  --name fleetmind-sqlserver-prod \
  --resource-group fleetmind-prod-rg \
  --location eastus \
  --admin-user "sqladmin" \
  --admin-password "YourStrong!Passw0rd"

# Create the Azure SQL Database (Basic tier is a cost-conscious starting point)
az sql db create \
  --resource-group fleetmind-prod-rg \
  --server fleetmind-sqlserver-prod \
  --name FleetMindDb \
  --service-objective Basic
```

### 🛑 CRITICAL STEP: Configure the SQL Firewall

By default, Azure SQL Database blocks **ALL external connections**, including connections coming from other Azure services like your own App Service! If you skip this step, your deployed API will throw cryptic connection-timeout errors that look like application bugs.

We must explicitly enable "Allow Azure services and resources to access this server" using the special `0.0.0.0` rule convention:

```bash
az sql server firewall-rule create \
  --resource-group fleetmind-prod-rg \
  --server fleetmind-sqlserver-prod \
  --name AllowAllWindowsAzureIps \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

## 2. Apply EF Core Migrations

Azure SQL Database is largely wire-compatible with SQL Server for Entity Framework Core. You apply migrations exactly as you would locally, overriding the connection string via an environment variable.

*Assuming you are in the `backend/` directory on your local machine:*
```bash
export ConnectionStrings__DefaultConnection="Server=tcp:fleetmind-sqlserver-prod.database.windows.net,1433;Initial Catalog=FleetMindDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrong!Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

dotnet ef database update --project FleetMind.Api
```
*(On Windows PowerShell, use `$env:ConnectionStrings__DefaultConnection = "..."`)*

---

## 3. Provision Azure App Service (Backend)

We deploy the `.NET 9` backend API into a Linux App Service.

```bash
# Create an App Service Plan (B1 is the minimum for production-like workloads, F1 is free)
az appservice plan create \
  --name fleetmind-api-plan \
  --resource-group fleetmind-prod-rg \
  --is-linux \
  --sku B1

# Create the App Service Web App (name must be globally unique)
az webapp create \
  --name fleetmind-ai-api \
  --plan fleetmind-api-plan \
  --resource-group fleetmind-prod-rg \
  --runtime "DOTNETCORE|9.0"
```

---

## 4. Provision Azure Static Web Apps (Frontend)

To serve the React SPA efficiently and obtain its final URL, provision the Static Web App:

```bash
az staticwebapp create \
  --name fleetmind-ai-frontend \
  --resource-group fleetmind-prod-rg \
  --source https://github.com/your-org/fleetmind-ai \
  --location eastus2 \
  --branch main \
  --token <YOUR_GITHUB_PERSONAL_ACCESS_TOKEN>
```
*(This triggers Azure's automated GitHub Actions deployment. If you are using Azure DevOps instead, you can create it via the Azure Portal and use the CI/CD pipeline from Milestone 146 to deploy).*

Note the final URL of the frontend (e.g., `https://calm-sea-12345.azurestaticapps.net`).

---

## 5. Configure Application Settings

Never store secrets in your source code or `appsettings.json`. Provide production settings natively via Azure Application Settings. Note the **double-underscore** (`__`) convention which ASP.NET Core natively maps to nested JSON configurations.

```bash
# 1. Database Connection String
az webapp config appsettings set -g fleetmind-prod-rg -n fleetmind-ai-api --settings \
  ConnectionStrings__DefaultConnection="Server=tcp:fleetmind-sqlserver-prod.database.windows.net,1433;Initial Catalog=FleetMindDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrong!Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 2. JWT Signing Key (Must be securely generated, >= 32 chars)
az webapp config appsettings set -g fleetmind-prod-rg -n fleetmind-ai-api --settings \
  Jwt__SigningKey="YOUR_RANDOMLY_GENERATED_PRODUCTION_SECRET_KEY"

# 3. CORS Policy (Set to the real deployed Frontend URL identified in step 4)
az webapp config appsettings set -g fleetmind-prod-rg -n fleetmind-ai-api --settings \
  Cors__AllowedOrigin="https://calm-sea-12345.azurestaticapps.net"

# 4. Optional: AI Provider Settings
az webapp config appsettings set -g fleetmind-prod-rg -n fleetmind-ai-api --settings \
  AiProvider__Provider="AzureOpenAI" \
  AiProvider__ApiKey="YOUR_AZURE_OPENAI_KEY" \
  AiProvider__Endpoint="https://YOUR_ENDPOINT.openai.azure.com"
```
*(Alternatively, configure these safely via the Azure Portal -> App Service -> Configuration -> Application settings).*

---

## 6. Deploy the Application

### Automated (Preferred)
Reference the CI/CD Pipeline (`azure-pipelines.yml`) built in **Milestone 146**. 
- Set up an Azure Service Connection in Azure DevOps.
- Map the frontend's deployment token to the `StaticWebAppDeploymentToken` pipeline variable.
- Trigger a build on the `main` branch.

### Manual Alternative
For an immediate, manual deployment of the backend:
```bash
# Publish locally
dotnet publish backend/FleetMind.Api/FleetMind.Api.csproj -c Release -o ./publish

# Zip the payload
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to App Service
az webapp deploy \
  --resource-group fleetmind-prod-rg \
  --name fleetmind-ai-api \
  --src-path deploy.zip \
  --type zip
```

---

## 7. Final Verification

1. **Verify Backend Health:**
   ```bash
   curl https://fleetmind-ai-api.azurewebsites.net/api/v1/health
   ```
   Confirm you receive an HTTP `200 OK` JSON response indicating a connected database.
2. **Verify Frontend E2E:**
   Navigate your browser to the deployed frontend URL (e.g., `https://calm-sea-12345.azurestaticapps.net`). Register a test user, confirm login, and verify API communication works without CORS failures.
