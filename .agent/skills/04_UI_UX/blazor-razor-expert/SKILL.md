---
name: blazor-razor-expert
description: >
  Expert guidance on Blazor (Server, WebAssembly, and .NET 8/10 Auto/Web), Razor Pages,
  and Razor components in C#. Use this skill whenever the user mentions Blazor, Razor
  components, .razor files, RenderFragment, EventCallback, IJSRuntime, cascading parameters,
  component lifecycle, StateHasChanged, Blazor routing, forms (EditForm, InputBase),
  virtualization, QuickGrid, SignalR in Blazor, Blazor WASM, InteractiveServer,
  InteractiveWebAssembly, PersistentState, ValidatableType, JS constructor interop,
  ReconnectModal, HideColumnOptionsAsync, RowClass, InputHidden, or Razor Pages.
  Trigger on: Blazor, .razor, RenderFragment, EditForm, IJSRuntime, StateHasChanged,
  cascading value, component lifecycle, Blazor Server, Blazor WASM, InteractiveServer,
  InteractiveWebAssembly, QuickGrid, PersistentState, ValidatableType.
---

# Blazor & Razor — Expert Reference (.NET 8 / .NET 10 LTS)

## ⚡ .NET 10 — Novedades Clave (LTS, 3 años de soporte)

> .NET 10 es LTS (como .NET 8). Todo lo de .NET 8 sigue funcionando. Lo nuevo es aditivo.

### AOT en Blazor — Estado en .NET 10

| Modo | AOT soporte |
|---|---|
| Blazor WebAssembly | ✅ Totalmente compatible con `PublishAot=true` |
| Blazor Server | ⚠️ AOT no aplica (corre en servidor con JIT) |
| Blazor Auto | ✅ La parte WASM puede publicarse AOT |
| Razor Pages / MVC | ⚠️ Experimental — ILLink trim sí, Native AOT limitado |

```xml
<!-- .csproj para WASM AOT -->
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <!-- En WASM, AOT = compilar IL a WASM nativo, no binario nativo del OS -->
</PropertyGroup>
```

**Reglas AOT en Blazor:**
- ✅ `[JsonSerializable]` + `JsonSerializerContext` — obligatorio
- ✅ `[ValidatableType]` (.NET 10) — source-gen, AOT-safe
- ✅ `IJSRuntime` / `IJSObjectReference` — AOT-safe
- ✅ `DotNetObjectReference` + `[JSInvokable]` — AOT-safe
- ❌ `JsonSerializer.Serialize<T>()` sin contexto — rompe en AOT
- ❌ `Assembly.GetTypes()` / `Activator.CreateInstance()` — rompe en AOT
- ❌ `AutoMapper`, `Castle DynamicProxy` — no AOT

```csharp
// En Blazor WASM AOT: registrar todos los tipos JSON
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(List<Order>))]
[JsonSerializable(typeof(ApiResponse<Order>))]
public partial class AppJsonContext : JsonSerializerContext { }

// En Program.cs (WASM)
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));
```

### [PersistentState] — Elimina el problema del doble render

```csharp
// ANTES (.NET 8) — 25+ líneas de boilerplate con PersistentComponentState
// AHORA (.NET 10) — un atributo
@inject IOrderService OrderService

@code {
    // Se fetcha UNA vez en prerender, se serializa en el HTML,
    // y se restaura automáticamente en el render interactivo.
    // También persiste el estado entre reconexiones de circuito.
    [PersistentState]
    public List<Order>? Orders { get; set; }

    [PersistentState]
    public OrderSortState? SortState { get; set; }  // sobrevive desconexiones

    protected override async Task OnInitializedAsync()
    {
        Orders ??= await OrderService.GetAllAsync();  // ??= evita re-fetch
    }
}
```

### JS Interop enriquecido — constructores y propiedades directas

```csharp
// .NET 10: instanciar clases JS y leer/escribir propiedades
var chart = await JS.InvokeConstructorAsync("Chart", canvasRef, options);
var count = await JS.GetValueAsync<int>("myApp.itemCount");
await JS.SetValueAsync("myApp.theme", "dark");

// Transferencia de byte[] — más eficiente en WASM (.NET 10)
// ByteArrayContent ahora usa transferencia binaria directa (no Base64)
await JS.InvokeVoidAsync("processBinary", largeByteArray);
```

### Validación con objetos anidados — [ValidatableType]

```csharp
// .NET 10: source-gen based — AOT safe, sin reflection
// Requiere: builder.Services.AddValidation()  en Program.cs

[ValidatableType]   // marca la clase para el source generator
public class OrderRequest
{
    [Required] public string CustomerName { get; set; } = "";
    [Range(1, 10000)] public decimal Total { get; set; }
    [ValidatableType]  // ← también en tipos anidados
    public required Address ShippingAddress { get; set; }
    public List<OrderItem> Items { get; set; } = [];  // valida colecciones
}

// En EditForm funciona automáticamente con AddValidation()
// [ValidatableType] es experimental — suprimir: #pragma warning disable BLEX0001
```

### QuickGrid .NET 10 — RowClass y HideColumnOptionsAsync

```razor
@* RowClass: estilo condicional por fila *@
<QuickGrid Items="@Orders" Virtualize="true" RowClass="GetRowClass">
    <PropertyColumn Property="@(o => o.Id)" Sortable="true" />
    <PropertyColumn Property="@(o => o.Status)" Sortable="true" />
    <TemplateColumn Title="Acciones">
        <button @onclick="@(() => CloseFilter())">Cerrar filtro</button>
    </TemplateColumn>
</QuickGrid>

@code {
    private string GetRowClass(Order o) => o.IsExpired ? "row-expired" : "";

    // .NET 10: cerrar el panel de opciones de columna programáticamente
    private QuickGrid<Order> _grid = default!;
    private async Task CloseFilter() => await _grid.HideColumnOptionsAsync();
}
```

### ReconnectModal — UI de reconexión built-in (.NET 10)

```razor
@* App.razor — reemplaza el viejo div#components-reconnect-modal *@
@* Respeta CSP, personalizable con CSS *@
<ReconnectModal />

@* O personalizado: *@
<ReconnectModal>
    <Reconnecting>
        <div class="reconnect-toast">Reconectando...</div>
    </Reconnecting>
    <Rejected>
        <div class="reconnect-error">Sesión expirada. <a href="">Recargar</a></div>
    </Rejected>
</ReconnectModal>
```

### InputHidden — campo oculto en formularios

```razor
@* .NET 10: nuevo componente built-in *@
<EditForm Model="_model" OnValidSubmit="Submit">
    <InputHidden @bind-Value="_model.TenantId" />
    <InputText @bind-Value="_model.Name" />
    <button type="submit">Guardar</button>
</EditForm>
```

### Estado de circuito persistente — reconexión sin perder estado

```csharp
// Program.cs
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(o =>
    {
        o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
    });

// Con HybridCache → estado va a Redis automáticamente
builder.Services.AddHybridCache();
```

### NotFound routing nativo

```razor
@* .NET 10: plantilla incluye NotFound.razor por defecto *@
@* Router.razor *@
<Router AppAssembly="typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
    </Found>
    <NotFound>
        <NotFoundPage />   @* componente estándar o custom *@
    </NotFound>
</Router>
```

### Breaking Change WASM .NET 10

```csharp
// ❌ .NET 10: HttpContent.ReadAsStreamAsync() ya NO devuelve MemoryStream
// Devuelve BrowserHttpReadStream (no soporta Read() síncrono)
var stream = await response.Content.ReadAsStreamAsync();
stream.Read(buffer, 0, count);  // ← CRASH en .NET 10 WASM

// ✅ Opción A: usar async
await stream.ReadAsync(buffer, 0, count);

// ✅ Opción B: copiar a MemoryStream si necesitas síncrono
var ms = new MemoryStream();
await stream.CopyToAsync(ms);
ms.Position = 0;

// ✅ Opción C: deshabilitar streaming global
// <WasmEnableStreamingResponse>false</WasmEnableStreamingResponse>
```

---

## Render Mode Quick Reference (.NET 8/10 — igual)

```razor
@* Static — no interactivity, no connection *@
@rendermode null

@* Server — SignalR, state on server *@
@rendermode InteractiveServer

@* WASM — runs in browser, larger initial load *@
@rendermode InteractiveWebAssembly

@* Auto — WASM initially served via Server, then switches to WASM after download *@
@rendermode InteractiveAuto

@* Per-component (set on component tag) *@
<MyCounter @rendermode="InteractiveServer" />
```

---

## 1. Component Lifecycle

```csharp
@code {
    // 1. Parameters set (on every render if parent re-renders)
    protected override void OnParametersSet() { }
    protected override async Task OnParametersSetAsync() { }

    // 2. Initialization (once)
    protected override void OnInitialized() { }
    protected override async Task OnInitializedAsync()
    {
        // ✅ Load data here — safe for prerendering
        Data = await DataService.GetAsync();
    }

    // 3. After render (DOM available from here)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // ✅ JS interop only here — DOM exists now
            await JS.InvokeVoidAsync("initChart", _chartRef);
        }
    }

    // 4. Cleanup
    public void Dispose() { /* unsubscribe events */ }
    public async ValueTask DisposeAsync() { /* async cleanup */ }
}
```

---

## 2. Component Communication Patterns

### Parent → Child: Parameters

```razor
@* Child.razor *@
@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }  // <slot>
    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
    [Parameter] public EventCallback<string> OnSelected { get; set; }
    [CascadingParameter] public ThemeProvider? Theme { get; set; }
}

@* Parent *@
<Child Title="Hello" OnSelected="HandleSelected">
    <p>Content goes here</p>
</Child>
```

### Child → Parent: EventCallback

```csharp
// Child raises event
await OnSelected.InvokeAsync(selectedValue);

// Parent handles
private void HandleSelected(string value) => _selection = value;
private async Task HandleSelectedAsync(string value)
{
    _selection = value;
    await SaveAsync(); // EventCallback<T> automatically calls StateHasChanged
}
```

### Cascading Values

```razor
@* App.razor — provide theme to entire tree *@
<CascadingValue Value="@_theme" Name="AppTheme">
    <Router ...>...</Router>
</CascadingValue>

@* Anywhere in tree *@
@code {
    [CascadingParameter(Name = "AppTheme")] public AppTheme Theme { get; set; } = default!;
}
```

---

## 3. State Management

### Component State

```csharp
// Simple — StateHasChanged is automatic after EventCallback
private int _count = 0;
private void Increment() => _count++;  // triggers re-render automatically in EventCallback

// Manual re-render (e.g., timer, external event)
private void OnExternalUpdate()
{
    _data = newData;
    InvokeAsync(StateHasChanged); // thread-safe
}
```

### App-Level State Service

```csharp
// Scoped service = one instance per Blazor circuit (Server) or session (WASM)
public class AppState
{
    public event Action? OnChange;
    private string _currentUser = "";

    public string CurrentUser
    {
        get => _currentUser;
        set { _currentUser = value; NotifyStateChanged(); }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Component subscribes
@inject AppState State
@implements IDisposable

@code {
    protected override void OnInitialized()
        => State.OnChange += () => InvokeAsync(StateHasChanged);

    public void Dispose()
        => State.OnChange -= () => InvokeAsync(StateHasChanged);
}
```

---

## 4. Forms — EditForm / InputBase

```razor
<EditForm Model="@_model" OnValidSubmit="HandleSubmit" FormName="OrderForm">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div class="field">
        <label>Customer</label>
        <InputText @bind-Value="_model.CustomerName" class="input" />
        <ValidationMessage For="@(() => _model.CustomerName)" />
    </div>

    <InputSelect @bind-Value="_model.Status">
        @foreach (var status in Enum.GetValues<OrderStatus>())
        {
            <option value="@status">@status</option>
        }
    </InputSelect>

    <InputDate @bind-Value="_model.DeliveryDate" />
    <InputNumber @bind-Value="_model.Quantity" />
    <InputCheckbox @bind-Value="_model.IsUrgent" />

    <button type="submit">Submit</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    private OrderModel _model { get; set; } = new();

    private async Task HandleSubmit()
    {
        await OrderService.CreateAsync(_model);
    }
}
```

### Custom InputBase<T>

```csharp
public class InputTemperature : InputBase<double>
{
    protected override bool TryParseValueFromString(string? value, out double result,
        out string validationErrorMessage)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            validationErrorMessage = "";
            return true;
        }
        validationErrorMessage = "Invalid temperature value.";
        return false;
    }
}
```

---

## 5. JS Interop

```csharp
@inject IJSRuntime JS

// Call JS from C#
await JS.InvokeVoidAsync("console.log", "Hello from Blazor");
var result = await JS.InvokeAsync<string>("getLocalStorage", "key");

// Efficient: IJSObjectReference (module isolation)
private IJSObjectReference? _module;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/chart.js");
}

await _module!.InvokeVoidAsync("initChart", _chartRef, _data);

// Call C# from JS (DotNetObjectReference)
var dotNetRef = DotNetObjectReference.Create(this);
await JS.InvokeVoidAsync("registerCallback", dotNetRef);

[JSInvokable]
public void OnDataReceived(string data) { ... }
```

---

## 6. Performance

### Virtualization

```razor
<Virtualize Items="@_largeList" Context="item" OverscanCount="5">
    <ItemContent>
        <OrderRow Order="@item" />
    </ItemContent>
    <Placeholder>
        <div class="skeleton-row"></div>
    </Placeholder>
</Virtualize>

@* With async data source *@
<Virtualize ItemsProvider="@LoadItems" ItemSize="50">
    <ItemContent Context="order">...</ItemContent>
</Virtualize>

@code {
    private async ValueTask<ItemsProviderResult<Order>> LoadItems(ItemsProviderRequest request)
    {
        var result = await OrderService.GetPagedAsync(request.StartIndex, request.Count, request.CancellationToken);
        return new(result.Items, result.TotalCount);
    }
}
```

### Prevent Unnecessary Re-Renders

```csharp
// Override ShouldRender for expensive components
protected override bool ShouldRender()
    => _isDirty;  // only re-render when data actually changed

// Use @key to help diff algorithm
@foreach (var order in Orders)
{
    <OrderRow @key="order.Id" Order="@order" />
}
```

### QuickGrid (built-in, virtualized data grid)

```razor
@using Microsoft.AspNetCore.Components.QuickGrid

<QuickGrid Items="@FilteredOrders" Virtualize="true" >
    <PropertyColumn Property="@(o => o.Id)" Sortable="true" />
    <PropertyColumn Property="@(o => o.CustomerName)" Title="Customer" Sortable="true" />
    <PropertyColumn Property="@(o => o.Total)" Format="C2" Sortable="true" />
    <TemplateColumn Title="Actions">
        <button @onclick="@(() => Edit(context))">Edit</button>
    </TemplateColumn>
</QuickGrid>
```

---

## 7. Routing

```razor
@page "/orders"
@page "/orders/{TenantSlug}"

@code {
    [Parameter] public string? TenantSlug { get; set; }
    [SupplyParameterFromQuery] public string? Filter { get; set; }  // ?filter=pending
}

@* Programmatic navigation *@
@inject NavigationManager Nav
Nav.NavigateTo($"/orders/{id}");
Nav.NavigateTo("/login", forceLoad: false);
```

---

## 8. Blazor Server vs WASM Tradeoffs

| | Blazor Server | Blazor WASM | Auto |
|---|---|---|---|
| State | Server-side | Browser | Hybrid |
| Startup | Fast | Slow (WASM download) | Fast initially |
| Offline | No | Yes | Partial |
| Latency | SignalR ping | None | Depends |
| Memory | Server RAM | Browser RAM | Both |
| Access to .NET | Full | Full | Full |
| JS libs | Via interop | Via interop | Via interop |
| PersistentState | ✅ | ✅ | ✅ |
| Circuit persistence | ✅ .NET 10 | N/A | ✅ |
| WASM bundle size | N/A | ~43KB JS (.NET 10, -76%) | Aplica |

---

## 9. Authentication in Blazor

```razor
<AuthorizeView Policy="SameTenant">
    <Authorized>
        <p>Welcome, @context.User.Identity!.Name!</p>
    </Authorized>
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
</AuthorizeView>

@* In code *@
@attribute [Authorize(Policy = "SameTenant")]
@code {
    [CascadingParameter] private Task<AuthenticationState> AuthState { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        var user  = state.User;
    }
}
```
