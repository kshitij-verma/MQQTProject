using log4net;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTProject
{
    public class FilePublisher
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FilePublisher));
        private readonly IMqttOperations mqttClientHandler;
        private const int MaxPayLoadSize = 1024;
        private List<byte> _receivedFileBytes = new List<byte>();
        private int _expectedFileSize = 0;

        public FilePublisher(IMqttOperations mqttClientHandler)
        {
            this.mqttClientHandler = mqttClientHandler;
        }

        public async Task PublishAsync(string filepath, string topic)
        {
            byte[] fileBytes = File.ReadAllBytes(filepath);
            int totalChunks = (int)Math.Ceiling(fileBytes.Length / (double)MaxPayLoadSize);

            log.Debug($"Total Chunks: {totalChunks}");

            for (int i = 0; i < totalChunks; i++)
            {
                int chunkSize = Math.Min(MaxPayLoadSize, fileBytes.Length - i * MaxPayLoadSize);
                byte[] chunk = new byte[chunkSize];
                Array.Copy(fileBytes, i * MaxPayLoadSize, chunk, 0, chunkSize);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(chunk)
                    .WithExactlyOnceQoS()
                    .Build();

                await mqttClientHandler.PublishAsync(message.Topic, Encoding.UTF8.GetString(chunk));
                log.Debug($"Published chunk of {i + 1 / totalChunks}");
            }
        }
    }
}
