using Airflow.Models.Scheduling;
using McMikeEngine.Mathematics._3D.Spherical;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airflow.Tests
{
    [TestClass]
    public class ArrivalGateManagerTests
    {
        /// <summary>
        /// Adds a gate and checks if that gain is retained by the manager
        /// </summary>
        [TestMethod]
        public void GateExists_ExactMatch_True()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);

            manager.SetGateTime(gate, 100);

            var exists = manager.GateExists(gate);

            Assert.IsTrue(exists);
        }

        /// <summary>
        /// Adds a gate and checks whether the manager treats a nearby gate as a distinct gate. Nearby gates should not be treated as the same
        /// </summary>
        [TestMethod]
        public void GateExists_NearbyCoordinate_False()
        {
            var manager = new ArrivalGateManager();

            var gate1 = new Coordinate(1.23400f, -0.842500f);
            var gate2 = new Coordinate(1.23401f, -0.842501f);

            manager.SetGateTime(gate1, 100);

            var exists = manager.GateExists(gate2);

            Assert.IsFalse(exists);
        }

        /// <summary>
        /// Adds a gate with same coordinate as an existing gate. Should update the gate time instead of adding a new gate. The gate count should still be 1.
        /// </summary>
        [TestMethod]
        public void SetGateTime_DuplicateGate_Count1()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);

            manager.SetGateTime(gate, 100);
            manager.SetGateTime(gate, 115);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(1, numGates);
        }

        /// <summary>
        /// Adds three distinct and far apart gates with the same times. Should have a gate count of 3.
        /// </summary>
        [TestMethod]
        public void SetGateTime_DistinctGates_Count3()
        {
            var manager = new ArrivalGateManager();

            var gate1 = new Coordinate(1.234f, -0.8425f);
            var gate2 = new Coordinate(0.142f, -0.4814f);
            var gate3 = new Coordinate(1.783f, -0.1976f);

            manager.SetGateTime(gate1, 100);
            manager.SetGateTime(gate2, 100);
            manager.SetGateTime(gate3, 100);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(3, numGates);
        }

        /// <summary>
        /// Updates an existing gate time
        /// Should still only have 1 gate
        /// </summary>
        [TestMethod]
        public void SetGateTime_NewTime_Count1()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);
            var time1 = 100;
            var time2 = 150;

            manager.SetGateTime(gate, time1);
            manager.SetGateTime(gate, time2);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(1, numGates);
        }

        /// <summary>
        /// Updates an existing gate time with a later time
        /// Should retain the latest time
        /// </summary>
        [TestMethod]
        public void SetGateTime_NewLaterTime_TimeUpdates()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);
            var time1 = 100;
            var time2 = 150;

            manager.SetGateTime(gate, time1);
            manager.SetGateTime(gate, time2);

            var gateTime = manager.GetGateTime(gate);

            Assert.AreEqual(time2, gateTime);
        }

        /// <summary>
        /// Updates an existing gate time with an earlier time
        /// Should retain the latest time
        /// </summary>
        [TestMethod]
        public void SetGateTime_NewEarlierTime_TimeUpdates()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);
            var time1 = 100;
            var time2 = 80;

            manager.SetGateTime(gate, time1);
            manager.SetGateTime(gate, time2);

            var gateTime = manager.GetGateTime(gate);

            Assert.AreEqual(time1, gateTime);
        }

        /// <summary>
        /// Sets a gate time, then sets a new, later time
        /// When calling the Update method with a time between the two times, the gate should not expire since the new time should be the later time
        /// Expects the gate count to still be 1
        /// </summary>
        [TestMethod]
        public void SetGateTime_NewTime_DoesNotExpire()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);
            var time1 = 100;
            var time2 = 150;

            manager.SetGateTime(gate, time1);
            manager.SetGateTime(gate, time2);

            manager.Update(125);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(1, numGates);
        }

        /// <summary>
        /// Sets a single gate time
        /// Should return the exact time entered
        /// </summary>
        [TestMethod]
        public void GetGateTime_Isolated_RemainsSame()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);
            var time = 100;

            manager.SetGateTime(gate, time);

            var gateTime = manager.GetGateTime(gate);

            Assert.AreEqual(time, gateTime);
        }

        /// <summary>
        /// Sets multiple nearby gate times
        /// Should return the exact time entered
        /// </summary>
        [TestMethod]
        public void GetGateTime_NearbyNeighbors_RemainsSame()
        {
            var manager = new ArrivalGateManager();

            var gate1 = new Coordinate(1.23400f, -0.842500f);
            var gate2 = new Coordinate(1.23401f, -0.842501f);
            var gate3 = new Coordinate(1.23402f, -0.842502f);

            var time1 = 100;
            var time2 = 115;
            var time3 = 130;

            manager.SetGateTime(gate1, time1);
            manager.SetGateTime(gate2, time2);
            manager.SetGateTime(gate3, time3);

            var gateTime = manager.GetGateTime(gate1);

            Assert.AreEqual(time1, gateTime);
        }

        /// <summary>
        /// Sets multiple far apart gate times
        /// Should return the exact time entered
        /// </summary>
        [TestMethod]
        public void GetGateTime_FarNeighbors_RemainsSame()
        {
            var manager = new ArrivalGateManager();

            var gate1 = new Coordinate(1.23400f, -0.842500f);
            var gate2 = new Coordinate(1.844f, -0.62f);
            var gate3 = new Coordinate(1.91f, -0.92f);

            var time1 = 100;
            var time2 = 115;
            var time3 = 130;

            manager.SetGateTime(gate1, time1);
            manager.SetGateTime(gate2, time2);
            manager.SetGateTime(gate3, time3);

            var gateTime = manager.GetGateTime(gate1);

            Assert.AreEqual(time1, gateTime);
        }

        /// <summary>
        /// Set a single gate time
        /// Should return the gate's entered time since there are no nearby gates
        /// </summary>
        [TestMethod]
        public void GetEarliestGateTime_Isolated_RemainsSame()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.234f, -0.8425f);
            var time = 100;

            manager.SetGateTime(gate, time);

            var gateTime = manager.GetGateOpenTime(gate);

            Assert.AreEqual(time, gateTime);
        }

        /// <summary>
        /// Set multiple nearby gates
        /// Should return the largest time of the three which is the earliest that the gate will be open
        /// </summary>
        [TestMethod]
        public void GetEarliestGateTime_NearbyNeighbors_SelectsMaxTime()
        {
            var manager = new ArrivalGateManager();

            var gate1 = new Coordinate(1.23400f, -0.842500f);
            var gate2 = new Coordinate(1.23401f, -0.842501f);
            var gate3 = new Coordinate(1.23402f, -0.842502f);

            var time1 = 100;
            var time2 = 115;
            var time3 = 130;

            manager.SetGateTime(gate1, time1);
            manager.SetGateTime(gate2, time2);
            manager.SetGateTime(gate3, time3);

            var gateTime = manager.GetGateOpenTime(gate1);

            Assert.AreEqual(time3, gateTime);
        }

        /// <summary>
        /// Sets multiple far apart gates
        /// Should return the same time entered for the gate time
        /// </summary>
        [TestMethod]
        public void GetEarliestGateTime_FarNeighbors_RemainsSame()
        {
            var manager = new ArrivalGateManager();

            var gate1 = new Coordinate(1.23400f, -0.842500f);
            var gate2 = new Coordinate(1.844f, -0.62f);
            var gate3 = new Coordinate(1.91f, -0.92f);

            var time1 = 100;
            var time2 = 115;
            var time3 = 130;

            manager.SetGateTime(gate1, time1);
            manager.SetGateTime(gate2, time2);
            manager.SetGateTime(gate3, time3);

            var gateTime = manager.GetGateOpenTime(gate1);

            Assert.AreEqual(time1, gateTime);
        }

        /// <summary>
        /// Sets a gate time and calls the update method at a time after the gate time
        /// Should see a gate count of 0 since the gate expired
        /// </summary>
        [TestMethod]
        public void Update_Expired_Count0()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.23400f, -0.842500f);
            var time = 100;

            manager.SetGateTime(gate, time);
            manager.Update(time + 10);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(0, numGates);
        }

        /// <summary>
        /// Sets a gate time and calls the update method at a time equal to the gate time
        /// Should see a gate count of 0 since the gate just expired
        /// </summary>
        [TestMethod]
        public void Update_JustExpired_Count0()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.23400f, -0.842500f);
            var time = 100;

            manager.SetGateTime(gate, time);
            manager.Update(time);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(0, numGates);
        }

        /// <summary>
        /// Sets a gate time and calls the update method at a time before the gate time
        /// Should see a gate count of 1 since the gate did not expire
        /// </summary>
        [TestMethod]
        public void Update_NotExpired_Count1()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.23400f, -0.842500f);
            var time = 100;

            manager.SetGateTime(gate, time);
            manager.Update(time - 10);

            var numGates = manager.Gates.Count;

            Assert.AreEqual(1, numGates);
        }

        /// <summary>
        /// Calls the reset method after setting a gate time
        /// Should see a gate count of 0
        /// </summary>
        [TestMethod]
        public void Reset_Count0()
        {
            var manager = new ArrivalGateManager();

            var gate = new Coordinate(1.23400f, -0.842500f);
            var time = 100;

            manager.SetGateTime(gate, time);
            manager.Reset();

            var numGates = manager.Gates.Count;

            Assert.AreEqual(0, numGates);
        }
    }
}
