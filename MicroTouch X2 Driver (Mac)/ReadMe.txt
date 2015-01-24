// MicroTouch X2 Driver Installation Instructions

NOTES: This driver requires version 4.4 or above firmware in the MicroTouch unit or it will not work properly.

1. Install the SiLabs Virtual COM Port (VCP) USB driver using the file SiLabsUSBDriverDisk.dmg.

2. Copy the file “focuserlist MicroTouch.txt” to the directory “Resources/Common/Miscellaneous Files” in the package for TheSkyX.

3. Copy the file “MicroTouch.ui” to the directory “Resources/Common/Plugins/FocuserPlugins” in the package for TheSkyX.

4. Copy the file “libMicroTouch.dylib” to the directory “Resources/Common/Plugins/FocuserPlugins” in the package for TheSkyX.

5. Connect your MicroTouch and power it up.

6. Open up TheSkyX and find your Focuser Panel.

7. Use the Focuser Setup drop down and select Choose, then navigate to Starizona and then MicroTouch.

8. Then select Settings in the Focuser Setup drop down.  For Serial Port use the drop down and you should see an entry like “/dev/cu.SLAB_USBtoUART” to select.

9. Hit OK and then select Connect in the Focuser Setup drop down. Then close the dialogs.

10.  If everything is working you should see the current focuser position and temperature.

Note:  To get to the Resources directory find your TheSkyX application and perform a “Show  Package Contents” on the application file and then you will have a Contents folder to open.
