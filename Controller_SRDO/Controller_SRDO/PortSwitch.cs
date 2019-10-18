using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ObjCRuntime;
using ServiceTool.Legacy.Dongle;
using ServiceTool.Legacy.REM;

namespace Controller_SRDO
{
    public class PortSwitch
    {
        enum State { RAW, LOGON, PASSTHROUGH }

        public static int PortState { get; set; }

        static bool EscapeFlag;

        static bool LeftBracketFlag;

        Data data;

        Command command;

        DBC dbc;

        public SRDO SrdoLocal;

        List<byte[]> WriteConfigList = new List<byte[]>();

        public event EventHandler<string> OnPortStatus;
        public event EventHandler<ServiceTool.Legacy.REM.MessageRDO> OnMesageReceived;
        public event EventHandler<int> OnPortswapError;
        public event EventHandler<int> OnDBCError;
        public event EventHandler OnDongleDisconnet;
        public event EventHandler<bool> OnPortZkip;

        //TODO Item# 1 Swap Protocol 
        /// <summary>
        /// Establish TCP connections to the data and command ports on the Otis 
        /// Wi-Fi dongle
        /// Init the DBC library
        /// Setup event handlers
        /// </summary>
        /// <returns></returns>
        internal async Task ConnectDongle()
        {
            data = new Data();
            command = new Command();

            await data.Connect();
            await command.Connect();

            dbc = new DBC(data);
            SrdoLocal = new SRDO(dbc);

            command.InitPort += async (sender, e) => await InitPort();
            command.DefaultBaud += async (sender, e) => await DefaultBaud();
            command.OnDisconnected += (sender, e) => CommandDisconnect();

            data.OnEscapeResponse += async (sender, e) => await EscapeResponse(e);
            data.OnPassThrough += async (sender, e) => await dbc.ProcessRaw(e);
            data.OnDisconnected += (sender, e) => DataDisconnect();

            dbc.OnMessageReceived += (sender, e) => MessageForUI(e);
            dbc.OnStatusChanged += (sender, e) => DBCErrorHandler(e);
            dbc.OnConfigChanged += async (sender, e) => await DBCWriteConfig(e);
            dbc.OnRDOError += (sender, e) => RDOErrorHandler(e);
            dbc.OnZkipStatus += (sender, e) => ZKIP(e);
            PortState = (int)State.RAW;
        }

        #region Dongle
        /// <summary>
        /// Inits the SVT logon sequence
        /// </summary>
        /// <returns>T</returns>
        internal async Task InitPort()
        {
            PortState = (int)State.LOGON;
            //EnterCommandmode
            Log("Enter Command Mode...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.DongleCommandMode);
            await Task.Delay(50);
            //Send break command
            Log("Send Break Command...");
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleBreakCMD);
            await Task.Delay(1000);
            //Send Carriage return
            Log("Carriage Return...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.CarriageReturn);
            await Task.Delay(200);
            //SendMMdata
            Log("MM...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.DoubleEm);
            //SVT logon
            Log("SVT Logon...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.CarriageReturn);
            //SVT escape sequence
            Log("SVT Escape Sequence...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.EscLogon);
            await Task.Delay(300);
            EscapeFlag = false;
            LeftBracketFlag = false;
        }

        /// <summary>
        /// Set the dongles default baud rate to 9600
        /// </summary>
        /// <returns>The baud.</returns>
        internal async Task DefaultBaud()
        {
            Log("Setting 9600 baud...");
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleDefaultBaud);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleUpdateConfig);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleGetBaudRate);
        }

        /// <summary>
        /// Ramp up the dongles baud rate to 115K and send the initial messages
        /// </summary>
        /// <returns>The aud.</returns>
        internal async Task GECBBaud()
        {
            Log("Setting REM baud rate...");
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleBaud38K);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleUpdateConfig);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleGetBaudRate);
            await Task.Delay(200);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleBaud57K);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleUpdateConfig);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleGetBaudRate);

            await Task.Delay(600);
            await dbc.SendREMRequestTimeStamp();

            await Task.Delay(600);
            await dbc.SendRequestAccessCommand(0x01);
            Log("Connected");
        }

        /// <summary>
        /// Send the break sequence to kick the controller back to REM & reboot dongle
        /// </summary>
        /// <returns>The baud.</returns>
        internal async Task SwapBackToSVT()
        {
            Log("Enter Command Mode...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.DongleCommandMode);
            await Task.Delay(50);
            //Send break command
            Log("Send Break Command...");
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleBreakCMD);
            await Task.Delay(1000);
            //Send Carriage return
            Log("Carriage Return...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.CarriageReturn);
            await Task.Delay(200);
            //SendMMdata
            Log("MM...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.DoubleEm);
            //SVT logon
            Log("SVT Logon...");
            await data.Send(ServiceTool.Legacy.Dongle.Constants.CarriageReturn);
            await command.Send(ServiceTool.Legacy.Dongle.Constants.DongleReboot);
        }

        /// <summary>
        /// Process the incoming data when in LOGON in state; Look for the SVT
        /// logon sequece back before going to PASSTHROUGH state
        /// if Logon on fails send out event
        /// </summary>
        /// <returns></returns>
        /// <param name="rawData">Raw data.</param>
        internal async Task EscapeResponse(byte[] rawData)
        {
            Console.WriteLine(ServiceTool.Legacy.REM.Helper.ByteArrayToString(rawData));

            foreach (byte i in rawData)
            {
                switch (i)
                {
                    case 0x1b:
                        EscapeFlag = true;
                        break;
                    case 0x5b:
                        LeftBracketFlag = true;
                        break;
                    case 0x79:
                        if ((EscapeFlag == true) && (LeftBracketFlag == true))
                        {
                            Log("Escape Response Received");
                            PortState = (int)State.PASSTHROUGH;
                            await GECBBaud();
                        }
                        break;
                    case 0x62:
                        //TODO Item# 2 SVT logon failed
                        OnPortswapError?.Invoke(this, ServiceTool.Legacy.Dongle.Constants.SVTLogonError);
                        break;
                    case 0x68:
                        OnPortswapError?.Invoke(this, ServiceTool.Legacy.Dongle.Constants.SVTLogonError);
                        //Port Swap not supported
                        Console.WriteLine("Fail");
                        break;
                }
            }
        }

        /// <summary>
        /// Closes the dongle.
        /// </summary>
        /// <returns>The dongle.</returns>
        internal async Task CloseDongle()
        {
            await data.Close();
            await command.Close();
        }

        /// <summary>
        /// Send event when data port disconnects
        /// </summary>
        internal void DataDisconnect()
        {
            OnDongleDisconnet?.Invoke(this, null);
        }

        /// <summary>
        /// Send event when command port disconnects
        /// </summary>
        internal void CommandDisconnect()
        {
            OnDongleDisconnet?.Invoke(this, null);
        }

        #endregion //Dongle

        #region RDO
        /// <summary>
        /// Sends an array of RDOS
        /// </summary>
        /// <returns></returns>
        /// <param name="rdoArray">Rdo array.</param>
        internal async Task SendMessageRead(uint[] rdoArray)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                foreach (uint item in rdoArray)
                {
                    buffer.AddRange(BitConverter.GetBytes((UInt16)item));
                }
                //TODO Item #5 GECB RDOs
                await dbc.SendReadRDO(buffer.ToArray());
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageRead");
            }
        }

        /// <summary>
        /// Sends a singel RDO
        /// </summary>
        /// <returns>The message legacy.</returns>
        /// <param name="rdo">Rdo.</param>
        internal async Task SendMessageRead(uint rdo)
        {
            if (dbc != null)
            {
                //TODO Item #5 GECB RDOs
                await dbc.SendReadRDO(BitConverter.GetBytes((UInt16)rdo));
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageRead");
            }
        }

        /// <summary>
        /// Sends the message write.
        /// </summary>
        /// <returns>The message write.</returns>
        /// <param name="rdoArray">Rdo array.</param>
        internal async Task SendMessageWrite(byte[] rdoArray)
        {
            if (dbc != null)
            {
                //byte[] testData = { 0x29, 0x0C, 0x02, 0x01, 0x01 };
                WriteConfigList.Add(rdoArray);
                await dbc.SendREMStartConfig();
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageWrite");
            }
        }

        /// <summary>
        /// Sends the sub system data message.
        /// </summary>
        /// <returns></returns>
        /// <param name="rdoIdBase">Rdo identifier base.</param>
        /// <param name="rdoIdCount">Rdo identifier count.</param>
        internal async Task SendSubSystemDataMessage(int rdoIdBase, byte rdoIdCount)
        {
            if (dbc != null)
            {
                //TODO Item #7 Drive RDOs
                await dbc.SendREMReadSubsystem((ushort)rdoIdBase, (char)rdoIdCount, (char)1);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendSubSystemDataMessage");
            }
        }

        /// <summary>
        /// DBCWs the rite config.
        /// </summary>
        /// <param name="status">Status.</param>
        internal async Task DBCWriteConfig(int status)
        {
            switch (status)
            {
                case (int)ConfigStatus.START_CONFIG_OK:
                case (int)ConfigStatus.START_ALREADY_CONFIG:
                    foreach (byte[] config in WriteConfigList)
                    {
                        await dbc.SendWriteRDO(config);
                    }
                    WriteConfigList.Clear();
                    break;
                case (int)ConfigStatus.START_CONFIG_DENIED:
                    Console.WriteLine("DBCWriteConfig - Config Denied");
                    break;
                case (int)ConfigStatus.WRITE_RDO_OK:
                    await dbc.SendREMWriteConfiguration();
                    break;
                case (int)ConfigStatus.WRITE_RDO_ERROR:
                    Console.WriteLine("DBCWriteConfig - Write Error");
                    break;
                case (int)ConfigStatus.WRITE_FLASH_BAD:
                case (int)ConfigStatus.WRITE_FLASH_FULL:
                    Console.WriteLine("DBCWriteConfig - FLASH issue");
                    break;
                case (int)ConfigStatus.WRITE_FLASH_OK:
                    await dbc.SendREMReset();
                    break;
                case (int)ConfigStatus.RESET_COMPLETE:
                    break;
                case (int)ConfigStatus.RESET_NOT_ATTEMPTED:
                    break;
                case (int)ConfigStatus.RESET_ERROR:
                    break;

            }
        }
        #endregion //Legacy REM

        #region EventHandlers
        /// <summary>
        /// Sends out the current logon state to the UI
        /// </summary>
        /// <returns></returns>
        /// <param name="msg">Message.</param>
        internal void Log(string msg)
        {
            OnPortStatus?.Invoke(this, msg);
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Eventhandler to consume messages coming from dongle
        /// </summary>
        /// <param name="msg">Message.</param>
        internal void MessageForUI(ServiceTool.Legacy.REM.MessageRDO msg)
        {
            if (dbc != null)
            {
                OnMesageReceived?.Invoke(this, msg);
            }
            else
            {
                Console.WriteLine("DBC init failed; - MessageForUI");
            }
        }

        /// <summary>
        /// Event handler for DBC library status
        /// </summary>
        /// <param name="status">Status.</param>
        internal void DBCErrorHandler(int status)
        {
            switch (status)
            {
                case (int)ErrorStatus.SVT_LOGON:
                    OnPortswapError?.Invoke(this, ServiceTool.Legacy.Dongle.Constants.SVTLogonError);
                    break;
                case (int)ErrorStatus.SVT_CONNECTED:
                    OnPortswapError?.Invoke(this, ServiceTool.Legacy.Dongle.Constants.SVTConnected);
                    break;
                case (int)ErrorStatus.SVT_TIMEOUT:
                case (int)ErrorStatus.SVT_NO_RESPONSE:
                    OnPortswapError?.Invoke(this, ServiceTool.Legacy.Dongle.Constants.SVTNoResponse);
                    break;
            }
        }

        internal void ZKIP(bool status)
        {
            OnPortZkip?.Invoke(this, status);
        }

        /// <summary>
        /// RDOEs the rror handler.
        /// </summary>
        /// <param name="error">Error.</param>
        internal void RDOErrorHandler(int error)
        {
            OnDBCError?.Invoke(this, error);
        }
        #endregion //EventHandler

        /// <summary>
        /// Resend this instance.
        /// </summary>
        /// <returns>The resend.</returns>
        internal async Task Resend()
        {
            if (dbc != null)
            {
                //TODO Item #7 Drive RDOs
                await dbc.Resend();
            }
        }

        enum ConfigStatus
        {
            START_CONFIG_OK,
            START_CONFIG_DENIED,
            START_ALREADY_CONFIG,
            WRITE_RDO_OK,
            WRITE_RDO_ERROR,
            WRITE_FLASH_OK,
            WRITE_FLASH_BAD,
            WRITE_FLASH_FULL,
            RESET_COMPLETE,
            RESET_NOT_ATTEMPTED,
            RESET_ERROR
        }

        enum ErrorStatus
        {
            SVT_TIMEOUT,
            SVT_LOGON,
            SVT_CONNECTED,
            SVT_NO_RESPONSE,
            SVT_INVALID_RDO,
            SVT_INVALID_WRITE
        };

    }
}