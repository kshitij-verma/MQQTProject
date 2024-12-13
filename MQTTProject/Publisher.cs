using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTProject
{
    public class Publisher
    {
        private readonly IMqttOperations mqttClientHandler;

        public Publisher(IMqttOperations mqttClientHandler)
        {
            this.mqttClientHandler = mqttClientHandler;
        }

        public async Task PublishAsync(string topic, string message)
        {
            // Publish the message           
            await mqttClientHandler.PublishAsync(topic, message);
        }
    }
}
