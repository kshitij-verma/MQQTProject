using log4net;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTProject
{

    public class Subscriber
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Subscriber));
        private readonly IMqttOperations mqttClientHandler;
        public event EventHandler<MqttApplicationMessageReceivedEventArgs> OnMessageReceived;

        public Subscriber(IMqttOperations mqttClientHandler)
        {
            this.mqttClientHandler = mqttClientHandler;
            // Subscribe to the MessageReceived event from the mqttClientHandler
            this.mqttClientHandler.MessageReceived += MessageReceived;
        }

        public async Task SubscribeAsync(string topic)
        {
            await  mqttClientHandler.SubscribeAsync(topic);
            log.Debug($"Subscribed to message topic: {topic} ");
        }

        private void MessageReceived(string topic, string message)
        {
            log.Debug($"Subscriber messsage received on topic {topic}: {message}");
        }
    }
}
