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
namespace ServiceTool.Legacy.Dongle
{
    public static class Constants
    {
        public const string Address                  = "10.10.10.1";
        public const int CommandPort                 = 2000;
        public const int DataPort                    = 3000;
        public const string DongleCommandMode        = "M";
        public const string DoubleEm                 = "MM";
        public static readonly byte[] CarriageReturn = { 0x0D };
        public static readonly byte[] EscLogon       = { 0x1B, 0x5b, 0x79 };
        public const string DongleBreakCMD           = "uart_break 17 uart0 100\n";
        public const string DongleDefaultBaud        = "set uart.baud 0 9600\n";
        public const string DongleBaud38K            = "set uart.baud 0 38400\n";
        public const string DongleBaud57K            = "set uart.baud 0 57600\n";
        public const string DongleBaud115K           = "set uart.baud 0 115200\n";
        public const string DongleUpdateConfig       = "uart_update 0\n";
        public const string DongleGetBaudRate        = "get uart.baud 0\n";
        public const string DongleReboot             = "reboot\n";

        public const int SVTTimeOut = 0;
        public const int SVTLogonError = 1;
        public const int SVTConnected = 2;
        public const int SVTNoResponse = 3;
        public const int DBCInvalidRDO = 4;
        public const int DBCInvalidWrite = 5;
    }
}