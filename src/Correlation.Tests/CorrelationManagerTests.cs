using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Albumprinter.CorrelationTracking.Correlation.Tests
{
    public sealed class CorrelationManagerTests
    {
        public abstract class TestBase
        {
            protected TestBase()
            {
                CorrelationManager = new CorrelationManager();
            }

            public CorrelationManager CorrelationManager { get; private set; }
        }

        public sealed class Current
        {
            [Fact]
            public void Current_should_not_be_empty()
            {
                // act
                var actual = CorrelationManager.Instance;

                // assert
                Assert.NotNull(actual);
            }
        }

        public sealed class UseScope : TestBase
        {
            [Fact]
            public void Should_create_the_new_scope_and_restore_the_previous_one_on_dispose()
            {
                // assert
                Assert.Equal(CorrelationScope.Zero, CorrelationScope.Current);

                // act
                var scopeId1 = Guid.NewGuid();
                using (CorrelationManager.UseScope(scopeId1))
                {
                    // assert
                    Assert.Equal(scopeId1, CorrelationScope.Current.CorrelationId);

                    // act
                    var scopeId2 = Guid.NewGuid();
                    using (CorrelationManager.UseScope(scopeId2))
                    {
                        // assert
                        Assert.Equal(scopeId2, CorrelationScope.Current.CorrelationId);
                    }

                    // assert
                    Assert.Equal(scopeId1, CorrelationScope.Current.CorrelationId);
                }

                // assert
                Assert.Equal(CorrelationScope.Zero, CorrelationScope.Current);
            }
        }

        public sealed class Scope : TestBase
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task Should_be_available_during_the_async_operation(bool continueOnCapturedContext)
            {
                // arrange
                var expected = Guid.NewGuid();

                using (CorrelationManager.UseScope(expected))
                {
                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);

                    // act
                    await Task.Delay(50).ConfigureAwait(continueOnCapturedContext);

                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                }
            }

            [Fact]
            public void Should_be_available_in_thread_pool()
            {
                // arrange
                var expected = Guid.NewGuid();

                using (CorrelationManager.UseScope(expected))
                {
                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);

                    // act
                    var autoResetEvent = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(
                        state =>
                        {
                            // assert
                            Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                            autoResetEvent.Set();
                        });
                    autoResetEvent.WaitOne(50);

                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                }
            }

            [Fact]
            public async Task Should_be_available_using_task_run()
            {
                // arrange
                var expected = Guid.NewGuid();

                using (CorrelationManager.UseScope(expected))
                {
                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);

                    // act
                    await Task.Run(
                        async () =>
                        {
                            await Task.Yield();

                            // assert
                            Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                        }).ConfigureAwait(false);

                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                }
            }

            [Fact]
            public async Task Should_be_available_using_task_factory()
            {
                // arrange
                var expected = Guid.NewGuid();

                using (CorrelationManager.UseScope(expected))
                {
                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);

                    // act
                    await Task.Factory.StartNew(
                        async () =>
                        {
                            await Task.Yield();

                            // assert
                            Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                        }).ConfigureAwait(false);

                    // assert
                    Assert.Equal(expected, CorrelationScope.Current.CorrelationId);
                }
            }
        }
    }
}
