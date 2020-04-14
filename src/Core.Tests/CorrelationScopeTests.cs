using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Xunit;

namespace Core.Tests
{
    public sealed class CorrelationScopeTests
    {
        public sealed class Serialize
        {
            [Fact]
            public void Should_be_serialized_with_BinaryFormatter()
            {
                // arrange
                var expected = new CorrelationScope(Guid.NewGuid(), Guid.NewGuid().ToString());

                var serializer = new BinaryFormatter();

                // act
                var stream = new MemoryStream();
                serializer.Serialize(stream, expected);

                stream.Position = 0;
                var actual = (CorrelationScope)serializer.Deserialize(stream);

                // assert
                Assert.Equal(expected.CorrelationId, actual.CorrelationId);
                Assert.Equal(expected.RequestId, actual.RequestId);
            }
        }
    }
}