using Microsoft.Extensions.Configuration;
using NLog;
using ParkingManager.Database.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace ParkingManager.Database
{
    public class DataAccess
    {
        private readonly IConfiguration Configuration;
        public string _connectionString;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public DataAccess(IConfiguration configuration)
        {
            Configuration = configuration;
            _connectionString = Configuration.GetConnectionString("ConnectionString");
        }

        public List<Dictionary<string, object>> PullAllInPackingRules()
        {
            var serialisedResult = new List<Dictionary<string, object>>();

            using (IDbConnection dbConnection = new DbConnection(_connectionString).Connection)
            {
                dbConnection.Open();
                SqlCommand command;
                SqlDataReader reader;

                //string query = $"select RegistrationId from CardGenerationDB.dbo.{table} where photoName = '{PhotoName}'";
                string query = $"select * from PackingRules";

                try
                {
                    command = new SqlCommand(query, (SqlConnection)dbConnection);
                    //Task task = new Task(() => command.ExecuteReader());
                    //await Task.FromResult(task);

                    reader = command.ExecuteReader();

                    serialisedResult = Serialize(reader);

                    logger.Info(serialisedResult);

                }catch(Exception e)
                {
                    logger.Error(e);
                }

                return serialisedResult;

            }
           
        }

        public bool InsertPackingTickets(string name, string entrytime, string exittime, int hoursspent, double amounttopay, DateTime date)
        {
            var serialisedResult = new List<Dictionary<string, object>>();
            //DataTable schema = null;

            using (IDbConnection dbConnection = new DbConnection(_connectionString).Connection)
            {
                dbConnection.Open();
                SqlCommand command;
                SqlDataReader reader;
                bool inserted = false;

                //string query = $"select RegistrationId from CardGenerationDB.dbo.{table} where photoName = '{PhotoName}'";
                //string query = $" * from PackingTickets";
                //string query = $"USE db_a483f5_usertest; DESCRIBE ParkingTickets; ";
                string query = $"insert into PackingTickets (Name, EntryTime, ExitTime, HoursSpent, AmountToPay, Date) values ('{name}', '{entrytime}', '{exittime}', {hoursspent}, {amounttopay}, '{date}') ";

                try
                {
                    command = new SqlCommand(query, (SqlConnection)dbConnection);

                    inserted = command.ExecuteNonQuery() > 0;


                }
                catch (Exception e)
                {
                    logger.Error(e);
                }

                return inserted;

            }

        }

        public List<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }

        private Dictionary<string, object> SerializeRow(List<string> cols, SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        public List<PackingTickets> PullAllTicketsForADay(string date)
        {
            var serialisedResult = new List<Dictionary<string, object>>();
            List<PackingTickets> tickets = new List<PackingTickets>();

            using (IDbConnection dbConnection = new DbConnection(_connectionString).Connection)
            {
                dbConnection.Open();
                SqlCommand command;
                SqlDataReader reader;

                //date = 
                //string query = $"select RegistrationId from CardGenerationDB.dbo.{table} where photoName = '{PhotoName}'";
                string query = $"select * from PackingTickets where Date = '{date}' ";

                try
                {
                    command = new SqlCommand(query, (SqlConnection)dbConnection);
                    //Task task = new Task(() => command.ExecuteReader());
                    //await Task.FromResult(task);

                    reader = command.ExecuteReader();

                    serialisedResult = Serialize(reader);

                    for(int i=0; i<serialisedResult.Count; i++)
                    {
                        tickets.Add(new PackingTickets
                        {
                            Id = Convert.ToInt32(serialisedResult[i]["Id"]),
                            Name = serialisedResult[i]["Name"].ToString(),
                            EntryTime = serialisedResult[i]["EntryTime"].ToString(),
                            ExitTime = serialisedResult[i]["ExitTime"].ToString(),
                            HoursSpent =  Convert.ToInt32(serialisedResult[i]["HoursSpent"]),
                            AmountToPay = Convert.ToDouble(serialisedResult[i]["AmountToPay"]),
                            Date = Convert.ToDateTime(serialisedResult[i]["Date"])
                        });
                    }


                }
                catch (Exception e)
                {
                    logger.Error(e);
                }

                return tickets;

            }
        }
    }
}
