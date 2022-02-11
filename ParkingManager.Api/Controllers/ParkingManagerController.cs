using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using ParkingManager.Database.Models;
using ParkingManager.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingManager.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParkingManagerController : ControllerBase
    {
        private IConfiguration _configuration;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public ParkingManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "CALCULATE TOTAL PARKING COST")]
        [HttpPost]
        [Route("totalcost")]

        public JsonResult CalculateTotalCostAsync([FromBody]IncomingRequest request)
        {
            OutgoingResponse response = new OutgoingResponse();

            if (!ModelState.IsValid)
            {
                response = new OutgoingResponse { cost = 0.0 };
                var json = new JsonResult(response);
                json.StatusCode = 404;
                return json;
            }
            else
            {
                response = new ParkingManagerService(_configuration).ComputeTotalCostOfTicket(request.E, request.L);
                var json = new JsonResult(response);
                json.StatusCode = response.cost > 0.0 ? 200 : 400;
                return json;
            }
        }



        [SwaggerOperation(Summary = "GET LIST OF TICKETS")]
        [HttpGet]
        [Route("tickets/{date}")]

        public JsonResult GetAllTickets([FromRoute] string date)
        {
            OutgoingTicketResponse response = new OutgoingTicketResponse();

            if (!ModelState.IsValid)
            {
                response = new OutgoingTicketResponse();
                var json = new JsonResult(response);
                json.StatusCode = 404;
                return json;
            }
            else
            {
                response = new ParkingManagerService(_configuration).GetAllTicketsForADay(date);
                var json = new JsonResult(response);
                json.StatusCode = response.Tickets.Count > 0 ? 200 : 400;
                return json;
            }

            //return "";
        }
    }
}
