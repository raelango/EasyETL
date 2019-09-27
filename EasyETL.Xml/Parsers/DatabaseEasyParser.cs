﻿using EasyETL.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace EasyETL.Xml.Parsers
{
    public enum EasyDatabaseConnectionType
    {
        edctOledb,
        edctODBC,
        edctSQL
    }

    [DisplayName("Database Source")]
    [EasyField("ConnectionType", "Database Connection Type","ODBC","","ODBC;OleDb;SQL")]
    [EasyField("ConnectionString", "Connection String")]
    [EasyField("Query", "Query to Execute")]
    public class DatabaseEasyParser : AbstractEasyParser, IEasyFieldInterface
    {
        public string ConnectionString;
        public string Query;
        public EasyDatabaseConnectionType ConnectionType;

        public DatabaseEasyParser()
        {
            ConnectionType = EasyDatabaseConnectionType.edctODBC;
        }
        public DatabaseEasyParser(EasyDatabaseConnectionType connType, string connString = "") {
            ConnectionType = connType;
            ConnectionString = connString;
        }

        public override XmlDocument Load(string queryToExecute = "", XmlDocument xDoc = null)
        {
            if (String.IsNullOrWhiteSpace(queryToExecute)) queryToExecute = Query;
            if (!IsFieldSettingsComplete()) return null;
            return LoadFromQuery(queryToExecute, xDoc);
        }

        public XmlDocument LoadFromQuery(string sqlToExecute, XmlDocument xDoc = null)
        {
            DataSet ds = new DataSet();
            DataAdapter adapter = null;
            if (xDoc == null)
            {
                xDoc = new XmlDocument();
            }

            try
            {
                switch (ConnectionType)
                {
                    case EasyDatabaseConnectionType.edctODBC:
                        adapter = new OdbcDataAdapter(sqlToExecute, new OdbcConnection(ConnectionString));
                        break;
                    case EasyDatabaseConnectionType.edctOledb:
                        adapter = new OleDbDataAdapter(sqlToExecute, new OleDbConnection(ConnectionString));
                        break;
                    case EasyDatabaseConnectionType.edctSQL:
                        adapter = new SqlDataAdapter(sqlToExecute, new SqlConnection(ConnectionString));
                        break;
                }
                if (adapter != null)
                {
                    adapter.Fill(ds);
                }
                adapter.Dispose();
                xDoc.LoadXml(ds.GetXml());
            }
            catch
            {
            }
            return xDoc;
        }

        public virtual bool IsFieldSettingsComplete()
        {
            return !(String.IsNullOrWhiteSpace(ConnectionString) || String.IsNullOrWhiteSpace(Query));
        }

        public void LoadFieldSettings(Dictionary<string, string> settingsDictionary)
        {
            foreach (KeyValuePair<string, string> kvPair in settingsDictionary)
            {
                LoadSetting(kvPair.Key, kvPair.Value);
            }
        }

        public void SaveFieldSettingsToXmlNode(XmlNode parentNode)
        {
            Dictionary<string, string> settingsDict = GetSettingsAsDictionary();
            foreach (KeyValuePair<string, string> kvPair in settingsDict)
            {
                XmlElement xNode = parentNode.OwnerDocument.CreateElement("field");
                xNode.SetAttribute("name",kvPair.Key);
                xNode.SetAttribute("value", kvPair.Value);
                parentNode.AppendChild(xNode);
            }
        }


        public virtual void LoadSetting(string fieldName, string fieldValue)
        {
            switch (fieldName.ToLower())
            {
                case "connectionstring":
                    ConnectionString = fieldValue; break;
                case "connectiontype":
                    ConnectionType = (EasyDatabaseConnectionType)Enum.Parse(typeof(EasyDatabaseConnectionType), "edct" + fieldValue); break;
                case "query":
                    Query = fieldValue; break;
            }
        }

        public virtual Dictionary<string, string> GetSettingsAsDictionary()
        {
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            resultDict.Add("connectionstring", ConnectionString);
            resultDict.Add("connectiontype", ConnectionType.ToString().Substring(4));
            resultDict.Add("query", Query);
            return resultDict;
        }

    }
}