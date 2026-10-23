using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence;
using Xunit;

namespace TollCalculator.Infrastructure.UnitTests.Persistence;

public class TollDbContextModelTests
{
    private readonly TollDbContext _context;

    public TollDbContextModelTests()
    {
        var options = new DbContextOptionsBuilder<TollDbContext>()
            .UseSqlServer("Server=localhost;Database=TollCalculatorModelOnly;Trusted_Connection=True")
            .Options;

        _context = new TollDbContext(options);
    }

    [Theory]
    [InlineData(typeof(Vehicle), "Vehicles")]
    [InlineData(typeof(TollPassage), "TollPassages")]
    [InlineData(typeof(TollRateInterval), "TollRateIntervals")]
    [InlineData(typeof(Holiday), "Holidays")]
    [InlineData(typeof(TollSettings), "TollSettings")]
    public void Model_Entity_MapsToExpectedTable(Type entityType, string expectedTable)
    {
        var entity = _context.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(expectedTable, entity.GetTableName());
    }

    [Fact]
    public void Model_Vehicle_IsKeyedOnRegistrationNumber()
    {
        var key = _context.Model.FindEntityType(typeof(Vehicle))!.FindPrimaryKey();

        Assert.NotNull(key);
        Assert.Equal(nameof(Vehicle.RegistrationNumber), Assert.Single(key.Properties).Name);
    }

    [Fact]
    public void Model_VehicleType_IsStoredAsText()
    {
        var property = _context.Model.FindEntityType(typeof(Vehicle))!.FindProperty(nameof(Vehicle.VehicleType));

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.GetProviderClrType());
    }

    [Fact]
    public void Model_Vehicle_DoesNotPersistIsTollFree()
    {
        var property = _context.Model.FindEntityType(typeof(Vehicle))!.FindProperty(nameof(Vehicle.IsTollFree));

        Assert.Null(property);
    }

    [Fact]
    public void Model_TollPassage_HasUniqueIndexOnRegistrationNumberAndPassedAt()
    {
        var entity = _context.Model.FindEntityType(typeof(TollPassage))!;

        var index = entity.GetIndexes().Single(candidate => candidate.Properties.Count == 2);

        Assert.True(index.IsUnique);
        Assert.Equal(nameof(TollPassage.RegistrationNumber), index.Properties[0].Name);
        Assert.Equal(nameof(TollPassage.PassedAt), index.Properties[1].Name);
    }

    [Fact]
    public void Model_TollPassage_ReferencesVehicle()
    {
        var foreignKey = Assert.Single(_context.Model.FindEntityType(typeof(TollPassage))!.GetForeignKeys());

        Assert.Equal(typeof(Vehicle), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(TollPassage.RegistrationNumber), Assert.Single(foreignKey.Properties).Name);
    }

    [Fact]
    public void Model_Holiday_IsKeyedOnDate()
    {
        var key = _context.Model.FindEntityType(typeof(Holiday))!.FindPrimaryKey();

        Assert.NotNull(key);
        Assert.Equal(nameof(Holiday.Date), Assert.Single(key.Properties).Name);
    }

    [Fact]
    public void Model_TollRateInterval_UsesAShadowKey()
    {
        var key = _context.Model.FindEntityType(typeof(TollRateInterval))!.FindPrimaryKey();

        Assert.NotNull(key);
        Assert.True(Assert.Single(key.Properties).IsShadowProperty());
    }
}
