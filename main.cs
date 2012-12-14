using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

namespace installer
{
    public partial class main : Form
    {
        // Location of the XML file which holds the mirrors
        private static string mirrorXMLFile = "https://raw.github.com/xbianonpi/wiki/master/mirrors.xml";
        private static uint noUSBDeviceSelectedUint = 4294967290;

        // List with all the  USB devices & versions
        private List<uint> USBDevices;
        private List<version> versions;

        // Window for showing the progress
        private ProgressMeter windowProgressMeter;

        // WebClient for downloading files
        private WebClient webClient;

        // Selected items
        private version selectedVersion;
        private uint selectedUSBDevice;

        public main()
        {
            InitializeComponent();

            // Loading devices & versions
            this.listDevices();
            this.loadVersions();

            // Setting up the webClient
            this.webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
            webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
            this.updateUI();

            // Init selected variables
            this.selectedVersion = null;
            this.selectedUSBDevice = noUSBDeviceSelectedUint;
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Until the the download is finished the name of the file is "temp", rename it to selected XBian version
            version ver = this.versions[comboBoxVersions.SelectedIndex];
            File.Move(@"temp", ver.getArchiveName());
            this.initRestore();
        }

        public void loadVersions()
        {            
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(mirrorXMLFile);
            }
            catch (WebException ex)
            {
                MessageBox.Show("Unable to connect to the XBian server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            
            }

            XmlNodeList XMLverName = xmlDoc.GetElementsByTagName("name");
            XmlNodeList XMLlocations = xmlDoc.GetElementsByTagName("locations");

            this.versions = new List<version>();
            for (int i = 0; i < XMLverName.Count; i++)
            {
                string verName = XMLverName[i].InnerText;
                string[] locations = XMLlocations[i].InnerText.Split(';');
                version ver = new version(verName, locations);
                this.versions.Add(ver);
            }

            comboBoxVersions.Items.Clear();

            bool latest = true;
            foreach (version v in versions)
            {
                if (latest)
                {
                    comboBoxVersions.Items.Add(v.getVersionName() + " (latest)");
                    latest = false;
                }
                else
                    comboBoxVersions.Items.Add(v.getVersionName());
            }
        }

        private void listDevices()
        {
            // TODO ADD drive letter
            this.comboBoxSDcard.Items.Clear();
            this.USBDevices = new List<uint>();
            usbit32.ClearDevices();
            usbit32.FindDevices();
            usbit32.FindVolumes();
            uint usbdevice = usbit32.GetFirstDevice(true);          
            System.Text.StringBuilder deviceName = new System.Text.StringBuilder(100);
 
            while ((usbdevice != 0))
            {
                usbit32.GetFriendlyName(usbdevice, deviceName, 100);
                this.comboBoxSDcard.Items.Add(deviceName.ToString());
                this.USBDevices.Add(usbdevice);
                usbdevice = usbit32.GetNextDevice(true);        
            }
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        
        private void comboBoxVersions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxVersions.SelectedItem != null)
            {
                this.selectedVersion = this.versions[comboBoxVersions.SelectedIndex];
                this.updateUI();
            }
        }

        private void updateUI()
        {
            if (this.selectedVersion != null)
            {
                groupBox2.Enabled = true;
                groupBox1.ForeColor = Color.Green;

                if (this.selectedUSBDevice != noUSBDeviceSelectedUint)
                {
                    groupBox3.Enabled = true;
                    groupBox2.ForeColor = Color.Green;
                }
                else
                {
                    groupBox2.ForeColor = Color.Black;
                }
            }
            else 
            {
                groupBox1.ForeColor = Color.Black;
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;
            }
        }

        private void reset()
        {
            this.selectedVersion = null;
            this.selectedUSBDevice = noUSBDeviceSelectedUint;
            this.updateUI();
            this.comboBoxSDcard.SelectedItem = null;
            this.comboBoxVersions.SelectedItem = null;
        }

        private void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.windowProgressMeter.setProgress(e.ProgressPercentage, this.selectedVersion.getVersionName(), windowType.DOWNLOAD);
        }

        private void InstallBtn_Click(object sender, System.EventArgs e)
        {
            // Checking if the file is already downloaded
            if (System.IO.File.Exists(this.selectedVersion.getArchiveName()))
            {
                // Version has already been downloaded
                MessageBox.Show("Version already downloaded, continuing", "XBian installer");
                this.initRestore();
                this.windowProgressMeter = new ProgressMeter();
                this.windowProgressMeter.Show();
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Need to download this version, continue?", "XBian installer", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    this.webClient.DownloadFileAsync(new Uri(this.selectedVersion.getRandomMirror()), "temp");
                    this.windowProgressMeter = new ProgressMeter();
                    this.windowProgressMeter.Show();
                }
            } 
        }

        public void initRestore()
        {
            this.installTimer.Enabled = true;
            Thread t = new Thread(restore);
            t.Start();
        }

        private void restore()
        {
            UInt32 RestoreErrorNum = 0;
            usbit32.RestoreVolume(this.selectedUSBDevice, this.selectedVersion.getArchiveName(), 1, true, true, true, ref RestoreErrorNum);
            MessageBox.Show("XBian is succesfully installed on your SD Card, plug it into your Raspberry pi now");

            this.Invoke((MethodInvoker)delegate
            {
                this.installTimer.Stop();
                this.windowProgressMeter.Close();
                this.reset();
            });
        }

        private void installTimer_Tick(object sender, System.EventArgs e)
        {
            Console.WriteLine(usbit32.GetProgress(this.selectedUSBDevice));
            this.windowProgressMeter.setProgress(Convert.ToInt16(usbit32.GetProgress(this.selectedUSBDevice)) * 12, this.selectedVersion.getVersionName(), windowType.INSTALL);
        }

        private void comboBoxSDcard_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.comboBoxSDcard.SelectedItem != null)
            {
                this.selectedUSBDevice = Convert.ToUInt16(this.USBDevices[this.comboBoxSDcard.SelectedIndex]);
                this.updateUI();
            }
        }
    }
}

