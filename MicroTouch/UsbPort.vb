Imports System.ComponentModel
Imports System.Runtime.InteropServices

Public Class UsbPort

#Region "Public members"
  Public ReadOnly Property Connected() As Boolean
    Get
      Return m_connected
    End Get
  End Property

  Private device_id As Integer
  Public Property DeviceId() As Integer
    Get
      Return device_id
    End Get
    Set(ByVal value As Integer)
      device_id = value
    End Set
  End Property

  Private driver_id As Integer
  Public Property DriverId() As Integer
    Get
      Return driver_id
    End Get
    Set(ByVal value As Integer)
      driver_id = value
    End Set
  End Property

  Private serial_number As String
  Public Property SerialNumber() As String
    Get
      Return serial_number
    End Get
    Set(ByVal value As String)
      serial_number = value
    End Set
  End Property

  Private temp_offset As Double
  Public ReadOnly Property TempOffset() As Double
    Get
      Return temp_offset
    End Get
  End Property

  Private raw_temperature As Double
  Public Property RawTemperature() As Double
    Get
      Return raw_temperature
    End Get
    Set(ByVal value As Double)
      raw_temperature = value
    End Set
  End Property

  Private temp_sensor_status As Boolean
  Public ReadOnly Property TempSensorStatus() As Boolean
    Get
      Return temp_sensor_status
    End Get
  End Property

  Private corrupted_packets As Integer
  Public ReadOnly Property NumCorruptedPackets() As Integer
    Get
      Return corrupted_packets
    End Get
  End Property

  Private corrupted_motor_packets As Integer
  Public ReadOnly Property NumCorruptedMotorPackets() As Integer
    Get
      Return corrupted_motor_packets
    End Get
  End Property

  Private motor_errors As Integer
  Public ReadOnly Property NumMotorErrors() As Integer
    Get
      Return motor_errors
    End Get
  End Property

  Private lost_comms As Boolean
  Public ReadOnly Property IsLostComms() As Boolean
    Get
      Return lost_comms
    End Get
  End Property

  Private num_retries As Integer
  Public ReadOnly Property NumRetries() As Integer
    Get
      Return num_retries
    End Get
  End Property

  Private wireless As Boolean
  Public ReadOnly Property IsWireless() As Boolean
    Get
      Return wireless
    End Get
  End Property

  Private hand_version As String
  Public Property HandVersion() As String
    Get
      Return hand_version
    End Get
    Set(ByVal value As String)
      hand_version = value
    End Set
  End Property

  Private motor_version As String
  Public Property MotorVersion() As String
    Get
      Return motor_version
    End Get
    Set(ByVal value As String)
      motor_version = value
    End Set
  End Property

  Private address_byte As String
  Public Property AddressByte() As String
    Get
      Return address_byte
    End Get
    Set(ByVal value As String)
      address_byte = value
    End Set
  End Property

#End Region

#Region "Private Members"

  Private hUSBDevice As UInt32
  Private IOBufSize As Integer = 12
  Private IOBuf(IOBufSize) As Byte
  'Dim cobs As New cobs_encoder
  'Private cobs_byte As Byte = &H55
  Private RetryCount As Integer = 3
  'Private decode_buffer(256) As Byte
  'Private receive_buffer(256) As Byte
  Private objCommsLock As Object = New Object()
  Private Status As Integer
  Private writer As System.IO.StreamWriter
  Private logging As Boolean = False
  Private write_failures As Integer = 0
  Private m_connected As Boolean = False
  Private device_list As New List(Of String)

  'Dim writer As System.IO.StreamWriter = New System.IO.StreamWriter("c:\hyper_debug.txt")

  Private Enum PC_CMDS
    <Description("CMD_GET_STATUS")> CMD_GET_STATUS = &H80
    <Description("CMD_RESET_POSITION")> CMD_RESET_POSITION
    <Description("CMD_IS_MOVING")> CMD_IS_MOVING
    <Description("CMD_HALT")> CMD_HALT
    <Description("CMD_GET_TEMPERATURE")> CMD_GET_TEMPERATURE
    <Description("CMD_SET_COEFF")> CMD_SET_COEFF
    <Description("CMD_GET_COEFF")> CMD_GET_COEFF
    <Description("CMD_TEMPCOMP_ON")> CMD_TEMPCOMP_ON
    <Description("CMD_TEMPCOMP_OFF")> CMD_TEMPCOMP_OFF
    <Description("CMD_GET_TEMPCOMP")> CMD_GET_TEMPCOMP
    <Description("CMD_SNAPSHOT")> CMD_SNAPSHOT
    <Description("CMD_COMP_IMMEDIATE")> CMD_COMP_IMMEDIATE
    <Description("CMD_UPDATE_POSITION")> CMD_UPDATE_POSITION
    <Description("CMD_GET_POSITION")> CMD_GET_POSITION
    <Description("CMD_UP_PRESSED")> CMD_UP_PRESSED
    <Description("CMD_DN_PRESSED")> CMD_DN_PRESSED
    <Description("CMD_BTN_RELEASED")> CMD_BTN_RELEASED
    <Description("CMD_LEARN")> CMD_LEARN
    <Description("CMD_GET_LEARN")> CMD_GET_LEARN
    <Description("CMD_SET_RETRY")> CMD_SET_RETRY
    <Description("CMD_GET_VERSION")> CMD_GET_VERSION
    <Description("CMD_PROG_MODE")> CMD_PROG_MODE
    <Description("CMD_MOTOR_BOOT_ACK")> CMD_MOTOR_BOOT_ACK
    <Description("CMD_MOTOR_BOOT")> CMD_MOTOR_BOOT
    <Description("CMD_PROG_MOTOR")> CMD_PROG_MOTOR
    <Description("CMD_SET_SPEED_DELAY")> CMD_SET_SPEED_DELAY
    <Description("CMD_GET_SPEED_DELAY")> CMD_GET_SPEED_DELAY
    <Description("CMD_SET_STEPS")> CMD_SET_STEPS
    <Description("CMD_GET_STEPS")> CMD_GET_STEPS
    <Description("CMD_SET_MOTOR_SPEED")> CMD_SET_MOTOR_SPEED
    <Description("CMD_GET_MOTOR_SPEED")> CMD_GET_MOTOR_SPEED
    <Description("CMD_SET_TEMP_OFFSET")> CMD_SET_TEMP_OFFSET
    <Description("CMD_GET_TEMP_OFFSET")> CMD_GET_TEMP_OFFSET
    <Description("CMD_SET_MIN_POS")> CMD_SET_MIN_POS
    <Description("CMD_GET_MIN_POS")> CMD_GET_MIN_POS
    <Description("CMD_SET_MAX_POS")> CMD_SET_MAX_POS
    <Description("CMD_GET_MAX_POS")> CMD_GET_MAX_POS
    <Description("CMD_SET_RST_POS")> CMD_SET_RST_POS
    <Description("CMD_GET_RST_POS")> CMD_GET_RST_POS
  End Enum


#End Region

#Region "Public Methods"

  Public Sub OpenPort()
    Dim result As Boolean = False
    Dim DevStr(SI_MAX_DEVICE_STRLEN) As Byte
    Dim dwNumDevices As Integer = 0
    Dim dwHighVersion As Integer = 0
    Dim dwLowVersion As Integer = 0

    GetStoredSerialNumbers()

    Status = SI_GetNumDevices(dwNumDevices)
    If (Status = SI_SUCCESS) Then
      Status = SI_DEVICE_NOT_FOUND
      DeviceId = -1
      For device As Integer = 0 To (dwNumDevices - 1)
        SI_GetProductString(device, DevStr(0), SI_RETURN_SERIAL_NUMBER)
        If (ConvertToVBString(DevStr) = device_list(DriverId)) Then
          DeviceId = device
          Exit For
        End If
      Next
      If DeviceId = -1 Then
        DeviceId = 0
      End If
      Status = SI_SUCCESS
    End If

    If (Status = SI_SUCCESS) Then
      Status = SI_GetProductString(DeviceId, DevStr(0), SI_RETURN_VID)
      If (Status = SI_SUCCESS And String.Compare(ConvertToVBString(DevStr), "10c4") = 0) Then
        Status = SI_GetProductString(DeviceId, DevStr(0), SI_RETURN_PID)
        If (Status = SI_SUCCESS And String.Compare(ConvertToVBString(DevStr), "82f4") = 0) Then
          If (Status = SI_SUCCESS) Then
            Status = SI_SetTimeouts(CommTimeout, CommTimeout)
            Status = Status Or SI_Open(DeviceId, hUSBDevice)
            If (Status = SI_SUCCESS) Then
              ' Clear the CP210x device's buffers
              Status = SI_FlushBuffers(hUSBDevice, True, True)
              If (Status = SI_SUCCESS) Then
                ' Set the CP210x device's baud rate 
                Status = SI_SetBaudRate(hUSBDevice, 19200)
              End If
            End If
          End If
        Else
          Status = SI_DEVICE_NOT_FOUND
        End If
      Else
        Status = SI_DEVICE_NOT_FOUND
      End If
    End If
    If (Status = SI_SUCCESS) Then
      m_connected = True
      DebugMessage("UsbPort: OpenPort Successful")
    Else
      DebugMessage("UsbPort: OpenPort Failed")
    End If

  End Sub

  Public Sub ClosePort()
    If m_connected Then
      SI_Close(hUSBDevice)
      m_connected = False
      DebugMessage("UsbPort: Port Closed")
    End If
  End Sub

  Public Sub OpenLogFile()
    SyncLock objCommsLock
      OpenDebugFile()
    End SyncLock
  End Sub

  Public Sub CloseLogFile()
    SyncLock objCommsLock
      CloseDebugFile()
    End SyncLock
  End Sub

  Public Function GetNumDevicesConnected() As Integer
    Dim dwNumDevices As Integer = 0
    SI_GetNumDevices(dwNumDevices)
    Return dwNumDevices
  End Function

  Public Function GetSerialNumbers() As List(Of String)
    Dim dwNumDevices As Integer = 0
    Dim DevStr(SI_MAX_DEVICE_STRLEN) As Byte
    Dim device_list As New List(Of String)

    SI_GetNumDevices(dwNumDevices)

    For device As Integer = 0 To (dwNumDevices - 1)
      SI_GetProductString(device, DevStr(0), SI_RETURN_SERIAL_NUMBER)
      device_list.Add(ConvertToVBString(DevStr))
    Next

    Return device_list
  End Function

  Public Function GetStoredSerialNumbers() As List(Of String)
    My.MySettings.Default.Reload()
    device_list.Add(My.MySettings.Default.Driver1SerialNumber)
    device_list.Add(My.MySettings.Default.Driver2SerialNumber)
    device_list.Add(My.MySettings.Default.Driver3SerialNumber)
    device_list.Add(My.MySettings.Default.Driver4SerialNumber)
    Return device_list
  End Function

  Public Sub SetStoredSerialNumbers(ByRef device_list As List(Of String))
    My.MySettings.Default.Driver1SerialNumber = device_list(0)
    My.MySettings.Default.Driver2SerialNumber = device_list(1)
    My.MySettings.Default.Driver3SerialNumber = device_list(2)
    My.MySettings.Default.Driver4SerialNumber = device_list(3)
    My.MySettings.Default.Save()
  End Sub

  Public Function ConvertToVBString(ByVal Str)

    Dim NewString As String = ""
    Dim i As Integer

    'for the received string array, loop until we get
    'a 0 char, or until the max length has been obtained
    'then add the ascii char value to a vb string
    i = 0
    Do While (i < SI_MAX_DEVICE_STRLEN) And (Str(i) <> 0)
      NewString = NewString + Chr(Str(i))
      i = i + 1
    Loop

    ConvertToVBString = NewString

  End Function

#Region "...Focuser Methods..."

  Public Sub RetrieveFocuserStatus()
    Dim read_buffer(12) As Byte
    WriteCmdGetBytes(PC_CMDS.CMD_GET_STATUS, read_buffer, 11)
    corrupted_packets = (read_buffer(2) << 8) + read_buffer(1)
    corrupted_motor_packets = (read_buffer(4) << 8) + read_buffer(3)
    motor_errors = read_buffer(5)
    lost_comms = read_buffer(7)
    num_retries = (read_buffer(9) << 8) + read_buffer(8)
    wireless = read_buffer(10)
  End Sub

  Public Sub ResetFocuserPosition(ByVal val As UInt16)
    WriteCmdSetWordAsDigits(PC_CMDS.CMD_RESET_POSITION, val)
  End Sub

  Public Function GetFocuserIsMoving() As Boolean
    Return (WriteCmdGetByte(PC_CMDS.CMD_IS_MOVING) <> 0)
  End Function

  Public Sub HaltFocuser()
    WriteCmd(PC_CMDS.CMD_HALT)
  End Sub

  Public Function GetFocuserTemperature() As Double
    Dim read_buffer(10) As Byte
    Dim temperature As Int16 = 0
    Dim offset As Int16 = 0
    WriteCmdGetBytes(PC_CMDS.CMD_GET_TEMPERATURE, read_buffer, 6)
    temperature = (Convert.ToInt16(read_buffer(1)) << 8) + read_buffer(2)
    temp_sensor_status = read_buffer(3)
    offset = (Convert.ToInt16(read_buffer(5)) << 8) + read_buffer(4)
    raw_temperature = Convert.ToDouble(temperature / 16.0)
    temp_offset = Convert.ToDouble(offset / 16.0)
    Return raw_temperature + temp_offset
  End Function

  Public Sub SetFocuserCoeff(ByVal val As Double)
    Dim new_coeff As Integer
    new_coeff = val * Math.Pow(2, 7)
    WriteCmdSetInteger(PC_CMDS.CMD_SET_COEFF, new_coeff)
  End Sub

  Public Function GetFocuserCoeff() As Double
    Dim coeff As Int64 = WriteCmdGetInteger(PC_CMDS.CMD_GET_COEFF)
    Return Convert.ToDouble(coeff) / Math.Pow(2, 7)
  End Function

  Public Sub SetTempCompOn()
    WriteCmd(PC_CMDS.CMD_TEMPCOMP_ON)
  End Sub

  Public Sub SetTempCompOff()
    WriteCmd(PC_CMDS.CMD_TEMPCOMP_OFF)
  End Sub

  Public Function GetTempComp() As Boolean
    Return WriteCmdGetByte(PC_CMDS.CMD_GET_TEMPCOMP) <> 0
  End Function

  Public Sub Snapshot()
    WriteCmd(PC_CMDS.CMD_SNAPSHOT)
  End Sub

  Public Sub CompensateImmediate()
    WriteCmd(PC_CMDS.CMD_COMP_IMMEDIATE)
  End Sub

  Public Sub UpdateFocuserPosition(ByVal val As Integer)
    WriteCmdSetWordAsDigits(PC_CMDS.CMD_UPDATE_POSITION, val)
  End Sub

  Public Function GetFocuserPosition() As UInt16
    Return WriteCmdGetWord(PC_CMDS.CMD_GET_POSITION)
  End Function

  Public Sub FocuserButtonUpPressed()
    WriteCmd(PC_CMDS.CMD_UP_PRESSED)
  End Sub

  Public Sub FocuserButtonDownPressed()
    WriteCmd(PC_CMDS.CMD_DN_PRESSED)
  End Sub

  Public Sub FocuserButtonReleased()
    WriteCmd(PC_CMDS.CMD_BTN_RELEASED)
  End Sub

  Public Sub Learn()
    WriteCmd(PC_CMDS.CMD_LEARN)
  End Sub

  Public Function GetLearning() As Boolean
    Return WriteCmdGetByte(PC_CMDS.CMD_GET_LEARN) <> 0
  End Function

  Public Sub SetRetry(ByVal val As UInt16)
    WriteCmdSetWord(PC_CMDS.CMD_SET_RETRY, val)
  End Sub

  Public Sub GetFirmwareInfo()
    Dim read_buffer(10) As Byte
    WriteCmdGetBytes(PC_CMDS.CMD_GET_VERSION, read_buffer, 6)
    HandVersion = read_buffer(1).ToString() + "." + read_buffer(2).ToString()
    MotorVersion = read_buffer(3).ToString() + "." + read_buffer(4).ToString()
    AddressByte = Hex(read_buffer(5).ToString("X2"))
  End Sub

  Public Sub ProgramMode()
    WriteCmd(PC_CMDS.CMD_PROG_MODE)
  End Sub

  Public Sub ProgramMotor()
    WriteCmd(PC_CMDS.CMD_PROG_MOTOR)
  End Sub

  Public Sub SetSpeedDelay(ByVal val As Double)
    WriteCmdSetWord(PC_CMDS.CMD_SET_SPEED_DELAY, Convert.ToUInt16(val * 10))
  End Sub

  Public Function GetSpeedDelay() As Double
    Dim result As UInt16
    result = WriteCmdGetWord(PC_CMDS.CMD_GET_SPEED_DELAY)
    Return Convert.ToDouble(result / 10)
  End Function

  Public Sub SetSteps(ByVal val As Byte)
    WriteCmdSetByte(PC_CMDS.CMD_SET_STEPS, val)
  End Sub

  Public Function GetSteps() As Byte
    Return WriteCmdGetByte(PC_CMDS.CMD_GET_STEPS)
  End Function

  Public Sub SetMotorSpeed(ByVal val As Byte)
    WriteCmdSetWord(PC_CMDS.CMD_SET_MOTOR_SPEED, val)
  End Sub

  Public Function GetMotorSpeed() As Byte
    Return WriteCmdGetByte(PC_CMDS.CMD_GET_MOTOR_SPEED)
  End Function

  Public Sub SetTempOffset(ByVal val As Double)
    Dim offset As Int16
    Dim value As UInt16
    offset = Convert.ToInt16(val * 16.0)
    value = BitConverter.ToUInt16(BitConverter.GetBytes(offset), 0)
    WriteCmdSetWord(PC_CMDS.CMD_SET_TEMP_OFFSET, value)
  End Sub

  ' TempOffset is retrieved when Temperature is received
  'Public Function GetTempOffset() As UInt16
  '    Return WriteCmdGetWord(PC_CMDS.CMD_GET_TEMP_OFFSET)
  'End Function

  Public Sub SetFocuserMinValue(ByVal val As UInt16)
    WriteCmdSetWordAsDigits(PC_CMDS.CMD_SET_MIN_POS, val)
  End Sub

  Public Function GetFocuserMinValue() As UInt16
    Return WriteCmdGetWord(PC_CMDS.CMD_GET_MIN_POS)
  End Function

  Public Sub SetFocuserMaxValue(ByVal val As UInt16)
    WriteCmdSetWordAsDigits(PC_CMDS.CMD_SET_MAX_POS, val)
  End Sub

  Public Function GetFocuserMaxValue() As UInt16
    Return WriteCmdGetWord(PC_CMDS.CMD_GET_MAX_POS)
  End Function

  Public Sub SetFocuserResetValue(ByVal val As UInt16)
    WriteCmdSetWordAsDigits(PC_CMDS.CMD_SET_RST_POS, val)
  End Sub

  Public Function GetFocuserResetValue() As UInt16
    Return WriteCmdGetWord(PC_CMDS.CMD_GET_RST_POS)
  End Function

  Public Function BootEnter(ByVal is_motor As Boolean) As Boolean
    Dim data(5) As Byte
    Dim status As Boolean = False
    Dim index As Integer = 0
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False

    While index < 3 And Not status
      If (is_motor) Then
        data(0) = &H98
      Else
        data(0) = &H95
      End If
      DeviceWrite(data, 1)
      data(0) = Convert.ToByte("p"c)
      WriteMessage(data, 1)
      start_time = DateTime.Now
      end_time = DateTime.Now
      duration = end_time - start_time
      While duration.Seconds < 1 And Not status
        status = DeviceReadFast(data, 1)
        If data(0) = Convert.ToByte("S"c) Then
          status = True
          success = True
        Else
          end_time = DateTime.Now
          duration = end_time - start_time
        End If
      End While
      index += 1
    End While
    Return success
  End Function

  Public Sub BootExit()
    Dim data(2) As Byte
    Dim status As Boolean = False

    data(0) = Convert.ToByte("Z"c)
    WriteMessage(data, 1)
  End Sub

  Public Function BootChipErase() As Boolean
    Dim data(2) As Byte
    Dim status As Integer
    Dim index As Integer = 0
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False

    data(0) = Convert.ToByte("e"c)
    WriteMessage(data, 1)
    Threading.Thread.Sleep(5)
    start_time = DateTime.Now
    end_time = DateTime.Now
    duration = end_time - start_time
    While duration.Seconds < 15 And Not status
      status = DeviceReadFast(data, 1)
      If data(0) = 13 Then
        status = True
        success = True
      Else
        end_time = DateTime.Now
        duration = end_time - start_time
      End If
    End While
    Return success
  End Function

  Public Function BootWritePage() As Boolean
    Dim data(2) As Byte
    Dim status As Boolean = False
    Dim index As Integer = 0
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False

    data(0) = Convert.ToByte("m"c)
    WriteMessage(data, 1)
    start_time = DateTime.Now
    end_time = DateTime.Now
    duration = end_time - start_time
    While duration.Seconds < 8 And Not status
      status = DeviceReadFast(data, 1)
      If data(0) = 13 Then
        status = True
        success = True
      Else
        end_time = DateTime.Now
        duration = end_time - start_time
      End If
    End While
    Return success
  End Function

  Public Function BootSetAddress(ByVal address As UInt16) As Boolean
    Dim data(4) As Byte
    Dim status As Boolean = False
    Dim index As Integer = 0
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False

    data(0) = Convert.ToByte("A"c)
    data(1) = ((address >> 8) And &HFF)
    data(2) = (address And &HFF)
    WriteMessage(data, 3)
    start_time = DateTime.Now
    end_time = DateTime.Now
    duration = end_time - start_time
    While duration.Seconds < 8 And Not status
      status = DeviceReadFast(data, 1)
      If data(0) = 13 Then
        status = True
        success = True
      Else
        end_time = DateTime.Now
        duration = end_time - start_time
      End If
    End While
    Return success
  End Function

  Public Function BootProgramMemory(ByVal prog_byte As Byte, ByVal high_byte As Boolean) As Boolean
    Dim data(4) As Byte
    Dim status As Boolean = False
    Dim index As Integer = 0
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False

    If high_byte Then
      data(0) = Convert.ToByte("C"c)
    Else
      data(0) = Convert.ToByte("c"c)
    End If
    data(1) = prog_byte
    WriteMessage(data, 2)
    start_time = DateTime.Now
    end_time = DateTime.Now
    duration = end_time - start_time
    While duration.Seconds < 8 And Not status
      status = DeviceReadFast(data, 1)
      If data(0) = 13 Then
        status = True
        success = True
      Else
        end_time = DateTime.Now
        duration = end_time - start_time
      End If
    End While
    Return success
  End Function

#End Region

#End Region

#Region "Private Methods"

  Private Sub RestartInterface()
    DebugMessage("UsbPort: Restarting Interface")
    SyncLock objCommsLock
      ClosePort()
      OpenPort()
    End SyncLock
  End Sub

  Private Function WriteMessage(ByRef buffer() As Byte, ByVal num_bytes As Byte) As Boolean
    Dim success As Boolean = False
    Dim tries As Byte = 0

    If Not m_connected Then
      RestartInterface()
    End If

    If m_connected Then
      DebugMessage("WriteMessage - Cmd: " + CType(buffer(0), PC_CMDS).ToString())
      SyncLock objCommsLock
        Do While Not success And tries < 10
          If tries = 3 Or tries = 6 Or tries = 9 Then
            RestartInterface()
          End If
          If m_connected Then
            success = DeviceWrite(buffer, num_bytes)
          Else
            System.Threading.Thread.Sleep(10)
          End If
          tries += 1
        Loop
      End SyncLock
    Else
      DebugMessage("Throwing: NotConnectedException")
      Throw New NotConnectedException
    End If

    Return success
  End Function

  Private Sub WriteCmd(ByVal cmd As PC_CMDS)
    Dim write_buffer(64) As Byte
    write_buffer(0) = cmd
    WriteMessage(write_buffer, 1)
  End Sub

  Private Sub WriteByte(ByVal bbyte As Byte)
    Dim write_buffer(64) As Byte
    write_buffer(0) = bbyte
    WriteMessage(write_buffer, 1)
  End Sub

  Private Sub WriteCmdSetByte(ByVal cmd As PC_CMDS, ByVal bbyte As Byte)
    Dim write_buffer(64) As Byte
    write_buffer(0) = cmd
    write_buffer(1) = bbyte
    WriteMessage(write_buffer, 2)
  End Sub

  Private Sub WriteCmdSetWord(ByVal cmd As PC_CMDS, ByVal word As UInt16)
    Dim write_buffer(64) As Byte
    write_buffer(0) = cmd
    write_buffer(1) = word And &HFF
    write_buffer(2) = (word >> 8) And &HFF
    WriteMessage(write_buffer, 3)
  End Sub

  Private Sub WriteCmdSetInteger(ByVal cmd As PC_CMDS, ByVal dword As UInt32)
    Dim write_buffer(64) As Byte
    write_buffer(0) = cmd
    write_buffer(1) = dword And &HFF
    write_buffer(2) = (dword >> 8) And &HFF
    write_buffer(3) = (dword >> 16) And &HFF
    write_buffer(4) = dword >> 24
    WriteMessage(write_buffer, 5)
  End Sub

  Private Sub WriteCmdSetWordAsDigits(ByVal cmd As PC_CMDS, ByVal word As UInt16)
    Dim write_buffer(64) As Byte
    Dim digit1 As UInt16 = word Mod 10
    Dim digit2 As UInt16 = Int(word / 10) Mod 10
    Dim digit3 As UInt16 = Int(word / 100) Mod 10
    Dim digit4 As UInt16 = Int(word / 1000)

    write_buffer(0) = cmd
    write_buffer(1) = digit1
    write_buffer(2) = digit2
    write_buffer(3) = digit3
    write_buffer(4) = digit4
    WriteMessage(write_buffer, 5)
  End Sub

  Private Function ValidateCmdReceived(ByRef buffer() As Byte, ByVal cmd As PC_CMDS, ByVal size As Byte) As Boolean
    Dim checksum As UInt16 = 0
    Dim pkt_checksum As UInt16 = 0
    Dim result As Boolean = False

    If buffer(0) = cmd Then  ' validate checksum
      '    If size = 4 Then
      ' checksum = buffer(0)
      'checksum += buffer(1)
      'pkt_checksum = buffer(2) * 256 + buffer(3)
      'ElseIf size = 5 Then
      '    checksum = buffer(0)
      '   checksum += buffer(1)
      '  checksum += buffer(2)
      ' pkt_checksum = buffer(3) * 256 + buffer(4)
      'ElseIf size = 7 Then
      '   checksum = buffer(0)
      '  checksum += buffer(1)
      ' checksum += buffer(2)
      'checksum += buffer(3)
      'checksum += buffer(4)
      'pkt_checksum = buffer(6) * 256 + buffer(6)
      'End If
      'If checksum = pkt_checksum Then
      result = True  ' packet validated
      'End If
    End If

    If result Then
      'DebugMessage("ValidateCmdReceived - Packet Validated")
    Else
      DebugMessage("ValidateCmdReceived - Packet Failed")
    End If

    ValidateCmdReceived = result
  End Function

  Private Function WriteCmdGetResponse(ByVal cmd As PC_CMDS, ByRef bytes() As Byte, ByVal num_bytes As Byte) As Boolean
    Dim success As Boolean = False
    Dim tries As Byte = 0
    Dim byte_string As String = ""
    Dim write_buffer(64) As Byte

    write_buffer(0) = cmd

    If Not m_connected Then
      RestartInterface()
    End If

    If m_connected Then
      SyncLock objCommsLock
        DebugMessage("WriteCmdGetResponse - Cmd: " + CType(cmd, PC_CMDS).ToString())
        Do While Not success And tries < 10
          If tries = 3 Or tries = 6 Or tries = 9 Then
            RestartInterface()
          End If
          If m_connected Then
            If DeviceWrite(write_buffer, 1) Then
              If DeviceRead(bytes, num_bytes) Then
                byte_string = "DeviceRead - Bytes:"
                For index As Integer = 0 To num_bytes - 1
                  byte_string += " " + String.Format("{0:x2}", bytes(index))
                Next
                DebugMessage(byte_string)
                If ValidateCmdReceived(bytes, cmd, num_bytes) Then
                  success = True
                Else
                  SI_FlushBuffers(hUSBDevice, True, True)
                  DebugMessage("WriteCmdGetResponse - Flushing Buffers")
                End If
              Else
                DebugMessage("WriteCmdGetResponse - Read Failed")
              End If
            Else
              DebugMessage("WriteCmdGetResponse - Write Failed")
            End If
          Else
            System.Threading.Thread.Sleep(10)
          End If
          tries += 1
        Loop
      End SyncLock
    Else
      DebugMessage("Throwing: NotConnectedException")
      Throw New NotConnectedException
    End If

    WriteCmdGetResponse = success

  End Function

  Private Function WriteCmdGetByte(ByVal cmd As PC_CMDS) As Byte
    Dim read_buffer(64) As Byte

    If WriteCmdGetResponse(cmd, read_buffer, 2) Then
      WriteCmdGetByte = read_buffer(1)
    Else
      DebugMessage("Throwing: CommunicationFailureException")
      Throw New CommunicationFailureException
    End If

  End Function

  Private Function WriteCmdGetWord(ByVal cmd As PC_CMDS) As UInt16
    Dim read_buffer(64) As Byte

    If WriteCmdGetResponse(cmd, read_buffer, 3) Then
      WriteCmdGetWord = read_buffer(2) * 256 + read_buffer(1)
    Else
      DebugMessage("Throwing: CommunicationFailureException")
      Throw New CommunicationFailureException
    End If

  End Function

  Private Function WriteCmdGetInteger(ByVal cmd As PC_CMDS) As Int32
    Dim read_buffer(10) As Byte
    Dim result As Int32
    If WriteCmdGetResponse(cmd, read_buffer, 5) Then
      result = Convert.ToInt32(read_buffer(4)) << 24
      result += Convert.ToInt32(read_buffer(3)) << 16
      result += Convert.ToInt32(read_buffer(2)) << 8
      result += Convert.ToInt32(read_buffer(1))
      WriteCmdGetInteger = result
    Else
      DebugMessage("Throwing: CommunicationFailureException")
      Throw New CommunicationFailureException
    End If
  End Function

  Private Sub WriteCmdGetBytes(ByVal cmd As PC_CMDS, ByRef read_buffer As Byte(), ByVal num As Byte)
    If Not WriteCmdGetResponse(cmd, read_buffer, num) Then
      DebugMessage("Throwing: CommunicationFailureException")
      Throw New CommunicationFailureException
    End If

  End Sub

  Private Function DeviceRead(ByRef Buffer() As Byte, ByVal dwSize As Integer) As Boolean
    Dim ReadStatus As Integer
    Dim dwBytesRead As Integer
    Dim totalBytesRead As Integer
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False
    Dim error_code As Boolean = False
    Dim num_to_read As Byte = dwSize

    If Not SI_SetTimeouts(CommTimeout, CommTimeout) = SI_SUCCESS Then
      DebugMessage("DeviceRead: SI_SetTimeouts Failed - Code: " + String.Format("{0:x2}", Status))
    End If
    System.Threading.Thread.Sleep(25)
    start_time = DateTime.Now
    end_time = DateTime.Now
    duration = end_time - start_time
    While duration.Milliseconds < 100 And Not success And Not error_code
      ReadStatus = SI_Read(hUSBDevice, Buffer(totalBytesRead), dwSize - totalBytesRead, dwBytesRead, 0)
      If ReadStatus = SI_SUCCESS Then
        totalBytesRead += dwBytesRead
        DebugMessage("DeviceRead: Bytes read: " + dwBytesRead.ToString)
      Else
        DebugMessage("DeviceRead: SI_Read Failed - Code: " + String.Format("{0:x2}", ReadStatus))
        If ReadStatus = SI_SYSTEM_ERROR_CODE Then
          DebugMessage("SI_SYSTEM_ERROR_CODE - GetLastError: " + String.Format("0x{0:x}", Marshal.GetLastWin32Error()))
          FixSurpriseRemoval()
        End If
        error_code = True
      End If
      If totalBytesRead = dwSize Then
        success = True
      End If
      end_time = DateTime.Now
      duration = end_time - start_time
    End While

    DeviceRead = success

  End Function

  Private Function DeviceReadFast(ByRef Buffer() As Byte, ByVal dwSize As Integer) As Boolean
    Dim ReadStatus As Integer
    Dim dwBytesRead As Integer
    Dim totalBytesRead As Integer
    Dim start_time As DateTime
    Dim end_time As DateTime
    Dim duration As TimeSpan
    Dim success As Boolean = False
    Dim error_code As Boolean = False
    Dim num_to_read As Byte = dwSize

    If Not SI_SetTimeouts(CommTimeout, CommTimeout) = SI_SUCCESS Then
      DebugMessage("DeviceRead: SI_SetTimeouts Failed - Code: " + String.Format("{0:x2}", Status))
    End If
    start_time = DateTime.Now
    end_time = DateTime.Now
    duration = end_time - start_time
    While duration.Milliseconds < 100 And Not success And Not error_code
      ReadStatus = SI_Read(hUSBDevice, Buffer(totalBytesRead), dwSize - totalBytesRead, dwBytesRead, 0)
      If ReadStatus = SI_SUCCESS Then
        totalBytesRead += dwBytesRead
        DebugMessage("DeviceRead: Bytes read: " + dwBytesRead.ToString)
      Else
        DebugMessage("DeviceRead: SI_Read Failed - Code: " + String.Format("{0:x2}", ReadStatus))
        If ReadStatus = SI_SYSTEM_ERROR_CODE Then
          DebugMessage("SI_SYSTEM_ERROR_CODE - GetLastError: " + String.Format("0x{0:x}", Marshal.GetLastWin32Error()))
          FixSurpriseRemoval()
        End If
        error_code = True
      End If
      If totalBytesRead = dwSize Then
        success = True
      End If
      end_time = DateTime.Now
      duration = end_time - start_time
    End While

    DeviceReadFast = success

  End Function

  Private Function DeviceWrite(ByRef Buffer() As Byte, ByVal dwSize As Integer) As Boolean
    Dim WriteStatus As Integer
    Dim dwBytesWritten As Integer = 0

    If Not SI_SetTimeouts(CommTimeout, CommTimeout) = SI_SUCCESS Then
      DebugMessage("DeviceWrite: SI_SetTimeouts Failed - Code: " + String.Format("{0:x2}", Status))
    End If

    DebugMessage("DeviceWrite: Cmd: " + String.Format("{0:x2}", Buffer(0)) + ", Num Bytes: " + dwSize.ToString)
    WriteStatus = SI_Write(hUSBDevice, Buffer(0), dwSize, dwBytesWritten, 0)

    If WriteStatus = SI_SUCCESS And dwBytesWritten = dwSize Then
      DeviceWrite = True
    Else
      DebugMessage("DeviceWrite: SI_Write Failed - Code: " + String.Format("{0:x2}", WriteStatus))
      If WriteStatus = SI_SYSTEM_ERROR_CODE Then
        DebugMessage("SI_SYSTEM_ERROR_CODE - GetLastError: " + String.Format("0x{0:x}", Marshal.GetLastWin32Error()))
        FixSurpriseRemoval()
      End If
      DeviceWrite = False
    End If

  End Function

  Private Function FixSurpriseRemoval() As Boolean
    Dim result As Boolean = False

    SI_Close(hUSBDevice)

    Status = SI_Open(0, hUSBDevice)
    If (Status = SI_SUCCESS) Then
      ' Clear the CP210x device's buffers
      Status = SI_FlushBuffers(hUSBDevice, True, True)
      If (Status = SI_SUCCESS) Then
        ' Set the CP210x device's baud rate 
        Status = SI_SetBaudRate(hUSBDevice, 19200)
      End If
    End If

    If (Status = SI_SUCCESS) Then
      DebugMessage("SurpriseRemoval: Obtained new USB handle")
      FixSurpriseRemoval = True
    Else
      DebugMessage("SurpriseRemoval: Failed to get USB handle")
      FixSurpriseRemoval = False
    End If

  End Function

  Private Sub OpenDebugFile()
    Dim tm As DateTime = DateTime.Now
    Dim basefilename As String
    Dim suffix As Integer = 1
    'Dim directory As String = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MicroTouch Controller_is1", "Inno Setup: App Path", "")
    Dim directory As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    If directory <> "" Then
      directory += "\Starizona\MicroTouch Controller\LogFiles\"
      System.IO.Directory.CreateDirectory(directory)
      basefilename = directory + "MicroTouch_" + DeviceId.ToString() + "_Log_" + tm.Month.ToString + tm.Day.ToString + tm.Year.ToString + "_"
      If Not logging Then
        While System.IO.File.Exists(basefilename + suffix.ToString + ".txt")
          suffix += 1
        End While
        writer = New System.IO.StreamWriter(basefilename + suffix.ToString + ".txt")
        logging = True
      End If
    End If
  End Sub

  Private Sub CloseDebugFile()
    If logging Then
      writer.Close()
      logging = False
    End If
  End Sub

  Private Sub DebugMessage(ByRef message As String)
    If logging Then
      Dim output_line As String
      Dim tm As DateTime = DateTime.Now
      output_line = String.Format("{0:hh:mm:ss:ffff}", tm) + ": " + message
      writer.WriteLine(output_line)
    End If
  End Sub

#End Region

End Class
