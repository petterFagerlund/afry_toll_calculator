using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence;

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
    [InlineData(typeof(TollPassage), "TollPassages")]
    [InlineData(typeof(TollRateInterval), "TollRateIntervals")]
    public void Model_Entity_MapsToExpectedTable(Type entityType, string expectedTable)
    {
        var entity = _context.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(expectedTable, entity.GetTableName());
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
    public void Model_TollPassage_DoesNotRequireARegisteredVehicle()
    {
        var foreignKeys = _context.Model.FindEntityType(typeof(TollPassage))!.GetForeignKeys();

        Assert.Empty(foreignKeys);
    }

    [Fact]
    public void Model_TollPassage_StoresTheReportedVehicleTypeByName()
    {
        var property = _context.Model.FindEntityType(typeof(TollPassage))!
            .FindProperty(nameof(TollPassage.VehicleType))!;

        Assert.Equal(typeof(string), property.GetProviderClrType());
        Assert.Equal(20, property.GetMaxLength());
    }

    [Fact]
    public void Model_TollRateInterval_UsesAShadowKey()
    {
        var key = _context.Model.FindEntityType(typeof(TollRateInterval))!.FindPrimaryKey();

        Assert.NotNull(key);
        Assert.True(Assert.Single(key.Properties).IsShadowProperty());
    }
}
