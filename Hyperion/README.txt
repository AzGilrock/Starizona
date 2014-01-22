

There are 3 different installation executables you will need to load to get the Hyperion up and running.

1. Hyperion USB Driver
	Inside the HyperionUSBDriverSetup.zip file you run the file HyperionUSBDriverInstaller.exe.
	This will install the USB device driver to allow the Hyperion to communicate with the PC.

2. Hyperion ASCOM Driver
	Install HyperionDriverSetup.exe to install the ASCOM driver which provides all the interfaces
	for programs to communicate with the Hyperion.  If you are only using 3rd party programs to
	control the Hyperion you should be able to stop right here but we recommend you also install
	the control GUI program described next.  This install the Hyperion Server which provides the
	following COM objects that may be used to communicate with the Hyperion scope.
	
		- ASCOM.HyperionServer.Focuser
		- ASCOM.HyperionServer.Guider.Focuser
		- ASCOM.HyperionServer.Rotator
		- ASCOM.HyperionServer.CustomCommands


3. Hyperion Control Software
	Install HyperionSoftwareSetup.exe to install a program that can control the Hyperion from
	the PC.  This program executes all commands through the Hyperion Server that was installed
	by step 2 above.  You can run this program side by side with any other ASCOM program that
	talks to the interfaces in the Hyperion.  This program is useful when you need to execute
	any of the commands from the CustomCommands interface that are not provided by the ASCOM
	standards.

The file Hyperion_Firmware_2_7.bin is the latest firmware that works with this software.

If you have any problems with installing or running this software please contact
Starizona at (520) 292-5010. You can also join the ASCOM-Talk yahoo group but we only monitor the
group periodically so if you don't get help quick enough it's better to call the store.  