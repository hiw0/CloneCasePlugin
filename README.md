# CloneCasePlugin

Microsoft Dynamics 365 / Dataverse plugin for cloning Case (Incident) records.

This repository demonstrates backend customization and workflow automation within the Dynamics 365 ecosystem using C# plugins and Dataverse actions.

## Overview

The plugin is designed to automate duplication of support/service cases while preserving important business context.

Typical use cases:

- recurring support workflows
- escalation handling
- templated case creation
- operational efficiency improvements

## Technical focus

- Dynamics 365 CE / Dataverse
- C# plugin development
- Custom Actions
- Entity operations
- CRM workflow automation
- Secure server-side execution

## Engineering concepts demonstrated

- Plugin registration and execution pipelines
- Dataverse entity manipulation
- Server-side business logic
- Separation of UI actions from backend execution
- CRM automation architecture

## Example flow

```mermaid
flowchart LR
    A[User clicks Clone Case] --> B[Custom Action]
    B --> C[C# Plugin Execution]
    C --> D[Read Existing Incident]
    D --> E[Create New Incident]
    E --> F[Return New Record ID]
```

## Why this project matters

This project demonstrates practical enterprise software engineering beyond simple CRUD applications:

- integrating with large business platforms
- extending enterprise systems safely
- designing automation workflows
- building backend business logic in production-style environments

## Future improvements

- selective field-copy configuration
- attachment cloning
- audit logging
- async processing support
- configurable cloning templates
