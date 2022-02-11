using Microsoft.Extensions.Configuration;
using NLog;
using ParkingManager.Database;
using ParkingManager.Database.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace ParkingManager.Services
{
    public class ParkingManagerService
    {

        private IConfiguration _configuration;
        private string _connectionString;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public ParkingManagerService(IConfiguration configuration)
        {
            _configuration = configuration;
            //_connectionString = _configuration.GetConnectionString("ConnectionString");
        }

        public OutgoingResponse ComputeTotalCostOfTicket(string start, string end)
        {
            DataAccess access = new DataAccess(_configuration);
            double cost = 0.0;
            //CultureInfo culture = new CultureInfo("en-US");
            DateTime startTime = Convert.ToDateTime(start);
            DateTime endTime = Convert.ToDateTime(end);
            TimeSpan diff = endTime - startTime;

            try
            {
                var rules = access.PullAllInPackingRules();

                if (rules.Count == 0)
                {
                    logger.Error("There are no rules in the PackingRules table");
                    return new OutgoingResponse();
                    
                }
                else
                {
                    var entrance = new PackingRules
                    {
                        Id = Convert.ToInt32(rules[0]["Id"]),
                        Description = rules[0]["RuleDescription"].ToString(),
                        cost = Convert.ToDouble(rules[0]["Amount"])
                    };
                    var fullorpatial = new PackingRules
                    {
                        Id = Convert.ToInt32(rules[1]["Id"]),
                        Description = rules[1]["RuleDescription"].ToString(),
                        cost = Convert.ToDouble(rules[1]["Amount"])
                    };
                    var successivefullorpartial = new PackingRules
                    {
                        Id = Convert.ToInt32(rules[2]["Id"]),
                        Description = rules[2]["RuleDescription"].ToString(),
                        cost = Convert.ToDouble(rules[2]["Amount"])
                    };

                    //add entrance cost
                    if (diff > TimeSpan.Zero)
                        cost += entrance.cost;

                    //check if time is up to or greater than 60 mins
                    if (diff >= TimeSpan.FromHours(1))
                        cost += fullorpatial.cost;

                    if (diff > TimeSpan.FromHours(1))
                    {
                        var successiveHours = diff - TimeSpan.FromHours(1);
                        var hours = Convert.ToInt32(successiveHours.TotalHours);

                        cost += (hours * successivefullorpartial.cost);
                    }


                    var ticket = access.InsertPackingTickets("Ticket", startTime.ToString("HH:mm"), endTime.ToString("HH:mm"), Convert.ToInt32(diff.TotalHours), cost, DateTime.UtcNow);

                    if (ticket)
                    {
                        logger.Info("Ticket has been saved into the database");
                    }
                    else
                    {
                        logger.Info("Ticket could not be saved into the database");
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e);
            }

            return new OutgoingResponse { cost = cost };
            
        }

        public OutgoingTicketResponse GetAllTicketsForADay(string date)
        {
            DataAccess access = new DataAccess(_configuration);

            List<PackingTickets> tickets = new List<PackingTickets>();

            try
            {
                tickets = access.PullAllTicketsForADay(date);

            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            return new OutgoingTicketResponse { Tickets = tickets };
        }
    }
}
