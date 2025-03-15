using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Queues;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Tests
{
    [TestFixture]
    public class BuildsRepositoryTests
    {
        private static readonly DateTime CurrentTime = DateTime.Now;

        private SimulationSettings GetSomeSettings()
        {
            return new SimulationSettings
            {
                Agents = [
                    new AgentConfiguration
                    {
                        Name = "TestAgent1"
                    }
                ],
                BuildConfigurations = [
                    new BuildConfiguration
                    {
                        Name = "BuildConfig1",
                        CompatibleAgents = ["TestAgent1"],
                        BuildDependencies = ["BuildConfig2"]
                    },
                    new BuildConfiguration
                    {
                        Name = "BuildConfig2",
                        CompatibleAgents = ["TestAgent1"],
                        BuildDependencies = ["BuildConfig3", "BuildConfig5"]
                    },
                    new BuildConfiguration
                    {
                        Name = "BuildConfig3",
                        CompatibleAgents = ["TestAgent1"],
                        BuildDependencies = ["BuildConfig4"]
                    },
                    new BuildConfiguration
                    {
                        Name = "BuildConfig4",
                        CompatibleAgents = ["TestAgent1"]
                    },
                    new BuildConfiguration
                    {
                        Name = "BuildConfig5",
                        CompatibleAgents = ["TestAgent1"],
                        BuildDependencies = ["BuildConfig4"]
                    }
                ],
                QueuedBuilds = [
                    new QueuedBuild
                    {
                        Name = "BuildConfig1"
                    }
                ]
            };
        }

        [Test]
        public void GetBuild_WhenBuildAdded_ThenItCanBeTakenById()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            var build = sut.NewBuild(settings.BuildConfigurations[0], CurrentTime);
            var result = sut.GetBuild(build.Id);
            result.Should().BeSameAs(build);
        }

        [Test]
        public void NewBuild_WhenBuildAdded_ThenItHasCorrectProperties()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            var build = sut.NewBuild(settings.BuildConfigurations[0], CurrentTime);
            build.AgentId.Should().BeNull();
            build.BuildConfiguration.Should().Be(settings.BuildConfigurations[0].Name);
            build.CreatedTime.Should().Be(CurrentTime);
            build.EndTime.Should().BeNull();
            build.StartTime.Should().BeNull();
            build.Id.Should().Be(1);
            build.State.Should().Be(BuildState.Created);
        }

        [Test]
        public void EnumerateBuilds_WhenCalled_ThenItReturnsAllBuilds()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            for (var i = 0; i < 3; i++)
            {
                sut.NewBuild(settings.BuildConfigurations[i], CurrentTime);
            }
            
            sut.EnumerateBuilds().Should().HaveCount(3);
        }

        [Test]
        public void GetWaitingForDependencies_WhenCalled_ThenItReturnsAllBuildsWaitingForDependencies()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            for (var i = 0; i < 3; i++)
            {
                var build = sut.NewBuild(settings.BuildConfigurations[i], CurrentTime);
                if (i > 0)
                {
                    build.State = BuildState.WaitingForDependencies;
                }
            }

            sut.GetWaitingForDependencies().Should().HaveCount(2);
        }

        [Test]
        public void GetWaitingForAgents_WhenCalled_ThenItReturnsAllBuildsWaitingForAgents()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            var builds = Enumerable.Range(0, 6)
                .Select(i => sut.NewBuild(settings.BuildConfigurations[0], CurrentTime))
                .ToArray();

            for (var i = 0; i < 4; i++)
            {
                if (i > 1)
                {
                    builds[i].AgentId = i;
                }

                builds[i].State = BuildState.WaitingForAgent;
            }

            sut.GetWaitingForAgents().Should().HaveCount(2);
        }

        [Test]
        public void GroupRunningBuildsByBuildConfiguration_WhenCalled_ThenItReturnsAllBuilds()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            for (var i = 0; i < 3; i++)
            {
                var build = sut.NewBuild(settings.BuildConfigurations[i], CurrentTime);
                build.State = BuildState.Running;
            }

            var result = sut.GroupRunningBuildsByBuildConfiguration();
            result.Should().HaveCount(3);
            for (var i = 0; i < 3; i++)
            {
                result[settings.BuildConfigurations[i].Name].Should().HaveCount(1);
            }
        }

        [Test]
        public void GetRunningBuildsCount_WhenCalled_ThenItCountsAllRunningBuilds()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            for (var i = 0; i < 3; i++)
            {
                var build = sut.NewBuild(settings.BuildConfigurations[i], CurrentTime);
                build.State = BuildState.Running;
            }

            var result = sut.GetRunningBuildsCount();
            result.Should().Be(3);
        }

        [Test]
        public void GroupQueueByBuildConfiguration_WhenCalled_ThenItReturnsAllBuilds()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            for (var i = 0; i < 3; i++)
            {
                sut.NewBuild(settings.BuildConfigurations[i], CurrentTime);
            }

            var result = sut.GroupQueueByBuildConfiguration();
            result.Should().HaveCount(3);
            for (var i = 0; i < 3; i++)
            {
                result[settings.BuildConfigurations[i].Name].Should().HaveCount(1);
            }
        }

        [Test]
        public void GetQueueLengtht_WhenCalled_ThenItCountsAllQueuedBuilds()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            for (var i = 0; i < 3; i++)
            {
                sut.NewBuild(settings.BuildConfigurations[i], CurrentTime);
            }

            var result = sut.GetQueueLength();
            result.Should().Be(3);
        }

        [Test]
        public void ResolveBuildConfigurationDependencies_WhenCalledWithRecursion_ThenItReturnsDistinctValues()
        {
            var settings = GetSomeSettings();
            var sut = new BuildsRepository(new SimulationPayload(settings));

            var result = sut.ResolveBuildConfigurationDependencies(settings.BuildConfigurations[0], true)
                .ToArray();

            result.Should().NotContain(settings.BuildConfigurations[0]);
            result.Should().OnlyHaveUniqueItems(s => s.Name);
            result.Should().HaveCount(4);
        }
    }
}
