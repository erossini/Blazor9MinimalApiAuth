# Protected Minimal API with Individual Accounts

This is a basic project in `NET9` and `Blazor` with minimal APIs protected by *Individual Accounts*. Create a new project and the authentication has to be with *Individual Accounts*.

The version of `NET8` is available in this [repository](https://github.com/erossini/Blazor8MinimalApiAuth).

## Scenario

In this project, I want to display and edit a client record and the address. All the interactions with the database must be through the APIs and 
the APIs require the user authentication in order to return a valid list of data.

## Step 1

The solution created out-of-the-box from the Visual Studio template is not creating the structure for the Identity in the database. 
So, from the _Package Manager Console_ run the command

```powershell
update-database
```

### Migration from the code

The idea for this repository is to have a full working solution with some APIs protected with the *Individual Accounts* to use as a template project.
For this reason, I don't want to run any command but the application has to sort it out by itself. 

In the _Program.cs_, I am going to add the code to verify if the database is created and if the migration are applied. 
This code has to be added after the creation of the `app`.

```csharp
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    var context = service.GetService<ApplicationDbContext>();

    try
    {
        context?.Database?.EnsureCreated();
        context?.Database?.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}
```

With this code, the application will check every time if there is any update to the structure to apply and automatically run the code.

### Add logs with Serilog

In the server project, I add _Serilog_ for the logs and configure to save them in the `logs` folder with a generic name that contains the date.

```csharp
Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/net9demo-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

builder.Services.AddSerilog();
```

## Step 2

In this next step, I want to add the connection with the database and add the APIs to save and retrieve data from the database using the APIs.
To achieve that, I explain in small parts every single activity or project and then how to put everything together.

I like to build the project using [Clean Architecture](https://puresourcecode.com/dotnet/net-core/architecting-asp-net-core-applications/). 
I have created a few posts where I discussed how to do it with [ASP.NET Core](https://puresourcecode.com/tag/aspnet-core/).
Using this concepts, I am going to create this project.

### Add Entity Framework Core

The solution uses the database for saving the data. For this reason, we need *Entity Framework Core* to be added to several projects.
In the server project _MinimalApiAuth_, add the following packages:

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools

I will use those packages in the other projects in this solution.

### Add the Domain layer

The *Domain* layer is where I define the basic models for the data I will use across the application. Usually, a project based on `.NET Standard` is created.
For this project, the models will be used only in other `NET9` projects: so, I use `NET9` for all the projects.

I create a new project called `MinimalApiAuth.Domain` and then I add 2 new classes:

- *Client*: this is the model for the basic data for a client
- *ClientAddress*: each client can have one or more address

Each of those model, starts with the *Table* attribute to name the table in the database. Using `System.Text.Json`, I define the name of each
field in the `json`.

Now, I have to add some NuGet dependencies to the solution in order to add *Entity Framework Core* version `9.0.1`.

### Add the Persistence layer

The *Persistence* layer defines the connections with the database and everything related to that. I create a new project called `MinimalApiAuth.Persistence`.
For the purpose of this project, I am going to add a new project where I define the `DbContext` using *Entity Framework Core* and 
the registration of the context to use in the main project.

For this reason, I add the package `Microsoft.Extensions.Configuration` to read the configuration for the main project. Also, this project depends on the *Domain* project.
Also, this project is added as dependency in the main project `MinimalApiAuth`.

#### AddDbContext

Here is where I define the tables based on *Entity Framework Core*. Every table is based on the *Domain* models.

#### PersistenceServiceRegistration

This is an extension for `IServiceCollection` to add the configuration for the database. Using this extension, in the _Program.cs_ of the main project _MinimalApiAuth_,
I can add this line

```csharp
builder.Services.AddAppPersistenceServices(builder.Configuration);
```

to add include the settings in the dependency injection of the solution. Using the extension for *Entity Framework Core*, I read the connection configuration for the _appsettings.json_
and configure the SQL Server connection. The default key is `DefaultConnection` created by default.

### Add Migration

Now, I can create the migration for this database and its tables. Because I want to creat the `Migrations` folder and files in the `Persistence` project,
I add a new file called `AppDbContextFactory` to set the default connection only for the creation of the migration. Once the migration is created, I have to *Exclude* this file from the project.
If I have to create a new migration, I will include the file in the project again.

After that, I open the *Package Manager Console* and run the following command:

```powershell
add-migration InitialMigration -Context AppDbContext
```

After a successful building for the solution, this is creating the migration for the `AppDbContext`. In order to automatically run the migration form the main project,
I add the code to check if there is any pending migration in the _Program.cs_ or the main project as I did for the `ApplicationDbContext` above.

## Add endpoints

Using *Scaffolded Item* in Visual Studio, I add the first endpoint for `Client`. This is going to create a minimal API for the mayor HTTP verbs.
In the code, I add the `RequireAuthorization` to allow only authorized users to use the APIs. An example of the is the `GET` for the `Clients`:

```csharp
group.MapGet("/", async (AppDbContext db) =>
{
    return await db.Clients.ToListAsync();
})
.RequireAuthorization()
.WithName("GetAllClients")
.WithOpenApi();
```

If I want to add another endpoint for the `ClientAddress`, the *Scaffolded Item* procedure fails because there is a conflict with the one I have already created.
So, I'm going to comment in the _Program.cs_ of the server project

```csharp
app.MapClientEndpoints();
```

After that, I can use the procedure and create a new minimal APIs. After the creation, I can remove the comments and the application will work normally.

---
    
## PureSourceCode.com

[PureSourceCode.com](https://www.puresourcecode.com/) is my personal blog where I publish posts about technologies and in particular source code and projects in [.NET](https://www.puresourcecode.com/category/dotnet/). 

In the last few months, I created a lot of components for [Blazor WebAssembly](https://www.puresourcecode.com/tag/blazor-webassembly/) and [Blazor Server](https://www.puresourcecode.com/tag/blazor-server/).

My name is Enrico Rossini and you can contact me via:
- [Personal Twitter](https://twitter.com/erossiniuk)
- [LinkedIn](https://www.linkedin.com/in/rossiniuk)
- [Facebook](https://www.facebook.com/puresourcecode)

## Blazor Components

| Component name | Forum | NuGet | Website | Description |
| --- | --- | --- | --- | --- |
| AnchorLink | [Forum](https://puresourcecode.com/forum/anchorlink/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.AnchorLink) |     | An anchor link is a web link that allows users to leapfrog to a specific point on a website page. It saves them the need to scroll and skim read and makes navigation easier. This component is for [Blazor WebAssembly](https://www.puresourcecode.com/tag/blazor-webassembly/) and [Blazor Server](https://www.puresourcecode.com/tag/blazor-server/) |
| [Autocomplete for Blazor](https://www.puresourcecode.com/dotnet/net-core/autocomplete-component-for-blazor/) | [Forum](https://www.puresourcecode.com/forum/autocomplete-blazor/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Autocomplete) |     | Simple and flexible autocomplete type-ahead functionality for [Blazor WebAssembly](https://www.puresourcecode.com/tag/blazor-webassembly/) and [Blazor Server](https://www.puresourcecode.com/tag/blazor-server/) |
| [Browser Detect for Blazor](https://www.puresourcecode.com/dotnet/blazor/browser-detect-component-for-blazor/) | [Forum](https://www.puresourcecode.com/forum/browser-detect-for-blazor/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.BrowserDetect) | [Demo](https://browserdetect.puresourcecode.com) | Browser detect for Blazor WebAssembly and Blazor Server |
| [ChartJs for Blazor](https://www.puresourcecode.com/dotnet/blazor/blazor-component-for-chartjs/) | [Forum](https://www.puresourcecode.com/forum/chart-js-for-blazor/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Chartjs) | [Demo](https://chartjs.puresourcecode.com/) | Add beautiful graphs based on ChartJs in your Blazor application |
| [Clippy for Blazor](https://www.puresourcecode.com/dotnet/blazor/blazor-component-for-chartjs/) | [Forum](https://www.puresourcecode.com/forum/clippy/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Clippy) | [Demo](https://clippy.puresourcecode.com/) | Do you miss Clippy? Here the implementation for Blazor |
| [CodeSnipper for Blazor](https://www.puresourcecode.com/dotnet/blazor/code-snippet-component-for-blazor/) | [Forum](https://www.puresourcecode.com/forum/codesnippet-for-blazor/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.CodeSnippet) |     | Add code snippet in your Blazor pages for 196 programming languages with 243 styles |
| [Copy To Clipboard](https://www.puresourcecode.com/dotnet/blazor/copy-to-clipboard-component-for-blazor/) | [Forum](https://www.puresourcecode.com/forum/copytoclipboard/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.CopyToClipboard) |     | Add a button to copy text in the clipboard |
| [DataTable for Blazor](https://www.puresourcecode.com/dotnet/net-core/datatable-component-for-blazor/) | [Forum](https://www.puresourcecode.com/forum/forum/datatables/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.DataTable) | [Demo](https://datatable.puresourcecode.com/) | DataTable component for Blazor WebAssembly and Blazor Server |
| Google Tag Manager | \[Forum\]() | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.GoogleTagManager) | [Demo](https://datatable.puresourcecode.com/) | Adds Google Tag Manager to the application and manages communication with GTM JavaScript (data layer). |
| [Icons and flags for Blazor](https://www.puresourcecode.com/forum/icons-and-flags-for-blazor/) | [Forum](https://www.puresourcecode.com/forum/icons-and-flags-for-blazor/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Icons) |     | Library with a lot of SVG icons and SVG flags to use in your Razor pages |
| ImageSelect for Blazor | [Forum](https://puresourcecode.com/forum/imageselect/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.ImageSelect) |     | This is a Blazor component to display a dropdown list with images based on ms-Dropdown by Marghoob Suleman. This component is built with NET7 for [Blazor WebAssembly](https://www.puresourcecode.com/tag/blazor-webassembly/) and [Blazor Server](https://www.puresourcecode.com/tag/blazor-server/) |
| [Markdown editor for Blazor](https://www.puresourcecode.com/dotnet/blazor/markdown-editor-with-blazor/) | [Forum](https://puresourcecode.com/forum/markdowneditor/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.MarkdownEditor) | [Demo](https://markdown.puresourcecode.com/) | This is a Markdown Editor for use in Blazor. It contains a live preview as well as an embeded help guide for users. |
| [Modal dialog for Blazor](https://puresourcecode.com/dotnet/blazor/modal-dialog-component-for-blazor/) | [Forum](https://puresourcecode.com/forum/modaldialog/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.ModalDialog) |     | Simple Modal Dialog for Blazor WebAssembly |
| [Modal windows for Blazor](https://www.puresourcecode.com/dotnet/blazor/modal-dialog-component-for-blazor/) | [Forum](https://puresourcecode.com/forum/modaldialog/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Modals) |     | Modal Windows for Blazor WebAssembly |
| [Quill for Blazor](https://www.puresourcecode.com/dotnet/blazor/create-a-blazor-component-for-quill/) | [Forum](https://puresourcecode.com/forum/blazor-components/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Quill) |     | Quill Component is a custom reusable control that allows us to easily consume Quill and place multiple instances of it on a single page in our Blazor application |
| [ScrollTabs](https://www.puresourcecode.com/dotnet/blazor/scrolltabs-component-for-blazor/) |     | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.ScrollTabs) |     | Tabs with nice scroll (no scrollbar) and responsive |
| [Segment for Blazor](https://www.puresourcecode.com/dotnet/blazor/segment-control-for-blazor/) | [Forum](https://puresourcecode.com/forum/segments/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Segments) |     | This is a Segment component for Blazor Web Assembly and Blazor Server |
| [Tabs for Blazor](https://www.puresourcecode.com/dotnet/blazor/tabs-control-for-blazor/) | [Forum](https://puresourcecode.com/forum/tabs/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Tabs) |     | This is a Tabs component for Blazor Web Assembly and Blazor Server |
| [Timeline for Blazor](https://www.puresourcecode.com/dotnet/blazor/timeline-component-for-blazor/) | [Forum](https://puresourcecode.com/forum/timeline/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Timeline) |     | This is a new responsive timeline for Blazor Web Assembly and Blazor Server |
| [Toast for Blazor](https://www.puresourcecode.com/forum/psc-components-and-source-code/) | [Forum](https://puresourcecode.com/forum/blazor-components/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Toast) |     | Toast notification for Blazor applications |
| [Tours for Blazor](https://www.puresourcecode.com/forum/psc-components-and-source-code/) | [Forum](https://puresourcecode.com/forum/blazor-components/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.Tours) |     | Guide your users in your Blazor applications |
| TreeView for Blazor | [Forum](https://puresourcecode.com/forum/treeview/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.TreeView) |     | This component is a native Blazor TreeView component for [Blazor WebAssembly](https://www.puresourcecode.com/tag/blazor-webassembly/) and [Blazor Server](https://www.puresourcecode.com/tag/blazor-server/). The component is built with .NET7. |
| [WorldMap for Blazor](https://puresourcecode.com/dotnet/blazor/world-map-component-for-blazor) | [Forum](https://puresourcecode.com/forum/worldmap/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Blazor.Components.WorldMap) | [Demo](https://worldmap.puresourcecode.com/) | Show world maps with your data |

## C# libraries for .NET6

| Component name | Forum | NuGet | Description |
|---|---|---|---|
| [PSC.Evaluator](https://www.puresourcecode.com/forum/psc-components-and-source-code/) | [Forum](https://www.puresourcecode.com/forum/forum/psc-extensions/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Evaluator) | PSC.Evaluator is a mathematical expressions evaluator library written in C#. Allows to evaluate mathematical, boolean, string and datetime expressions. |
| [PSC.Extensions](https://www.puresourcecode.com/dotnet/net-core/a-lot-of-functions-for-net5/) | [Forum](https://www.puresourcecode.com/forum/forum/psc-extensions/) | ![NuGet badge](https://img.shields.io/nuget/v/PSC.Extensions) | A lot of functions for .NET5 in a NuGet package that you can download for free. We collected in this package functions for everyday work to help you with claim, strings, enums, date and time, expressions... |

## More examples and documentation

### Blazor
*   [Write a reusable Blazor component](https://www.puresourcecode.com/dotnet/blazor/write-a-reusable-blazor-component/)
*   [Getting Started With C# And Blazor](https://www.puresourcecode.com/dotnet/net-core/getting-started-with-c-and-blazor/)
*   [Setting Up A Blazor WebAssembly Application](https://www.puresourcecode.com/dotnet/blazor/setting-up-a-blazor-webassembly-application/)
*   [Working With Blazor Component Model](https://www.puresourcecode.com/dotnet/blazor/working-with-blazors-component-model/)
*   [Secure Blazor WebAssembly With IdentityServer4](https://www.puresourcecode.com/dotnet/blazor/secure-blazor-webassembly-with-identityserver4/)
*   [Blazor Using HttpClient With Authentication](https://www.puresourcecode.com/dotnet/blazor/blazor-using-httpclient-with-authentication/)
*   [InputSelect component for enumerations in Blazor](https://www.puresourcecode.com/dotnet/blazor/inputselect-component-for-enumerations-in-blazor/)
*   [Use LocalStorage with Blazor WebAssembly](https://www.puresourcecode.com/dotnet/blazor/use-localstorage-with-blazor-webassembly/)
*   [Modal Dialog component for Blazor](https://www.puresourcecode.com/dotnet/blazor/modal-dialog-component-for-blazor/)
*   [Create Tooltip component for Blazor](https://www.puresourcecode.com/dotnet/blazor/create-tooltip-component-for-blazor/)
*   [Consume ASP.NET Core Razor components from Razor class libraries | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries?view=aspnetcore-5.0&tabs=visual-studio)
*   [ChartJs component for Blazor](https://www.puresourcecode.com/dotnet/blazor/blazor-component-for-chartjs/)
*   [Labels and OnClickChart for ChartJs](https://www.puresourcecode.com/dotnet/blazor/labels-and-onclickchart-for-chartjs/)

### Blazor & NET8
* [Custom User Management with NET8 and Blazor (1st part)](https://puresourcecode.com/dotnet/blazor/custom-user-management-with-net8-and-blazor/)
* [NET8, Blazor and Custom User Management (2nd part)](https://puresourcecode.com/dotnet/blazor/net8-blazor-and-custom-user-management/)
