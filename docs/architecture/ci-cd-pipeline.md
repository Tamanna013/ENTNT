# CI/CD Pipeline Architecture

This document describes the Continuous Integration (CI) pipeline for the FleetMind AI project.

## Pipeline Overview

The CI pipeline runs automatically on every push and pull request to the `main` or `master` branches. It is defined in `azure-pipelines.yml` at the repository root and is divided into two parallel stages:

1. **Backend Build & Test (`BackendBuildAndTest`)**
   - Installs the .NET 9 SDK.
   - Restores dependencies and builds the ASP.NET Core API in Release mode.
   - Runs unit tests and measures code coverage (mocking `IUnitOfWork` entirely).
   - Runs integration tests (using the `CustomWebApplicationFactory` which utilizes an in-memory SQLite database provider, avoiding the need to spin up any external SQL Server services during CI).
   - Publishes the Code Coverage results to Azure DevOps.
   - Parses the Cobertura XML and explicitly enforces the **1% line-rate** minimum threshold defined in our test coverage policy.

2. **Frontend Build & Test (`FrontendBuildAndTest`)**
   - Installs Node.js 20.
   - Runs a clean, reproducible installation of frontend dependencies using `npm ci`.
   - Executes `npm run build` to ensure the production application compiles successfully.
   - Runs the Vitest automated test suite with v8 coverage using `npm run test:coverage`.
   - Parses the `lcov.info` file and enforces the **70% statement coverage** minimum threshold defined in our test coverage policy.

### Deliberate Exclusion of Playwright E2E Tests
The Playwright end-to-end test suite (`frontend/e2e/`) is **deliberately not run** in this CI pipeline. Running a full headless browser test journey requires actively running backend and frontend instances, as well as a real database state. This represents a meaningfully heavier infrastructure dependency than this build-and-test pipeline is designed for. 

In the future, the E2E suite should be integrated via a separate, optional deployment or post-deployment stage (e.g., spinning up ephemeral Docker containers for the database, API, and frontend before running Playwright).

## Connecting the Pipeline to Azure DevOps

Follow these exact steps to connect `azure-pipelines.yml` to a real Azure DevOps project:

1. **Prerequisites**
   - Ensure you have an active Azure DevOps organization and project. If you do not have one, create a free organization at [dev.azure.com](https://dev.azure.com).

2. **Create the Pipeline**
   - In your Azure DevOps project, navigate to **Pipelines** > **Pipelines** from the left-hand menu.
   - Click the **New pipeline** button (or **Create Pipeline** if it's your first one).

3. **Select your Code Hosting Provider**
   - Select the location where your FleetMind repository is hosted (e.g., **GitHub**, **Azure Repos Git**, **Bitbucket Cloud**).
   - Authenticate with the provider if necessary, and select the `fleetmind-ai` repository.

4. **Select the Configuration File**
   - On the *Configure your pipeline* step, select **Existing Azure Pipelines YAML file**.
   - In the dropdown, select the `main` or `master` branch.
   - For the path, choose `/azure-pipelines.yml`.
   - Click **Continue**.

5. **Run the Pipeline**
   - You will be presented with the YAML code editor. 
   - Click the **Run** button to initiate the first pipeline execution. 
   - Observe the parallel execution of both the Backend and Frontend stages.

## Pipeline Secrets and Variables

**No pipeline secrets or environment variables are currently required** for this purely CI-focused build-and-test pipeline. 

However, if future milestones introduce deployment tasks requiring authentication (e.g., Azure service connections, Docker Hub tokens, or production API keys), **never embed them directly in the `azure-pipelines.yml` file.**

The correct mechanism for sensitive values is Azure DevOps' **Pipeline Variables** or **Variable Groups**:
- Accessible via the **Variables** button in the top-right corner of the pipeline editor.
- Check the **"Keep this value secret"** lock icon for credentials, which will securely mask the value in the UI and logs.
- Reference them in the YAML securely using the syntax `$(VariableName)`.

## Deployment Setup

To enable the automated deployment stages (`DeployBackend` and `DeployFrontend`), you must connect this pipeline to your real Azure subscription.

### 1. Create an Azure Service Connection
The `AzureWebApp@1` task in the backend deployment stage requires an authenticated connection to Azure to push the deployed package.
1. In Azure DevOps, navigate to **Project Settings** (bottom left corner).
2. Under Pipelines, select **Service Connections**.
3. Click **New service connection**.
4. Select **Azure Resource Manager** and click **Next**.
5. Select **Service principal (automatic)** and authenticate with your real Azure subscription.
6. Name the Service Connection EXACTLY: `FleetMind-Azure-Connection`.
   *(If you choose a different name, you must update the `azureSubscription` input in `azure-pipelines.yml` to match).*

### 2. Update Placeholders
Once your Azure App Service and Azure Static Web Apps resources are provisioned in Azure (addressed in a later milestone), update the placeholder names in `azure-pipelines.yml` to match your actual resources:
- Update `appName: 'fleetmind-ai-api'` to match your real backend App Service name.
- For the frontend deployment (`AzureStaticWebApp@0`), configure the deployment token as a secret Pipeline Variable named `StaticWebAppDeploymentToken`. The task reads this via `$(StaticWebAppDeploymentToken)`.

**CRITICAL NOTE:** No credential material is ever stored in the YAML file itself. It relies exclusively on named references to securely-configured Service Connections and Pipeline Variables.

### 3. Verify Branch Gating
The deployment stages are explicitly gated to run **ONLY on successful main-branch builds** (`condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))`), and **NEVER** on a pull request.
1. Before merging to `main`, create a PR with these changes.
2. Observe the PR build: it should successfully run the `BackendBuildAndTest` and `FrontendBuildAndTest` stages, but the deployment stages should not execute (they should appear skipped or missing entirely).
3. Once the PR is merged to `main`, observe the subsequent pipeline run to verify it proceeds through the full build-test-deploy lifecycle.

**Why Azure Static Web Apps for the Frontend?**
We have deliberately chosen the `AzureStaticWebApp@0` task over a secondary `AzureWebApp@1` App Service for the React frontend. Azure Static Web Apps is Microsoft's purpose-built service for static Single Page Applications (SPAs). It offers global distribution, integrated CI/CD, and is vastly more cost-effective than running a full App Service container just to serve static `dist/` HTML/JS files. (If your enterprise architecture absolutely requires standardizing entirely on App Service, the `AzureWebApp@1` task can be used with a static or Node runtime, but Static Web Apps remains the recommended choice).
