using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Management;

namespace DronePair
{
    public partial class Form1 : Form
    {
        UdpClient udp;
        int iSequence;

        public Form1()
        {
            InitializeComponent();

            udp = null;
            label7.Visible = false; //"Done" label starts hidden
            label9.Visible = false;
            iSequence = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (udp == null)
            {
                try
                {
                    udp = new UdpClient(5556); //Create our UDP Socket
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Please disconnect any controller apps that are connected to the drone.", "ERROR");
                    return;
                }
            }
            //Manually set the default Drone IP and End Point (contains port)
            IPAddress droneAddress = IPAddress.Parse("192.168.1.1");
            IPEndPoint droneEP = new IPEndPoint(droneAddress, 5556);

            //Our encoder to convert to string to bytes
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            //Find the Mac Address of Network Card that is using ip 192.168.1.X
            string szMacAddress = FindMacAddress();

            //If we found a mac address, send the command
            if (szMacAddress.Length > 0)
            {
                //Encode the command into a byte array
                byte[] byteCommand = new byte[256];
                string stringCommand = "AT*CONFIG="+(iSequence++)+",\"network:owner_mac\",\"" + szMacAddress + "\"\r";
                byteCommand = enc.GetBytes(stringCommand);

                if( udp.Client == null || !udp.Client.Connected )
                    //Connect to Drone
                    udp.Connect(droneEP);

                //Send Command
                udp.Send(byteCommand, stringCommand.Length);

                if( udp.Client.Connected )
                    //Disconnect from Drone.
                    udp.Close();

                udp = null;                   
                //Show "Done" label
                label7.Visible = true;
            }
            //This will only occur if there is no 192.168.1.X ip found on your system.
            else
            {
                MessageBox.Show("There seems to be an error!  Make sure you are connected to the AR Drone!", "ERROR");
            }
        }



        public string FindMacAddress()
        {
            //Query our Network Card information from the system
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            string szMacAddress = "";

            //Loop through all Network Cards found
            foreach (ManagementObject mo in moc)
            {
                //If there is no MacAddress, we can safely skip
                if (mo["MacAddress"] == null)
                    continue;

                //Convert MacAddress to string format
                szMacAddress = mo["MacAddress"].ToString();

                //Grab the list of IP Address on Network Card
                string[] szIPAddresses = (string[])mo["IPAddress"];

                //No IP, we can safely skip
                if (szIPAddresses == null) 
                    continue;

                //Loop through each IP in the Network Card
                for (int i = 0; i < szIPAddresses.Length; i++)
                {
                    //Skip if the IP does not have matching first 3 octets of the IP
                    if (!szIPAddresses[i].Contains("192.168.1"))
                        continue;

                    //If we got this far, the MacAddress could be a winner!
                    szMacAddress = mo["MacAddress"].ToString();

                    //If it fails here, you need to dump your Network Card
                    if (szMacAddress.Length == 0)
                        continue;

                    //Show the Mac Address to the User
                    textBox1.Text = szMacAddress;

                    return szMacAddress;
                }
            }

            return ""; //Didn't find anything =(
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (udp == null)
            {
                try
                {
                    udp = new UdpClient(5556); //Create our UDP Socket
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Please disconnect any controller apps that are connected to the drone.", "ERROR");
                    return;
                }
            }
            //Manually set the default Drone IP and End Point (contains port)
            IPAddress droneAddress = IPAddress.Parse("192.168.1.1");
            IPEndPoint droneEP = new IPEndPoint(droneAddress, 5556);

            //Our encoder to convert to string to bytes
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            //Find the Mac Address of Network Card that is using ip 192.168.1.X
            string szName = textBox2.Text;

            //If we found a mac address, send the command
            if (szName.Length > 0)
            {
                //Encode the command into a byte array
                byte[] byteCommand = new byte[256];
                string stringCommand = "AT*CONFIG=" + (iSequence++) + ",\"network:ssid_single_player\",\"" + szName + "\"\r";
                byteCommand = enc.GetBytes(stringCommand);

                if (udp.Client == null || !udp.Client.Connected)
                    //Connect to Drone
                    udp.Connect(droneEP);

                //Send Command
                udp.Send(byteCommand, stringCommand.Length);

                if (udp.Client.Connected)
                    //Disconnect from Drone.
                    udp.Close();

                udp = null;
                //Show "Done" label
                label9.Visible = true;
            }
            //This will only occur if there is no 192.168.1.X ip found on your system.
            else
            {
                MessageBox.Show("There seems to be an error!  Make sure you are connected to the AR Drone!", "ERROR");
            }
        }
    }
}
