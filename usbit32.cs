using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

/*
USB Image Tool is based on a C# gui and a C++ Win32 interface.
Alexander Beug (http://alexpage.de) has been generous enough to share his I/O routine's API, as well as some comments
His comments have been tidied a bit, also wish to thank Christian Dosti for some translations. 
 */
class usbit32
{
    //--- get version of usbit32.dll (int 1530 for version 1.53)
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint CheckVersion();
    //--- check Windows version (min WinXP)
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint CheckWindows();
    //--- find USB devices (needs to be called to build a internal list of
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool FindDevices();
    //--- assign volumes/drives to the USB devices, needs to be called immidiately
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool FindVolumes();
    //--- clears the internal list of USB devices
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool ClearDevices();
    //returns the first found USB device, the device is identified by
    //the device number, you can see in the GUI as well, this number is
    //used for all routines, the physical switch is to signal, that you
    //are working in device mode, guess you should set it always, keep
    //in mind, that Vista/7 require admin rights for that, because you
    //call usbit32 now, your program would have to run in admin mode
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetFirstDevice(bool physical);
    //--- returns the next USB device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetNextDevice(bool physical);
    //--- returns the the Friendlyname of the USB-device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetFriendlyName(uint device, StringBuilder friendlyname, uint size);
    //--- returns the DevicePath of the USB device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetDevicePath(uint device, StringBuilder devicepath, uint size);
    //--- returns the DeviceID of the USB device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetDeviceID(uint device, StringBuilder deviceid, uint size);
    //--- returns the size of the USB device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern UInt64 GetDeviceSize(uint device);
    //--- returns the serial of the USB device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetSerialNumber(uint device, StringBuilder serialnumber, uint size);
    //--- returns the location of the USB device
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetLocationInfo(uint device, StringBuilder locationinfo, uint size);
    //--- returns the path of the USB drive
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetVolumePath(uint device, StringBuilder volumepath, uint size);
    //--- returns the name of the USB drive
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetVolumeName(uint device, StringBuilder volumename, uint size);
    //--- returns the filesystem of the USB drive
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetVolumeFS(uint device, StringBuilder volumefs, uint size);
    //--- returns the size of the USB drive
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern UInt64 GetVolumeSize(uint device);
    //--- returns the free memory of the USB drive
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern UInt64 GetVolumeFree(uint device);
    //--- return the progress in percent, to use this, it should be used from the GUI
    //--- thread (assuming, that you call the IO reoutines in a separate thread)
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetProgress(uint device);
    //--- check, if there is an operation running
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetRunning(uint device);
    //--- returns the current operation mode, 0 preparing, 1 reading, 2 writing
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetProgressMode(uint device);
    //--- shows the last Windows error code
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetLastErrorCode(uint device);
    //--- gets the Windows error text
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetLastErrorText(uint code, StringBuilder errtext, uint size);
    //--- make a backup, guess that's not so important for you - [nope it's not Alex :) -- sam]
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool BackupVolume(uint device, string backupfile, string backupfilename, uint compression, bool physical, bool sizeignore, bool checksum, ref uint error);
    //ok, here is the inportant part
    //device is the number you retrieved by the first next operations
    //restorefile is the full path to the image file
    //compression can be sent as a default but will be auto-detected if possible
    //physical means device mode, guess you should set this
    //sizeignore ignores size checks for gzip images
    //error returns an error code
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool RestoreVolume(uint device, string restorefile, uint compression, bool physical, bool sizeignore, bool trunc, ref uint error);
    //--- cancels the current operation
    [DllImport("usbit32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool CancelOperation(uint device, ref uint error);
}
