using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace firstBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ConfigInitialization())
            {
                string jsonData = File.ReadAllText("config.json");
                var config = JsonSerializer.Deserialize<ConfigJson>(jsonData);
                if (config.Token == "")
                {
                    Console.WriteLine("Error! empty token");
                }
                else if (config.Prefix == "")
                {
                    Console.WriteLine("Error! empty prefix");
                }
                else
                {
                    var bot = new Bot(config.Token, config.Prefix);
                    bot.RunBot();
                }

            }
            else
                Console.WriteLine("Please fill in the valid details in generated json file");
        }

        static bool ConfigInitialization()
        {
            try
            {
                File.Open("config.json", FileMode.Open);
            }
            catch (FileNotFoundException _)
            {
                string jsonString = JsonSerializer.Serialize(new ConfigJson(),
                    new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    }) ;
                File.WriteAllText("config.json", jsonString);
                return false;
            }
            return true;
        }
    }
}
