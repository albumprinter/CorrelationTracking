using System;
using System.Configuration;
using Xunit;

namespace Albumprinter.CorrelationTracking.Tracing.IIS.Tests
{
    public sealed class TrackingHttpModuleConfigurationTests
    {
        [Fact]
        public void FromConfig_should_take_defaults_if_unset()
        {
            // arrange
            var moduleName = Guid.NewGuid().ToString("N");

            // act
            var actual = TrackingHttpModuleConfiguration.FromConfig(moduleName);

            // assert
            Assert.Equal(@"/(api|v\d+)/|\.(asmx|svc)(\?|$)", actual.AllowedUrls.ToString());
            Assert.Contains("Accept", actual.AllowedHeaders);
            Assert.Contains("Content-Type", actual.AllowedHeaders);
            Assert.Equal(@"^[^:]+$", actual.DeniedUrls.ToString());
        }

        [Fact]
        public void FromConfig_should_take_values_from_config_if_set()
        {
            // arrange
            var moduleName = Guid.NewGuid().ToString("N");

            ConfigurationManager.AppSettings[moduleName + @":AllowedUrls"] = ".*AllowedUrls.*";
            ConfigurationManager.AppSettings[moduleName + @":AllowedHeaders"] = "X-EXT-1 X-EXT-2";
            ConfigurationManager.AppSettings[moduleName + @":DeniedUrls"] = ".*DeniedUrls.*";

            // act
            var actual = TrackingHttpModuleConfiguration.FromConfig(moduleName);

            // assert
            Assert.Equal(".*AllowedUrls.*", actual.AllowedUrls.ToString());
            Assert.Contains("X-EXT-1", actual.AllowedHeaders);
            Assert.Contains("X-EXT-2", actual.AllowedHeaders);
            Assert.Equal(".*DeniedUrls.*", actual.DeniedUrls.ToString());
        }
    }
}
