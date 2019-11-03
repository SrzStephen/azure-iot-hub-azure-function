using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace EnvironmentSenseToDB
{
    public class Pms7003
    {
        public int PM1_0 { get; set; }
        public int PM2_5 { get; set; }
        public int PM10 { get; set; }
        public int r0_3 { get; set; }
        public int r0_5 { get; set; }
        public int r1_0 { get; set; }
        public int r2_5 { get; set; }
        public int r5_0 { get; set; }
        public int r10 { get; set; }
    }

    public class Mhz19
    {
        public int co2 { get; set; }
        public int temp { get; set; }
    }

    public static class EnvironmentSenseToDB
    {
        private static HttpClient client = new HttpClient();


        [FunctionName("EnvironmentSenseToDB")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "MyConn")]EventData message, ILogger log)
        {
            string constr = System.Environment.GetEnvironmentVariable("DBConn");

            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            System.DateTime timestamp = (System.DateTime)message.SystemProperties["iothub-enqueuedtime"];
            string ts_string = "'" + timestamp.ToString("MM/dd/yyyy HH:mm:ss.fff") + "'";
            string device_name = "'" + (string)message.SystemProperties["iothub-connection-device-id"] + "'";
            string message_source = (string)message.SystemProperties["iothub-message-source"];
            string message_value = Encoding.UTF8.GetString(message.Body.Array);
            string insert_command;
            JObject jobj;



            if (message_value.Contains("{") && message_value.Contains(","))
            {

                try
                {
                    List<string> val = new List<string>();
                    jobj = JObject.Parse(message_value);
                    insert_command = $"Insert INTO dbo.window (dt, window_open,device) VALUES ({ts_string},{jobj["window"].ToString()},{device_name});";
                }
                catch
                {

                    return;
                }

                if (jobj["pms7003"] != null)
                {
                    Pms7003 pms_data = JsonConvert.DeserializeObject<Pms7003>(jobj["pms7003"].ToString());
                    insert_command += $"Insert INTO dbo.dust (dt, PM1_0,PM2_5,PM10,R0_3,R0_5,R1_0,R2_5,R5_0,R10,device) VALUES ({ts_string},{pms_data.PM1_0},{pms_data.PM2_5},{pms_data.PM10}," +
                        $"{pms_data.r0_3},{pms_data.r0_5},{pms_data.r1_0},{pms_data.r2_5},{pms_data.r5_0},{pms_data.r10},{device_name});";
                }
                if (jobj["mhz19"] != null)
                {
                    Mhz19 mhz19_data = JsonConvert.DeserializeObject<Mhz19>(jobj["mhz19"].ToString());
                    insert_command += $"Insert INTO dbo.co2 (dt, co2,temp,device) VALUES ({ts_string},{mhz19_data.co2},{mhz19_data.temp},{device_name});";
                }
                else
                {
                    return;
                }


                using (SqlConnection conn = new SqlConnection(constr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(insert_command, conn))
                    {
                        var rows = cmd.ExecuteNonQuery();

                        log.LogInformation($"{rows} rows were updated");
                        conn.Close();
                    }
                }
            }

        }
    }
}



