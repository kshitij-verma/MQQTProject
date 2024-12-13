using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTProject
{
    public interface IMqttOperations
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SubscribeAsync(string topic);
        Task PublishAsync(string topic, string message);
      
        event Action<string, string> MessageReceived;
        event Action<string, string> FileMessageReceived;
    }
}
