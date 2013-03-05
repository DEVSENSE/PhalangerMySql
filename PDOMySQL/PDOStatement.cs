using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Core;
using MySql.Data.MySqlClient;

namespace PHP.Library.Data
{
    /// <summary>
    /// PDO MySQL statement.
    /// </summary>
    public sealed class MySQLPDOStatement : PDOStatement
    {
        private readonly MySqlCommand m_com;
        
        internal MySQLPDOStatement(ScriptContext context, PDO pdo)
            : base(context, pdo)
        {
            pdo.PDOConnection.LastCommand = this.m_com = (MySqlCommand)pdo.Connection.CreateCommand();
        }

        /// <summary>
        /// Executes the statement.
        /// </summary>
        /// <returns></returns>
        public override bool ExecuteStatement()
        {
            var connection = this.m_pdo.PDOConnection;
            connection.ClosePendingReader();

            this.m_com.Transaction = (MySqlTransaction)this.m_pdo.Transaction;
            this.m_com.CommandTimeout = (int)this.m_pdo.GetAttribute(PDO.ATTR_TIMEOUT, 30);
            connection.PendingReader = this.m_com.ExecuteReader();
            return true;
        }

        /// <summary>
        /// Gets current command.
        /// </summary>
        protected override System.Data.IDbCommand CurrentCommand { get { return this.m_com; } }
        
        /// <summary>
        /// Gets current data reader.
        /// </summary>
        protected override System.Data.IDataReader CurrentReader { get { return this.m_pdo.PDOConnection.PendingReader; } }

        /// <summary>
        /// Closes underlaying data reader.
        /// </summary>
        protected override void CloseReader()
        {
            this.m_pdo.PDOConnection.ClosePendingReader();
        }

        /// <summary>
        /// Initialize the statement with a query.
        /// </summary>
        public override void Init(string query, Dictionary<int, object> options)
        {
            this.m_com.CommandText = query;
            System.Diagnostics.Debug.WriteLine("PDOMySQL: stmt.init query=" + query);

            if (options != null)
            {
                foreach (int key in options.Keys)
                {
                    this.m_pdo.SetAttribute(key, options[key]);
                }
            }
        }
    }
}
