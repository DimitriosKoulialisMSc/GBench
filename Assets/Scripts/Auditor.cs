using System;
using System.Net.NetworkInformation;
using UnityEngine;

/*<sumary>
 * Archived class
</summary>*/
[Serializable]
public class Auditor : MonoBehaviour
{
    
    public string MacAdress; // This will give us a unique key for our database.

    private IPGlobalProperties deviceProperties;
    private NetworkInterface[] nics;
    
    public Auditor(){}
    
    public Auditor(bool requestMacAddres) {
        if(requestMacAddres)
            this.MacAdress = GettMyMacAddress();
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
            for (int i = 0; i<bytes.Length; i++)
            {
                if(returned==null || returned.Length<=10) // Shortening only to device's mac address
                    returned = string.Concat(returned + (string.Format("{0}", bytes[i].ToString("X2"))));
/*                if (i != bytes.Length - 1)
                {          
                    if(returned.Length<=16)
                            returned = string.Concat(returned + "-");
                }*/
            }
        }
        return returned;
    }
}
