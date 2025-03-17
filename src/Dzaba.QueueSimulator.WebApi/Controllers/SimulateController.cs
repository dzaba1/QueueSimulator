using Dzaba.QueueSimulator.Lib;
using Dzaba.QueueSimulator.Lib.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dzaba.QueueSimulator.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimulateController : ControllerBase
    {
        private readonly ISimulation simulation;

        public SimulateController(ISimulation simulation)
        {
            ArgumentNullException.ThrowIfNull(simulation, nameof(simulation));

            this.simulation = simulation;
        }

        [HttpPost]
        public TimeEventData[] Post([FromBody][Required] SimulationSettings settings)
        {
            return simulation.Run(settings).ToArray();
        }
    }
}
