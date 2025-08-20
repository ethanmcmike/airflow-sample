using Airflow.Assets.Databases;
using Airflow.Models.AircraftModels;
using Airflow.Models.Airlines;
using McMikeEngine.Assets.Databases;
using McMikeEngine.Mathematics._3D.Spherical;
using System;
using System.Collections.Generic;

namespace Airflow.Data
{
    /// <summary>
    /// Database adapter for the Airflow core database
    /// Partial class for airline functionality
    /// </summary>
    public partial class AirflowAdapter
    {
        /// <summary>
        /// Selects airline data based on ID
        /// </summary>
        /// <param name="id">The airline ID defined by the database</param>
        /// <returns>Airline data matching the given ID, or null if not found</returns>
        public Airline GetAirline(int id)
        {
            var script = GetScript("select_airline");
            script.SetValue("id", id);

            var reader = ExecuteQuery(script);

            if (reader.Read())
            {
                var airline = new Airline();
                airline.ID = reader.GetInt32(0);
                airline.ICAO = reader.GetString(1);
                airline.IATA = reader.GetString(2);
                airline.Name = reader.GetString(3);
                airline.Callsign = reader.GetString(4);

                return airline;
            }

            return null;
        }

        /// <summary>
        /// Selects airline data based on ICAO
        /// </summary>
        /// <param name="icao">The airline ICAO code, eg "UAL" for United Airlines</param>
        /// <returns>Airline data matching the given ICAO, or null if not found</returns>
        public Airline GetAirline(string icao)
        {
            var script = GetScript("select_airline_by_icao");
            script.SetValue("icao", icao);

            var reader = ExecuteQuery(script);

            if (reader.Read())
            {
                var airline = new Airline();
                airline.ID = reader.GetInt32(0);
                airline.ICAO = reader.GetString(1);
                airline.IATA = reader.GetString(2);
                airline.Name = reader.GetString(3);
                airline.Callsign = reader.GetString(4);

                return airline;
            }

            return null;
        }

        public Airline GetAirlineByCallsign(string callsign)
        {
            var script = GetScript("select_airline_by_callsign");
            script.SetValue("callsign", callsign);

            var reader = ExecuteQuery(script);

            if (reader.Read())
            {
                var airline = new Airline();
                airline.ID = reader.GetInt32(0);
                airline.ICAO = reader.GetString(1);
                airline.IATA = reader.GetString(2);
                airline.Name = reader.GetString(3);
                airline.Callsign = reader.GetString(4);

                return airline;
            }

            return null;
        }

        /// <summary>
        /// Selects multiple airlines' data based on IDs
        /// </summary>
        /// <param name="ids">Airline IDs defined by the database</param>
        /// <returns>A Dictionary containing Airline objects corresponding to each ID</returns>
        public Dictionary<int, Airline> GetAirlines(IEnumerable<int> ids)
        {
            var result = new Dictionary<int, Airline>();

            var script = GetScript("select_airlines");

            //Create CSV list of IDs
            var idList = string.Join(",", ids);
            script.SetValue("ids", idList);

            var reader = ExecuteQuery(script);

            while (reader.Read())
            {
                var airline = new Airline();
                airline.ID = reader.GetInt32(0);
                airline.ICAO = reader.GetString(1);
                airline.IATA = reader.GetString(2);
                airline.Name = reader.GetString(3);
                airline.Callsign = reader.GetString(4);

                result[airline.ID] = airline;
            }

            return result;
        }

        /// <summary>
        /// Selects an airline's ID based on ICAO
        /// </summary>
        /// <param name="icao">The airline ICAO code</param>
        /// <returns>In integer ID for the airline defined by the database, 0 if not found</returns>
        public int GetAirlineID(string icao)
        {
            var script = GetScript("select_airline_id");
            script.SetValue("icao", icao);

            var reader = ExecuteQuery(script);

            if (reader.Read())
            {
                var id = reader.GetInt32(0);

                return id;
            }

            return 0;
        }

        /// <summary>
        /// Selects an airline's ICAO code based on ID
        /// </summary>
        /// <param name="id">An airline's ID defined by the database</param>
        /// <returns>A string ICAO code for the given airline, null if not found</returns>
        public string GetAirlineICAO(int id)
        {
            var script = GetScript("select_airline_icao");
            script.SetValue("id", id);

            var reader = ExecuteQuery(script);

            if (reader.Read())
            {
                var icao = reader.GetString(0);
                return icao;
            }

            return null;
        }

        /// <summary>
        /// Selects airline IDs that operator out of the given airport ID 
        /// </summary>
        /// <param name="airportID">An airport ID defined by the database</param>
        /// <returns>A list of OperatorData that contains the given airport ID as either a route origin or destination for an airline</returns>
        public List<OperatorData> GetAirportOperators(int airportID)
        {
            var result = new List<OperatorData>();

            var script = GetScript("select_airport_operators");
            script.SetValue("airport_id", airportID);

            using (var reader = ExecuteQuery(script))
            {
                while (reader.Read())
                {
                    var airline = reader.GetInt32(0);
                    var orig = reader.GetInt32(1);
                    var dest = reader.GetInt32(2);

                    var data = new OperatorData()
                    {
                        Airline = airline,
                        Origin = orig,
                        Destination = dest
                    };

                    result.Add(data);
                }
            }

            return result;
        }

        /// <summary>
        /// Selects airline
        /// </summary>
        /// <param name="airlineID"></param>
        /// <returns></returns>
        public Dictionary<int, int> GetFleet(int airlineID)
        {
            var result = new Dictionary<int, int>();

            var script = GetScript("select_airline_fleet");
            script.SetValue("airline_id", airlineID);

            using (var reader = ExecuteQuery(script))
            {
                while (reader.Read())
                {
                    var type = reader.GetInt32(0);
                    var count = reader.GetInt32(1);

                    result[type] = count;
                }
            }

            return result;
        }

        public Dictionary<int, Dictionary<int, int>> GetFleets(IEnumerable<int> airlineIDs)
        {
            var result = new Dictionary<int, Dictionary<int, int>>();

            var script = GetScript("select_airline_fleets");

            var ids = "";

            foreach (var id in airlineIDs)
                ids += $"{id},";
            ids = ids.TrimEnd(',');

            script.SetValue("airline_ids", ids);

            using (var reader = ExecuteQuery(script))
            {
                while (reader.Read())
                {
                    var airline = reader.GetInt32(0);
                    var type = reader.GetInt32(1);
                    var count = reader.GetInt32(2);

                    if (!result.ContainsKey(airline))
                        result[airline] = new Dictionary<int, int>();

                    result[airline][type] = count;
                }
            }

            return result;
        }

#if WRITE

        public void InsertAirlineRoute(string airlineIcao, string originIcao, string destIcao, string aircraftTypeIcao, int altitude)
        {
            var airline = GetAirlineID(airlineIcao);                                //TODO Pass ICAOs to script and find IDs in SQL
            var origin = GetAirportID(originIcao);
            var dest = GetAirportID(destIcao);
            var aircraftType = GetAircraftTypeId(aircraftTypeIcao);

            if (airline == 0)
                throw new Exception($"Could not find airline {airlineIcao}");

            if (origin == 0)
                throw new Exception($"Could not find airport {originIcao}");

            if (dest == 0)
                throw new Exception($"Could not find airport {destIcao}");

            if (aircraftType == 0)
                throw new Exception($"Could not find aircraft type {aircraftTypeIcao}");

            var values = $"({airline},{origin},{dest},{aircraftType},{altitude})";

            var script = GetScript("insert_airline_route");
            script.SetValue("values", values);
            ExecuteNonQuery(script);
        }

        public void InsertRoute(string region, string originIcao, string destIcao, string route)
        {
            var origin = GetAirportID(originIcao);
            var dest = GetAirportID(destIcao);

            var values = $"({origin},{dest},'{route}')";

            var script = GetScript("insert_route");
            script.SetValue("region", region);
            script.SetValue("values", values);
            ExecuteNonQuery(script);
        }

        public void Insert(IEnumerable<Airline> airlines)
        {
            var script = GetScript("insert_airlines");

            var values = "";

            foreach (var airline in airlines)
            {
                var name = airline.Name.Replace("'", "\'");

                values += $"('{airline.ICAO}','{airline.IATA}','{name}','{airline.Callsign}'),";
            }
            values = values.TrimEnd(',');

            script.SetValue("values", values);

            ExecuteNonQuery(script);
        }

        /// <summary>
        /// Copies from "airline_data" table
        /// </summary>
        public int ImportAirlines(string tableName)
        {
            var script = GetScript("import_airline_data");
            script.SetValue("table_name", tableName);

            using (var reader = ExecuteQuery(script))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }

        public int ImportOperators(string tableName)
        {
            var script = GetScript("import_operators");
            script.SetValue("table_name", tableName);

            using (var reader = ExecuteQuery(script))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }

        public int ImportFleets(string tableName)
        {
            var script = GetScript("import_fleets");
            script.SetValue("table_name", tableName);

            using (var reader = ExecuteQuery(script))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }

#endif

    }
}