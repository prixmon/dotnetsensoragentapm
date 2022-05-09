using System.Web;


namespace SensorAgentAPM
{
    public class SensorAPMIdentity
    {
        public string SensorAgentCommunicatorURL;// Sensor Agent Communicator URL
        public string OwnerId; // Owner ID which is provided inside Prixmon dashboard
        public string ProjectId;// Project Id
        public string Username;// APM Username
        public string Password;// APM Password
        public bool IsSecure;
    }
    public class APM
    {
        private static readonly HttpClient client = new HttpClient();

        private static Dictionary<string, DateTime> sensorAPMData;
        private static SensorAPMIdentity prvSensorAPMID;
        public static void InitSensorAPM(SensorAPMIdentity sensorAPMIdentity)
        {
            prvSensorAPMID = sensorAPMIdentity;
        }

        public static bool StartMetric(string methodName)
        {
            bool Result = !sensorAPMData.ContainsKey(methodName);
            if (Result)
            {
                sensorAPMData.Add(methodName, DateTime.Now);
            }
            return Result;
        }
        private static async Task submitToCommunicatorAsync(string methodName, TimeSpan methodRunDuration, DateTime startTime, DateTime EndTime)
        {
            var values = new Dictionary<string, string> { { "U", prvSensorAPMID.Username }, { "P", prvSensorAPMID.Password } };

            var authData = new FormUrlEncodedContent(values);
            string communicatorURL = prvSensorAPMID.SensorAgentCommunicatorURL + "/saveapm?oid=" + prvSensorAPMID.OwnerId + "&pid=" + HttpUtility.UrlEncode(prvSensorAPMID.ProjectId) + "&pm=" + HttpUtility.UrlEncode(methodName) + "&tm=" + methodRunDuration.Milliseconds + "&st=" + startTime.ToString() + "&et=" + EndTime.ToString();
            if (prvSensorAPMID.IsSecure)
            {
                communicatorURL = "https://" + communicatorURL;
            }
            else
            {
                communicatorURL = "http://" + communicatorURL;
            }

            await client.PostAsync(communicatorURL, authData);


        }
        public static void FinilizeMetric(string methodName)
        {
            TimeSpan methodRunDuration = DateTime.Now.Subtract(sensorAPMData[methodName]);
            submitToCommunicatorAsync(methodName, methodRunDuration, sensorAPMData[methodName], DateTime.Now);
            sensorAPMData.Remove(methodName);
        }
    }


}