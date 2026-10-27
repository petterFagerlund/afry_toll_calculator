using Microsoft.Data.SqlClient;
using TollCalculator.Domain.Enums;

namespace TollCalculator.WebApi.Host.IntegrationTests.Tolls;

internal sealed class TestVehicleSeeder
{
    private const string InsertVehicleSql =
        "INSERT INTO [Vehicles] ([RegistrationNumber], [VehicleType]) VALUES (@registrationNumber, @vehicleType);";

    private readonly string _connectionString;

    public TestVehicleSeeder(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
    }

    public async Task<string> AddAsync(string registrationNumber, VehicleType vehicleType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationNumber);

        var normalized = registrationNumber.Trim().ToUpperInvariant();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(InsertVehicleSql, connection);
        command.Parameters.AddWithValue("@registrationNumber", normalized);
        command.Parameters.AddWithValue("@vehicleType", vehicleType.ToString());

        await command.ExecuteNonQueryAsync();

        return normalized;
    }
}
