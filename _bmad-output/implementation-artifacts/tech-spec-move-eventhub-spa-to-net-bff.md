--- 
title: 'Move EventHub SPA to .NET BFF'
slug: 'move-eventhub-spa-to-net-bff'
created: '2026-03-21'
status: 'Completed'
stepsCompleted: [1, 2, 3, 4, 5, 6]
tech_stack: ['dotnet', 'angular', 'typescript', 'docker', 'terraform']
files_to_modify: ['eventhub-spa/src/app/core/api-base-url.token.ts', 'docker-compose.yml', '.infrastructure/modules/app_service_webapi/main.tf', '.infrastructure/modules/app_service_webapi/outputs.tf', '.infrastructure/modules/app_service_webapi/variables.tf']
code_patterns: ['angular-injection-token', 'dotnet-appsettings', 'docker-compose', 'terraform-modules']
test_patterns: ['xunit', 'karma-jasmine']
---

# Quick-Spec: Move EventHub SPA to .NET BFF

## Overview

### Problem Statement
The `EventHub.SPA` currently exposes the `EventHub.WebApi` URL in its static content, leading to configuration inflexibility and potential security implications.

### Solution
Introduce a new .NET BFF (Backend for Frontend) application. This BFF will serve the `EventHub.SPA`'s static assets and act as a proxy for all API requests to the `EventHub.WebApi`. The `EventHub.WebApi`'s base URL will be securely configured in the BFF's `appsettings.json`. This setup centralizes API URL management and enhances security by abstracting the direct API endpoint from the static frontend.

### In Scope
- Creation of a new .NET project for the BFF.
- Configuration of the BFF to host and serve the `eventhub-spa`'s static content.
- Implementation of a proxy mechanism within the BFF to forward API requests to the `EventHub.WebApi`.
- Configuration of the `EventHub.WebApi`'s base URL within the BFF's `appsettings.json`.
- Modification of the `eventhub-spa` to make API requests relative to the BFF's URL.
- Review and necessary updates to existing Terraform configurations to deploy and manage the new BFF and its interactions with other services.

### Out of Scope
- Adding new business logic or extensive features to the BFF beyond static content serving and API proxying.
- Significant changes to the core functionality or API contracts of the `EventHub.WebApi`.
- Modifications to the `EventHub.FunctionApp` or Azure CosmosDB.
- Implementation of advanced authentication or authorization mechanisms within the BFF, as this was out of scope in the initial architecture.

## Context for Development

*   The BFF should be a new .NET project, adhering to the existing .NET coding standards and project structure within the EventHub system.
*   The proxying mechanism should be efficient and transparent to the frontend, ideally leveraging ASP.NET Core's built-in capabilities or a well-established library.
*   Configuration of the `EventHub.WebApi` URL in `appsettings.json` should support environment-specific overrides.
*   Terraform review will involve identifying and modifying existing modules or creating new ones to accommodate the BFF's deployment and configuration within Azure. This includes considerations for hosting (e.g., Azure App Service) and networking.

## Implementation Plan

### Tasks

- [x] Task 1: Create new .NET BFF project
  - File: `EventHub.BFF/EventHub.BFF.csproj` (new)
  - Action: Create a new ASP.NET Core Empty Web project named `EventHub.BFF`.
  - Notes: Use `dotnet new web --no-https -n EventHub.BFF`

- [x] Task 2: Configure BFF to serve static SPA content
  - File: `EventHub.BFF/Program.cs`
  - Action: Add middleware to serve static files from the `eventhub-spa`'s build output.
  - Notes: This will involve configuring `app.UseStaticFiles()` and `app.UseSpaStaticFiles()`. The SPA's build output will be copied into the BFF's publish directory.

- [x] Task 3: Implement API proxying in BFF
  - File: `EventHub.BFF/Program.cs`
  - Action: Configure a reverse proxy to forward `/api` requests to the `EventHub.WebApi`.
  - Notes: Consider using `Microsoft.AspNetCore.Proxy` or `Yarp.ReverseProxy`. The target URL for `EventHub.WebApi` will be read from `appsettings.json`.

- [x] Task 4: Configure Web API URL in BFF's appsettings
  - File: `EventHub.BFF/appsettings.json` (new)
  - File: `EventHub.BFF/appsettings.Development.json` (new)
  - Action: Add a new configuration entry (e.g., `"WebApiBaseUrl": "http://eventhub-webapi:8080"`) to `appsettings.json` and `appsettings.Development.json`.
  - Notes: The development URL will likely point to the Docker service name for `eventhub-webapi`.

- [x] Task 5: Update `eventhub-spa` to use relative API URLs
  - File: `eventhub-spa/src/app/core/api-base-url.token.ts`
  - Action: Remove the logic for conditionally setting `http://localhost:5080` and always return an empty string or `/` to ensure API calls are relative to the BFF.
  - Notes: This ensures the SPA will make requests relative to its host (the BFF).

- [x] Task 6: Update `docker-compose.yml`
  - File: `docker-compose.yml`
  - Action: 
    1. Add a new service for `eventhub-bff` that builds the `EventHub.BFF` project.
    2. Configure `eventhub-spa` service to build its production output and copy it into the `eventhub-bff`'s static files directory (or mount it as a volume).
    3. Update the `eventhub-spa` service to depend on the `eventhub-bff` service.
    4. Expose the `eventhub-bff` port (e.g., `5000:8080`).
  - Notes: The `eventhub-spa` will no longer need to expose port 4200 directly.

- [x] Task 7: Create new Terraform module for BFF
  - File: `.infrastructure/modules/app_service_bff/main.tf` (new)
  - File: `.infrastructure/modules/app_service_bff/outputs.tf` (new)
  - File: `.infrastructure/modules/app_service_bff/variables.tf` (new)
  - Action: Create a new Terraform module for deploying the `EventHub.BFF` as an Azure App Service. This module should mirror the structure and conventions of the `app_service_webapi` module.
  - Notes: Include resources for the App Service Plan, App Service, and Application Insights integration.

- [x] Task 8: Integrate BFF Terraform module into environments
  - File: `.infrastructure/environments/dev/main.tf`
  - File: `.infrastructure/environments/test/main.tf`
  - Action: Instantiate the new `app_service_bff` module in both the `dev` and `test` environments.
  - Notes: Configure variables for the BFF's App Service, including linking it to the existing Application Insights and potentially the virtual network.

## Acceptance Criteria

- [x] AC 1: Given the `EventHub.BFF` is running locally via `docker-compose`, when navigating to `http://localhost:5000` (or configured BFF port), then the `eventhub-spa` application loads successfully.
- [x] AC 2: Given the `EventHub.BFF` is running locally, when an API call is made from the `eventhub-spa` (e.g., to `/api/events`), then the request is successfully proxied to the `EventHub.WebApi` and the correct response is received.
- [x] AC 3: Given the `EventHub.BFF` is configured with a specific `WebApiBaseUrl` in `appsettings.json`, when the BFF starts up, then it correctly uses this URL for proxying requests to the `EventHub.WebApi`.
- [x] AC 4: Given the `eventhub-spa` has been modified to use relative API URLs, when deployed and served by the BFF, then all API requests are correctly routed through the BFF.
- [x] AC 5: Given the `docker-compose.yml` has been updated, when running `docker-compose up`, then all services (including the new `eventhub-bff`) start without errors and `eventhub-spa` is accessible via the BFF.
- [x] AC 6: Given the new `app_service_bff` Terraform module is applied to the `dev` environment, when checking Azure resources, then a new Azure App Service for the BFF is created, configured, and linked to Application Insights.
- [x] AC 7: Given the `eventhub-spa` is being served by the BFF, when a user accesses the application, then the `EventHub.WebApi` URL is not directly exposed in the client-side static content.

## Dependencies

*   ASP.NET Core (for the BFF)
*   Yarp.ReverseProxy (or similar library for proxying in .NET)
*   Existing `EventHub.WebApi` for API calls.
*   Existing `eventhub-spa` for static content.
*   Docker and Docker Compose for local development.
*   Terraform for Azure infrastructure deployment.

## Testing Strategy

*   **Unit Tests:** Implement unit tests for the BFF's proxying logic and configuration loading using `xUnit`.
*   **Integration Tests:**
    *   **Local:** Verify the `docker-compose` setup correctly serves the SPA and proxies API calls.
    *   **Deployed:** After Terraform deployment, verify the deployed BFF serves the SPA and proxies API calls to the `EventHub.WebApi` in Azure.
*   **Manual Testing:** Manually navigate the `eventhub-spa` via the BFF to ensure all functionality works as expected. Inspect network requests to confirm API URLs are being proxied correctly.

## Notes

*   Consider the caching strategy for static assets served by the BFF.
*   Ensure appropriate CORS policies are configured in the BFF for the `EventHub.WebApi` if necessary (though with a single origin through the BFF, this might be simplified).
*   The `EventHub.WebApi` will still need to handle its own CORS if it has other direct clients, but for calls coming from the BFF, it will be same-origin.

## Review Notes
- Adversarial review completed
- Findings: 7 total, 7 fixed, 0 skipped
- Resolution approach: Walk through and Fix automatically