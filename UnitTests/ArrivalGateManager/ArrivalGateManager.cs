using Airflow.Mathematics;
using Airflow.Models.AirspaceModels.Regions;
using Java.Util;
using McMikeEngine.Mathematics._3D.Spherical;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Airflow.Models.Scheduling
{
    /// <summary>
    /// Stores aircraft times of entry into terminal airspace in order to simulate enroute spacing for procedurally generated arrivals
    /// Gates are defined as coordinates where aircraft flight paths intersect with the airspace boundary
    /// </summary>
    public class ArrivalGateManager
    {
        /// <summary>
        /// Minimum distance [radians] between arrivals as they enter the terminal airspace
        /// </summary>
        private float mMinSeparation;
        /// <summary>
        /// Minimum distance [nm] between arrivals as they enter the terminal airspace
        /// Default is 5nm
        /// </summary>
        public float MinSeparation
        {
            get => ATCSMath.RadiansToDistance(mMinSeparation);
            set
            {
                mMinSeparation = ATCSMath.DistanceToRadians(value);
            }
        }

        /// <summary>
        /// Primary storage of gate coordinates and times
        /// Key: A Coordinate defining a gate where an aircraft will enter the airspace
        /// Value: The time the aircraft is expected to enter the airspace at the gate
        /// </summary>
        private Dictionary<Coordinate, float> mGateTimes;

        /// <summary>
        /// Stores gates which are nearby neighbors
        /// Key: A Coordinate defining a gate
        /// Value: A HashSet of Coordinates which are within MinSeparation of the key gate
        /// </summary>
        private Dictionary<Coordinate, HashSet<Coordinate>> mNearGates;

        /// <summary>
        /// Gate expiration queue sorted for fast access to the earliest gate time
        /// Key: (float) Gate expiration time [s]
        /// Value: (HashSet<Coordinate>) Gates with the key expiration time
        /// </summary>
        private SortedList mExpQueue;

        /// <summary>
        /// A set of all coordinates with gate times stored
        /// </summary>
        public HashSet<Coordinate> Gates => new HashSet<Coordinate>(mGateTimes.Keys);

        public ArrivalGateManager()
        {
            mGateTimes = new Dictionary<Coordinate, float>();
            mNearGates = new Dictionary<Coordinate, HashSet<Coordinate>>();
            mExpQueue = new SortedList();

            MinSeparation = 5;
        }

        /// <summary>
        /// Stores a new gate time, or updates an existing gate time
        /// </summary>
        /// <param name="gate">A coordinate defining a gate where the aircraft path will intersect with the airspace boundary</param>
        /// <param name="t">The time the aircraft will cross the gate</param>
        public void SetGateTime(Coordinate gate, float t)
        {
            //New gate
            if (!mGateTimes.ContainsKey(gate))
            {
                //Init gate neighbors
                mNearGates[gate] = new HashSet<Coordinate>();

                //Find nearby gates
                foreach (var existingGate in mGateTimes.Keys)
                {
                    var dist = gate.DistanceTo(existingGate);

                    //Within distance
                    if (dist < mMinSeparation)
                    {
                        //Set gates as neighbors
                        mNearGates[gate].Add(existingGate);
                        mNearGates[existingGate].Add(gate);
                    }
                }
            }
            //Existing gate
            else
            {
                //Select the max time between new and existing times
                var oldTime = mGateTimes[gate];
                t = Math.Max(oldTime, t);

                //Remove existing expiration time
                if (mExpQueue.ContainsKey(oldTime))
                    ((HashSet<Coordinate>)mExpQueue[oldTime]).Remove(gate);
            }

            //Set gate time
            mGateTimes[gate] = t;

            //Add to expiration queue
            if (!mExpQueue.ContainsKey(t))
                mExpQueue[t] = new HashSet<Coordinate>();

            ((HashSet<Coordinate>)mExpQueue[t]).Add(gate);
        }

        public bool GateExists(Coordinate gate)
        {
            return mGateTimes.ContainsKey(gate);
        }

        /// <summary>
        /// Gets the time associated with the gate, 0 if gate is not stored
        /// </summary>
        /// <param name="coord">A coordinate representing a gate</param>
        /// <returns>The gate time [s] associated with the coordinate, 0 if not found</returns>
        public float GetGateTime(Coordinate coord)
        {
            if (mGateTimes.ContainsKey(coord))
                return mGateTimes[coord];

            return 0;
        }

        /// <summary>
        /// Gets the earliest gate time that a potential gate will be open
        /// The gate does NOT need to be an existing gate
        /// </summary>
        /// <param name="coord">A coordinate for a potential gate</param>
        /// <returns>Time [s] that this gate will be open</returns>
        public float GetGateOpenTime(Coordinate coord)
        {
            float coordTime = 0;

            //Find nearby gates
            foreach (var existingGate in mGateTimes.Keys)
            {
                var dist = coord.DistanceTo(existingGate);

                if (dist < mMinSeparation)
                {
                    var gateTime = mGateTimes[existingGate];

                    coordTime = Math.Max(gateTime, coordTime);
                }
            }

            return coordTime;
        }

        /// <summary>
        /// Removes gate
        /// </summary>
        /// <param name="gate"></param>
        public void RemoveGate(Coordinate gate)
        {
            //Remove from neighbor gates' nearby lists
            if (mNearGates.ContainsKey(gate))
            {
                foreach (var neighbor in mNearGates[gate])
                {
                    mNearGates[neighbor].Remove(gate);
                }
            }

            //Remove from expiration queue
            if (mGateTimes.ContainsKey(gate))
            {
                var gateTime = mGateTimes[gate];

                if (mExpQueue.ContainsKey(gateTime))
                    ((HashSet<Coordinate>)mExpQueue[gateTime]).Remove(gate);
            }

            //Remove the gate itself
            mGateTimes.Remove(gate);
            mNearGates.Remove(gate);
        }

        /// <summary>
        /// Removes expired gates
        /// </summary>
        /// <param name="t">Current time [s]</param>
        public void Update(float t)
        {
            //Remove expired gates
            while (mExpQueue.Count > 0 && t >= (float)mExpQueue.GetKey(0))
            {
                //Find coordinates that expire
                //Create a copy to avoid CollectionModifiedException while removing each gate
                var coords = new HashSet<Coordinate>((HashSet<Coordinate>)mExpQueue.GetByIndex(0));

                foreach (var coord in coords)
                    RemoveGate(coord);

                mExpQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears all gates and times
        /// </summary>
        public void Reset()
        {
            mGateTimes.Clear();
            mNearGates.Clear();
            mExpQueue.Clear();
        }
    }
}
