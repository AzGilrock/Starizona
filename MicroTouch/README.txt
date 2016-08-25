

There are 3 different installation executables you will need to load to get the MicroTouch up and running.

1. MicroTouch USB Driver  (Note: The same driver works for the Hyperion and the MicroTouch.)
	Unzip the USBXpress Driver.zip file. Run the file StarizonaUSBDriverInstaller_x64.exe if you
	are on a 64-bit operating system or StarizonaUSBDriverInstaller_x32.exe for 32-bit.
	This will install the USB device driver to allow the MicroTouch to communicate with the PC.

2. MicroTouch ASCOM Driver
	Install MicroTouchDriverSetup_1_5.exe to install the ASCOM driver which provides all the interfaces
	for programs to communicate with the MicroTouch.  If you are only using 3rd party programs to
	control the MicroTouch you should be able to stop right here but we recommend you also install
	the control GUI program described next.  This install the MicroTouch Server which provides the
	following COM objects that may be used to communicate with the MicroTouch scope.
	
		- ASCOM.MicroTouchServer.Focuser1
		- ASCOM.MicroTouchServer.Focuser2
		- ASCOM.MicroTouchServer.Focuser3
		- ASCOM.MicroTouchServer.Focuser4

	Note:  This driver allows you to communicate with up to 4 focusers.  The ASCOM setup dialog will
	allow you to select which device ID from the units is assigned to which focuser driver (1-4).
	To configure this from the GUI that will be installed in step 3 launch the program, and from
	the menu select Focusers / ASCOM Setup and hit OK on the dialog.  Make sure the top line shows
	that focusers are detected then use the dropdown to assign the focusers to a driver channel.
	The driver is designed such that is should work without doing this configuration if you are
	only using one focuser.

3. MicroTouch Control Software
	Install MicroTouchControllerSetup_1_3.exe to install a program that can control the MicroTouch
	from the PC.  This program executes all commands through the MicroTouch Server that was installed
	by step 2 above.  You can run this program side by side with any other ASCOM program that
	talks to the interfaces in the MicroTouch.  This program is useful when you need to execute
	any of the commands over the interface that are not provided by the ASCOM standards.

The file MicroTouch_4_5.bin is the latest firmware for the handcontroller and MicroTouch_Motor_2_5.bin
is the firmware file if you are running a wireless version that has a motor box.

If you have any problems with installing or running this software please contact
Starizona at (520) 292-5010. You can also join the ASCOM-Talk yahoo group but we only monitor the
group periodically so if you don't get help quick enough it's better to call the store.  