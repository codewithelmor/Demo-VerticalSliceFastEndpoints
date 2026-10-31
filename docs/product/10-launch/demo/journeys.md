# Journeys

| ID | Journey | Scenes | Steps (each checked) | Result shown |
|---|---|---|---|---|
| J0 | Meet the project and stack | s01-s02 | README and csproj render from the real files | Stack and versions visible |
| J1 | Architecture tour | s03-s08 | Highlighted ranges found by pattern in real files (a missing pattern fails the run) | Slice anatomy, Program.cs, audit/soft delete, Dapper |
| J2 | Setup from scratch | s09-s11 | Captured `dotnet --version`, build, and first-run log on an empty DB | Build succeeded; 5 migrations applied; 2 roles seeded; listening |
| J3 | Live API in Scalar (`automation/j3-scalar-api.mjs`) | s12-s21 | 56 automated steps, HTTP status checked on each call | See below |
| J4 | AI-assisted development | s22-s26 | Real files: CLAUDE.md, .claude tree, settings.json | Conventions, agents, commands, skills, Stop hook, limits |
| J5 | Call to action | s27 | End card | Repo link |

## J3 detail

1. Open Scalar, show v1 operations, switch to v2 (no operations), back to v1.
2. Register admin -> 200, `isSuccess: true`.
3. Register with bad email and short password -> 400 with rule messages.
4. Login -> 200, token present (blurred); claims checked: role Admin, 7-day lifetime.
5. POST /products without token -> 401; set bearer token; POST -> 201 with id.
6. POST /products with empty title -> 400 "Title is required."
7. GET /products as JSON -> 200; Accept application/xml -> 200 XML; Accept text/csv -> 406.
8. GET /Users (Dapper) -> 200 with the demo admin.
9. DELETE /products/{id} -> 204; then SQL query shows the row with IsDeleted = 1 (s21).
