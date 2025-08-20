namespace Airflow.Data
{
    public struct OperatorData
    {
        /// <summary>
        /// Airline ID defined by database
        /// </summary>
        public int Airline;

        /// <summary>
        /// Origin airport ID defined by database
        /// </summary>
        public int Origin;
        
        /// <summary>
        /// Destination airport ID defined by database
        /// </summary>
        public int Destination;
    }
}