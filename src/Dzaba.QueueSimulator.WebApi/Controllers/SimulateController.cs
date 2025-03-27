using Dzaba.QueueSimulator.Lib;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.WebApi.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.QueueSimulator.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [HandleErrors]
    public class SimulateController : ControllerBase
    {
        private readonly ISimulation simulation;
        private readonly ICsvSerializer csvSerializer;
        private readonly ISimulationContext simulationContext;

        public SimulateController(ISimulation simulation,
            ICsvSerializer csvSerializer,
            ISimulationContext simulationContext)
        {
            ArgumentNullException.ThrowIfNull(simulation, nameof(simulation));
            ArgumentNullException.ThrowIfNull(csvSerializer, nameof(csvSerializer));
            ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

            this.simulation = simulation;
            this.csvSerializer = csvSerializer;
            this.simulationContext = simulationContext;
        }

        [HttpPost]
        [ValidateModel]
        public SimulationReport Post([FromBody][Required] SimulationSettings settings)
        {
            simulationContext.SetSettings(settings);
            return simulation.Run(settings);
        }

        [HttpPost("csv")]
        [ValidateModel]
        public string PostCsv([FromBody][Required] SimulationSettings settings)
        {
            var report = Post(settings);

            return csvSerializer.Serialize(report.Events, simulationContext.Payload).Trim();
        }
    }
}
