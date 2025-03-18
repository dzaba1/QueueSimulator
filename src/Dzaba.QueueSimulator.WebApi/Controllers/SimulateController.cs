using Dzaba.QueueSimulator.Lib;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.WebApi.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dzaba.QueueSimulator.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [HandleErrors]
    public class SimulateController : ControllerBase
    {
        private readonly ISimulation simulation;
        private readonly ICsvSerializer csvSerializer;

        public SimulateController(ISimulation simulation,
            ICsvSerializer csvSerializer)
        {
            ArgumentNullException.ThrowIfNull(simulation, nameof(simulation));
            ArgumentNullException.ThrowIfNull(csvSerializer, nameof(csvSerializer));

            this.simulation = simulation;
            this.csvSerializer = csvSerializer;
        }

        [HttpPost]
        [ValidateModel]
        public TimeEventData[] Post([FromBody][Required] SimulationSettings settings)
        {
            return simulation.Run(settings).ToArray();
        }

        [HttpPost("csv")]
        [ValidateModel]
        public string PostCsv([FromBody][Required] SimulationSettings settings)
        {
            var events = simulation.Run(settings);

            return csvSerializer.Serialize(events, settings).Trim();
        }
    }
}
