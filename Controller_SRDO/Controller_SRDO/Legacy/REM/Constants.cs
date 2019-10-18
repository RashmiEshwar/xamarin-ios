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
namespace ServiceTool.Legacy.REM
{
    public static class Constants
    {
        public const byte DBC_DESTINATION_TYPE        = 0x01;
        public const byte DBC_RAW                     = 0;
        public const byte DBC_SLIP                    = 1;
        public const byte TransportType               = DBC_SLIP;
        //Slip
        public const byte SLIP_END                    = 0xC0;

        public const byte SLIP_ESC                    = 0xDB;
        public const byte SLIP_ESC_END                = 0xDC;
        public const byte SLIP_ESC_ESC                = 0xDD;

        public const byte DBC_WILDCARD                = 0xFF;
        public const uint DBC_NULL_ADDRESS            = 0;
        public const uint DBC_NULL_TRANSACTION        = 0;
        public const uint DBC_WILDCARD_ADDRESS        = 0xFFFFFFFF;
        public const uint DBC_DEFAULT_ADDRESS         = 0x00FEFEFE;
        public const byte DBC_GCS_TYPE                = 0x13;
        public const uint DBC_MINIMUM_TX_NUMBER       = 0x00001000;
        public const uint DBC_MAXIMUM_TX_NUMBER       = 0x00007FFF;
        public const uint DBC_MAX_MESSAGE_LENGTH      = 2048;
        public const byte DBC_NO_FRAGMENTATION        = 0x00;
        public const uint DBC_MESSAGE_HEADER_SIZE     = 18;
        public const uint DBC_MESSAGE_TRAILER_SIZE    = 2;

        public const uint DBC_TRUE                    = 1;
        public const uint DBC_FALSE                   = 0;
        public const uint RCL_TRUE                    = DBC_TRUE;
        public const uint RCL_FALSE                   = DBC_FALSE;

        public const int DBC_TIMED_OUT                = 0;
        public const int DBC_LOGON_ERROR              = 1;
        public const int DBC_CONNECTED                = 2;
        public const int DBC_NO_RESPONSE              = 3;
        public const int DBC_INVALID_RDO              = 4;

        // Message Types
        public const byte REM_SEED_CMD = 0x40;
        public const byte REM_SEED_RSP = 0x41;
        public const byte REM_REQUEST_ACCESS_CMD      = 0x42;
        public const byte REM_REQUEST_ACCESS_RSP      = 0x43;
        public const byte REM_RESET_ACCESS_CMD        = 0x4E;
        public const byte REM_ZKIP_CMD                = 0x4A;
        public const byte REM_ZKIP_RSP                = REM_ZKIP_CMD + 1;
        public const byte REM_READ_RDO                = 0x0A;
        public const byte REM_READ_RDO_RSP            = REM_READ_RDO + 1;
        public const byte REM_WRITE_RDO               = 0x0C;
        public const byte REM_WRITE_RDO_RSP           = 0x0D;
        public const byte REM_RTD_START_CMD           = 0x68;
        public const byte REM_RTD_START_RSP           = 0x69;
        public const byte REM_RTD_STOP_CMD            = 0x6A;
        public const byte REM_RTD_STOP_RSP            = 0x6B;
        public const byte REM_RTD_DATA                = 0x6C;
        public const UInt16 REM_RTD_POLL_RATE         = 300;
        public const byte REM_RTD_TYPE                = 1;
        public const byte REM_STREAM_DATA             = 0x86;
        public const byte REM_READ_RDO_PUBLIC         = 0x8A;
        public const byte REM_READ_RDO_PUBLIC_RSP     = REM_READ_RDO_PUBLIC + 1;
        public const byte REM_HEARTBEAT_CMD           = 0x5C;
        public const byte REM_HEARTBEAT_RSP           = 0x5D;
        public const byte READ_RDO_PUBLIC             = 0x22;
        public const byte REM_TIME_STAMP              = 0x44;
        public const byte REM_TIME_STAMP_RSP          = 0x45;
        public const byte REM_REQUEST_TIME_STAMP      = 0x46;
        public const byte REM_REQUEST_TIME_STAMP_RSP  = 0x47;
        public const byte REM_START_CONFIG            = 0x28;
        public const byte REM_START_CONFIG_RSP        = 0x29;
        public const byte REM_WRITE_CONFIG            = 0x32;
        public const byte REM_WRITE_CONFIG_RSP        = 0x33;
        public const byte REM_RESET                   = 0x34;
        public const byte REM_RESET_RSP               = 0x35;

        public const short REM_RDO_SCN                = 704;
        public const uint REM_CRC_SIZE                = 2;
        public const uint REM_READ_RSP_HEADER_SIZE    = 4;
        public const uint REM_READ_RSP_MINIMUM_SIZE   = 8;
        public const byte REM_READ_SUBSYSTEM_DATA     = 0x8C;
        public const byte REM_READ_SUBSYSTEM_DATA_RSP = 0x8D;

        public const byte REM_ZKIP_LEVEL_ADJUSTER     = 7;
        public const uint REM_ZKIP_STATUS_OFT_A       = 5;
        public const byte REM_ZKIP_STATUS             = 0;

        public const byte REM_ZKIP_E                  = 0x65;
        public const byte REM_ZKIP_I                  = 0x69;
        public const byte REM_ZKIP_H                  = 0x68;
        public const byte REM_ZKIP_J                  = 0x6A;
        public const byte REM_ZKIP_K                  = 0x6B;

        public const byte REM_ZKIP_P                  = 0x70;
        public const byte REM_ZKIP_Q                  = 0x71;
        public const byte REM_ZKIP_R                  = 0x72;
        public const byte REM_ZKIP_U                  = 0x75;
        public const byte REM_ZKIP_V                  = 0x76;
        public const byte REM_ZKIP_Z                  = 0x7A;

        public const byte REM_READ_DATA_ITEM          = 0x02;
        public const byte REM_READ_DATA_ITEM_RSP      = 0x03;
        public const byte REM_READ_DATA_ITEM_PARA     = 0x04;
        public const byte REM_READ_DATA_ITEM_PARA_RSP = 0x05;

        public const byte REM_WRITE_DATA_ITEM         = 0x06;
        public const byte REM_WRITE_DATA_ITEM_RSP     = 0x07;
        public const byte REM_WRITE_DATA_PARA         = 0x1C;
        public const byte REM_WRITE_DATA_PARA_RSP     = 0x1D;

        public const byte REM_DIALOG_COMMAND          = 0x08;
        public const byte REM_DIALOG_COMMAND_RSP      = 0x09;
    }
}