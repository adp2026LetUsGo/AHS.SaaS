# Target root folder inside your workspace
$root = "C:\Users\armando\Documents\_AHS\projects\AHS.SaaS\dotnet10-rag"

# Create root directory
New-Item -ItemType Directory -Path $root -Force | Out-Null

# Helper function to create files
function Write-File($path, $content) {
    $folder = Split-Path $path
    if (!(Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }
    Set-Content -Path $path -Value $content -Encoding UTF8
}

# -------------------------
# 00-Index
# -------------------------
Write-File "$root\00-Index\index.md" @"
# .NET 10 RAG Dataset — Navigation Index

This dataset contains structured, chunk-ready documentation for the major components of .NET 10.
Excluded technologies: Visual Basic, F#, Windows Forms, WPF, and Windows-specific MAUI APIs.

## 1. Runtime
- JIT
- GC
- NativeAOT
- Performance

## 2. Libraries
- Networking
- Collections
- Numerics
- Security

## 3. SDK
- CLI
- Containers
- AOT
- Tooling

## 4. C# 14
- field keyword
- Extension blocks
- Null-conditional assignment
- Language enhancements

## 5. ASP.NET Core
- Hosting
- Minimal APIs
- Blazor
- Authentication
- Performance

## 6. EF Core
- Performance
- Interceptors
- Mapping
- Tooling

## 7. Aspire
- Components
- Orchestration
- Observability

## 8. MAUI (Cross-platform only)
- UI
- Handlers
- Shell
- Essentials
- Performance
"@

# -------------------------
# 01-Runtime
# -------------------------
Write-File "$root\01-Runtime\overview.md" @"
# .NET 10 Runtime — Overview

The .NET 10 runtime introduces performance improvements, expanded hardware support, and enhancements to the JIT, GC, and NativeAOT toolchain.
"@

Write-File "$root\01-Runtime\jit.md" @"
# JIT Improvements in .NET 10

The JIT compiler in .NET 10 delivers better throughput and more aggressive optimizations.
"@

Write-File "$root\01-Runtime\nativeaot.md" @"
# NativeAOT in .NET 10

NativeAOT continues to mature in .NET 10, offering smaller binaries and faster startup times.
"@

Write-File "$root\01-Runtime\gc.md" @"
# Garbage Collection Improvements in .NET 10

The GC in .NET 10 focuses on reducing latency and improving memory efficiency.
"@

Write-File "$root\01-Runtime\performance.md" @"
# Runtime Performance in .NET 10

.NET 10 delivers broad performance improvements across the runtime.
"@

# -------------------------
# 02-Libraries
# -------------------------
Write-File "$root\02-Libraries\overview.md" @"
# .NET 10 Libraries — Overview

The .NET 10 libraries introduce improvements across networking, collections, numerics, and security.
"@

Write-File "$root\02-Libraries\networking.md" @"
# Networking Improvements in .NET 10

HTTP/3, sockets, and TLS improvements enhance performance and reliability.
"@

Write-File "$root\02-Libraries\collections.md" @"
# Collections Improvements in .NET 10

Faster dictionary lookups, reduced memory usage, and improved LINQ performance.
"@

Write-File "$root\02-Libraries\numerics.md" @"
# Numerics in .NET 10

SIMD improvements and faster BigInteger operations.
"@

Write-File "$root\02-Libraries\security.md" @"
# Security Enhancements in .NET 10

Cryptography and authentication improvements.
"@

# -------------------------
# 03-SSDK
# -------------------------
Write-File "$root\03-SDK\overview.md" @"
# .NET 10 SDK — Overview

The .NET 10 SDK introduces improvements to the CLI, container tooling, AOT workflows, and developer productivity.
"@

Write-File "$root\03-SDK\cli.md" @"
# CLI Improvements in .NET 10

Faster restore, improved templates, and better diagnostics.
"@

Write-File "$root\03-SDK\containers.md" @"
# Container Tooling in .NET 10

Smaller base images and faster publish-to-container workflows.
"@

Write-File "$root\03-SDK\aot.md" @"
# AOT Tooling in .NET 10

Faster compilation, better trimming, and improved error reporting.
"@

Write-File "$root\03-SDK\tooling.md" @"
# Tooling Enhancements in .NET 10

Better IDE support and faster incremental builds.
"@

# -------------------------
# 04-CSharp14
# -------------------------
Write-File "$root\04-CSharp14\overview.md" @"
# C# 14 — Overview

C# 14 introduces language features designed to reduce ceremony and improve clarity.
"@

Write-File "$root\04-CSharp14\field-keyword.md" @"
# The `field` Keyword in C# 14

Allows direct access to the backing field of an auto-property.
"@

Write-File "$root\04-CSharp14\extension-blocks.md" @"
# Extension Blocks in C# 14

Groups extension members under a single scope.
"@

Write-File "$root\04-CSharp14\null-conditional-assignment.md" @"
# Null-Conditional Assignment in C# 14

Assign only when the target is non-null.
"@

Write-File "$root\04-CSharp14\language-enhancements.md" @"
# Additional Language Enhancements in C# 14

Pattern matching, lambda improvements, and reduced ceremony.
"@

# -------------------------
# 05-ASPNETCore
# -------------------------
Write-File "$root\05-ASPNETCore\overview.md" @"
# ASP.NET Core in .NET 10 — Overview

ASP.NET Core in .NET 10 focuses on performance, simplified hosting, and improved developer experience.
"@

Write-File "$root\05-ASPNETCore\hosting.md" @"
# Hosting Improvements in ASP.NET Core

Faster startup, simplified configuration, and better diagnostics.
"@

Write-File "$root\05-ASPNETCore\minimal-apis.md" @"
# Minimal APIs in .NET 10

Better routing, validation, and parameter binding.
"@

Write-File "$root\05-ASPNETCore\blazor.md" @"
# Blazor in .NET 10

Faster rendering, better interactivity, and reduced memory usage.
"@

Write-File "$root\05-ASPNETCore\authentication.md" @"
# Authentication in ASP.NET Core 10

Improved token validation and identity provider integration.
"@

Write-File "$root\05-ASPNETCore\performance.md" @"
# ASP.NET Core Performance in .NET 10

Reduced allocations, faster JSON serialization, and better Kestrel throughput.
"@

# -------------------------
# 06-EFCore
# -------------------------
Write-File "$root\06-EFCore\overview.md" @"
# EF Core in .NET 10 — Overview

EF Core 10 introduces performance improvements, better interceptors, and enhanced mapping.
"@

Write-File "$root\06-EFCore\performance.md" @"
# EF Core Performance Improvements

Faster queries, reduced memory usage, and better batching.
"@

Write-File "$root\06-EFCore\interceptors.md" @"
# EF Core Interceptors in .NET 10

More granular interception points and better diagnostics.
"@

Write-File "$root\06-EFCore\mapping.md" @"
# Mapping Improvements in EF Core 10

Better TPH performance and improved owned types.
"@

Write-File "$root\06-EFCore\tooling.md" @"
# EF Core Tooling in .NET 10

Faster migrations and improved scaffolding.
"@

# -------------------------
# 07-Aspire
# -------------------------
Write-File "$root\07-Aspire\overview.md" @"
# .NET Aspire — Overview

Opinionated orchestration, components, and observability for cloud-native .NET applications.
"@

Write-File "$root\07-Aspire\components.md" @"
# Aspire Components

Prebuilt service components and simplified configuration.
"@

Write-File "$root\07-Aspire\orchestration.md" @"
# Aspire Orchestration

Declarative service composition and environment provisioning.
"@

Write-File "$root\07-Aspire\observability.md" @"
# Aspire Observability

Distributed tracing, metrics, and logging integration.
"@

# -------------------------
# 08-MAUI (Cross-platform only)
# -------------------------
Write-File "$root\08-MAUI\overview.md" @"
# .NET MAUI — Cross-Platform Overview

This section covers only the cross-platform aspects of .NET MAUI.
"@

Write-File "$root\08-MAUI\cross-platform-ui.md" @"
# Cross-Platform UI in MAUI

Unified XAML UI, shared layouts, and native controls.
"@

Write-File "$root\08-MAUI\handlers-common.md" @"
# Common Handlers in MAUI

Cross-platform control handlers with consistent behavior.
"@

Write-File "$root\08-MAUI\shell.md" @"
# MAUI Shell

Unified navigation model with flyouts and tabs.
"@

Write-File "$root\08-MAUI\essentials.md" @"
# MAUI Essentials

Sensors, connectivity, device info, and secure storage.
"@

Write-File "$root\08-MAUI\performance.md" @"
# MAUI Performance in .NET 10

Faster startup, reduced memory usage, and better handler performance.
"@

# -------------------------
# metadata.json
# -------------------------
Write-File "$root\metadata.json" @"
{
  `"dataset`": `"dotnet10-rag`",
  `"version`": `"1.0`",
  `"language`": `"en`",
  `"excluded_sections`": [
    `"Visual Basic`",
    `"F#`",
    `"Windows Forms`",
    `"WPF`",
    `"MAUI Windows-specific`"
  ],
  `"included_sections`": [
    `"Runtime`",
    `"Libraries`",
    `"SDK`",
    `"C# 14`",
    `"ASP.NET Core`",
    `"EF Core`",
    `"Aspire`",
    `"Containers`",
    `"NativeAOT`",
    `"MAUI Cross-platform`",
    `"Tooling`",
    `"Performance`"
  ],
  `"chunking`": {
    `"size_tokens`": 1000,
    `"overlap_tokens`": 150,
    `"method`": `"semantic + structural`"
  }
}
"@
