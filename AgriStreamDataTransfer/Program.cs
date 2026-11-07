

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string azureConnection =
    configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection not found.");

string[] tables =
{
    "AspNetUsers",
    "AspNetRoles",
    "AspNetUserRoles",
    "AspNetUserClaims",
    "AspNetUserLogins",
    "AspNetUserTokens",
    "AspNetRoleClaims",
    "Farms"
};

try
{
    Console.WriteLine("AGRISTREAM AZURE DATA VERIFICATION");
    Console.WriteLine("----------------------------------");

    string token = await GetAzureSqlAccessTokenAsync();

    var builder = new SqlConnectionStringBuilder(azureConnection);
    builder.Remove("Authentication");

    await using var connection =
        new SqlConnection(builder.ConnectionString);

    connection.AccessToken = token;

    await connection.OpenAsync();

    Console.WriteLine($"Connected: {connection.Database}");

    Console.WriteLine("\nAZURE RECORD COUNTS");

    foreach (string table in tables)
    {
        string sql = $"SELECT COUNT_BIG(*) FROM [dbo].[{table}]";

        await using var command = new SqlCommand(sql, connection);

        long count = Convert.ToInt64(
            await command.ExecuteScalarAsync()
        );

        Console.WriteLine($"{table}: {count}");
    }

    Console.WriteLine("\nVERIFICATION COMPLETED.");
}
catch (Exception ex)
{
    Console.WriteLine("\nVERIFICATION ERROR:");
    Console.WriteLine(ex.GetBaseException().Message);
}

static async Task<string> GetAzureSqlAccessTokenAsync()
{
    string azPath =
        @"C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd";

    var startInfo = new ProcessStartInfo
    {
        FileName = azPath,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    startInfo.ArgumentList.Add("account");
    startInfo.ArgumentList.Add("get-access-token");
    startInfo.ArgumentList.Add("--resource");
    startInfo.ArgumentList.Add("https://database.windows.net/");
    startInfo.ArgumentList.Add("--query");
    startInfo.ArgumentList.Add("accessToken");
    startInfo.ArgumentList.Add("--output");
    startInfo.ArgumentList.Add("tsv");

    using var process = new Process { StartInfo = startInfo };

    process.Start();

    Task<string> outputTask =
        process.StandardOutput.ReadToEndAsync();

    Task<string> errorTask =
        process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    string output = await outputTask;
    string error = await errorTask;

    if (process.ExitCode != 0)
        throw new InvalidOperationException(error);

    string token = output.Trim();

    if (string.IsNullOrWhiteSpace(token))
        throw new InvalidOperationException("Empty Azure SQL token.");

    return token;
}


