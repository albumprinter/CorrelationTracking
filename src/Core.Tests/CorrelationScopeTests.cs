using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Xunit;

namespace Albumprinter.CorrelationTracking.Correlation.Core.Tests
{
    public sealed class CorrelationScopeTests
    {
        public sealed class Serialize
        {
            [Fact]
            public void Should_be_serialized_with_SoapFormatter()
            {
                // arrange
                var expected = new CorrelationScope(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

                var serializer = new SoapFormatter();

                // act
                var stream = new MemoryStream();
                serializer.Serialize(stream, expected);

                stream.Position = 0;
                var actual = (CorrelationScope)serializer.Deserialize(stream);

                // assert
                Assert.Equal(expected.ProcessId, actual.ProcessId);
                Assert.Equal(expected.CorrelationId, actual.CorrelationId);
                Assert.Equal(expected.RequestId, actual.RequestId);
            }

            [Fact]
            public void Should_be_serialized_with_BinaryFormatter()
            {
                // arrange
                var expected = new CorrelationScope(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

                var serializer = new BinaryFormatter();

                // act
                var stream = new MemoryStream();
                serializer.Serialize(stream, expected);

                stream.Position = 0;
                var actual = (CorrelationScope)serializer.Deserialize(stream);

                // assert
                Assert.Equal(expected.ProcessId, actual.ProcessId);
                Assert.Equal(expected.CorrelationId, actual.CorrelationId);
                Assert.Equal(expected.RequestId, actual.RequestId);
            }
        }
    }
}