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
namespace ServiceTool.Legacy.REM
{
    public class Message
    {
        /**
         DBC message structure
         - stDestination: Destination address
         - stSource: Source address
         - uiTransaction: Transaction id
         - uiMsgType: Message type
         - uiMsgPriority: Message priority 1-10
         - uiMsgFragmentation: Fragmented data
         - uiMsgDataSize: Size of MsgData in bytes
         - ucMsgData: Data to send
         - uiCrc16: CRC
         */
        public DBCAddress Destination { get; set; }
        public DBCAddress Source { get; set; }
        public ushort Transaction { get; set; }
        public byte Type { get; set; }
        public byte Priority { get; set; }
        public byte Fragmentation { get; set; }
        public ushort Size { get; set; }
        public byte[] Data { get; set; }
        public ushort CRC { get; set; }
    }

    public class MessageStore
    {
        public ushort TID { get; set; }
        public Message Message { get; set; }
    }

    #region SRDORead
    public class SRDOReadResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
        public byte Page { get; set; }
        public ushort SRDO { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
        public int Error { get; set; }
    }

    public class SRDOReadParameterizedResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
        public byte Page { get; set; }
        public ushort SRDO { get; set; }
        public ushort P1 { get; set; }
        public ushort P2 { get; set; }
        public uint P3 { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
        public int Error { get; set; }
    }

    public class SRDOReadParameterized
    {
        public ushort SRDO { get; set; }
        public ushort P1 { get; set; }
        public ushort P2 { get; set; }
        public uint P3 { get; set; }
    }
    #endregion

    #region SRDOWrite
    public class SRDOWrite
    {
        public ushort SRDO { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
    }

    public class SRDOWriteResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
        public byte Page { get; set; }
        public ushort SRDO { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
        public int Error { get; set; }
    }

    public class SRDOWriteParameterized
    {
        public ushort SRDO { get; set; }
        public ushort P1 { get; set; }
        public ushort P2 { get; set; }
        public uint P3 { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
    }

    public class SRDOWriteParameterizedResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
        public byte Page { get; set; }
        public ushort SRDO { get; set; }
        public ushort P1 { get; set; }
        public ushort P2 { get; set; }
        public uint P3 { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
        public int Error { get; set; }
    }
    #endregion

    #region SRDOCR
    public class SRDOCommand
    {
        public byte Command { get; set; }
        public ushort ArgumentLength { get; set; }
        public byte[] Argument { get; set; }
    }

    public class SRDOCommandResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
        public byte Response { get; set; }
        public ushort ArgumentLength { get; set; }
        public byte[] Argument { get; set; }
    }
    #endregion

    public class RTDStart
    {
        public uint Type { get; set; }
        public uint PollRate { get; set; }
        public uint Page { get; set; }
        public uint[] Srdo { get; set; }
    }

    public class RTDStartResponse
    {
        public byte Status { get; set; }
        public ushort PollRate { get; set; }
        public byte[] Data { get; set; }
    }

    public class RTDDataResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
        public ushort Length { get; set; }
        public byte[] Data { get; set; }
    }

    public class RTDStopResponse
    {
        public byte DeviceType { get; set; }
        public ushort TID { get; set; }
    }
}