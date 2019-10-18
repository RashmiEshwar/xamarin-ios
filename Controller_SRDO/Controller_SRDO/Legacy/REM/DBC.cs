// Copyright 2019 Otis Elevator Co. All Rights Reserved.
//
//  The Source Code and the information it contains is the property of the
// Otis Elevator Company ("Otis"). The information it contains is highly
// confidential and is a trade secret of Otis. Access to this work is limited
// to those selected employees of Otis having a specific need to use it on
// behalf of Otis. It shall not be used by any other person for any purpose;
// it may not be reproduced, distributed, or disclosed by or to anyone,
// including any employee not having a specific need to use it without the
// express written permission of an officer of Otis.
//
//  Any unauthorized reproduction, disclosure, or distribution of copies by
// any person of any portion of this work may be a violation of the copyright
// law of the United States of America and other countries, and could result
// in the awarding of statutory damages of up to $150,000 dollars (17 USC 504)
// for infringement, and may result in further civil and criminal penalties.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ServiceTool.Legacy.Dongle;
using ServiceTool.Network;
using ZkipLibrary.Rnd;
using ZkipLibrary.Zkip;

namespace ServiceTool.Legacy.REM
{
    public class DBC
    {
        //static uint MyAddress = Constants.DBC_DEFAULT_ADDRESS;
        static byte MyType = Constants.DBC_GCS_TYPE;
        static uint MyTransactionNumber = Constants.DBC_MINIMUM_TX_NUMBER;
        static Data sendMessage;
        bool fragmentHeader;
        bool responseFlag;
        byte[] myTimestamp;

        List<byte> rawMesssageList = new List<byte>();
        List<byte[]> fullMessageList = new List<byte[]>();
        public List<MessageRDO> constructedMessageList = new List<MessageRDO>();
        public List<MessageStore> outgoing = new List<MessageStore>();

        public event EventHandler<MessageRDO> OnMessageReceived;
        public event EventHandler<int> OnStatusChanged;
        public event EventHandler<int> OnConfigChanged;
        public event EventHandler<int> OnRDOError;
        public event EventHandler<SRDOReadResponse> OnSRDORead;
        public event EventHandler<SRDOReadParameterizedResponse> OnSRDOReadPara;
        public event EventHandler<SRDOWriteResponse> OnSRDOWrite;
        public event EventHandler<SRDOWriteParameterizedResponse> OnSRDOWritePara;
        public event EventHandler<SRDOCommandResponse> OnSRDOCommandDialog;
        public event EventHandler<RTDDataResponse> OnSRDORTD;
        public event EventHandler<RTDStopResponse> OnRTDStop;
        public event EventHandler<bool> OnZkipStatus;

        Timer noRspTmr;

        ZkipClass zkip;
        RndClass rnd;

        public DBC(Data data)
        {
            sendMessage = data;
            Init();
        }

        /// <summary>
        /// Init this instance.
        /// </summary>
        void Init()
        {
            noRspTmr = new Timer
            {
                Interval = 6000
            };
            noRspTmr.Elapsed += (sender, e) =>
            {
                noRspTmr.Stop();

                if (responseFlag == false)
                {
                    OnStatusChanged?.Invoke(this, Constants.DBC_TIMED_OUT);
                }
            };

            noRspTmr.Start();
            rnd = new RndClass();
            rnd.Rnd_Init();
            zkip = new ZkipClass();
        }
        #region BUILD_REM_MESSAGE
        /// <summary>
        /// Builds the dbc message from dbc structure.
        /// </summary>
        /// <returns>The dbc message from dbc structure.</returns>
        /// <param name="stDbcMsg">St dbc message.</param>
        static byte[] BuildDbcMessageFromDbcStructure(Message stDbcMsg)
        {
            var dbcMsg = stDbcMsg;
            var buffer = new List<byte>();
            uint dbcAddress = 0;
            uint length = 0;
            uint truncateLength = 0;
            ushort crc16 = 0;

            // And, update any WILDCARD/NULL fields in the message.  NOTE:  This is done here so the APP can see what was sent.
            if ((dbcMsg.Source.Address == Constants.DBC_WILDCARD) || (dbcMsg.Source.Address == Constants.DBC_NULL_ADDRESS))
            {
                //dbcMsg.stSource.uiAddress = uiMyAddress
            }

            //Source Type
            if (dbcMsg.Source.Type == Constants.DBC_WILDCARD)
            {
                dbcMsg.Source.Type = MyType;
            }

            //Source transaction
            if (dbcMsg.Transaction == Constants.DBC_NULL_TRANSACTION)
            {
                dbcMsg.Transaction = (ushort) GetTransactionNumber();

            }

            // And, determine the destination address of the message.
            //      NOTE:  If the passed in address is NULL, send it to the WILDCARD address space.
            dbcAddress = dbcMsg.Destination.Address;

            if (dbcAddress == Constants.DBC_NULL_ADDRESS)
            {
                //dbcAddress = (int)Constants.DBC_WILDCARD_ADDRESS;
            }

            //byte[] bytes = BitConverter.GetBytes(dbcAddress);
            buffer.AddRange(BitConverter.GetBytes(dbcAddress));
            // GetBytes upCast to two byte array for a byte LOL
            buffer.Add(dbcMsg.Destination.Id);
            buffer.Add(dbcMsg.Destination.Type);

            // And, the source address fields - follow NMS NETWORK ORDER
            buffer.AddRange(BitConverter.GetBytes(dbcMsg.Source.Address));
            buffer.Add(dbcMsg.Source.Id);
            buffer.Add(dbcMsg.Source.Type);

            // And, the transaction number and function code - follow NMS NETWORK ORDER
            buffer.AddRange(BitConverter.GetBytes(dbcMsg.Transaction));
            buffer.Add(dbcMsg.Type);

            // And, handle the fragmentation of the DBC message
            // The fragmentation byte is divided into two parts
            // The upper 4 bits are the fragmentation flag - it is 0 if no fragmenation and 1 if the message is fragmented.
            // THe lower 4 bits are the number of fragmented messages.  It starts at the total fragments and decrements to 0.
            buffer.Add(dbcMsg.Fragmentation);

            length = dbcMsg.Size;

            // if uiMessageLength overflows pucBuffer, truncate the data portion to fit within the buffer.
            truncateLength = length + Constants.DBC_MESSAGE_HEADER_SIZE + Constants.DBC_MESSAGE_TRAILER_SIZE;
            if (truncateLength > Constants.DBC_MAX_MESSAGE_LENGTH)
            {
                length = Constants.DBC_MAX_MESSAGE_LENGTH - Constants.DBC_MESSAGE_HEADER_SIZE + Constants.DBC_MESSAGE_TRAILER_SIZE;
            }

            // And, serialize the message's data length
            // And, serialize the message's data length
            //buffer.append(contentsOf: Cereal.sharedInstance.toByteArray(CUnsignedShort(uiMessageLength)))
            buffer.AddRange(BitConverter.GetBytes((ushort) length));

            // So, copy the data
            buffer.AddRange(dbcMsg.Data);

            // And, calculate the CRC
            crc16 = CRC.CRC16(buffer.ToArray(), (ulong) buffer.Count, 0x00);

            // And, copy the CRC-16 to the last two bytes past the data - follow NMS NETWORK ORDER
            buffer.AddRange(BitConverter.GetBytes(crc16));

            //int[] z = list.ToArray();
            return buffer.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static uint GetTransactionNumber()
        {
            MyTransactionNumber += 1;

            if (MyTransactionNumber > Constants.DBC_MAXIMUM_TX_NUMBER)
            {
                MyTransactionNumber = Constants.DBC_MINIMUM_TX_NUMBER;
            }

            //Console.WriteLine(MyTransactionNumber);

            return MyTransactionNumber;
        }

        /// <summary>
        /// Slip the specified buffer.
        /// </summary>
        /// <returns>The slipped buffer</returns>
        /// <param name="buffer">Buffer.</param>
        byte[] Slip(byte[] buffer)
        {
            List<byte> byteList = new List<byte>(buffer);
            List<byte> slipList = new List<byte>();

            slipList.Add(Constants.SLIP_END);

            foreach (byte item in byteList)
            {
                switch (item)
                {
                    case Constants.SLIP_END:
                        slipList.Add(Constants.SLIP_ESC);
                        slipList.Add(Constants.SLIP_ESC_END);
                        break;
                    case Constants.SLIP_ESC:
                        slipList.Add(Constants.SLIP_ESC);
                        slipList.Add(Constants.SLIP_ESC_ESC);
                        break;
                    default:
                        slipList.Add(item);
                        break;
                }
            }

            slipList.Add(Constants.SLIP_END);

            return slipList.ToArray();
        }

        /// <summary>
        /// Deslip the buffer
        /// </summary>
        /// <returns>The de slip buffer</returns>
        /// <param name="buffer">Buffer.</param>
        byte[] DeSlip(byte[] buffer)
        {
            List<byte> byteList = new List<byte>(buffer);
            List<byte> deSlipList = new List<byte>();

            byte cLastChar = 0;

            foreach (byte item in byteList)
            {
                switch (item)
                {
                    case Constants.SLIP_END:
                        break;
                    case Constants.SLIP_ESC:
                        break;
                    case Constants.SLIP_ESC_ESC:
                        if (cLastChar == Constants.SLIP_ESC)
                        {
                            deSlipList.Add(Constants.SLIP_ESC);
                        }
                        else
                        {
                            deSlipList.Add(Constants.SLIP_ESC_ESC);
                        }
                        break;
                    case Constants.SLIP_ESC_END:
                        if (cLastChar == Constants.SLIP_ESC)
                        {
                            deSlipList.Add(Constants.SLIP_END);
                        }
                        else
                        {
                            deSlipList.Add(Constants.SLIP_ESC_END);
                        }
                        break;
                    default:
                        deSlipList.Add(item);
                        break;
                }

                cLastChar = item;
            }

            return deSlipList.ToArray();
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="messageType">Message type.</param>
        /// <param name="data">Data.</param>
        /// <param name="destAddress">Destination address.</param>
        /// <param name="destID">Destination identifier.</param>
        /// <param name="destType">Destination type.</param>
        async Task SendMessage(byte messageType, byte[] data, uint destAddress = 0xFFFFFFFF, byte destID = 0xFF, byte destType = 0x01)
        {
            DBCAddress destination = new DBCAddress()
            {
            Address = destAddress,
            Id = destID,
            Type = destType
            };

            DBCAddress source = new DBCAddress()
            {
                Address = Constants.DBC_WILDCARD_ADDRESS,
                Id = 0x01,
                Type = 0x0C
            };

            Message rawREM = new Message()
            {
                Destination = destination,
                Source = source,
                Transaction = (ushort) GetTransactionNumber(),
                Type = messageType,
                Priority = 0,
                Fragmentation = 0,
                Size = (ushort) data.Length,
                Data = data,
                CRC = 0
            };

            var fullREM = BuildDbcMessageFromDbcStructure(rawREM);
            Console.WriteLine("Sent:  " + ByteArrayToString(fullREM));
            var fullMsg = Slip(fullREM);
            //Console.WriteLine("Sending REM msg... Slip " + ByteArrayToString(fullMsg) + " L" + fullMsg.Length);
            MessageStore store = new MessageStore()
            {
                TID = rawREM.Transaction,
                Message = rawREM
            };

            outgoing.Add(store);
            await sendMessage.Send(fullMsg);
        }

        string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ");
        }
        #endregion //BUILD_REM_MESSAGE

        #region SEND_REM_MESSAGE
        /// <summary>
        /// Broadcast a heartbeat
        /// </summary>
        /// <returns></returns>
        public async Task HeartBeat(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_HEARTBEAT_CMD;
            byte[] data = Array.Empty<byte>();

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Respond to a heartbeat
        /// </summary>
        /// <returns></returns>
        public async Task HeartBeatResponse(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_HEARTBEAT_RSP;
            byte[] data = Array.Empty<byte>();

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Get the SCN
        /// </summary>
        /// <returns>The scn.</returns>
        public async Task SCN(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_READ_RDO;
            byte[] data = BitConverter.GetBytes((UInt16) 704);

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Sends the REM read RDO message
        /// </summary>
        /// <returns></returns>
        /// <param name="msgRDO">Message rdo.</param>
        public async Task SendReadRDO(byte[] msgRDO, byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_READ_RDO;

            await SendMessage(msgType,
                msgRDO,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        public async Task SendWriteRDO(byte[] msgRDO, byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_WRITE_RDO;

            await SendMessage(msgType,
                msgRDO,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        //In this case the app has to use the REM_READ_SUBSYSTEM_DATA command with the following data attached!
        //unsigned short RdoIdBase,
        //unsigned char RdoIdCount,
        //unsigned char SubsystemIdDestination   //for the drive use : 1
        /// <summary>
        /// Sends the REM read subsystem message
        /// </summary>
        /// <returns>The REMR ead subsystem.</returns>
        /// <param name="rdoIdBase">Rdo identifier base.</param>
        /// <param name="rdoIdCount">Rdo identifier count.</param>
        /// <param name="subSystemIdDestination">Sub system identifier destination.</param>
        public async Task SendREMReadSubsystem(ushort rdoIdBase, char rdoIdCount, char subSystemIdDestination, byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_READ_SUBSYSTEM_DATA;

            var buffer = new List<byte>();

            var rdo = BitConverter.GetBytes(rdoIdBase);
            //if(rdo[0] == 0)
            Array.Reverse(rdo);

            buffer.AddRange(rdo);
            buffer.Add((byte) rdoIdCount);
            buffer.Add((byte) subSystemIdDestination);

            await SendMessage(msgType,
                buffer.ToArray(),
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Sends the REM request time stamp.
        /// </summary>
        /// <returns>The REMR equest time stamp.</returns>
        /// <param name="myDest">My destination.</param>
        public async Task SendREMRequestTimeStamp(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_REQUEST_TIME_STAMP;
            byte[] data = Array.Empty<byte>();

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Sends the REM start config.
        /// </summary>
        /// <returns>The REMS tart config.</returns>
        /// <param name="myDest">My destination.</param>
        public async Task SendREMStartConfig(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_START_CONFIG;

            byte[] startConfig = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var buffer = new List<byte>();

            buffer.AddRange(myTimestamp);
            buffer.AddRange(startConfig);

            await SendMessage(msgType,
                buffer.ToArray(),
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Sends the REM Write configuration.
        /// </summary>
        /// <returns>The REMW rite configuration.</returns>
        /// <param name="myDest">My destination.</param>
        public async Task SendREMWriteConfiguration(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_WRITE_CONFIG;
            byte[] data = Array.Empty<byte>();

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Sends the REM Reset.
        /// </summary>
        /// <returns>The REMR eset.</returns>
        /// <param name="myDest">My destination.</param>
        public async Task SendREMReset(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_RESET;
            byte[] data = BitConverter.GetBytes((byte) 0x00);

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                (byte) destType);
        }

        /// <summary>
        /// Sends the ZKIP request access REM message
        /// </summary>
        /// <returns></returns>
        /// <param name="myDest">Destination.</param>
        public async Task SendRequestAccessCommand(byte myDest = 0x01)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = myDest;
            var msgType = Constants.REM_RESET_ACCESS_CMD;
            byte[] data = Array.Empty<byte>();

            await SendMessage(msgType,
                data,
                (uint) destAddress,
                (byte) destID,
                destType);
        }

        /// <summary>
        /// Sends the ZKIP reponse REM message
        /// </summary>
        /// <returns>The REMZ kip response.</returns>
        /// <param name="zkipRsp">Zkip rsp.</param>
        /// <param name="index">Index.</param>
        /// <param name="zkipData">Zkip data.</param>
        /// <param name="dest">Destination.</param>
        async Task SendREMZkipResponse(byte zkipRsp, byte index, byte[] zkipData, byte dest)
        {
            var destAddress = Constants.DBC_WILDCARD_ADDRESS;
            var destID = 0xFF;
            var destType = dest;
            var msgType = Constants.REM_ZKIP_RSP;

            var buffer = new List<byte>();

            buffer.Add(zkipRsp);
            buffer.Add(index);
            buffer.AddRange(zkipData);

            await SendMessage(msgType,
                buffer.ToArray(),
                (uint) destAddress,
                (byte) destID,
                destType);
        }
        #endregion //SEND_MESSAGE

        #region SEND_SRDO
        public async Task SendSRDOMessage(byte msgType, byte[] rawData, uint destAddress, byte destID, byte destType)
        {
            await SendMessage(msgType,
                rawData,
                destAddress,
                destID,
                destType);
        }
        #endregion

        #region PROCESS_MESSAGE
        /// <summary>
        /// Processes the incoming ZKIp command REM message
        /// </summary>
        /// <returns></returns>
        /// <param name="msg">Message.</param>
        async Task ProcessZkipCommand(byte[] msg)
        {
            byte[] zkipStatus = new byte[2];
            Array.Copy(msg, 18, zkipStatus, 0, 1);
            var zkipStatusVal = BitConverter.ToUInt16(zkipStatus, 0);

            byte[] zkipIndex = new byte[2];
            Array.Copy(msg, 19, zkipIndex, 0, 1);
            var zkipIndexVal = BitConverter.ToUInt16(zkipIndex, 0);
            //Console.WriteLine("SubsystemId Val " + SubsystemIdVal);

            byte[] zkipCommand = new byte[2];
            Array.Copy(msg, 20, zkipCommand, 0, 1);
            var zkipCommandVal = BitConverter.ToUInt16(zkipCommand, 0);

            byte destType = 0x01;

            switch (zkipCommandVal)
            {
                case Constants.REM_ZKIP_H:
                    zkip.zkip_command((char) zkipCommandVal, 0, 0);
                    var zkipAnswer_h = zkip.zkip_handler((char) zkipCommandVal, 0, 0, Constants.REM_ZKIP_LEVEL_ADJUSTER);
                    await SendREMZkipResponse(Constants.REM_ZKIP_H, zkipAnswer_h.mcs_data[0], Array.Empty<byte>(), destType);
                    Console.WriteLine("H");
                    break;
                case Constants.REM_ZKIP_J:
                    //Rec'v: FF FF FF FF 01 0C FE FE FE 00 01 13 04 10 4A 00 05 00 00 00 6A 05 00 4F 63
                    var zkipAnswer_j = zkip.zkip_handler((char) zkipCommandVal, msg[21], 0, Constants.REM_ZKIP_LEVEL_ADJUSTER);
                    var zkipDataLength = zkipAnswer_j.mcs_data.Length - 2;
                    byte[] zkipData = new byte[zkipDataLength];
                    Array.Copy(zkipAnswer_j.mcs_data, 1, zkipData, 0, zkipDataLength);
                    await SendREMZkipResponse(Constants.REM_ZKIP_J, zkipAnswer_j.mcs_data[0], zkipData, destType);
                    Console.WriteLine("J");
                    break;
                case Constants.REM_ZKIP_K:
                    //FF FF FF FF 01 0C FE FE FE 00 01 13 08 10 4A 00 05 00 00 00 6B 05 30 FA C9
                    var zkipAnswer_k = zkip.zkip_handler((char) zkipCommandVal, msg[21], msg[22], Constants.REM_ZKIP_LEVEL_ADJUSTER);

                    var zkipRSP = Constants.REM_ZKIP_R;

                    if (zkipAnswer_k.mcs_data[33] == Constants.REM_ZKIP_P)
                    {
                        zkipRSP = Constants.REM_ZKIP_P;
                    }
                    else
                    {
                        zkipRSP = Constants.REM_ZKIP_Q;
                    }

                    var zkipDataLength_k = zkipAnswer_k.mcs_data.Length - 2;
                    byte[] zkipData_k = new byte[zkipDataLength_k];
                    Array.Copy(zkipAnswer_k.mcs_data, 1, zkipData_k, 0, zkipDataLength_k);
                    await SendREMZkipResponse(zkipRSP, zkipAnswer_k.mcs_data[0], zkipData_k, destType);
                    Console.WriteLine("K");
                    break;
                case Constants.REM_ZKIP_I:
                    var zkipAnswer_i = zkip.zkip_handler((char) zkipCommandVal, msg[21], 0, Constants.REM_ZKIP_LEVEL_ADJUSTER);
                    var zkipDataLength_i = zkipAnswer_i.mcs_data.Length - 2;
                    byte[] zkipData_i = new byte[zkipDataLength_i];
                    Array.Copy(zkipAnswer_i.mcs_data, 1, zkipData_i, 0, zkipDataLength_i);
                    await SendREMZkipResponse(Constants.REM_ZKIP_Z, zkipAnswer_i.mcs_data[0], zkipData_i, destType);
                    Console.WriteLine("I");
                    break;
                case Constants.REM_ZKIP_E:
                    var zkipAnswer_e = zkip.zkip_handler((char) zkipCommandVal, msg[21], msg[22], Constants.REM_ZKIP_LEVEL_ADJUSTER);

                    var zkipRSP_e = Constants.REM_ZKIP_R;

                    if (zkipAnswer_e.mcs_data[33] == Constants.REM_ZKIP_U)
                    {
                        zkipRSP_e = Constants.REM_ZKIP_U;
                    }
                    else
                    {
                        zkipRSP_e = Constants.REM_ZKIP_V;
                    }

                    var zkipDataLength_e = zkipAnswer_e.mcs_data.Length - 2;
                    byte[] zkipData_e = new byte[zkipDataLength_e];
                    Array.Copy(zkipAnswer_e.mcs_data, 1, zkipData_e, 0, zkipDataLength_e);
                    await SendREMZkipResponse(zkipRSP_e, zkipAnswer_e.mcs_data[0], zkipData_e, destType);
                    Console.WriteLine("E");
                    break;
                case Constants.REM_ZKIP_STATUS:
                    if (zkipStatusVal == Constants.REM_ZKIP_STATUS_OFT_A)
                    {
                        await SCN();
                        await SendREMRequestTimeStamp();
                        Console.WriteLine("ZkipPass");
                        OnZkipStatus?.Invoke(this, true);
                    }
                    else
                    {
                        OnZkipStatus?.Invoke(this, false);
                        Console.WriteLine("Not OFT_A access level");
                    }
                    break;
            }
        }

        /// <summary>
        /// Processes the incoming RAW bytes; handles multiple REM messages
        /// </summary>
        /// <returns></returns>
        /// <param name="msgRaw">Message raw.</param>
        public async Task ProcessRaw(byte[] msgRaw)
        {
            int countHeader = 0;

            foreach (byte lByte in msgRaw)
            {
                if (lByte == 0xC0)
                {
                    countHeader += 1;
                }
            }

            if (countHeader == 2)
            {
                //Console.WriteLine("Full " + Helper.ByteArrayToString(msgRaw));
                //Console.WriteLine("Full deslip " + Helper.ByteArrayToString(DeSlip(msgRaw)));
                //fullMessageList.Add(DeSlip(msgRaw));
                await ProcessMessage(DeSlip(msgRaw));
            }
            else
            {
                //Console.WriteLine("Fragment " + countHeader + " " + Helper.ByteArrayToString(msgRaw));

                for (int i = 0; i < msgRaw.Length; i++)
                {
                    if (msgRaw[i] == 0xC0)
                    {
                        if (fragmentHeader == false)
                        {
                            //WE have the header
                            fragmentHeader = true;
                            rawMesssageList.Add(msgRaw[i]);
                        }
                        else
                        {
                            //At the end
                            fragmentHeader = false;
                            rawMesssageList.Add(msgRaw[i]);
                            //Console.WriteLine("Full Message " + Helper.ByteArrayToString(DeSlip(rawMesssageList.ToArray())));
                            //Add to full message list here
                            await ProcessMessage(DeSlip(rawMesssageList.ToArray()));
                            rawMesssageList.Clear();
                        }
                    }
                    else
                    {
                        rawMesssageList.Add(msgRaw[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a good REM message and send to respective handler
        /// </summary>
        /// <returns></returns>
        /// <param name="msgREM">Message rem.</param>
        async Task ProcessMessage(byte[] msgREM)
        {
            //Rem Message
            //0 - 5 Dest Address
            //6 - 11 Source Adddress
            //12 - 13 Transaction
            //14 - 15 Type
            //16 - 17 Length
            //18 - X Payload
            //X - 2 CRC
            byte[] msgID = new byte[2];

            if (responseFlag == false)
            {
                responseFlag = true;
                OnStatusChanged?.Invoke(this, Constants.DBC_CONNECTED);
            }

            var validCRC = VerifyCRC(msgREM);
            //Console.WriteLine("Rec'v: " + Helper.ByteArrayToString(msgREM) );

            if ((msgREM.Length > 16) && (validCRC == true))
            {
                Array.Copy(msgREM, 14, msgID, 0, 2);
                var msgIDChar = BitConverter.ToChar(msgID, 0);
                var myMsg = ProcessesHeader(msgREM);
                sendMessage.status.IncomingCount += 1;
                Console.WriteLine("In " + sendMessage.status.IncomingCount);

                RemoveFromList(myMsg);
                Console.WriteLine("Current size " + outgoing.Count);
                switch (msgIDChar)
                {
                    case (char) Constants.REM_HEARTBEAT_CMD:
                        await HeartBeatResponse();
                        break;
                    case (char) Constants.REM_READ_RDO_RSP:
                        ProcessReadRDOs(msgREM);
                        break;
                    case (char) Constants.REM_READ_SUBSYSTEM_DATA_RSP:
                        ProcessSubsystemRDO(msgREM);
                        break;
                    case (char) Constants.REM_ZKIP_CMD:
                        //TODO Item #4 ZKIP handling
                        await ProcessZkipCommand(msgREM);
                        break;
                    case (char) Constants.REM_START_CONFIG_RSP:
                        ProccessStartConfigRsp(myMsg);
                        break;
                    case (char) Constants.REM_WRITE_CONFIG_RSP:
                        ProccessWriteConfigRsp(myMsg);
                        break;
                    case (char) Constants.REM_WRITE_RDO_RSP:
                        ProccessWriteRDORsp(myMsg);
                        break;
                    case (char) Constants.REM_RESET_RSP:
                        ProccessResetRsp(myMsg);
                        break;
                    case (char) Constants.REM_TIME_STAMP_RSP:
                        break;
                    case (char) Constants.REM_REQUEST_TIME_STAMP_RSP:
                        ProccessTimeStamp(myMsg);
                        break;
                    case (char) Constants.REM_READ_DATA_ITEM_RSP:
                        ProcessSRDORead(myMsg);
                        break;
                    case (char) Constants.REM_READ_DATA_ITEM_PARA_RSP:
                        ProcessSRDOReadPara(myMsg);
                        break;
                    case (char) Constants.REM_WRITE_DATA_ITEM_RSP:
                        ProcessSRDOWrite(myMsg);
                        break;
                    case (char) Constants.REM_WRITE_DATA_PARA_RSP:
                        ProcessSRDOWritePara(myMsg);
                        break;
                    case (char) Constants.REM_DIALOG_COMMAND_RSP:
                        ProcessSRDOCommandDialog(myMsg);
                        break;
                    case (char) Constants.REM_RTD_START_RSP:
                        ProcessRTDStart(myMsg);
                        break;
                    case (char) Constants.REM_RTD_STOP_RSP:
                        ProcessRTDStop(myMsg);
                        break;
                    case (char) Constants.REM_RTD_DATA:
                        ProcessRTDData(myMsg);
                        break;
                }
            }
        }

        /// <summary>
        /// Removes from list.
        /// </summary>
        /// <param name="myMessage">My message.</param>
        public void RemoveFromList(Message myMessage)
        {
            for (int i = 0; i < outgoing.Count; i++)
            {
                MessageStore msg = outgoing[i];
                if (msg.TID == myMessage.Transaction)
                {
                    outgoing.RemoveAt(i);
                }
            }

        }

        /// <summary>
        /// Resend this instance.
        /// </summary>
        /// <returns>The resend.</returns>
        public async Task Resend()
        {
            List<MessageStore> resend = new List<MessageStore>(outgoing);
            outgoing.Clear();
            for (int i = 0; i < resend.Count; i++)
            {
                resend[i].Message.Transaction = (ushort) GetTransactionNumber();
                var fullREM = BuildDbcMessageFromDbcStructure(resend[i].Message);

                Console.WriteLine("Re-Sent:  " + ByteArrayToString(fullREM));
                var fullMsg = Slip(fullREM);

                MessageStore store = new MessageStore()
                {
                    TID = resend[i].Message.Transaction,
                    Message = resend[i].Message
                };

                outgoing.Add(store);
                Console.WriteLine("Added " + outgoing.Count);
                await Task.Delay(300);
                await sendMessage.Send(fullMsg);
            }

            //Console.WriteLine(outgoing.Count);
            //await Resend();
        }

        /// <summary>
        /// Verifies the crc.
        /// </summary>
        /// <returns><c>true</c>, if crc was verifyed, <c>false</c> otherwise.</returns>
        /// <param name="msg">Message.</param>
        bool VerifyCRC(byte[] msg)
        {
            List<Byte> data = new List<Byte>(msg);

            //Console.WriteLine("CRC Data Size " + data.Count);
            if (data.Count == 0)
            {
                return false;
            }
            var remoteCRC = data.GetRange(data.Count - 2, 2);
            data.RemoveAt(data.Count - 1);
            data.RemoveAt(data.Count - 1);

            Console.WriteLine("RCV'v: " + Helper.ByteArrayToString(msg));
            // And, calculate the CRC
            ushort crc16 = CRC.CRC16(data.ToArray(), (ulong) data.Count, 0x00);
            // And, copy the CRC-16 to the last two bytes past the data - follow NMS NETWORK ORDER
            var myCRC = BitConverter.GetBytes(crc16);

            //Console.WriteLine("Remote CRC'v: " + Helper.ByteArrayToString(remoteCRC.ToArray()) + " Calculated CRC'v: " + Helper.ByteArrayToString(myCRC));

            if (BitConverter.ToUInt16(remoteCRC.ToArray(), 0) == BitConverter.ToUInt16(myCRC, 0))
            {
                return true;
            }

            return false;
        }
        #endregion //Process Message

        #region SRDO_RECV
        /// <summary>
        /// Processes the SRDO Read.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessSRDORead(Message msg)
        {
            var srdoRead = new SRDOReadResponse();
            List<Byte> data = new List<Byte>(msg.Data);

            //Parse out msg data payload and store into the SRDO Read message obj
            if (data.Count > 2)
            {
                //Device Type
                srdoRead.DeviceType = msg.Source.Type;
                srdoRead.TID = msg.Transaction;
                //Page
                srdoRead.Page = data[0];
                //SRDO
                var srdo = data.GetRange(1, 2);
                srdoRead.SRDO = BitConverter.ToUInt16(srdo.ToArray(), 0);
                data.RemoveRange(1, 2);
                //Data
                var dataLength = data.GetRange(1, 2);
                srdoRead.Length = BitConverter.ToUInt16(dataLength.ToArray(), 0);
                data.RemoveRange(1, 2);
                // Check for error status
                if (srdoRead.Length == 0x8002)
                {
                    //Get error code
                    var errorCode = data.GetRange(1, 2);
                    srdoRead.Error = BitConverter.ToUInt16(errorCode.ToArray(), 0);
                    data.RemoveRange(1, 2);
                    data.Append(srdoRead.Page);
                    OnSRDORead?.Invoke(this, srdoRead);
                    msg.Data = data.ToArray();
                    ProcessSRDORead(msg);
                }
                else
                {
                    //Get data
                    var srdoData = data.GetRange(1, srdoRead.Length);
                    srdoRead.Data = srdoData.ToArray();
                    srdoRead.Error = 0;
                    data.RemoveRange(1, srdoRead.Length);
                    msg.Data = data.ToArray();
                    OnSRDORead?.Invoke(this, srdoRead);
                    ProcessSRDORead(msg);
                }
            }
        }

        /// <summary>
        /// Processes the SRDORead para.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessSRDOReadPara(Message msg)
        {
            var srdoRead = new SRDOReadParameterizedResponse();
            List<Byte> data = new List<Byte>(msg.Data);

            //Parse out msg data payload and store into the SRDO Read message obj
            if (data.Count > 2)
            {
                //Device Type
                srdoRead.DeviceType = msg.Source.Type;
                srdoRead.TID = msg.Transaction;
                //Page
                srdoRead.Page = data[0];
                //SRDO
                var srdo = data.GetRange(1, 2);
                srdoRead.SRDO = BitConverter.ToUInt16(srdo.ToArray(), 0);
                data.RemoveRange(1, 2);
                //P1
                var p1 = data.GetRange(1, 2);
                srdoRead.P1 = BitConverter.ToUInt16(p1.ToArray(), 0);
                data.RemoveRange(1, 2);
                //P2
                var p2 = data.GetRange(1, 2);
                srdoRead.P2 = BitConverter.ToUInt16(p2.ToArray(), 0);
                data.RemoveRange(1, 2);
                //P3
                var p3 = data.GetRange(1, 4);
                srdoRead.P3 = BitConverter.ToUInt32(p3.ToArray(), 0);
                data.RemoveRange(1, 4);
                //Length
                var dataLength = data.GetRange(1, 2);
                srdoRead.Length = BitConverter.ToUInt16(dataLength.ToArray(), 0);
                data.RemoveRange(1, 2);

                // Check for error status
                if (srdoRead.Length == 0x8002)
                {
                    //Get error code
                    var errorCode = data.GetRange(1, 2);
                    srdoRead.Error = BitConverter.ToUInt16(errorCode.ToArray(), 0);

                    data.RemoveRange(1, 2);
                    data.Append(srdoRead.Page);

                    msg.Data = data.ToArray();
                    OnSRDOReadPara?.Invoke(this, srdoRead);
                    ProcessSRDOReadPara(msg);
                }
                else
                {
                    //Get data
                    var srdoData = data.GetRange(1, srdoRead.Length);
                    srdoRead.Data = srdoData.ToArray();
                    data.RemoveRange(1, srdoRead.Length);
                    msg.Data = data.ToArray();

                    OnSRDOReadPara?.Invoke(this, srdoRead);
                    ProcessSRDOReadPara(msg);
                }
            }
        }

        /// <summary>
        /// Processes the SRDOWrite.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessSRDOWrite(Message msg)
        {
            List<Byte> data = new List<Byte>(msg.Data);

            if (msg.Size == 1)
            {
                var sRDO = new SRDOWriteResponse
                {
                //Write is good
                //Device Type
                DeviceType = msg.Source.Type,
                //Page
                Page = data[0],
                TID = msg.Transaction
                };
                OnSRDOWrite?.Invoke(this, sRDO);
            }
            else
            {
                //Handle error
                var sRDOError = new SRDOWriteResponse();
                //Parse out msg data payload and store into the SRDO Read message obj
                if (data.Count > 2)
                {
                    //Device Type
                    sRDOError.DeviceType = msg.Source.Type;
                    sRDOError.TID = msg.Transaction;
                    //Page
                    sRDOError.Page = data[0];
                    //SRDO
                    var mySrdo = data.GetRange(1, 2);
                    sRDOError.SRDO = BitConverter.ToUInt16(mySrdo.ToArray(), 0);
                    data.RemoveRange(1, 2);
                    //Data
                    var dataLength = data.GetRange(1, 2);
                    sRDOError.Length = BitConverter.ToUInt16(dataLength.ToArray(), 0);
                    data.RemoveRange(1, 2);
                    // Check for error status
                    if (sRDOError.Length == 0x8002)
                    {
                        //Get error code
                        var errorCode = data.GetRange(1, 2);
                        sRDOError.Error = BitConverter.ToUInt16(errorCode.ToArray(), 0);

                        data.RemoveRange(1, 2);
                        data.Append(sRDOError.Page);
                        // emit srdoRead message
                        msg.Data = data.ToArray();
                        OnSRDOWrite?.Invoke(this, sRDOError);
                        ProcessSRDOWrite(msg);
                    }

                }
            }
        }

        /// <summary>
        /// Processes the SRDOWritepara.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessSRDOWritePara(Message msg)
        {
            List<Byte> data = new List<Byte>(msg.Data);

            if (msg.Size == 1)
            {
                var sRDO = new SRDOWriteParameterizedResponse
                {
                //Write is good
                //Device Type
                DeviceType = msg.Source.Type,
                //Page
                Page = data[0],
                TID = msg.Transaction
                };
                OnSRDOWritePara?.Invoke(this, sRDO);
            }
            else
            {
                //Handle error
                var sRDOError = new SRDOWriteParameterizedResponse();
                //Parse out msg data payload and store into the SRDO Read message obj
                if (data.Count > 2)
                {
                    //Device Type
                    sRDOError.DeviceType = msg.Source.Type;
                    sRDOError.TID = msg.Transaction;
                    //Page
                    sRDOError.Page = data[0];
                    //SRDO
                    var srdo = data.GetRange(1, 2);
                    sRDOError.SRDO = BitConverter.ToUInt16(srdo.ToArray(), 0);
                    data.RemoveRange(1, 2);
                    //P1
                    var p1 = data.GetRange(1, 2);
                    sRDOError.P1 = BitConverter.ToUInt16(p1.ToArray(), 0);
                    data.RemoveRange(1, 2);
                    //P2
                    var p2 = data.GetRange(1, 2);
                    sRDOError.P2 = BitConverter.ToUInt16(p2.ToArray(), 0);
                    data.RemoveRange(1, 2);
                    //P3
                    var p3 = data.GetRange(1, 4);
                    sRDOError.P3 = BitConverter.ToUInt32(p3.ToArray(), 0);
                    data.RemoveRange(1, 4);
                    //Length
                    var dataLength = data.GetRange(1, 2);
                    sRDOError.Length = BitConverter.ToUInt16(dataLength.ToArray(), 0);
                    data.RemoveRange(1, 2);

                    // Check for error status
                    if (sRDOError.Length == 0x8002)
                    {
                        //Get error code
                        var errorCode = data.GetRange(1, 2);
                        sRDOError.Error = BitConverter.ToUInt16(errorCode.ToArray(), 0);

                        data.RemoveRange(1, 2);
                        data.Append(sRDOError.Page);
                        // emit srdoRead message
                        msg.Data = data.ToArray();
                        OnSRDOWritePara?.Invoke(this, sRDOError);
                        ProcessSRDOWritePara(msg);
                    }
                    else
                    {
                        var sRDO = new SRDOWriteParameterizedResponse
                        {
                            //Write is good
                            //Device Type
                            DeviceType = msg.Source.Type,
                            //Page
                            Page = data[0],
                            TID = msg.Transaction
                        };
                        OnSRDOWritePara?.Invoke(this, sRDO);
                        // Length = 0 the write was good;
                        // emit srdoRead message
                        ProcessSRDOWritePara(msg);
                    }

                }
            }
        }

        /// <summary>
        /// Processes the SRDOC ommand dialog.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessSRDOCommandDialog(Message msg)
        {
            var sRDO = new SRDOCommandResponse();
            List<Byte> data = new List<Byte>(msg.Data);

            sRDO.DeviceType = msg.Source.Type;
            sRDO.TID = msg.Transaction;

            sRDO.Response = data[0];

            var augLenth = data.GetRange(1, 2);
            sRDO.ArgumentLength = BitConverter.ToUInt16(augLenth.ToArray(), 0);

            if (sRDO.ArgumentLength > 2)
            {
                var srdoData = data.GetRange(3, sRDO.ArgumentLength);
                sRDO.Argument = srdoData.ToArray();
            }

            OnSRDOCommandDialog?.Invoke(this, sRDO);
            //TODO insert 
        }

        /// <summary>
        /// Processes the RTDS tart.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessRTDStart(Message msg)
        {
            var rtdStart = new RTDStartResponse();
            // No response from controller?
        }

        /// <summary>
        /// Processes the RTDS top.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessRTDStop(Message msg)
        {
            var sRDO = new RTDStopResponse
            {
                DeviceType = msg.Source.Type,
                TID = msg.Transaction
            };

            if (msg.Size == 0)
            {
                //OK
                OnRTDStop?.Invoke(this, sRDO);
            }
        }

        /// <summary>
        /// Processes the RTDD ata.
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessRTDData(Message msg)
        {
            var sRDO = new RTDDataResponse
            {
                DeviceType = msg.Source.Type,
                TID = msg.Transaction,
                Length = msg.Size,
                Data = msg.Data
            };
            // To parse out data one needs to know the entry in sRDO info file
            // Current format
            // 1 byte page; 2 bytes sRDO; X bytes data;
            // sRDO data is derived from (Type * Quantity)
            OnSRDORTD?.Invoke(this, sRDO);
        }
        #endregion //SRDO_RECV

        #region LEGACY_RDO
        /// <summary>
        /// Processes the subsystem data rdo.
        /// 
        /// </summary>
        /// <param name="msg">Message.</param>
        void ProcessSubsystemRDO(byte[] msg)
        {
            /*
             * 
             *  FF FF FF FF FF 01 
                FF FF FF FF 01 0C 
                06 10 
                0A 00 
                08 00 17 
                00 00 19 result
                00 00 21 rdo id
                00 22 ss
                00 00 24 data length
                00
                
             * */

            //Check size
            //TID

            byte[] transaction = new byte[2];
            Array.Copy(msg, 12, transaction, 0, 2);
            var transactionID = BitConverter.ToUInt16(transaction, 0);
            //Console.WriteLine("TID " + transactionID);

            //Payload Length
            byte[] msgSz = new byte[2];
            Array.Copy(msg, 16, msgSz, 0, 2);
            var msgValue = BitConverter.ToUInt16(msgSz, 0);
            //Console.WriteLine("Response Size " + msgValue);

            byte[] result = new byte[2];
            Array.Copy(msg, 18, result, 0, 2);
            var resultVal = BitConverter.ToUInt16(result, 0);
            //Console.WriteLine("Result Val " + resultVal);

            byte[] RDOid = new byte[2];
            Array.Copy(msg, 20, RDOid, 0, 2);
            Array.Reverse(RDOid);
            var RDOidVal = BitConverter.ToUInt16(RDOid, 0);
            //Console.WriteLine("RDO id Val " + RDOidVal);

            byte[] SubsystemId = new byte[2];
            Array.Copy(msg, 22, SubsystemId, 0, 1);
            var SubsystemIdVal = BitConverter.ToUInt16(SubsystemId, 0);
            //Console.WriteLine("SubsystemId Val " + SubsystemIdVal);

            byte[] dataLength = new byte[2];
            Array.Copy(msg, 23, dataLength, 0, 2);
            Array.Reverse(dataLength);
            var dataLengthVal = BitConverter.ToUInt16(dataLength, 0);
            //Console.WriteLine("Data Length Val " + dataLengthVal);

            byte[] subSystemData = new byte[dataLengthVal];
            Array.Copy(msg, 25, subSystemData, 0, dataLengthVal);

            //Console.WriteLine(Helper.ByteArrayToString(subSystemData));

            //byte[] dataBuffer = new byte[dataLengthVal];
            //Array.Copy(msg, 25, dataBuffer, 0, dataLengthVal);

            //Console.WriteLine("TID " + transactionID + 
            //" Msg Size " + msgValue + 
            //" Result " + resultVal +
            //" RDO id " + RDOidVal + 
            //" SubsystemId " + SubsystemIdVal +
            //" Data Length " + dataLengthVal +
            //" Rec'v Length " + msg.Length);

            MessageRDO oneMsg = new MessageRDO()
            {
                Device = 0,
                Transaction = (ushort) transactionID,
                RDO = (ushort) RDOidVal,
                //Data = rdoPayloadData
                Data = Helper.ByteArrayToString(subSystemData),
                Size = msgValue
            };

            InsertMessageToList(oneMsg);
        }

        /// <summary>
        /// Processes the read RDO.
        /// 
        /// Rem Message
        /// 0 - 5 Dest Address
        /// 6 - 11 Source Adddress
        /// 12 - 13 Transaction
        /// 14 - 15 Type
        /// 16 - 17 Length of RDO Payload
        /// 18 - 19 RDO id
        /// 20 - RDO length
        /// 21 - RDO Extra
        /// 22- X Payload
        /// X - 2 CRC
        /// </summary>
        /// <param name="msgReadRDO">Message read rdo.</param>
        void ProcessReadRDOs(byte[] msgReadRDO)
        {
            List<byte> readRDOList = new List<Byte>(msgReadRDO);

            byte[] transaction = new byte[2];
            Array.Copy(msgReadRDO, 12, transaction, 0, 2);
            var transactionID = BitConverter.ToUInt16(transaction, 0);
            //Console.WriteLine("TID " + transactionID);

            //Payload Length
            byte[] msgSz = new byte[2];
            Array.Copy(msgReadRDO, 16, msgSz, 0, 2);
            //int msgValue = Convert.ToInt32(msgReadRDO[16]);
            var msgValue = BitConverter.ToUInt16(msgSz, 0);
            //Console.WriteLine("Response Size " + msgValue);

            //Rdo id?
            //Two different types;
            byte[] rdoID = new byte[2];
            Array.Copy(msgReadRDO, 18, rdoID, 0, 2);
            var rdoIDVal = BitConverter.ToUInt16(rdoID, 0);
            //Console.WriteLine("RDO id " + rdoIDVal);

            //First RDO packet
            byte[] rdoPayload = new byte[2];
            Array.Copy(msgReadRDO, 20, rdoPayload, 0, 2);
            var rdoPayloadVal = BitConverter.ToUInt16(rdoPayload, 0);

            if (rdoPayloadVal > Constants.DBC_MAX_MESSAGE_LENGTH)
            {
                rdoPayload[1] &= byte.MaxValue ^ (1 << 7);
                rdoPayload[1] &= byte.MaxValue ^ (1 << 6);
                rdoPayload[1] &= byte.MaxValue ^ (1 << 5);
                //Array.Copy(msgReadRDO, 20, rdoPayload, 0, 1);
                rdoPayloadVal = BitConverter.ToUInt16(rdoPayload, 0);

                byte[] rdoPayloadError = new byte[rdoPayloadVal];
                Array.Copy(msgReadRDO, 22, rdoPayloadError, 0, rdoPayloadVal);

                //TODO Item# 3 RDO error
                //Grab the payload and parse error; Payload is two bytes
                byte[] error = new byte[rdoPayloadVal];
                Array.Copy(msgReadRDO, 22, error, 0, rdoPayloadVal);
                var errorVal = BitConverter.ToUInt16(error, 0);
                OnRDOError?.Invoke(this, errorVal);
            }
            //Console.WriteLine("RDO data " + rdoPayloadVal);

            var verifyMessage = msgValue - (rdoPayloadVal + Constants.REM_READ_RSP_HEADER_SIZE);
            var msgOffset = 0; //GetMessageOffset(rdoIDVal);

            Console.WriteLine("TID " + transactionID +
                " Msg Size " + msgValue +
                " RDO id " + rdoIDVal +
                " RDO Size " + rdoPayloadVal);

            if (verifyMessage == 0)
            {
                byte[] rdoPayloadData = new byte[rdoPayloadVal];
                Array.Copy(msgReadRDO, 22, rdoPayloadData, 0, rdoPayloadVal);
                //Console.WriteLine(System.Text.Encoding.ASCII.GetString(rdoPayloadData));
                MessageRDO oneMsg = new MessageRDO()
                {
                    Device = 0,
                    Transaction = (ushort) transactionID,
                    RDO = (ushort) rdoIDVal,
                    //Data = rdoPayloadData
                    Data = Helper.ByteArrayToString(rdoPayloadData),
                    Size = rdoPayloadVal
                };

                InsertMessageToList(oneMsg);
                //Console.WriteLine(System.Text.Encoding.ASCII.GetString(rdoPayloadData));
            }
            else
            {
                byte[] rdoPayloadData = new byte[rdoPayloadVal];
                Array.Copy(msgReadRDO, 22, rdoPayloadData, 0, rdoPayloadVal);
                //Console.WriteLine(System.Text.Encoding.ASCII.GetString(rdoPayloadData));
                MessageRDO oneMsg = new MessageRDO()
                {
                    Device = 0,
                    Transaction = (ushort) transactionID,
                    RDO = (ushort) rdoIDVal,
                    //Data = rdoPayloadData
                    Data = Helper.ByteArrayToString(rdoPayloadData),
                    Size = rdoPayloadVal
                };

                InsertMessageToList(oneMsg);
                //Remove first message
                var messageLengthToRemove = rdoPayloadVal + Constants.REM_READ_RSP_HEADER_SIZE + msgOffset;
                //Console.WriteLine("Message size: " + messageLengthToRemove);
                ushort newLength = (ushort) (msgValue - messageLengthToRemove);
                var newLengthByte = BitConverter.GetBytes(newLength);
                readRDOList[16] = newLengthByte[0];
                readRDOList[17] = newLengthByte[1];

                readRDOList.RemoveRange(18, (int) messageLengthToRemove);
                //Console.WriteLine(System.Text.Encoding.ASCII.GetString(rdoPayloadData));
                Console.WriteLine("Multi " + Helper.ByteArrayToString(readRDOList.ToArray()));
                ProcessReadRDOs(readRDOList.ToArray());
            }

        }

        /// <summary>
        /// Proccesses the time stamp.
        /// </summary>
        /// <param name="myMsg">My message.</param>
        void ProccessTimeStamp(Message myMsg)
        {
            myTimestamp = myMsg.Data;
            Console.WriteLine(ByteArrayToString(myMsg.Data));
        }

        #endregion //LEGACY_RDO

        /// <summary>
        /// Inserts the message to list and send out the message as an event
        /// </summary>
        /// <param name="msg">Message.</param>
        public void InsertMessageToList(MessageRDO msg)
        {
            constructedMessageList.Add(msg);
            OnMessageReceived?.Invoke(this, msg);
        }

        /// <summary>
        /// Gets the message offset.
        /// </summary>
        /// <returns>The message offset.</returns>
        /// <param name="rdoToCheck">Rdo to check.</param>
        int GetMessageOffset(int rdoToCheck)
        {
            var myOffset = 6;

            switch (rdoToCheck)
            {
                case Constants.REM_RDO_SCN:
                    myOffset = 6;
                    break;
            }

            return myOffset;
        }

        //Rewrite this TODO
        /// <summary>
        /// Processeses the header.
        /// </summary>
        /// <returns>The header.</returns>
        /// <param name="msg">Message.</param>
        public Message ProcessesHeader(byte[] msg)
        {
            byte[] destinationAddressArray = new byte[4];
            Array.Copy(msg, 0, destinationAddressArray, 0, 4);
            var destinationAddress = BitConverter.ToUInt32(destinationAddressArray, 0);

            byte[] sourceAddressArray = new byte[4];
            Array.Copy(msg, 6, sourceAddressArray, 0, 4);
            var sourceAddress = BitConverter.ToUInt32(destinationAddressArray, 0);

            byte[] transaction = new byte[2];
            Array.Copy(msg, 12, transaction, 0, 2);
            var transactionID = BitConverter.ToUInt16(transaction, 0);

            byte[] msgSz = new byte[2];
            Array.Copy(msg, 16, msgSz, 0, 2);
            var msgValue = BitConverter.ToUInt16(msgSz, 0);

            byte[] payload = new byte[msgValue];
            Array.Copy(msg, 18, payload, 0, msgValue);

            byte[] crcArray = new byte[2];
            Array.Copy(msg, msg.Length - 2, crcArray, 0, 2);
            var crcValue = BitConverter.ToUInt16(crcArray, 0);

            DBCAddress destination = new DBCAddress()
            {
                Address = destinationAddress,
                Id = msg[4],
                Type = msg[5]
            };

            DBCAddress source = new DBCAddress()
            {
                Address = sourceAddress,
                Id = msg[10],
                Type = msg[11]
            };

            Message rawREM = new Message()
            {
                Destination = destination,
                Source = source,
                Transaction = transactionID,
                Type = msg[14],
                Priority = 0,
                Fragmentation = 0,
                Size = msgValue,
                Data = payload,
                CRC = crcValue
            };

            return rawREM;
        }

        public void SetupDeviceForWriteConfig()
        {

        }

        /// <summary>
        /// Proccesses the start config rsp.
        /// </summary>
        /// <param name="myMsg">My message.</param>
        public void ProccessStartConfigRsp(Message myMsg)
        {
            var startConfigRsp = myMsg.Data;

            switch (startConfigRsp[0])
            {
                case 0:
                    Console.WriteLine("Config Mode!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.START_CONFIG_OK);
                    break;
                case 1:
                    Console.WriteLine("Configuration Denied!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.START_CONFIG_DENIED);
                    break;
                case 2:
                    Console.WriteLine("Already in Config Mode!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.START_ALREADY_CONFIG);
                    break;
            }
        }

        /// <summary>
        /// Proccesses the write RDOR sp.
        /// </summary>
        /// <param name="myMsg">My message.</param>
        public void ProccessWriteRDORsp(Message myMsg)
        {
            var msgSize = myMsg.Size;

            if (msgSize == 0)
            {
                Console.WriteLine("Successfull Write!");
                OnConfigChanged?.Invoke(this, (int) ConfigStatus.WRITE_RDO_OK);
            }
            else
            {
                //0-1 RDO
                //2-3 Length & Error flag
                //4-5 Error code
                byte[] errorArray = new byte[2];
                Array.Copy(myMsg.Data, myMsg.Size - 2, errorArray, 0, 2);
                var errorValue = BitConverter.ToUInt16(errorArray, 0);
                Console.WriteLine("Error {0}", errorValue);
                OnRDOError?.Invoke(this, errorValue);
            }
        }

        /// <summary>
        /// Proccesses the write config rsp.
        /// </summary>
        /// <param name="myMsg">My message.</param>
        public void ProccessWriteConfigRsp(Message myMsg)
        {
            var writeConfigRsp = myMsg.Data;

            switch (writeConfigRsp[0])
            {
                case 0:
                    Console.WriteLine("Flash successfully written!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.WRITE_FLASH_OK);
                    break;
                case 1:
                    Console.WriteLine("Flash write failed due to bad state!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.WRITE_FLASH_BAD);
                    break;
                case 2:
                    Console.WriteLine("Flash write failed due to flash full!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.WRITE_FLASH_FULL);
                    break;
            }
        }

        /// <summary>
        /// Proccesses the reset rsp.
        /// </summary>
        /// <param name="myMsg">My message.</param>
        public void ProccessResetRsp(Message myMsg)
        {
            var startConfigRsp = myMsg.Data;

            switch (startConfigRsp[0])
            {
                case 0:
                    Console.WriteLine("Reset Completed!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.RESET_COMPLETE);
                    break;
                case 1:
                    Console.WriteLine("Reset Not Attempted!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.RESET_NOT_ATTEMPTED);
                    break;
                case 2:
                    Console.WriteLine("Reset Error!");
                    OnConfigChanged?.Invoke(this, (int) ConfigStatus.RESET_ERROR);
                    break;
            }
        }

        #region Enums
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
            };

            enum RDOError
            {
            RESPONSE_TOO_LARGE = 6001,
            UNAVAILABLE_RESOURCE,
            INVALID_RDO_ID,
            RDO_IS_RO,
            INVALID_PARAMETER,
            INCORRECT_SIZE,
            DATA_INVALID,
            DATA_NOT_INITIALIZED,
            DATA_NOT_AVAILABLE
            };
            #endregion
    }
}