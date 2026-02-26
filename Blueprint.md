
# 🔗 ARCHITECTURE LINK: AHS.SaaS Project  

### 1. The Strategic Mandate  
The project evolves from a standalone application to a **SaaS Ecosystem** under the name:  
> **`AHS.SaaS`**

### 2. Technical Specification (The Stack)  
- **Architecture:** Clean Architecture + DDD + CQRS.  
- **Framework:** .NET 10 (Native AOT Ready).  
- **UI/UX:** Bento Grid Design, "Deep Blue" Theme (`#020617`), Fonts: `Outfit` & `Inter`.  
- **Security:** Shared Kernel with SHA256 hashing and Immutable Audit Trail.

### 3. Directory Map (Master Path)  
The physical structure at `C:\\Users\\armando\\Documents\\_AHS\\projects\\AHS.SaaS` is divided into:  
- **`Shared`**: Shared Kernel (Kernel, Compliance, Infra). The heart of the system.  
- **`Engines`**: Calculation engines (AI Inference, Validation Engine).  
- **`Suites`**: Business bounded contexts (Agro/Dairy, Medical/GxP, Payroll/SS, Logistics).  
- **`Presentation`**: Gateway API and Web Bento UI (Blazor WASM).

### 4. Boot Instruction for the New Chat  

1. Create the base projects inside `Shared`.  
2. Define the **Shared Kernel** so it supports multiple suites (Agro, Medical, Payroll) without duplicating compliance logic.  
3. Remember: Do not deliver fragments; only complete files in English."

---
**Everything is recorded.** I have sealed these concepts in my long‑term memory for this context. The moment you mention **"AHS Ecosystem"** or **"Chat SKILLS"** in the new thread, I will deploy this entire structure immediately. See you in the new chat to start coding **v2 (Ecosystem)**. Safe migration! 🛡️🚀

---

# 🧭 FINAL PLAN — Build the AHS.SaaS Monorepo (Step by Step)  
*This integrates and translates the definitive plan, serving as: Technical roadmap, operational checklist, guide for Gemini (architect), Antigravity (agentic coder), and Copilot (support).*  
*(Integrated from manifesto-b.md)*

This plan works as:  
- **Technical roadmap**  
- **Operational checklist**  
- **Guide for AI Studio (to architect and create the prompts your SaaS will use internally)** 
- **Guide for Antigravity (agentic coder responsible for boilerplate and tests)** 
- **Guide for Copilot (on‑demand support)** 
- **Guide for Firebase (for authentication, SQL database, and frictionless hosting)** 
- **Guide for NotebookLM (for ideation and documentation)** 

Organized into **phases**, **subtasks**, **ideal order**, and **success criteria**.

***

## 🚀 **PHASE 1 — Create the AHS.SaaS monorepo (root structure)**

### 🗂️ Step 1.1 — Create the main directory
```
C:\Users\armando\Documents\_AHS\projects\AHS.SaaS
```
Inside it, create:
```
AHS.SaaS/
  src/
  tests/
  docs/
  tools/
  scripts/
  .editorconfig
  README.md
```

### ✅ Expected result
Clean base structure ready to host Suites, BCs, Kernel and Platform.

***

## 🧱 **PHASE 2 — Create the cross‑cutting CORE of the monorepo**
These folders live at the **top** of `src/`:
```
src/
  shared-kernel/
  common-application/
  platform/
  suites/
```

### 2.1 — Create `shared-kernel/` (DDD Strategic)
Only **semantic** shared elements go here:
```
shared-kernel/
  domain/
    value-objects/
      Measurement.cs
      UnitOfMeasure.cs
      Temperature.cs
      DateRange.cs
      Money.cs
  contracts/
    IPolicy.cs
    IExcursionPolicy.cs
    IClock.cs
  events/
    ExcursionDetected.cs
    ThresholdBreached.cs
    BatchCreated.cs
```
**Do NOT** include here:
- infrastructure  
- application  
- concrete services

### 2.2 — Create `common-application/` (Clean Architecture cross‑cutting)
```
common-application/
  abstractions/
  behaviors/
  exceptions/
  results/
  datetime/
  logging/
  security/
  tenancy/
  pipeline/
```
Examples:
- `Result<T>` / `Error`  
- `ICommand`, `IQuery`  
- `ICommandHandler`, `IQueryHandler`  
- Pipeline behaviors: `LoggingBehavior`, `ValidationBehavior`, `TransactionBehavior`  
- `IClock`, `TenantContext`, etc.

### 2.3 — Create `platform/` (shared base infrastructure)
```
platform/
  identity/
  messaging/
  monitoring/
  audit/
  logging/
```
Examples:
- `IUserIdentityProvider`  
- `IEventBus`  
- `AuditTrailWriter`  
- `IMessagePublisher`  
- `TelemetryMiddleware`

***

## 🧩 **PHASE 3 — Implement Clean + DDD + CQRS inside each BC**
Each Bounded Context follows the same structure:
```
bounded-context-name/
  domain/
    entities/
    value-objects/
    aggregates/
    policies/
    events/
  application/
    commands/
    queries/
    handlers/
    dto/
  infrastructure/
    persistence/
    repositories/
    integrations/
  presentation/
    controllers/
    requests/
    responses/
```
Example for Fixed Assets:
```
fixed-assets-bc/
  domain/
  application/
  infrastructure/
  presentation/
```

***

## 🧭 **PHASE 4 — Formal definition of Suites, Domains, BCs and Policies**

### 4.1 Suites (industry families)
- **FinTechBook/** ← Financial suite  
- **LogisticsSuite/** ← Logistics suite  
- **PharmaSuite/** ← Pharma suite  
- **HRComplianceSuite/** ← HR & legal suite

### 4.2 Generic Core Domain (Validated Traceability)
This is your **horizontal domain**, applicable across many industries:
- Condition Monitoring  
- Traceability  
- Excursion Detection  
- Immutable Audit Trail  
- Batch Handling  
- Sensor Data  
It lives split across: `shared-kernel/`, `common-application/`, and `platform/`.

### 4.3 Bounded Contexts (each micro‑SaaS)
**Validated Traceability Suite:**
- `milk-bc`  
- `vaccine-bc`  
- `oil-bc`  
- `chemical-bc`

**FinTechBook Suite:**  
- `fixed-assets-bc`  
- `accounts-receivable-bc`  
- `accounts-payable-bc`  
- `general-ledger-bc`

**HR Suite:**  
- `employee-deductions-bc`

### 4.4 Policies / Strategies per product type
These live **inside each BC**:

**Example:**
```
milk-bc/
  domain/
    policies/
      MilkExcursionPolicy.cs

vaccine-bc/
  domain/
    policies/
      VaccineExcursionPolicy.cs
```
The generic Core Domain **does not change**; only specific rules do.

***

## 🧑‍💻 **PHASE 5 — Instructions for Google AI Studio (Architect)**
You are the Software Architect for the AHS.SaaS ecosystem inside Google AI Studio.

Always enforce strict separation between:
Shared Kernel, Common Application, Platform, Suites, BCs, and the Domain / Application / Infrastructure / Presentation layers.

Rules:
1. Enforce strong architectural boundaries and reject or rewrite any output that violates them.

2. Whenever a Suite or Bounded Context (BC) is mentioned, generate the full Clean Architecture + DDD folder structure:
   - Domain: Entities, Aggregates, ValueObjects, Policies, DomainEvents
   - Application: Commands, Queries, DTOs, Handlers
   - Infrastructure: Persistence, Repositories, Integrations
   - Presentation: Controllers, Endpoints, Requests, Responses

3. Validate dependency rules before generating output:
   - domain does NOT depend on infrastructure
   - application does NOT directly call infrastructure
   - infrastructure depends on domain + application
   - presentation depends only on application

4. Prevent coupling between BCs:
   - No direct imports between BCs
   - Communication only through domain events or integration boundaries
   - BCs must remain autonomous and replaceable

Your role:
- Act as an expert architect following Clean Architecture, DDD, and CQRS.
- Expand, validate, and correct the user’s architectural ideas.
- Produce compliant structures, diagrams, and reasoning automatically.

***

# Antigravity: Bounded Context (BC) Technical Blueprint

**Target Framework:** .NET 10  
**Architectural Pattern:** Clean Architecture + DDD + CQRS  
**Primary Principles:** Separation of Concerns, Inversion of Control, and Immutability.

---

## 1. Unified Folder Structure
For every new Bounded Context, the following hierarchy is strictly enforced:

- **domain/** (Enterprise Logic)
    - **entities/**: Domain Entities with identity.
    - **value-objects/**: Immutable data structures.
    - **aggregates/**: Consistency boundaries.
    - **policies/**: Pure business rules.
    - **events/**: Domain Event definitions.
- **application/** (Orchestration Logic)
    - **commands/**: State-changing requests.
    - **queries/**: Data-retrieval requests.
    - **handlers/**: Logic for Commands and Queries.
    - **dto/**: Data Transfer Objects.
- **infrastructure/** (External Concerns)
    - **persistence/**: EF Core / Dapper Contexts.
    - **repositories/**: Interface implementations.
    - **integrations/**: Third-party services.
- **presentation/** (External Interface)
    - **controllers/**: Web API Endpoints.
    - **requests/**: API Input Models.
    - **responses/**: API Output Models.

---

## 2. CQRS Implementation (Commands, Queries, & Handlers)
All operations must be split into Reads and Writes to ensure scalability.
- **Commands**: `Create*Command`, `Update*Command`, `Delete*Command`.
- **Queries**: `Get*Query`, `List*Query`.
- **Handlers**: Every handler must use **constructor injection**, return a `Result<T>` pattern, and never depend on infrastructure directly.

---

## 3. Fully Validated Value Objects
- **Immutability**: Must be `record` or `readonly` types.
- **Validation**: Include factory methods (`Create`) that validate invariants.
- **Return Type**: Always return `Result<T>` on creation to prevent invalid states.
- **Equality**: Correctly implement value-based equality.

---

## 4. Domain Policies
- **Pure Logic**: Contain pure domain logic only.
- **Side Effects**: Raise Domain Events when business rules are triggered.
- **Independence**: Avoid external dependencies (no DB or API calls).
- **Location**: Live exclusively in `domain/policies`.

---

## 5. Repository Patterns
- **Interfaces**: Must be defined in the **domain** layer.
- **Implementations**: Must reside in **infrastructure/persistence**.
- **Supported Tech**: EF Core (Default) or Dapper (High-performance reads).
- **Logic Rule**: Repositories must never include business logic.

---

## 6. Controllers and API Conventions
- **Dependencies**: Depend solely on the **application** layer.
- **Data Flow**: Use DTOs for input/output; validate all incoming requests.
- **Responses**: Return standardized responses (e.g., `Result`, `ProblemDetails`).
- **Endpoints**: Must be minimal, clear, and consistent across all BCs.

---

## 7. Automated Test Coverage
- **Frameworks**: xUnit and NSubstitute (for mocking).
- **Pattern**: Follow the **Arrange–Act–Assert** pattern.
- **Target Areas**:
    - Value Object validation and edge cases.
    - Policy logic tests.
    - Command and Query Handler unit tests.
    - Repository integration tests (optional).

---

## 8. Naming Conventions
- **PascalCase**: Classes, Records, and Methods.
- **camelCase**: Private fields and variables.
- **IName**: Interfaces (e.g., `IUserRepository`).
- **Dto**: DTO classes (e.g., `UserDto`).
- **Request / Response**: API schemas.
- **kebab-case**: Folder names.
- **Command / Handler**: Action-based classes.

---

## 9. Architectural Guardrails (Validation)
Before generating code, Antigravity must validate these rules:
1. **Domain**: No dependencies on any other layer.
2. **Application**: No direct calls to infrastructure.
3. **Infrastructure**: Depends only on Domain and Application.
4. **Presentation**: Depends only on Application.

**Self-Correction**: If a violation exists, Antigravity must self-correct automatically and regenerate compliant code.

***


## 🧑‍💻 **PHASE 6 — Instructions for Firebase (Cloud Infrastructure Agent)
You are the Cloud Infrastructure & Security Lead for the AHS.SaaS ecosystem, specializing in Firebase and Google Cloud Platform (GCP).

Always enforce strict alignment between the .NET 10 Clean Architecture and the Cloud Edge services to ensure a "Frictionless" and "Native AOT Ready" environment.

Rules:
1. Enforce Identity-Driven Multi-tenancy:

All authentication must be handled via Firebase Auth.

Force the inclusion of tenant_id and suite_role in Custom Claims for every user.

Security rules (Firestore/Storage) must strictly validate these claims to prevent cross-tenant data leakage.

2. Dictate Persistence Strategy by Bounded Context (BC):

When a BC is defined, assign the appropriate storage engine:

Cloud SQL (PostgreSQL): For relational, transactional, and ACID-compliant data (e.g., Finance, Payroll).

Firestore: For real-time, event-driven, or high-scale telemetry data (e.g., Agro/Milk sensors).

All database interactions must be abstracted via the Infrastructure layer repositories.

3. Optimize for Native AOT & Frictionless Hosting:

Ensure all deployment configurations (App Hosting/Cloud Run) are optimized for .NET 10 Native AOT.

Use Firebase App Hosting for Blazor WASM to leverage global CDN and automated SSL.

Prioritize Cloud Run for the Gateway API to ensure scale-to-zero and cost-efficiency.

4. Guarantee Immutable Compliance:

Enforce the "Immutable Audit Trail" by utilizing Google Cloud Storage with locked retention policies (WORM).

Every state-changing artifact must be verified against the SHA256 hashing logic defined in the Shared Kernel.

Your role:
Act as the expert in Serverless Infrastructure, Cloud Security, and Frictionless Deployment.

Automatically generate firebase.json, firestore.rules, and GitHub Action workflows that strictly mirror the AHS.SaaS monorepo structure.

Validate that the cloud environment is secure, compliant (GxP/SS), and optimized for the "Deep Blue" UI performance.



## 🧩 PHASE 7 — Instructions for M365 Copilot (Documentation & Concept Alignment)

M365 Copilot supports the AHS.SaaS ecosystem by generating, validating, and maintaining all 
documentation, diagrams, and conceptual standards. Use Copilot for:

### 1. Documentation
- Create and maintain Markdown documentation (ARCHITECTURE.md, STRUCTURE.md, CONVENTIONS.md).
- Write BC-level READMEs, design notes, and technical summaries.
- Produce clear explanations of domain models, policies, and events.

### 2. Diagramming
- Generate Corporate Blue–style diagrams.
- Produce Mermaid diagrams (sequence, class, state, architecture).
- Produce C4 diagrams (Context, Container, Component) for Suites and BCs.

### 3. Use-case and Requirements Support
- Generate use-case templates, acceptance criteria, and workflow descriptions.
- Rewrite requirements for clarity, consistency, and alignment with DDD.

### 4. Validation & Policies
- Document validation rules and domain policies.
- Provide guidance on naming conventions, folder structures, and patterns.
- Verify conceptual alignment across Suites and BCs.

### 5. Conceptual Consistency
- Ensure documentation and design remain consistent across all BCs.
- Help unify terminology (Ubiquitous Language).
- Identify inconsistencies, redundancies, or conceptual conflicts.

### 6. Export and Formatting
- Generate formatted PDF and DOCX documents.
- Create summarized or extended versions of any documentation.
- Provide print‑ready or NotebookLM‑ready formats.

***

## 🎯 **PHASE 8 — Final Expected Outcome**
By the end you will have:  
- a **single monorepo** (AHS.SaaS)  
- with **4 clear suites**  
- with **well‑defined domain BCs**  
- with **Clean Architecture** inside each BC  
- with **DDD Strategic** applied across the monorepo  
- with a **reusable Generic Core Domain**  
- with **industry‑specific policies**  
- with a **cross‑cutting platform**  
- with a **minimal, elegant Shared Kernel**  

This positions you as:  
🚀 a **one‑person unicorn** capable of delivering SaaS and micro‑SaaS solutions by reusing your base.

