using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTProject;
using System;
using System.IO;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using log4net.Config;
using log4net;
using System.Reflection;


public class MqttClientHandler : IMqttOperations
{
    private static readonly ILog log = LogManager.GetLogger(typeof(MqttClientHandler));
    private readonly IMqttClient _mqttClient;
    private readonly IMqttClientOptions _options;
    private readonly IConfigurationRoot _configuration;
    public event Action<string, string> MessageReceived;
    public event Action<string, string> FileMessageReceived;
    private const int RETRY_TIME = 1000;
   
    public MqttClientHandler ()
    {


        // Set up config file appsettings.json
        _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())  // Base directory where the app is running
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

        var host = _configuration.GetSection("Host").Value;
        var port = int.Parse(_configuration.GetSection("Port").Value);
       
        // Create an instance of the MQTT client
        _mqttClient = new MqttFactory().CreateMqttClient();

        // Handle disconnection and implement custom reconnection logic
        _mqttClient.UseDisconnectedHandler(async e =>
        {
            Console.WriteLine("Disconnected from the MQTT broker.");
            // Attempt to reconnect
            await ReconnectAsync();
        });

        // Set up MQTT client options
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .Build();

        // Subscribe to the OnMessageReceived event of the MQTT client
        _mqttClient.UseApplicationMessageReceivedHandler(f =>
        {
            string topic = f.ApplicationMessage.Topic;
            string payload = System.Text.Encoding.UTF8.GetString(f.ApplicationMessage.Payload);

            MessageReceived?.Invoke(topic, payload);

            log.Info($"Subscriber messsage received on topic {topic}: {payload}");
        });

        // Subscribe to the OnFileMessageReceived event of the MQTT client
        _mqttClient.UseApplicationMessageReceivedHandler(e =>
        {
            string fileTopic = e.ApplicationMessage.Topic;
            string filePayload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            FileMessageReceived?.Invoke(fileTopic, filePayload);
        });

    }

    // Connect to the MQTT broker
    public async Task ConnectAsync()
    {
        try
        {
            // Establish the connection to the broker
            await _mqttClient.ConnectAsync(_options, CancellationToken.None);
            log.Info("Connected to the MQTT broker.");
        }
        catch (Exception ex)
        {
            log.Error($"Error connecting to MQTT broker: {ex.Message}");
        }
    }

    // Disconnect from the MQTT broker
    public async Task DisconnectAsync()
    {
        try
        {
            // Disconnect from the broker
            await _mqttClient.DisconnectAsync(CancellationToken.None);
            log.Info("Disconnected from the MQTT broker.");
        }
        catch (Exception ex)
        {
            log.Error($"Error disconnecting from MQTT broker: {ex.Message}");
        }
    }

    // Subscribe to a topic
    public async Task SubscribeAsync(string topic)
    {
        try
        {
            // Subscribe to the specified topic
             await _mqttClient.SubscribeAsync(topic);
            log.Debug ($"Subscribed to topic: {topic}");
        }
        catch (Exception ex)
        {
            log.Error($"Error subscribing to topic {topic}: {ex.Message}");
        }
    }
    // Publish a message to a topic
    public async Task PublishAsync(string topic, string message)
    {
        try
        {
            // Create the message
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithExactlyOnceQoS()
                .Build();

            // Publish the message
            await _mqttClient.PublishAsync(mqttMessage);
            log.Debug($"Published message to topic {topic}: {message}");
        }
        catch (Exception ex)
        {
            log.Error($"Error publishing message to {topic}: {ex.Message}");
        }
    }
    //Reconnect
    public async Task ReconnectAsync()
    {
        while (true)
        {
            Console.WriteLine($"Retrying to reconnect");
            await _mqttClient.ConnectAsync(_options);
            Thread.Sleep(RETRY_TIME);
        }
    }
}
