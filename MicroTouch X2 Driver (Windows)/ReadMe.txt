// MicroTouch X2 Driver (Windows) Installation Instructions

Note: I'm showing the file paths for my TheSkyX installation modify the driver letter and folder as needed if you installed it somewhere else.

1. Install the MicroTouch USB driver provided by Starizona.  I've also include a copy here in the file MicroTouch_Install_3_8_NoASCOM.zip.  There are a lot of instructions in there but you really should only need to run the MicroTouchUSB2Installer.exe file possibly as Administrator.

2. Copy the files MicroTouch.ui, MicroTouchX2Driver.dll, and SiUSBXp.dll to the directory C:\Program Files (x86)\Software Bisque\TheSkyX Professional Edition\Resources\Common\PlugIns\FocuserPlugIns.

3. I believe it wasn't working unless I also put a copy of the SiUSBXp.dll file into "TheSkyX Professional Edition" directory alongside TheSkyX.exe which means it may not be needed in the other folder.  I don't like having to put this file here so I'll be looking for other options.

4. Connect your MicroTouch and power it up.

5. Open up TheSkyX and find your Focuser Panel.

6. Use the Focuser Setup drop down and select Choose, then navigate to Starizona and then MicroTouch.

7. Then select Settings in the Focuser Setup drop down.  For Serial Port use the drop down and you should see an entry like “USB00” to select.

8. Hit OK and then select Connect in the Focuser Setup drop down. Then close the dialogs.

9.  If everything is working you should see the current focuser position and temperature.
