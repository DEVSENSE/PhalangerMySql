using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Core;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace PHP.Library.Data
{
    /// <summary>
    /// Represents PDO MySQL driver.
    /// </summary>
    public sealed class MySQLPDODriver : PDODriver
    {
        /// <summary>
        /// Gets PDO driver scheme name.
        /// </summary>
        public override string Scheme { get { return "mysql"; } }

        private static void SetupConnectionString(MySqlConnectionStringBuilder/*!*/csb, string dsn_data)
        {
            if (string.IsNullOrEmpty(dsn_data))
                return;

            var dataarr = dsn_data.Split(';');
            for (int i = 0; i < dataarr.Length; i++)
            {
                string dataitem = dataarr[i];
                int eqindex = dataitem.IndexOf('=');
                string itemname, itemvalue;
                if (eqindex == -1)
                {
                    itemname = dataitem;
                    itemvalue = null;
                }
                else
                {
                    itemname = dataitem.Remove(eqindex);
                    itemvalue = dataitem.Substring(eqindex + 1);
                }

                if (itemname.EqualsOrdinalIgnoreCase("host"))
                    csb.Server = itemvalue;
                else if (itemname.EqualsOrdinalIgnoreCase("dbname"))
                    csb.Database = itemvalue;
                //else if (itemname.EqualsOrdinalIgnoreCase("initStatements"))
                //    csb                    
            }
        }

        /// <summary>
        /// Creates new PDO DB connection.
        /// </summary>
        public override PDOConnection OpenConnection(ScriptContext context, string dsn_data, string username, string password, object argdriver_options)
        {
            ////Determine file path
            //string filename = dsn_data.Replace('/', Path.DirectorySeparatorChar);
            //string filePath = Path.GetFullPath(Path.Combine(context.WorkingDirectory, filename));

            var csb = new MySqlConnectionStringBuilder();
            SetupConnectionString(csb, dsn_data);
            csb.AllowUserVariables = true;
            csb.AllowZeroDateTime = true;
            if (username != null) csb.UserID = username;
            if (password != null) csb.Password = password;
            if (argdriver_options is PhpArray)
            {
                // TODO: process argdriver_options
            }

            var con = new PDOConnection(csb.GetConnectionString(true), new MySqlConnection(), "PDO mysql connection");
            con.Connect();            
            return con;
        }

        public override object ConvertDbValue(object sqlValue, string dataType)
        {
            if (sqlValue != null && sqlValue.GetType() == typeof(MySqlDateTime))
            {
                MySqlDateTime sql_date_time = (MySqlDateTime)sqlValue;
                if (sql_date_time.IsValidDateTime)
                    return ConvertDateTime(dataType, sql_date_time.GetDateTime());

                if (dataType == "DATE" || dataType == "NEWDATE")
                    return "0000-00-00";
                else
                    return "0000-00-00 00:00:00";
            }

            return base.ConvertDbValue(sqlValue, dataType);
        }

        /// <summary>
        /// Quotes given command.
        /// </summary>
        public override object Quote(ScriptContext context, object strobj, PDOParamType param_type)
        {
            var obj = MySql.EscapeString(strobj);
            Debug.Assert(obj != null);

            // quote the string with '...'

            if (obj.GetType() == typeof(string))
            {
                return "'" + (string)obj + "'";
            }
            else if (obj.GetType() == typeof(PhpBytes))
            {
                var bytes = (PhpBytes)obj;
                byte[] data = new byte[bytes.Length + 2];
                Array.Copy(bytes.ReadonlyData, 0, data, 1, bytes.Length);
                data[0] = data[data.Length - 1] = (byte)'\'';

                return new PhpBytes(data);
            }
            else
            {
                Debug.Fail();
                return String.Empty;
            }
        }

        /// <summary>
        /// Creates new PDO statement.
        /// </summary>
        public override PDOStatement CreateStatement(ScriptContext context, PDO pdo)
        {
            MySQLPDOStatement stmt = new MySQLPDOStatement(context, pdo);
            return stmt;
        }

        protected override bool IsValueValidForAttribute(int att, object value)
        {
            PDOAttributeType attE = (PDOAttributeType)att;
            switch (attE)
            {
                case PDOAttributeType.PDO_ATTR_EMULATE_PREPARES:
                    return value is bool;
                case PDOAttributeType.PDO_ATTR_ERRMODE:
                    return Enum.IsDefined(typeof(PDOErrorMode), value);
                default:
                    break;
            }
            return false;
        }

        public override object GetLastInsertId(ScriptContext context, PDO pdo, string name)
        {
            var cmd = pdo.PDOConnection.LastCommand;
            if (cmd is MySqlCommand)
                return ((MySqlCommand)cmd).LastInsertedId;
            else
                return false;
        }
    }
}

