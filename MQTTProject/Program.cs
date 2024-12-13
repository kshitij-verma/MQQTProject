using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTProject;
using System;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using log4net.Config;
using log4net;
using System.Reflection;

class Program
{
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));
    static async Task Main(string[] args)
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        log.Info("LogTest!");

        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())  // Base directory where the app is running
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

        var folder = configuration.GetSection("DirectoryToPublishFrom").Value;
        var file = configuration.GetSection("FileToPublish").Value;

        //// Create an instance of MqttClientHandler
        IMqttOperations mqttClient = new MqttClientHandler();

        // Connect to the MQTT broker
        await mqttClient.ConnectAsync();

        var fileSubscriber = new FileSubscriber(mqttClient);
        // Subscribe to a file topic
        string fileTopic = "FileTOPIC/test";
        await fileSubscriber.SubscribeAsync(fileTopic);
        Thread.Sleep(1000);

        int x = 0;
        int y = 1;
        while (x != y)
        {
            //var subscriber = new Subscriber(mqttClient);
            //// Subscribe to a topic
            //string subscribeTopic = "mytopic/test";
            //await subscriber.SubscribeAsync(subscribeTopic);
            //Thread.Sleep(1000);

           

            //var publisher = new Publisher(mqttClient);
            //// Publish a message to the topic
            //string message = "Hello, MQTT!";
            //await publisher.PublishAsync(subscribeTopic, message);
            
            var filepublisher = new FilePublisher(mqttClient);
            //Publish a file message to the topic
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), folder, file);
            await filepublisher.PublishAsync(filepath, fileTopic);
        }

        // Wait for a key press before disconnecting
        Console.WriteLine("Press any key to disconnect...");
        Console.ReadKey();

        // Disconnect from the MQTT broker
        await mqttClient.DisconnectAsync();
    }
}

