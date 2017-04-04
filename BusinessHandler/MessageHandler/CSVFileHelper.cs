﻿
using BusinessHandler.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace BusinessLogic
{
    public class CSVFileHelper
    {
        public static List<DocData> OpenDocCSV(string filePath)
        {
            var resultList = new List<DocData>();
            string wholeText = File.ReadAllText(filePath);
            string[] records = wholeText.Split(new string[] { "\",\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string strLine in records)
            {
                string[] aryLine = strLine.Split(',');
                var data = new DocData();
                data.CityName = string.IsNullOrWhiteSpace(aryLine[0]) ? "" : aryLine[0].Trim('"');
                data.DocId = string.IsNullOrWhiteSpace(aryLine[1]) ? "" : aryLine[1].Trim('"');
                data.LocalPath = string.IsNullOrWhiteSpace(aryLine[2]) ? "" : aryLine[2].Trim('"'); ;
                data.DocType = string.IsNullOrWhiteSpace(aryLine[3]) ? "" : aryLine[3].Trim('"'); ;
                data.DocUrl = string.IsNullOrWhiteSpace(aryLine[4]) ? "" : aryLine[4].Trim('"'); ;
                data.CanBeRead = string.IsNullOrWhiteSpace(aryLine[5]) ? "" : aryLine[5].Trim('"');
                data.DocFilePath = filePath;
                resultList.Add(data);
            }

            return resultList;
        }

        public static List<QueryData> OpenQueryCSV(string filePath)
        {
            HandQueryGuid(filePath);
            var resultList = new List<QueryData>();
            string wholeText = File.ReadAllText(filePath);
            string[] records = wholeText.Split(new string[] { "\",\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string strLine in records)
            {
                if (string.IsNullOrEmpty(strLine.Trim('\r', '\n')))
                {
                    continue;
                }

                var aryLine = strLine.Split(new string[] { "\",\"" }, StringSplitOptions.None);
                var data = new QueryData();
                data.CityName = string.IsNullOrWhiteSpace(aryLine[1]) ? "" : aryLine[1].Trim('"');
                data.DocId = string.IsNullOrWhiteSpace(aryLine[2]) ? "" : aryLine[2].Trim('"');
                data.MeetingTitle = string.IsNullOrWhiteSpace(aryLine[3]) ? "" : aryLine[3].Trim('"');
                var dt = DateTime.MinValue;
                if (DateTime.TryParse(string.IsNullOrWhiteSpace(aryLine[4]) ? "" : aryLine[4].Trim('"'), out dt))
                {
                    data.MeetingDate = dt;
                    data.MeetingDateDisplay = data.MeetingDate.ToString("yyyy/MM/dd");
                }

                data.MeetingLocation = string.IsNullOrWhiteSpace(aryLine[5]) ? "" : aryLine[5].Trim('"');

                data.KeyWord = string.IsNullOrWhiteSpace(aryLine[6]) ? "" : aryLine[6].Trim('"');
                data.Content = string.IsNullOrWhiteSpace(aryLine[7]) ? "" : aryLine[7].Trim('"');
                data.QueryGuid = string.IsNullOrWhiteSpace(aryLine[0]) ? "" : aryLine[0].Trim('"');
                data.Comment = aryLine.Length < 9 ? "" : aryLine[8].Trim('"');
                data.QueryFilePath = filePath;
                resultList.Add(data);
            }

            return resultList;
        }

        public static void HandQueryGuid(string filePath)
        {
            List<String> lines = new List<String>();
            using (StreamReader sr = new StreamReader(filePath))
            {
                string strLine = "";
                string[] aryLine = null;
                while ((strLine = sr.ReadLine()) != null)
                {
                    aryLine = strLine.Split(',');
                    var data = new QueryData();
                    var guid = string.IsNullOrWhiteSpace(aryLine[0]) ? "" : aryLine[0].Trim('"');
                    if (IsGuidByParse(guid))
                    {
                        break;
                    }
                    else
                    {
                        guid = Guid.NewGuid().ToString();
                        strLine = '"' + guid + '"' + ',' + strLine;
                    }
                    lines.Add(strLine);
                }
            }
            if (lines.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    foreach (String line in lines)
                        writer.WriteLine(line);
                }
            }
        }

        public static void UpdateQueryCSV(DocQueryResultModel message)
        {
            List<String> lines = new List<String>();
            if (File.Exists(message.QueryFilePath))
            {
                string[] contents = File.ReadAllText(message.QueryFilePath).Split(new string[] { "\",\r\n", "\"\r\n" }, StringSplitOptions.None);

                for (int i = 0; i < contents.Length; i++)
                {
                    string line = contents[i];

                    if (line.Contains(","))
                    {
                        String[] split = line.Split(new string[] { "\",\"" }, StringSplitOptions.None);

                        if (split[2].Trim('"').Equals(message.DocId) && split[0].Trim('"').Equals(message.QueryGuid))
                        {
                            //split[8] = '"' + message.Comment + "\",";
                            split[split.Length - 1] = split.Length == 8 ?
                                string.Format("\"{0}\",\"{1}\",", split[7].Trim(',', ('"'), (char)32, (char)160), message.Comment) :
                                string.Format("{0}\",", message.Comment);
                            line = String.Join("\",\"", split);
                        }
                        else
                        {
                            line = line + "\",";
                        }
                    }

                    lines.Add(line);
                }

                File.WriteAllLines(message.QueryFilePath, lines, Encoding.UTF8);
            }
        }



        public static bool IsGuidByParse(string strSrc)

        {

            Guid g = Guid.Empty;

            return Guid.TryParse(strSrc, out g);

        }
    }
}
