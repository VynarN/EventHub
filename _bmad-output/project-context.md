---
project_name: 'EventHub'
user_name: 'EventHubDev'
date: '2026-03-19'
sections_completed:
  ['technology_stack', 'language_rules', 'framework_rules', 'testing_rules', 'quality_rules', 'workflow_rules', 'anti_patterns']
status: 'complete'
rule_count: 18
optimized_for_llm: true
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

- **Frontend:** Angular SPA, TypeScript, SCSS. Utilizes Angular CLI and Webpack for tooling. Testing with Karma and Jasmine (default), with potential for Cypress/Playwright for E2E.
- **Backend:** .NET Web API (C#, .NET 10 or latest stable version, ASP.NET Core). Unit testing with xUnit, integration testing with Microsoft.AspNetCore.Mvc.Testing.
- **Serverless:** Azure Function App.
- **Messaging:** Azure Service Bus (Premium Tier for guaranteed throughput).
- **Database:** Azure CosmosDB (using `Id` as partition key, auto-scale provisioned throughput).
- **Containerization:** Docker for all components, orchestrated with `docker-compose` for local development.
- **CI/CD:** GitHub Actions.
- **Monitoring & Logging:** Azure Application Insights (for .NET Web API and Azure Function App).
- **Error Handling:** RFC 9457 (Problem Details for HTTP APIs) for .NET Web API.
- **Hosting:** Azure Static Web Apps (Angular SPA), Azure App Service (.NET Web API), Azure Functions Consumption Plan (Azure Function App).

## Critical Implementation Rules

### Language-Specific Rules

- **TypeScript/JavaScript (Angular SPA):**
    - **Configuration:** Strict mode for TypeScript for robust type checking.
    - **Import/Export:** Use relative paths within the same module; absolute paths/aliases for shared/core modules.
    - **Error Handling:** Global handling via Angular's `ErrorHandler`; service-level handling with RxJS `catchError`.
- **C# (.NET Web API & Azure Function App):**
    - **Configuration:** Standard .NET configuration using `appsettings.json` and environment variables.
    - **Import/Export:** Standard C# `using` directives.
    - **Error Handling:** Middleware for global exception handling in Web API (RFC 9457 compliant); `try-catch` in Azure Function App for message processing.

### Framework-Specific Rules

- **Angular SPA:**
    - **Hooks Usage:** Prefer Angular lifecycle hooks (`ngOnInit`, `ngOnDestroy`). Avoid direct DOM manipulation.
    - **Component Structure:** Feature-based modules; Smart/Dumb component principles for data/logic vs. UI.
    - **State Management:** Simple service with RxJS `BehaviorSubject`; immutable state updates.
    - **Performance:** Lazy loading for feature modules.
- **.NET Web API:**
    - **API Route Conventions:** RESTful endpoints with `kebab-case` resource paths.
    - **Dependency Injection:** Utilize built-in .NET Core DI.
    - **Middleware Usage:** Employ middleware for cross-cutting concerns (error handling, logging).
- **Azure Function App:**
    - **Trigger Types:** Azure Service Bus queue/topic triggers.
    - **Idempotency:** Implement idempotency for event processing.
    - **Error Handling:** Robust `try-catch` with dead-lettering for failed messages.

### Testing Rules

-   **Test Organization:** Unit tests co-located with code (`.spec.ts` for Angular). Integration tests for .NET Web API in a separate test project.
-   **Mock Usage:** Standard mocking frameworks (Jasmine spies for Angular, Moq for C#).
-   **Test Coverage:** Comprehensive testing expected; no explicit coverage percentage mandated yet.
-   **Integration vs Unit Tests:** Unit tests focus on isolated logic; integration tests verify component/service interactions.

### Code Quality & Style Rules

-   **Linting/Formatting:** Adherence to Angular CLI's default linting (TSLint/ESLint) and Prettier for Angular SPA. Standard C# code style guidelines for .NET.
-   **Code Organization:**
    -   **Angular SPA:** Feature-based modules within `src/app/`, with `core` and `shared` modules for common elements.
    -   **.NET Web API:** Standard ASP.NET Core MVC structure (`Controllers`, `Models`, `Services`).
    -   **Azure Function App:** Single function per file or organized into subfolders.
-   **Naming Conventions:** (Refer to previous sections for detailed naming rules for Database, API, and Code elements).
-   **Documentation:** Clear, concise comments for complex logic; JSDoc for TypeScript; `README.md` for each major component.

### Development Workflow Rules

-   **Git/Repository:**
    -   **Branch Naming:** `feature/<feature-name>` or `bugfix/<bug-description>`.
    -   **Commit Messages:** Concise, descriptive, follow Conventional Commits (e.g., `feat:`, `fix:`).
    -   **PR Requirements:** At least one reviewer approval, passing CI/CD checks (GitHub Actions).
-   **Deployment:** Continuous deployment via GitHub Actions to Azure services (Static Web Apps, App Service, Azure Functions).

### Critical Don't-Miss Rules

-   **Anti-Patterns to Avoid:**
    -   **Inconsistent Naming:** Adhere strictly to defined naming conventions (e.g., `PascalCase`, `camelCase`, `kebab-case`).
    -   **Ignoring RFC 9457:** Do not implement custom error response formats; adhere to RFC 9457.
    -   **Direct DOM Manipulation (Angular):** Avoid direct DOM manipulation; use Angular directives/services.
    -   **Mutable State Updates (Angular):** Use immutable updates for state objects (e.g., new instances for RxJS `BehaviorSubject`).
-   **Edge Cases:**
    -   **Idempotency:** Ensure all Azure Function App event processing is idempotent.
    -   **Pagination Boundaries:** Handle empty results, last page, and invalid page numbers for pagination.
    -   **CosmosDB Partition Key:** Correctly use `Id` as the partition key for efficient CosmosDB queries.
-   **Security:**
    -   **Input Validation:** Always validate and sanitize all user inputs.
    -   **Secrets Management:** Do not hardcode secrets; use environment variables or Azure Key Vault.
-   **Performance:**
    -   **N+1 Queries:** Avoid N+1 query problems in .NET Web API for CosmosDB.
    -   **Large Payloads:** Be mindful of excessively large payloads in communication between components.
