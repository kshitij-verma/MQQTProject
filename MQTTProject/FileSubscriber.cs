using log4net;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MQTTProject
{
    internal class FileSubscriber
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FileSubscriber));
        private readonly IMqttOperations mqttClientHandler;
        private List<byte> _receivedFileBytes = new List<byte>();
        private readonly IConfigurationRoot _configuration;
        public event EventHandler<MqttApplicationMessageReceivedEventArgs> OnFileMessageReceived;

        public FileSubscriber(IMqttOperations mqttClientHandler)
        {
            // Set up config file appsettings.json
            _configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())  // Base directory where the app is running
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            this.mqttClientHandler = mqttClientHandler;
            // Subscribe to the FileMessageReceived event from the mqttClientHandler
            this.mqttClientHandler.FileMessageReceived += FileMessageReceived;
        }

        public async Task SubscribeAsync(string topic)
        {
            await mqttClientHandler.SubscribeAsync(topic);
            log.Info($"Subscribed to File message topic: {topic} ");
        }

        private void FileMessageReceived(string topic, string message)
        {
            log.Debug ($"FILE Subscriber file received on topic {topic}: {message}");
            var rootDirectory = _configuration.GetSection("RootDirectory").Value;
            
            byte[] chunk = System.Text.Encoding.UTF8.GetBytes(message);
            _receivedFileBytes.AddRange(chunk);
            log.Debug($"Received chunk of {chunk.Length} bytes");

            DirectoryInfo rootSubscriberDir = Directory.CreateDirectory(rootDirectory);

            //Write the file to disk
            File.WriteAllBytes(rootSubscriberDir + "\\receivedFile.json", _receivedFileBytes.ToArray());
           log.Debug("File saved successfully.");
        }
    }
}
