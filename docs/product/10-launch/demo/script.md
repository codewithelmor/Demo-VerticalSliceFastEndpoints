# Demo script: VerticalSliceFastEndpoints (repo commit 9a638cc)

Voice: en-US-AndrewNeural (Microsoft neural stock voice, AI-generated, not a clone). About 148 words per minute.
The `Voice-over` column is the spoken text. The captions use the on-screen spelling (see `scenes.json`, field `caption`).
Scenes marked *Scalar* are the real app running on localhost against a throwaway LocalDB database.
Scenes marked *Code viewer* show real repo files. Scenes marked *Terminal replay* show real command output captured earlier, replayed on screen and labelled as such.

## Introduction

### s01 - Meet the project  (J0, 15.6 s voice, 44 words)

- **Screen:** Code viewer: README.md (top)
- **Actions:** Scroll README intro, highlight first paragraph
- **Result held on screen:** README intro visible
- **On-screen text:** Vertical slices, FastEndpoints
- **Voice-over:** Hi, I'm Elmor. This is a .NET 10 web API I built around one idea: keep every feature small, separate, and easy to find. I'll show how it's built, how to run it from scratch, and how AI tooling fits into the workflow.

## Technology stack

### s02 - Technology stack  (J0, 21.6 s voice, 43 words)

- **Screen:** Code viewer: VerticalSliceFastEndpoints.csproj package references
- **Actions:** Highlight each PackageReference as it is named
- **Result held on screen:** csproj with package versions visible
- **On-screen text:** The stack
- **Voice-over:** Here's the stack. .NET 10 and ASP.NET Core. FastEndpoints instead of controllers. FluentValidation for requests. ASP.NET Core Identity with JWT bearer tokens. Entity Framework Core on SQL Server, plus Dapper for one raw query. And Scalar for the API docs.

## Architecture

### s03 - Feature folders  (J1, 17.4 s voice, 35 words)

- **Screen:** Folder tree of Features/ (from git ls-files)
- **Actions:** Highlight Features/Product/CreateProduct
- **Result held on screen:** Four files of one slice highlighted
- **On-screen text:** One folder per feature
- **Voice-over:** The architecture is vertical slices. Instead of folders for controllers, services, and repositories, each feature gets its own folder. Under Features there's Auth, Product, and Users. Each slice holds its endpoint, request, response, and validator.

### s04 - Anatomy of a slice  (J1, 19.9 s voice, 46 words)

- **Screen:** Code viewer: CreateProductEndpoint.cs
- **Actions:** Highlight Configure() lines, then HandleAsync
- **Result held on screen:** Configure and HandleAsync visible
- **On-screen text:** Route, version, role, throttle
- **Voice-over:** Here's the create product slice. In Configure, the endpoint declares everything about itself. The route, the API version, the Admin role, a limit of 20 requests a minute, and its OpenAPI description. HandleAsync maps the request, saves through the DbContext, and returns a 201.

### s05 - Validation beside the endpoint  (J1, 11.3 s voice, 29 words)

- **Screen:** Code viewer: CreateProductValidation.cs
- **Actions:** Highlight RuleFor Title and Description
- **Result held on screen:** Validator rules visible
- **On-screen text:** Validation per slice
- **Voice-over:** Validation sits right next to it. A FluentValidation class says the title is required, and can't pass 50 characters. If a request breaks a rule, the handler never runs.

### s06 - Program.cs wiring  (J1, 27.8 s voice, 58 words)

- **Screen:** Code viewer: Program.cs
- **Actions:** Highlight Identity, JWT, UseFastEndpoints, OpenApiDocument, serializers, Scalar
- **Result held on screen:** Each region highlighted in turn
- **On-screen text:** Program.cs
- **Voice-over:** Program.cs wires the shared pieces. Identity, the SQL Server context, JWT validation, CORS policies, and FastEndpoints. Routes get an api prefix and a version prefix, so you get paths like /api/v1/products. Two OpenAPI documents, split by version, feed Scalar. And a custom serializer adds XML next to JSON.

### s07 - Audit stamps and soft delete  (J1, 18.5 s voice, 49 words)

- **Screen:** Code viewer: Data/Custom/ApplicationDbContext.Custom.cs
- **Actions:** Highlight soft delete block, then audit stamps
- **Result held on screen:** ApplyAuditInformation visible
- **On-screen text:** Audit and soft delete, once
- **Voice-over:** The data layer handles audit and soft delete in one place. On save, the DbContext stamps created and updated dates with the current user's ID. And if you delete a soft-deletable entity, it turns the delete into an update that marks it deleted. Endpoints never do this by hand.

### s08 - Dapper for one read  (J1, 9.8 s voice, 27 words)

- **Screen:** Code viewer: GetUsersEndpoint.cs
- **Actions:** Highlight the SQL and the QueryAsync call
- **Result held on screen:** SQL string and QueryAsync visible
- **On-screen text:** Dapper where raw SQL fits
- **Voice-over:** For one read, I used Dapper instead of EF Core. The users list is a paged raw SQL query, joined to roles, and mapped straight into DTOs.

## Setup from scratch

### s09 - Prerequisites  (J2, 18.6 s voice, 48 words)

- **Screen:** Terminal replay: dotnet --version, global.json, dotnet build
- **Actions:** Show captured output of the commands
- **Result held on screen:** Build succeeded, 0 errors
- **On-screen text:** Prerequisites
- **Voice-over:** Now, setup from scratch. You need the .NET 10 SDK, pinned in global.json, and a SQL Server. I'm using SQL Server LocalDB. The build passes. It does print a few warnings, including a vulnerable transitive package, and I'll come back to that at the end.

### s10 - Configuration  (J2, 22.8 s voice, 54 words)

- **Screen:** Code viewer: appsettings.json and appsettings.Development.json
- **Actions:** Highlight ConnectionStrings, ApiSettings, DatabaseInitializer
- **Result held on screen:** Config visible, secret masked
- **On-screen text:** Secret masked on screen
- **Voice-over:** Next, configuration. The connection string lives in appsettings.json. The JWT section is called ApiSettings, with a secret, an issuer, and an audience. I've masked the secret on screen. The development file turns on the database initializer, with Migrate and Seed both set to true. In production you'd keep Migrate off.

### s11 - dotnet run on an empty database  (J2, 19.9 s voice, 48 words)

- **Screen:** Terminal replay: dotnet run log (filtered)
- **Actions:** Show captured startup log lines
- **Result held on screen:** 'Seeded 2 role(s)' and 'Now listening' visible
- **On-screen text:** Migrate and seed on first run
- **Voice-over:** For this demo I point the app at a fresh database with an environment variable. Then, dotnet run. On startup, the initializer finds 5 pending migrations, applies them to the empty database, and seeds the Admin and Customer roles. FastEndpoints registers 11 endpoints, and the app is listening.

## Live API in Scalar

### s12 - Open Scalar, check versions  (J3, 17.0 s voice, 43 words)

- **Screen:** Scalar /scalar
- **Actions:** Show v1 operations, switch to v2 (empty), switch back
- **Result held on screen:** v2 shows no operations
- **On-screen text:** Scalar: v1 and v2 documents
- **Voice-over:** Now the live API. This is Scalar, running against the app on localhost. Version 1 has the auth, products, and users endpoints. Switch to version 2, and the document is empty. The wiring is there, but there are no version 2 endpoints yet.

### s13 - Register an admin  (J3, 5.9 s voice, 14 words)

- **Screen:** Scalar Test Request: POST /api/v1/auth/register
- **Actions:** Type JSON body, Send
- **Result held on screen:** 200 with isSuccess true
- **On-screen text:** POST /auth/register
- **Voice-over:** First, register an admin. The request is plain JSON, and the response says success.

### s14 - Validation errors  (J3, 8.2 s voice, 22 words)

- **Screen:** Scalar Test Request: POST /api/v1/auth/register
- **Actions:** Send an invalid email and short password
- **Result held on screen:** 400 with email and password errors
- **On-screen text:** 400 with per-rule messages
- **Voice-over:** Bad input gets rejected. A bad email and a short password return a 400, with one message for each broken rule.

### s15 - Log in and read the token  (J3, 13.1 s voice, 36 words)

- **Screen:** Scalar Test Request: POST /api/v1/auth/login
- **Actions:** Send credentials, show response (token blurred)
- **Result held on screen:** 200 with token (blurred)
- **On-screen text:** JWT: id, role, issuer, audience
- **Voice-over:** Now log in. The response includes a JWT, which I'm blurring on screen. It carries the user's ID and the Admin role, plus an issuer and an audience, and it expires in seven days.

### s16 - Protected endpoint: 401 then 201  (J3, 10.4 s voice, 32 words)

- **Screen:** Scalar Test Request: POST /api/v1/products
- **Actions:** Send without auth, set Bearer token, send again
- **Result held on screen:** 401 then 201 with id
- **On-screen text:** 401 without token, 201 with it
- **Voice-over:** Without a token, creating a product returns 401. I set the bearer token in Scalar, and send it again. Now it's a 201, with the new product's ID.

### s17 - Validation on a slice  (J3, 7.2 s voice, 19 words)

- **Screen:** Scalar Test Request: POST /api/v1/products
- **Actions:** Send empty title
- **Result held on screen:** 400 with title errors
- **On-screen text:** Validator runs first
- **Voice-over:** An empty title returns a 400, with the validator's messages. The slice's rules run before the handler does.

### s18 - JSON, XML, and 406  (J3, 16.9 s voice, 38 words)

- **Screen:** Scalar Test Request: GET /api/v1/products
- **Actions:** Send; change accept header to application/xml; then text/csv
- **Result held on screen:** JSON, XML, then 406
- **On-screen text:** Accept: json, xml, csv
- **Voice-over:** Now, content negotiation. By default the list comes back as JSON. Change the Accept header to application/xml, and the same endpoint returns XML. Ask for text/csv, and it returns 406, Not Acceptable.

### s19 - Users via Dapper  (J3, 7.8 s voice, 20 words)

- **Screen:** Scalar Test Request: GET /api/v1/Users
- **Actions:** Send
- **Result held on screen:** 200 with the demo admin and role
- **On-screen text:** GET /users (Dapper)
- **Voice-over:** Last call before deleting, the users list. That's the Dapper query, returning the admin I registered, with the Admin role.

### s20 - Soft delete: the API call  (J3, 3.7 s voice, 10 words)

- **Screen:** Scalar Test Request: DELETE /api/v1/products/{id}
- **Actions:** Send DELETE with the new id
- **Result held on screen:** 204 No Content
- **On-screen text:** DELETE /products/{id}
- **Voice-over:** Now delete the product. The API returns 204.

### s21 - Soft delete: the row is still there  (J3, 7.5 s voice, 23 words)

- **Screen:** Terminal replay of a SQL query on the Products table
- **Actions:** Show captured query result
- **Result held on screen:** IsDeleted = 1, creator and deleted-by stamped
- **On-screen text:** Deleted, but still in the table
- **Voice-over:** But the row isn't gone. A quick query shows it's still in the table, flagged as deleted, with who created it, and when.

## AI-assisted development

### s22 - CLAUDE.md  (J4, 16.1 s voice, 41 words)

- **Screen:** Code viewer: CLAUDE.md
- **Actions:** Highlight Overview, Architecture and the data layer rule
- **Result held on screen:** Rules visible
- **On-screen text:** CLAUDE.md: written project rules
- **Voice-over:** Now, how AI-assisted development fits in. The repo has a CLAUDE.md file for Claude Code. It states the stack, the commands, the architecture, and the non-obvious rules. For example, don't hard-delete, and don't set audit fields by hand.

### s23 - Docs as conventions  (J4, 16.5 s voice, 40 words)

- **Screen:** Code viewer: CLAUDE.md Code Generation Guidelines
- **Actions:** Highlight the four /docs references
- **Result held on screen:** Four docs listed
- **On-screen text:** Read /docs first
- **Voice-over:** It also tells the assistant to read the docs folder before generating code. There are four short convention docs: features, API configuration, authentication, and the data layer. So generated code starts from written conventions that a reviewer can check against.

### s24 - The .claude folder  (J4, 25.6 s voice, 58 words)

- **Screen:** Code viewer: .claude folder tree
- **Actions:** Highlight agents, commands, skills
- **Result held on screen:** Tree visible
- **On-screen text:** Agents, commands, skills
- **Voice-over:** The .claude folder holds the tooling. A docs-reference-updater agent keeps CLAUDE.md's doc list current. A create-docs command scaffolds a new convention doc. A commit command writes Conventional Commit messages, and asks before it commits. And skills cover EF Core migrations, database updates, and moving usings to global usings. Plus skill-creator, for building more.

### s25 - Stop hook  (J4, 10.8 s voice, 30 words)

- **Screen:** Code viewer: .claude/settings.json
- **Actions:** Highlight the hook command
- **Result held on screen:** Hook config visible
- **On-screen text:** Stop hook keeps README current
- **Voice-over:** One more. A Stop hook in settings.json runs a Python script after each response. It regenerates the project structure tree in the README, so the docs don't drift.

### s26 - Honest limits  (J4, 12.8 s voice, 36 words)

- **Screen:** Code viewer: CLAUDE.md overview line on tests; build warning
- **Actions:** Highlight 'There are no tests in the repo'
- **Result held on screen:** Gap statement visible
- **On-screen text:** Known gaps
- **Voice-over:** Two honest notes. The repo has no automated tests yet, and AI tooling doesn't change that. That's the next thing I'd add. And the build flags a vulnerable transitive package, which I still need to update.

## Call to action

### s27 - Call to action  (J5, 12.2 s voice, 33 words)

- **Screen:** End card
- **Actions:** Hold end card with repository link
- **Result held on screen:** End card visible
- **On-screen text:** Let's talk
- **Voice-over:** If you're hiring engineers who use AI tools with discipline, written conventions, repeatable commands, and a habit of checking the results, I'd like to talk. The code is on GitHub. Thanks for watching.

Total voice-over: 395 s (6.6 min) before holds and title card.
