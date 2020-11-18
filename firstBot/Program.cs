using System;
using System.IO;

namespace firstBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ConfigInitialization())
            {

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
                string data = "{\n  \"token\": \"\",\n  \"prefix\": \"\"\n}";
                using (var json = new StreamWriter("config.json"))
                {
                    json.Write(data);
                }
                return false;
            }
            return true;
        }
    }
}
