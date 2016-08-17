using System;
using System.Web;
using FakeItEasy;
using Xunit;

namespace Albumprinter.CorrelationTracking.Tracing.IIS.Tests
{
    public sealed class TrackingHttpModuleStateTests
    {
        [Theory]
        [InlineData("http://example.com/", "", "", false)]
        [InlineData("http://example.com/api/", "/api/", "", true)]
        [InlineData("http://example.com/api/", "/api/", "^[^:]$", true)]
        [InlineData("http://example.com/api/security", "/api/", "(security)", false)]
        public void IsTrackable(string uri, string allowedUrls, string deniedUrls, bool expected)
        {
            // arrange
            var context = A.Fake<HttpContextBase>();
            A.CallTo(() => context.Request.Url).Returns(new Uri(uri));
            var configuration = new TrackingHttpModuleConfiguration(allowedUrls, new string[0], deniedUrls);

            // act
            var actual = TrackingHttpModuleState.IsTrackable(context, configuration);

            // assert
            Assert.Equal(expected, actual);
        }
    }
}