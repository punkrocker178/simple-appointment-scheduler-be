# Entity Relationship Diagram

```mermaid
erDiagram
    DEALERSHIP {
        guid id PK
        string name
        string address
        string phone
        string timezone
    }
    
    TECHNICIAN {
        guid id PK
        guid dealershipId FK
        string firstName
        string lastName
        boolean isActive
    }
    
    SERVICE_BAY {
        guid id PK
        guid dealershipId FK
        string name
        boolean isActive
    }
    
    SKILL {
        guid id PK
        string name
        string description
    }
    
    SERVICE_TYPE {
        guid id PK
        guid dealershipId FK
        guid skillId FK
        string name
        string description
        int durationMinutes
        decimal price
        boolean isActive
    }
    
    TECHNICIAN_SKILL {
        guid technicianId PK_FK
        guid skillId PK_FK
    }
    
    CUSTOMER {
        guid id PK
        string firstName
        string lastName
        string email
        string phone
        datetime createdAt
    }
    
    VEHICLE {
        guid id PK
        guid customerId FK
        string make
        string model
        int year
    }
    
    APPOINTMENT {
        guid id PK
        guid customerId FK
        guid technicianId FK
        guid serviceBayId FK
        guid serviceTypeId FK
        datetime startTime
        datetime endTime
        enum status
    }
    
    DEALERSHIP ||--o{ TECHNICIAN: "manages"
    DEALERSHIP ||--o{ SERVICE_BAY: "contains"
    DEALERSHIP ||--o{ SERVICE_TYPE: "offers"
    
    TECHNICIAN ||--o{ APPOINTMENT: "performs"
    TECHNICIAN }o--|| DEALERSHIP: "works at"
    
    TECHNICIAN }o--o{ SKILL: "has"
    TECHNICIAN_SKILL }o--|| TECHNICIAN: ""
    TECHNICIAN_SKILL }o--|| SKILL: ""
    
    SERVICE_TYPE }o--|| SKILL: "requires"
    
    CUSTOMER ||--o{ VEHICLE: "owns"
    CUSTOMER ||--o{ APPOINTMENT: "schedules"
    
    VEHICLE ||--o{ APPOINTMENT: "for"
    
    SERVICE_BAY ||--o{ APPOINTMENT: "uses"
    
    SERVICE_TYPE ||--o{ APPOINTMENT: "provides"
```

## Entity Descriptions

### Core Entities

**Dealership**
- Central hub for all operations
- Contains multiple technicians, service bays, and service types
- Stores timezone information for scheduling

**Technician**
- Works at a specific dealership
- Has multiple skills through the TechnicianSkill junction table
- Can have multiple appointments

**Customer**
- Owns vehicles
- Can schedule multiple appointments

**Vehicle**
- Belongs to a customer
- Can have multiple appointments for different services

### Service Management

**Skill**
- Represents a required capability (e.g., "Oil Change", "Engine Diagnostics")
- Many-to-many relationship with technicians via TechnicianSkill
- Required for specific service types

**ServiceType**
- Specific service offered by dealership
- Requires a specific skill
- Has pricing and duration information

**ServiceBay**
- Physical location where appointments are performed
- Belongs to a specific dealership
- Can have multiple appointments

### Operational Entity

**Appointment**
- Links customer, vehicle, technician, service bay, and service type
- Has start/end times
- Can be in states: Scheduled, InProgress, Completed, Cancelled

**TechnicianSkill** (Junction Table)
- Many-to-many relationship between technician and skill
- Allows tracking which skills each technician possesses

## Authentication

Authentication is separate from scheduling (`CUSTOMER` has no FK to auth `USER`).

```mermaid
erDiagram
    USER {
        guid id PK
        string email UK
        string passwordHash
        guid roleId FK
        string firstName
        string lastName
        boolean isActive
        datetime createdAt
    }

    ROLE {
        guid id PK
        string name UK
        string description
    }

    PERMISSION {
        guid id PK
        string name UK
        string description
    }

    ROLE_PERMISSION {
        guid roleId PK_FK
        guid permissionId PK_FK
    }

    USER }o--|| ROLE : "has"
    ROLE ||--o{ ROLE_PERMISSION : ""
    PERMISSION ||--o{ ROLE_PERMISSION : ""
```

### Authentication entities

**User**
- Login account for the application API
- Not linked to `Customer` in the current schema
- Each user has exactly one `Role` via `RoleId`

**Role**
- Named access level (`Admin`, `Staff`, `User`)
- Many-to-many with permissions through `RolePermission`

**Permission**
- String claim name used for authorization (e.g. `appointments:read:own`)
- Seeded in [`AuthSeedData.cs`](../Infrastructure/Persistence/AuthSeedData.cs)

**RolePermission** (Junction Table)
- Many-to-many relationship between role and permission

### Seed data (authentication)

Default roles and permissions are seeded via EF Core `HasData` in the `AddAuthentication` migration.

| Role | Permissions |
|------|-------------|
| Admin | `appointments:read`, `appointments:read:own`, `appointments:write`, `users:manage` |
| Staff | `appointments:read`, `appointments:write` |
| User | `appointments:read:own`, `appointments:write` |

`appointments:read:own` is enforced in application code when JWT/endpoints are added (e.g. filter appointments by a future `User` ↔ `Customer` link). The database stores only the permission name.
