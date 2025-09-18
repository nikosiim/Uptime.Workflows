# API Contracts

⚠️ **Notice:**  
Domain layer (Core project) models and enums **should not be directly exposed in the API**.  
Instead, use API contract types (DTOs, enums) in this folder as a middleware between your domain logic and your API surface.

Whenever you add, remove, or change a contract here that corresponds to a domain model or enum in the Core project,  
**you must update the mapping code and keep both the API contract and domain model in sync.**

- **Source of truth:** Core project (`Uptime.Workflows.Core`)
- **API contract:** This folder (`Uptime.Workflows.Api/Contracts`)

This ensures your API remains decoupled from internal business logic, but stays synchronized.  
See `Mapper.cs` for mapping logic and keep it updated alongside any contract or domain changes.