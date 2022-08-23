using System;
using System.Text;
using System.Text.Json;
using Deliver.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Deliver.Controllers
{
    [Route("api/empresa")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private ILogger<EmpresaController> _logger;

        public EmpresaController(ILogger<EmpresaController> logger)
        {
            _logger = logger;
        }

        public IActionResult InsertEmpresa(Empresa empresa)
        {
            try
            {
                #region Inserir na fila

                var factory = new ConnectionFactory() {HostName = "localhost"};
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: 
                        "empresaQueue",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    string message = JsonSerializer.Serialize(empresa);
                    var body = Encoding.UTF8.GetBytes(message);
                    
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "empresaQueue",
                        basicProperties: null,
                        body: body
                        );
                    Console.WriteLine(" [x] Sent {0}", message);
                }
                #endregion
                return Accepted(empresa);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocorreu um erro ao adicionar a empresa");
                return new StatusCodeResult(500);
            } 
        }
    }
}