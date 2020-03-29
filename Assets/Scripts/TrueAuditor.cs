using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    class TrueAuditor
    {
        #region Variables
        public string MacAdress; // This will give us a unique key for our database.
        public string testA, testB, testC;
        public string score;
        public string dateTestCompleted;

        public string deviceCollectiveInfo;

        private IPGlobalProperties deviceProperties;
        private NetworkInterface[] nics;
        #endregion

        #region Constructors
        public TrueAuditor() { }

        public TrueAuditor(bool requestMacAddres, bool testA, bool testB, bool testC)
        {
            if (requestMacAddres)
                MacAdress = GettMyMacAddress();

            deviceCollectiveInfo = GetMyDeviceCollectiveInformation();

            if (testA)
                this.testA = "done";

            if (testB)
                this.testB = "done";

            if (testC)
                this.testC = "done";

            if (testA && testB && testC)
                score = Decimal.Round(Evaluator.totalScore).ToString();

            dateTestCompleted = GetCurrentDateTime();
        }


        #endregion

        #region Generate complemetary data
        private string GetMyDeviceCollectiveInformation()
        {
            return "Model:" + SystemInfo.deviceModel.ToString() + " Name: " + SystemInfo.deviceName.ToString() + " Type: " + SystemInfo.deviceType.ToString();
        }

        public String GetCurrentDateTime()
        {
            return DateTime.Now.ToString();
        }

        public String GettMyMacAddress()
        {
            string returned = null;
            deviceProperties = IPGlobalProperties.GetIPGlobalProperties();
            nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (returned == null || returned.Length <= 10) // Shortening only to device's mac address
                        returned = string.Concat(returned + (string.Format("{0}", bytes[i].ToString("X2"))));

                }
            }

            return returned;
        }

        #endregion
    }
}
