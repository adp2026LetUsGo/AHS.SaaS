# Rule: .NET 10 & Native AOT Enforcement
**ID:** R-001
**Applies to:** All projects under `src/` (shared-kernel, common-application, platform, suites)

1. **Native AOT:** Every new project must include `<PublishAot>true</PublishAot>` in the `.csproj`.
2. **No Reflection:** Do not use libraries or code patterns that rely on runtime reflection. Use Source Generators for JSON and DI.
3. **Primary Constructors:** Use C# 14 primary constructors for all class-based dependency injection.
4. **Result Types:** Use `Microsoft.AspNetCore.Http.HttpResults` for Minimal API returns to ensure AOT compatibility.
5. **JSON Context:** All serialization must use `JsonSerializerContext` to support Native AOT.