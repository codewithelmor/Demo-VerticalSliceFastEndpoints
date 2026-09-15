using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using VerticalSliceFastEndpoints.Features.User.GetUsers;

namespace Features.User.GetUsers;

public class GetUsersEndpoint : Endpoint<GetUsersRequest, GetUsersResponse>
{
    private readonly string _connectionString;

    public GetUsersEndpoint(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public override void Configure()
    {
        Get("/Users");
        Version(1);
        AllowAnonymous();

        Description(x => x
            .WithTags("Users")
            .WithDescription("Retrieves a paginated list of Users from the database.")
            .Produces<GetUsersResponse>(200)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(GetUsersRequest req, CancellationToken ct)
    {
        // 1. Query the database with pagination bounds
        // SQL Server high-performance pagination with Total Record Tracking via window functions
        const string sql = @"
            SELECT 
                u.[Id] AS [UserId],
                u.[UserName],
                u.[Email],
                u.[EmailConfirmed],
                r.[Name] AS [RoleName],
                COUNT(*) OVER() AS [TotalCount] -- Calculates total rows before pagination
            FROM 
                [dbo].[AspNetUsers] u
            LEFT JOIN 
                [dbo].[AspNetUserRoles] ur ON u.[Id] = ur.[UserId]
            LEFT JOIN 
                [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id]
            ORDER BY 
                u.[UserName] ASC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;";

        using IDbConnection db = new SqlConnection(_connectionString);

        // Dictionary to track and merge user rows with multiple roles
        var userDictionary = new Dictionary<string, UserItemResponse>();
        int totalRecords = 0;

        // Dapper Multi-Mapping: This processes rows where columns belong to different data types.
        // <UserItemResponse, string, int, UserItemResponse> means:
        //   1st Type (UserItemResponse): Input object representing the user columns.
        //   2nd Type (string): Input item representing the RoleName column.
        //   3rd Type (int): Input item representing the TotalCount column.
        //   4th Type (UserItemResponse): The final merged object returned by this function.
        await db.QueryAsync<UserItemResponse, string, int, UserItemResponse>(
            sql,
            // This lambda function runs automatically for EVERY single row returned by SQL.
            (user, roleName, totalCount) =>
            {
                // 1. Capture the total rows match count from the SQL window function.
                // It's the same on every row, so we only need to save it once (when it's still 0).
                if (totalRecords == 0) totalRecords = totalCount;

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
            param: new { Offset = (req.PageNumber - 1) * req.PageSize, PageSize = req.PageSize },

            // Crucial: Tells Dapper where to cut off columns in the SQL result grid.
            // Everything before "RoleName" belongs to the UserItemResponse object.
            // "RoleName" maps to the string parameter. "TotalCount" maps to the int parameter.
            splitOn: "RoleName,TotalCount"
        );

        var items = userDictionary.Values.ToList();

        // 2. Fetch total count for metadata mapping
        var totalCount = items.Count();

        // 3. Populate and return response envelope
        var response = new GetUsersResponse
        {
            Items = items,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalCount = totalCount
        };

        await Send.OkAsync(response, ct);
    }
}
