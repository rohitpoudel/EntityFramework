// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class BuiltInDataTypesSqlServerFixture : BuiltInDataTypesFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testStore;

        public BuiltInDataTypesSqlServerFixture()
        {
            _testStore = SqlServerTestStore.CreateScratch();

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(_testStore.Connection);

            _options = optionsBuilder.Options;

            using (var context = new DbContext(_serviceProvider, _options))
            {
                context.Database.EnsureCreated();
            }
        }

        public override DbContext CreateContext()
        {
            var context = new DbContext(_serviceProvider, _options);
            context.Database.UseTransaction(_testStore.Transaction);
            return context;
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            MakeRequired<MappedDataTypes>(modelBuilder);

            modelBuilder.Entity<BuiltInDataTypes>(b =>
                {
                    b.Ignore(dt => dt.TestUnsignedInt16);
                    b.Ignore(dt => dt.TestUnsignedInt32);
                    b.Ignore(dt => dt.TestUnsignedInt64);
                    b.Ignore(dt => dt.TestCharacter);
                    b.Ignore(dt => dt.TestSignedByte);
                });

            modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
                {
                    b.Ignore(dt => dt.TestNullableUnsignedInt16);
                    b.Ignore(dt => dt.TestNullableUnsignedInt32);
                    b.Ignore(dt => dt.TestNullableUnsignedInt64);
                    b.Ignore(dt => dt.TestNullableCharacter);
                    b.Ignore(dt => dt.TestNullableSignedByte);
                });

            modelBuilder.Entity<MappedDataTypes>(b =>
                {
                    b.Key(e => e.Int);
                    b.Property(e => e.Int)
                        .StoreGeneratedPattern(StoreGeneratedPattern.None);
                });

            modelBuilder.Entity<MappedNullableDataTypes>(b =>
            {
                b.Key(e => e.Int);
                b.Property(e => e.Int)
                    .StoreGeneratedPattern(StoreGeneratedPattern.None);
            });

            modelBuilder.Entity<MappedSizedDataTypes>()
                .Property(e => e.Id)
                .StoreGeneratedPattern(StoreGeneratedPattern.None);

            modelBuilder.Entity<MappedScaledDataTypes>()
                .Property(e => e.Id)
                .StoreGeneratedPattern(StoreGeneratedPattern.None);

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>()
                .Property(e => e.Id)
                .StoreGeneratedPattern(StoreGeneratedPattern.None);

            MapColumnTypes<MappedDataTypes>(modelBuilder);
            MapColumnTypes<MappedNullableDataTypes>(modelBuilder);

            MapSizedColumnTypes<MappedSizedDataTypes>(modelBuilder);
            MapSizedColumnTypes<MappedScaledDataTypes>(modelBuilder);
            MapPreciseColumnTypes<MappedPrecisionAndScaledDataTypes>(modelBuilder);
        }

        private static void MapColumnTypes<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            var entityType = modelBuilder.Entity<TEntity>().Metadata;

            foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties)
            {
                var typeName = propertyInfo.Name;

                if (typeName.EndsWith("Max"))
                {
                    typeName = typeName.Substring(0, typeName.IndexOf("Max")) + "(max)";
                }

                typeName = typeName.Replace('_', ' ');

                entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = typeName;
            }
        }

        private static void MapSizedColumnTypes<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            var entityType = modelBuilder.Entity<TEntity>().Metadata;

            foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties.Where(p => p.Name != "Id"))
            {
                entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = propertyInfo.Name.Replace('_', ' ') + "(3)";
            }
        }

        private static void MapPreciseColumnTypes<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            var entityType = modelBuilder.Entity<TEntity>().Metadata;

            foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties.Where(p => p.Name != "Id"))
            {
                entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = propertyInfo.Name.Replace('_', ' ') + "(5, 2)";
            }
        }

        public override void Dispose() => _testStore.Dispose();

        public override bool SupportsBinaryKeys => true;

        public override DateTime DefaultDateTime => new DateTime();
    }

    public class MappedDataTypes
    {
        public int Int { get; set; }
        public long Bigint { get; set; }
        public short Smallint { get; set; }
        public byte Tinyint { get; set; }
        public bool Bit { get; set; }
        public decimal Money { get; set; }
        public decimal Smallmoney { get; set; }
        public double Float { get; set; }
        public float Real { get; set; }
        public double Double_precision { get; set; }
        public DateTime Date { get; set; }
        public DateTimeOffset Datetimeoffset { get; set; }
        public DateTime Datetime2 { get; set; }
        public DateTime Smalldatetime { get; set; }
        public DateTime Datetime { get; set; }
        public TimeSpan Time { get; set; }
        public string Char { get; set; }
        public string Character { get; set; }
        public string Varchar { get; set; }
        public string Char_varying { get; set; }
        public string Character_varying { get; set; }
        public string VarcharMax { get; set; }
        public string Char_varyingMax { get; set; }
        public string Character_varyingMax { get; set; }
        public string Nchar { get; set; }
        public string National_character { get; set; }
        public string Nvarchar { get; set; }
        public string National_char_varying { get; set; }
        public string National_character_varying { get; set; }
        public string NvarcharMax { get; set; }
        public string National_char_varyingMax { get; set; }
        public string National_character_varyingMax { get; set; }
        public string Text { get; set; }
        public string Ntext { get; set; }
        public byte[] Binary { get; set; }
        public byte[] Varbinary { get; set; }
        public byte[] Binary_varying { get; set; }
        public byte[] VarbinaryMax { get; set; }
        public byte[] Binary_varyingMax { get; set; }
        public byte[] Image { get; set; }
        public decimal Decimal { get; set; }
        public decimal Dec { get; set; }
        public decimal Numeric { get; set; }
    }

    public class MappedSizedDataTypes
    {
        public int Id { get; set; }
        public string Char { get; set; }
        public string Character { get; set; }
        public string Varchar { get; set; }
        public string Char_varying { get; set; }
        public string Character_varying { get; set; }
        public string Nchar { get; set; }
        public string National_character { get; set; }
        public string Nvarchar { get; set; }
        public string National_char_varying { get; set; }
        public string National_character_varying { get; set; }
        public byte[] Binary { get; set; }
        public byte[] Varbinary { get; set; }
        public byte[] Binary_varying { get; set; }
    }

    public class MappedScaledDataTypes
    {
        public int Id { get; set; }
        public float Float { get; set; }
        public float Double_precision { get; set; }
        public DateTimeOffset Datetimeoffset { get; set; }
        public DateTime Datetime2 { get; set; }
        public decimal Decimal { get; set; }
        public decimal Dec { get; set; }
        public decimal Numeric { get; set; }
    }

    public class MappedPrecisionAndScaledDataTypes
    {
        public int Id { get; set; }
        public decimal Decimal { get; set; }
        public decimal Dec { get; set; }
        public decimal Numeric { get; set; }
    }

    public class MappedNullableDataTypes
    {
        public int? Int { get; set; }
        public long? Bigint { get; set; }
        public short? Smallint { get; set; }
        public byte? Tinyint { get; set; }
        public bool? Bit { get; set; }
        public decimal? Money { get; set; }
        public decimal? Smallmoney { get; set; }
        public double? Float { get; set; }
        public float? Real { get; set; }
        public double? Double_precision { get; set; }
        public DateTime? Date { get; set; }
        public DateTimeOffset? Datetimeoffset { get; set; }
        public DateTime? Datetime2 { get; set; }
        public DateTime? Smalldatetime { get; set; }
        public DateTime? Datetime { get; set; }
        public TimeSpan? Time { get; set; }
        public string Char { get; set; }
        public string Character { get; set; }
        public string Varchar { get; set; }
        public string Char_varying { get; set; }
        public string Character_varying { get; set; }
        public string VarcharMax { get; set; }
        public string Char_varyingMax { get; set; }
        public string Character_varyingMax { get; set; }
        public string Nchar { get; set; }
        public string National_character { get; set; }
        public string Nvarchar { get; set; }
        public string National_char_varying { get; set; }
        public string National_character_varying { get; set; }
        public string NvarcharMax { get; set; }
        public string National_char_varyingMax { get; set; }
        public string National_character_varyingMax { get; set; }
        public string Text { get; set; }
        public string Ntext { get; set; }
        public byte[] Binary { get; set; }
        public byte[] Varbinary { get; set; }
        public byte[] Binary_varying { get; set; }
        public byte[] VarbinaryMax { get; set; }
        public byte[] Binary_varyingMax { get; set; }
        public byte[] Image { get; set; }
        public decimal? Decimal { get; set; }
        public decimal? Dec { get; set; }
        public decimal? Numeric { get; set; }
    }
}
