# FreeCRM Base Application

FreeCRM is a fully functioning CRM starter kit built with **ASP.NET Core 9** and **Blazor WebAssembly**.  
It’s designed to be the **“base app”** that you can merge into your own products with minimal conflict resolution.  
The project makes heavy use of **partial classes**, **partial Razor components**, and companion `*.App.*` files so that the core platform can receive updates while your team keeps custom business logic in a clearly separated layer.

---

## 📘 Table of Contents

1. [Why the Project Is Structured This Way](#why-the-project-is-structured-this-way)  
2. [Solution Layout](#solution-layout)  
3. [Getting Started](#getting-started)  
4. [Customizing the Base App](#customizing-the-base-app)  
5. [Utilities Included](#utilities-included)  
   - [Rename Tool](#rename-tool)  
   - [Module Removal Tool](#module-removal-tool)  
6. [Configuration](#configuration)  
7. [Contributing & Staying Up to Date](#contributing-and-staying-up-to-date)

---

## 🧩 Why the Project Is Structured This Way

| Concept | Purpose |
| --- | --- |
| **Partial classes / Razor files** | Core logic lives in the default files, while anything in the companion `.App.` files is reserved for your custom behaviour. You rarely touch the base file, so pulling upstream updates is safer. |
| **`*.App.*` naming** | Anything that ends in `.App.cs`, `.App.razor`, `.appsettings`, etc. is **your customization surface**. The defaults ship with stubbed methods, properties, or markup to help you understand where to plug in. |
| **App-specific methods** | Methods such as `Program.AppModifyStart`, `ConfigurationHelpersLoadApp`, and `LoadData` in the `.App.` components are intentionally empty extension points ready for your code. |
| **Optional modules** | CRM features such as Appointments, Email Templates, Invoices, Locations, Payments, Services, and Tags can be removed with the dedicated tooling so you only ship what you need. |
| **Rename utilities** | The repository includes utilities to rename the solution, regenerate GUIDs, and update namespaces so the base app can become *your* branded app with a single command. |

Because the customization surface is explicitly marked, merging upstream changes generally only requires resolving a small handful of conflicts inside files that contain `.App.` in their name.

---

## 🧱 Solution Layout

```
FreeCRM.sln
├── CRM/                  # ASP.NET Core host for Blazor WebAssembly + REST APIs
├── CRM.Client/           # Blazor WebAssembly UI
├── CRM.DataAccess/       # Data access layer, repositories, external service helpers
├── CRM.DataObjects/      # Shared DTOs and configuration models
├── CRM.EFModels/         # Entity Framework Core models & migrations
├── CRM.Plugins/          # Plugin runtime and sample plugins
├── Rename FreeCRM.exe    # Utility to rename the solution, projects, and namespaces
└── Remove Modules...     # Utility to strip optional modules from the base app
```

Each project mirrors the same customization approach:  
the base implementation ships in files such as `DataAccess.cs` or `Index.razor`,  
and your overrides live beside them in `DataAccess.App.cs` or `Index.App.razor`.

---

## 🚀 Getting Started

### 1. Install prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- A SQL database connection string (`localdb`, SQL Server, or another provider configured in `appsettings.json`)

### 2. Restore packages and build
```bash
dotnet restore
dotnet build
```

### 3. Run the development server

```bash
dotnet run --project CRM/CRM.csproj
```

The Blazor WebAssembly front end will be hosted by the ASP.NET Core backend.
Update `appsettings.Development.json` for local secrets such as OAuth keys or connection strings.

---

## 🧠 Customizing the Base App

1. **Use the `.App.` files first** – Add your application-specific logic, services, and UI elements to the companion files. Examples:

   * `CRM/Program.App.cs` exposes hooks (`AppModifyBuilderStart`, `AppModifyBuilderEnd`, `AppModifyStart`, `AppModifyEnd`) for configuring the ASP.NET Core pipeline.
   * `CRM.Client/Shared/AppComponents/Index.App.razor` contains empty methods such as `LoadData` where you can fetch and display additional information on the home view.
   * `CRM.DataAccess/DataAccess.App.cs` lets you extend the repository without editing the generated base class.

2. **Share code across layers** – Use shared projects (`CRM.DataObjects`, `CRM.Client`, `CRM.DataAccess`) to move common DTOs and helpers so both client and server stay in sync.

3. **Configuration helpers** – Extend `ConfigurationHelper.App.cs` to surface configuration values that the Blazor client can consume without modifying the core helper implementation.

4. **Styling** – Update `CRM.Client/wwwroot/css/site.App.css` to brand the application. The core stylesheet stays untouched so you can accept upstream design tweaks without losing your custom theme.

5. **Extend authentication & authorization** – Populate `AuthenticationPoliciesApp` or inject services inside `Program.AppModifyBuilderStart` to hook into the built-in cookie/JWT pipeline.

6. **Plugins** – Drop compiled assemblies or `.plugin` manifests into the `CRM/Plugins` directory. The server loads them at startup (`Program` configures the plugin loader automatically) and exposes the DI container to your plugin code.

> ⚠️ If you must touch a non-`.App.` file, consider creating a partial class or overriding method inside an `.App.` file instead. That way future merges remain painless.

---

## 🛠 Utilities Included

### ⚙️ Rename Tool

FreeCRM ships with a **Rename Utility** (`Rename FreeCRM.exe`) that can rename the entire solution and regenerate all project GUIDs and namespaces to match your product branding.

You can run the tool interactively or pass the new name directly from the command line:

```bash
"Rename FreeCRM.exe" MyNewProduct
```

This renames all projects, creates new GUIDs, and updates namespaces automatically.

**Example:**

```bash
"Rename FreeCRM.exe" MyNewProjectName
```

---

### 🧹 Module Removal Tool

The **Module Removal Utility** (`Remove Modules from FreeCRM.exe`) lets you strip optional components you don’t need — such as:

> Appointments, EmailTemplates, Invoices, Locations, Payments, Services, or Tags

Run it from the command line using either of these syntaxes:

```bash
"Remove Modules from FreeCRM.exe" remove:Module1,Module2,etc.
"Remove Modules from FreeCRM.exe" keep:Module1,Module2,etc.
```

**Examples:**

```bash
"Remove Modules from FreeCRM.exe" remove:Appointments,Payments
"Remove Modules from FreeCRM.exe" keep:Tags
"Remove Modules from FreeCRM.exe" remove:all
```

Do **not** use spaces in the command-line options.
The recognized module names are:

* Appointments
* EmailTemplates
* Invoices
* Locations
* Payments
* Services
* Tags

If you discover code that should have been removed but wasn’t, please open an issue on GitHub including the **file name** and **line number**.

---

## ⚙️ Configuration

Key settings live in:

* `CRM/appsettings.json`
* `CRM/appsettings.Development.json`

**Important values include:**

| Key                                                                          | Description                                                                                            |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `ConnectionStrings:AppData`                                                  | Database connection string used by `CRM.DataAccess`.                                                   |
| `AzureSignalRurl`                                                            | Enables Azure SignalR integration when populated. Otherwise, the app uses the built-in SignalR server. |
| `LocalModeUrl`, `DatabaseType`, `AllowApplicationEmbedding`, `AnalyticsCode` | Consumed inside `Program` and exposed to components through configuration helpers.                     |
| `GloballyDisabledModules` / `GloballyEnabledModules`                         | Toggle modules without recompiling custom code.                                                        |

---

## 🤝 Contributing and Staying Up to Date

1. Keep your custom changes inside the `.App.` files whenever possible.
2. When upstream changes are published, merge them into your branch — you’ll usually only need to reconcile a handful of `.App.` files.
3. Submit issues or PRs if you find bugs in the base implementation or discover incomplete module removal.

---

This approach lets you treat **FreeCRM** as a **vendor-supplied core platform** while tailoring it to your business with **predictable merge behaviour**.
