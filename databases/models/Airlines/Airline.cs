namespace Airflow.Models.AircraftModels
{
    public class Airline
    {
        /// <summary>
        /// ID defined by database
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// ICAO code, eg "UAL" for United Airlines
        /// </summary>
        public string ICAO { get; set; }

        /// <summary>
        /// IATA code, eg "UA" for United Airlines
        /// </summary>
        public string IATA { get; set; }

        /// <summary>
        /// Full name, eg "Alaska Airlines"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Radio callsign, eg "Springbok" for South African Airways
        /// </summary>
        public string Callsign { get; set; }

        public Airline()
        {
            ICAO = "";
            IATA = "";
            Name = "";
            Callsign = "";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}