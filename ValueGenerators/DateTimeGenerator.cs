using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace DapperEFCorePostgreSQL.ValueGenerators
{
    public class DateTimeGenerator : ValueGenerator
    {
        public override bool GeneratesTemporaryValues => false;

        protected override object NextValue([NotNullAttribute] EntityEntry entry)
        {
            return DateTime.UtcNow;
        }
    }
}
