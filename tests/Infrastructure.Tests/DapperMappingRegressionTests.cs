using System.Data;
using Dapper;
using FluentAssertions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Infrastructure.Tests;

public sealed class DapperMappingRegressionTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("appdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var conn = new NpgsqlConnection(_container.GetConnectionString());
        await conn.OpenAsync();

        await conn.ExecuteAsync(@"
            CREATE TABLE product_regression (
                id uuid PRIMARY KEY,
                name_en text NOT NULL,
                name_ar text NOT NULL,
                option_group_id uuid NOT NULL,
                price_adjustment numeric(18,2) NOT NULL,
                is_default boolean NOT NULL,
                display_order integer NOT NULL
            );");

        await conn.ExecuteAsync(@"
            INSERT INTO product_regression (
                id,
                name_en,
                name_ar,
                option_group_id,
                price_adjustment,
                is_default,
                display_order
            ) VALUES (
                @Id,
                @NameEn,
                @NameAr,
                @OptionGroupId,
                @PriceAdjustment,
                @IsDefault,
                @DisplayOrder
            );",
            new
            {
                Id = Guid.NewGuid(),
                NameEn = "Classic Burger",
                NameAr = "برجر كلاسيك",
                OptionGroupId = Guid.NewGuid(),
                PriceAdjustment = 12.50m,
                IsDefault = true,
                DisplayOrder = 3
            });
    }

    [Fact]
    public async Task QuerySingleAsync_WithSnakeCaseColumns_MapsToPascalCaseProperties()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        await using var conn = new NpgsqlConnection(_container.GetConnectionString());
        await conn.OpenAsync();

        var row = await conn.QuerySingleAsync<ProductRegressionRow>(@"
            SELECT
                id,
                name_en,
                name_ar,
                option_group_id,
                price_adjustment,
                is_default,
                display_order
            FROM product_regression");

        row.Id.Should().NotBe(Guid.Empty);
        row.NameEn.Should().Be("Classic Burger");
        row.NameAr.Should().Be("برجر كلاسيك");
        row.OptionGroupId.Should().NotBe(Guid.Empty);
        row.PriceAdjustment.Should().Be(12.50m);
        row.IsDefault.Should().BeTrue();
        row.DisplayOrder.Should().Be(3);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private sealed class ProductRegressionRow
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = default!;
        public string NameAr { get; init; } = default!;
        public Guid OptionGroupId { get; init; }
        public decimal PriceAdjustment { get; init; }
        public bool IsDefault { get; init; }
        public int DisplayOrder { get; init; }
    }
}
