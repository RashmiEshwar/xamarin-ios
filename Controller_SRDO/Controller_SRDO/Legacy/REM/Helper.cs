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
using System.Linq;
using System.Text;

namespace ServiceTool.Legacy.REM
{
    static public class Helper
    {
        static public string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ");
        }
        public static string HextoString(string InputText)
        {

            byte[] bb = Enumerable.Range(0, InputText.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(InputText.Substring(x, 2), 16))
                             .ToArray();
            return System.Text.Encoding.ASCII.GetString(bb);

        }
    }


}
// public static string  HexStringToString(string hex)
//{
//    if (hex == null || (hex.Length & 1) == 1)
//    {
//        throw new ArgumentException();
//    }
//    var sb = new StringBuilder();
//    for (var i = 0; i < hex.Length; i += 2)
//    {
//        var hexChar = hex.Substring(i, 2);
//        sb.Append((char)Convert.ToByte(hexChar, 16));
//    }
//    return sb.ToString();
//}



