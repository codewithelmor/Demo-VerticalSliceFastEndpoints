using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using VerticalSliceFastEndpoints.Features.User.GetUsers;

namespace Features.User.GetUser;

public class GetUserEndpoint : EndpointWithoutRequest<GetUserResponseDto>
{
    private readonly string _connectionString;

    public GetUserEndpoint(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public override void Configure()
    {
        Get("/Users/{id}");
        Version(1);
        AllowAnonymous();

        Description(x => x
            .WithTags("Users")
            .WithDescription("Get a single User by its ID.")
            .Produces<GetUserResponseDto>(200)
            .ProducesProblem(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Extract the {id} parameter safely from the route path
        var id = Route<Guid>("id");

        // 1. Query the database with pagination bounds
        // SQL Server high-performance pagination with Total Record Tracking via window functions
        const string sql = @"
            SELECT 
                u.[Id] AS [UserId],
                u.[UserName],
                u.[Email],
                u.[EmailConfirmed],
                r.[Name] AS [RoleName]
            FROM 
                [dbo].[AspNetUsers] u
            LEFT JOIN 
                [dbo].[AspNetUserRoles] ur ON u.[Id] = ur.[UserId]
            LEFT JOIN 
                [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id]
            WHERE 
                u.[Id] = @Id;";

        using IDbConnection db = new SqlConnection(_connectionString);

        // Dictionary to track and merge user rows with multiple roles
        var userDictionary = new Dictionary<string, GetUserResponseDto>();

        // Dapper Multi-Mapping: This processes rows where columns belong to different data types.
        // <GetUserResponseDto, string, int, GetUserResponseDto> means:
        //   1st Type (GetUserResponseDto): Input object representing the user columns.
        //   2nd Type (string): Input item representing the RoleName column.
        //   3rd Type (GetUserResponseDto): The final merged object returned by this function.
        await db.QueryAsync<GetUserResponseDto, string, GetUserResponseDto>(
            sql,
            // This lambda function runs automatically for EVERY single row returned by SQL.
            (user, roleName) =>
            {
                // 2. Check if we have already created a C# object for this specific User ID.
                // Since a user can have multiple roles, their user columns will repeat across multiple rows.
                if (!userDictionary.TryGetValue(user.UserId, out var existingUser))
                {
                    // If the user isn't in our dictionary yet, initialize them
                    existingUser = user;

                    // Add them to our dictionary tracker so we don't duplicate them on the next row
                    userDictionary.Add(existingUser.UserId, existingUser);
                }

                // 3. Add the role name to this user's list of roles.
                // If a user has 3 roles, this block runs 3 times for that user, appending the role names.
                if (!string.IsNullOrEmpty(roleName))
                {
                    existingUser.Roles.Add(roleName);
                }

                // 4. Return the tracked user object to satisfy Dapper's internal iterator requirements.
                return existingUser;
            },
            // Pass parameters to the SQL script safely to prevent SQL injection vulnerabilities.
            param: new { Id = id },
            // Crucial: Tells Dapper where to cut off columns in the SQL result grid.
            // Everything before "RoleName" belongs to the UserItemResponseDto object.
            splitOn: "RoleName"
        );

        // 2. Return 404 if the User doesn't exist (or is soft-deleted)
        if (!userDictionary.Any())
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 3. Map to response object
        var response = userDictionary.First().Value;

        // 4. Returns HTTP 200 with the response object in the body
        await Send.OkAsync(response, ct);
    }
}
