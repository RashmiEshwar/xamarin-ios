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
using System.Threading.Tasks;
using ServiceTool.Legacy.REM;

namespace ServiceTool.Legacy.Dongle
{
    public class SRDO
    {
        readonly DBC dbc;
        //EventHandlers
        public event EventHandler<SRDOReadResponse> OnReadRsp;
        public event EventHandler<SRDOWriteResponse> OnWriteRsp;
        public event EventHandler<SRDOCommandResponse> OnCommandRsp;
        public event EventHandler<SRDOReadParameterizedResponse> OnReadParaRsp;
        public event EventHandler<SRDOWriteParameterizedResponse> OnWriteParaRsp;
        public event EventHandler<RTDDataResponse> OnRTDRsp;
        public event EventHandler<RTDStopResponse> OnRTDStopRsp;

        public SRDO(DBC myDbc)
        {
            dbc = myDbc;

            dbc.OnSRDORead += (sender, e) => SRDOResponse(e);
            dbc.OnSRDOReadPara += (sender, e) => SRDOResponse(e);
            dbc.OnSRDOWrite += (sender, e) => SRDOResponse(e);
            dbc.OnSRDOWritePara += (sender, e) => SRDOResponse(e);
            dbc.OnSRDOCommandDialog += (sender, e) => SRDOCommand(e);
            dbc.OnSRDORTD += (sender, e) => SRDORTDData(e);
            dbc.OnRTDStop += (sender, e) => SRDORTDStop(e);
        }

        #region SRDO
        /// <summary>
        /// Sends the message read.
        /// </summary>
        /// <returns>The message read.</returns>
        /// <param name="page">Page.</param>
        /// <param name="rdoArray">Rdo array.</param>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageRead(uint page, uint[] rdoArray, byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                buffer.Add(Convert.ToByte(page));

                foreach (uint item in rdoArray)
                {
                    buffer.AddRange(BitConverter.GetBytes((ushort) item));
                }

                await dbc.SendSRDOMessage(REM.Constants.REM_READ_DATA_ITEM,
                    buffer.ToArray(),
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageRead");
            }
        }

        /// <summary>
        /// Sends the message read.
        /// </summary>
        /// <returns>The message read.</returns>
        /// <param name="page">Page.</param>
        /// <param name="srdoPara">Srdo para.</param>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageRead(uint page, SRDOReadParameterized[] srdoPara, byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                buffer.Add(Convert.ToByte(page));

                for (int i = 0, srdoParaLength = srdoPara.Length; i < srdoParaLength; i++)
                {
                    SRDOReadParameterized srdoLocal = srdoPara[i];
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.SRDO));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.P1));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.P2));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.P3));
                }

                await dbc.SendSRDOMessage(
                    REM.Constants.REM_READ_DATA_ITEM_PARA,
                    buffer.ToArray(),
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
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
        /// <param name="page">Page.</param>
        /// <param name="srdoWrite">Srdo write.</param>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageWrite(uint page, SRDOWrite[] srdoWrite, byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                buffer.Add(Convert.ToByte(page));

                for (int i = 0, srdWriteLength = srdoWrite.Length; i < srdWriteLength; i++)
                {
                    SRDOWrite srdoLocal = srdoWrite[i];
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.SRDO));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.Length));
                    buffer.AddRange(srdoLocal.Data);
                }

                await dbc.SendSRDOMessage(
                    REM.Constants.REM_WRITE_DATA_ITEM,
                    buffer.ToArray(),
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageWrite");
            }
        }

        /// <summary>
        /// Sends the message write.
        /// </summary>
        /// <returns>The message write.</returns>
        /// <param name="page">Page.</param>
        /// <param name="srdoPara">Srdo para.</param>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageWrite(uint page, SRDOWriteParameterized[] srdoPara, byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                buffer.Add(Convert.ToByte(page));

                for (int i = 0, srdoParaLength = srdoPara.Length; i < srdoParaLength; i++)
                {
                    SRDOWriteParameterized srdoLocal = srdoPara[i];
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.SRDO));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.P1));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.P2));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.P3));
                    buffer.AddRange(BitConverter.GetBytes(srdoLocal.Length));
                    buffer.AddRange(srdoLocal.Data);
                }

                await dbc.SendSRDOMessage(
                    REM.Constants.REM_WRITE_DATA_PARA,
                    buffer.ToArray(),
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageWrite");
            }
        }

        /// <summary>
        /// Sends the message command dialog.
        /// </summary>
        /// <returns>The message command dialog.</returns>
        /// <param name="sRDO">S rdo.</param>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageCommandDialog(SRDOCommand sRDO, byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                buffer.Add(Convert.ToByte(sRDO.Command));
                buffer.AddRange(BitConverter.GetBytes(sRDO.ArgumentLength));
                buffer.AddRange(sRDO.Argument);

                //await dbc.SendReadSRDOWriteParameterized(buffer.ToArray());
                await dbc.SendSRDOMessage(
                    REM.Constants.REM_DIALOG_COMMAND,
                    buffer.ToArray(),
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageCommandDialog");
            }
        }

        /// <summary>
        /// Sends the message RTDS tart.
        /// </summary>
        /// <returns>The message RTDS tart.</returns>
        /// <param name="rtd">Rtd.</param>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageRTDStart(RTDStart rtd, byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                var buffer = new List<byte>();

                buffer.Add(Convert.ToByte(rtd.Type));
                buffer.AddRange(BitConverter.GetBytes(rtd.PollRate));

                foreach (uint item in rtd.Srdo)
                {
                    buffer.Add(Convert.ToByte(rtd.Page));
                    buffer.AddRange(BitConverter.GetBytes((ushort) item));
                }

                await dbc.SendSRDOMessage(
                    REM.Constants.REM_RTD_START_CMD,
                    buffer.ToArray(),
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageRTDStart");
            }
        }

        /// <summary>
        /// Sends the message RTD stop.
        /// </summary>
        /// <returns>The message RTDS top.</returns>
        /// <param name="myDest">My destination.</param>
        internal async Task SendMessageRTDStop(byte myDest = REM.Constants.DBC_DESTINATION_TYPE)
        {
            if (dbc != null)
            {
                byte[] msg = Array.Empty<byte>();

                await dbc.SendSRDOMessage(
                    REM.Constants.REM_RTD_STOP_CMD,
                    msg,
                    REM.Constants.DBC_WILDCARD_ADDRESS,
                    0xFF,
                    myDest);
            }
            else
            {
                Console.WriteLine("DBC init failed; - SendMessageRTDStop");
            }
        }

        #endregion

        #region EventHandlers
        internal void SRDOResponse(SRDOReadResponse srdo)
        {
            //Console.WriteLine(srdo.TID);
            OnReadRsp?.Invoke(this, srdo);
        }

        internal void SRDOResponse(SRDOReadParameterizedResponse srdo)
        {
            //Console.WriteLine(srdo.TID);
            OnReadParaRsp?.Invoke(this, srdo);
        }

        internal void SRDOResponse(SRDOWriteResponse srdo)
        {
            //Console.WriteLine(srdo.TID);
            OnWriteRsp?.Invoke(this, srdo);
        }

        internal void SRDOResponse(SRDOWriteParameterizedResponse srdo)
        {
            //Console.WriteLine(srdo.TID);
            OnWriteParaRsp?.Invoke(this, srdo);
        }

        internal void SRDOCommand(SRDOCommandResponse srdo)
        {
            OnCommandRsp?.Invoke(this, srdo);
        }

        internal void SRDORTDStart(RTDStartResponse rtd)
        {
            //Blah?
        }

        internal void SRDORTDData(RTDDataResponse rtd)
        {
            //Console.WriteLine(rtd.TID);
            OnRTDRsp?.Invoke(this, rtd);
        }

        internal void SRDORTDStop(RTDStopResponse rtd)
        {
            //Console.WriteLine(rtd.TID);
            OnRTDStopRsp?.Invoke(this, rtd);
        }
        #endregion //EventHandler
    }
}