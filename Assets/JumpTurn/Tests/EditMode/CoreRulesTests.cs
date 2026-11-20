using NUnit.Framework;

namespace JumpTurn.Tests
{
    public sealed class CoreRulesTests
    {
        [TestCase(0, 1, 1)]
        [TestCase(3, 1, 0)]
        [TestCase(0, -1, 3)]
        [TestCase(-5, 1, 0)]
        public void QuarterTurnWrapsCorrectly(int current, int delta, int expected)
        {
            Assert.That(QuarterTurn.Add(current, delta), Is.EqualTo(expected));
        }

        [Test]
        public void PhaseOnlyMatchesActiveLayer()
        {
            Assert.That(PhaseRules.IsColliderEnabled(StagePhase.Positive, StagePhase.Positive), Is.True);
            Assert.That(PhaseRules.IsColliderEnabled(StagePhase.Negative, StagePhase.Positive), Is.False);
        }

        [Test]
        public void TimeFormatUsesCentiseconds()
        {
            Assert.That(RunTimeFormat.Format(65.239f), Is.EqualTo("01:05.23"));
            Assert.That(RunTimeFormat.Format(-12f), Is.EqualTo("00:00.00"));
        }

        [Test]
        public void CatalogContainsFourRoomsAndThreeShards()
        {
            Assert.That(RoomCatalog.Count, Is.EqualTo(4));
            int shardCount = 0;
            for (int i = 0; i < RoomCatalog.Count; i++)
            {
                shardCount += RoomCatalog.Get(i).shards.Length;
            }

            Assert.That(shardCount, Is.EqualTo(3));
        }

        [Test]
        public void ExitUnlocksAtExpectedShardCount()
        {
            Assert.That(ShardRules.IsRoomUnlocked(0, 0), Is.True);
            Assert.That(ShardRules.IsRoomUnlocked(1, 0), Is.False);
            Assert.That(ShardRules.IsRoomUnlocked(1, 1), Is.True);
            Assert.That(ShardRules.IsRoomUnlocked(3, 3), Is.True);
        }

        [Test]
        public void SpikeLaneLeavesLandingAndTakeoffSpace()
        {
            RoomSpec room = RoomCatalog.Get(2);
            HazardSpec spikes = room.hazards[0];
            PlatformSpec spikePlatform = room.platforms[2];

            float platformLeft = spikePlatform.center.x - spikePlatform.size.x * 0.5f;
            float platformRight = spikePlatform.center.x + spikePlatform.size.x * 0.5f;
            float spikesLeft = spikes.center.x - spikes.size.x * 0.5f;
            float spikesRight = spikes.center.x + spikes.size.x * 0.5f;

            Assert.That(spikesLeft - platformLeft, Is.GreaterThanOrEqualTo(2.5f));
            Assert.That(platformRight - spikesRight, Is.GreaterThanOrEqualTo(0.75f));
            Assert.That(spikes.size.x, Is.LessThanOrEqualTo(1.3f));
        }
    }
}
