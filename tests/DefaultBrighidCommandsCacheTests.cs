using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;

using NUnit.Framework;

namespace Brighid.Commands.Client
{
    public class DefaultBrighidCommandsCacheTests
    {
        [TestFixture]
        public class ParametersCacheCountTests
        {
            [Test, Auto]
            public async Task ShouldReturnNumberOfCachedParameters(
                string keyA,
                string keyB,
                [Target] DefaultBrighidCommandsCache cache
            )
            {
                await cache.GetOrCreateParametersAsync(keyA, Factory);
                await cache.GetOrCreateParametersAsync(keyB, Factory);

                cache.ParametersCacheCount.Should().Be(2);
            }

            private Task<ICollection<CommandParameter>> Factory(ICacheEntry entry)
            {
                entry.SetPriority(CacheItemPriority.Normal);
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                entry.SetSize(1);
                return Task.FromResult<ICollection<CommandParameter>>(new List<CommandParameter>());
            }
        }

        [TestFixture]
        public class ClearAllParametersTests
        {
            [Test, Auto]
            public async Task ShouldRemoveAllCachedParameters(
                string keyA,
                string keyB,
                [Target] DefaultBrighidCommandsCache cache
            )
            {
                await cache.GetOrCreateParametersAsync(keyA, Factory);
                await cache.GetOrCreateParametersAsync(keyB, Factory);

                cache.ClearAllParameters();

                cache.ParametersExist(keyA).Should().BeFalse();
                cache.ParametersExist(keyB).Should().BeFalse();
            }

            private Task<ICollection<CommandParameter>> Factory(ICacheEntry entry)
            {
                entry.SetPriority(CacheItemPriority.Normal);
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                entry.SetSize(1);
                return Task.FromResult<ICollection<CommandParameter>>(new List<CommandParameter>());
            }
        }
    }
}
