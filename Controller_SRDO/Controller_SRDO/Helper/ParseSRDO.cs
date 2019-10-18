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

using System.Collections.Generic;
using System.IO;
using Foundation;
using Newtonsoft.Json;

namespace ServiceTool.Engineering
{

    public class Info
    {
        public string JsonVersion { get; set; }
        public string Comment { get; set; }
        public string Data_version { get; set; }
        public string Date { get; set; }
    }

    public class Srdo
    {
        public int rdo { get; set; }
        public int component { get; set; }
        public string name { get; set; }
        public string defaultValue { get; set; }
        public string type { get; set; }
        public int quantity { get; set; }
        public string minimum { get; set; }
        public string maximum { get; set; }
        public object alternativeValues { get; set; }
        public string access { get; set; }
    }

    public class Page
    {
        public int group { get; set; }
        public List<Srdo> srdo { get; set; }
    }

    public class RootObject
    {
        public List<Info> Info { get; set; }
        public List<Page> page { get; set; }
    }

    public class ParseSRDO
    {
        private static ParseSRDO parseSRDO = null;
        public RootObject rdoModelObj;
        public string SCNIDRes;
        public string ConfigurationFileVersionRes;

        public static ParseSRDO SharedInstance()
        {
            if (parseSRDO == null)
            {
                parseSRDO = new ParseSRDO();
            }
            return parseSRDO;
        }

        public bool ParseFileAtPath(string filePath)
        {
            NSFileManager fileManager = new NSFileManager();
            if (!fileManager.FileExists(filePath))
            {
                fileManager.Dispose();
                fileManager = null;
                return false;
            }
            string text = File.ReadAllText(filePath);
            RootObject model = JsonConvert.DeserializeObject<RootObject>(text);
            rdoModelObj = model;
            //Console.WriteLine("Count "+ rdoModelObj.page[0].srdo.Count);
            fileManager.Dispose();
            fileManager = null;
            return true;
        }
    }
}