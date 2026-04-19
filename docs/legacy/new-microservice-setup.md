# Workflow: Create New MicroSaaS Service

ID: W-001 (Version: 2026.1 - Monorepo Optimized)

1. Analyze
Check the docs/SUITES.md or BLUEPRINT.md to identify the correct family (Suite) for the new service and ensure port/namespace alignment.

2. Scaffold
Use the automation scripts located in the /scripts folder to create the physical directory structure under:
C:\Users\armando\Documents\_AHS\projects\AHS.SaaS\src\suites\[SuiteName]\[BoundedContextName].

3. Configure (The .NET 10 Standard)
Create the .csproj file ensuring it targets .NET 10 and explicitly enables Native AOT to maintain ecosystem performance:

XML
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishAot>true</PublishAot> </PropertyGroup>
</Project>
4. Register
Add the new project to the root solution file AHS.Platform.slnx using the XML Solution Folder format.

Core Logic: Must reside in src/shared-kernel or src/common-application.

Infrastructure: Must reside in src/platform.

5. Validation
Run the dotnet build command and ensure the project is recognized by the Native AOT analyzer without warnings.