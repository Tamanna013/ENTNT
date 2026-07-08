using System;
using Microsoft.Data.SqlClient;

var email = args.Length > 0 ? args[0] : null;
if (string.IsNullOrEmpty(email)) {
    Console.WriteLine("No email provided.");
    return;
}

var connStr = "Server=(localdb)\\mssqllocaldb;Database=FleetMindDb;Trusted_Connection=True;TrustServerCertificate=True;";
try {
    using var conn = new SqlConnection(connStr);
    conn.Open();
    using var cmd = new SqlCommand("SELECT TOP 1 Token FROM EmailVerificationTokens WHERE UserId = (SELECT Id FROM Users WHERE Email = @email) ORDER BY CreatedAt DESC", conn);
    cmd.Parameters.AddWithValue("@email", email);
    var token = cmd.ExecuteScalar();
    if (token != null) {
        Console.WriteLine(token);
    } else {
        Console.WriteLine("TOKEN_NOT_FOUND");
    }
} catch (Exception ex) {
    Console.WriteLine("ERROR: " + ex.Message);
}
