using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using MySql.Data.MySqlClient;
using System.Reflection.Emit;

namespace PHP.Library.Data
{
    internal class MySqlDataReaderHelper
    {
        /// <summary>
        /// Locker object to protect critical sections.
        /// </summary>
        private static readonly object locker = new object();

        /// <summary>
        /// Generate unprotected (private) field read of ((<typeparamref name="TArg0"/>)<paramref name="arg0"/>).<paramref name="fieldName"/>.
        /// </summary>
        /// <typeparam name="TArg0">Type of object instance.</typeparam>
        /// <typeparam name="TResult">Type of field type.</typeparam>
        /// <param name="arg0">Object instance.</param>
        /// <param name="fieldName">Field name.</param>
        /// <returns>Delegate calling given field load.</returns>
        private static Func<TArg0, TResult> GenerateUnprotectedFieldRead<TArg0, TResult>(TArg0/*!*/arg0, string fieldName)
        {
            try
            {
                // type of instance:
                Type argType = arg0.GetType();

                // obtain required field:
                var fld = argType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                Debug.Assert(fld != null, "!" + fieldName);
                Debug.Assert(
                    typeof(TResult).IsAssignableFrom(fld.FieldType) ||
                    (fld.FieldType.IsEnum && typeof(TResult).IsAssignableFrom(fld.FieldType.GetEnumUnderlyingType())),
                    fieldName + ".FieldType must be assignable to " + typeof(TResult).Name);

                // create dynamic method with skipped JIT visibility checks:
                var getter = new DynamicMethod("get_" + fieldName + "#1", typeof(TResult), new Type[] { typeof(TArg0) }, true);
                var il = getter.GetILGenerator();

                // return ((argType)<arg0>).<fieldName>;
                il.Emit(OpCodes.Ldarg_0);   // <arg0>
                il.Emit(OpCodes.Castclass, argType);    // (TArg0)<arg0>
                il.Emit(OpCodes.Ldfld, fld);    // (TArg0)<arg0>.<fld>
                il.Emit(OpCodes.Ret);

                //
                return (Func<TArg0, TResult>)getter.CreateDelegate(typeof(Func<TArg0, TResult>));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Make <paramref name="reader"/>.resultSet unprotected read.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static object/*ResultSet*/ResultSet(MySqlDataReader/*!*/reader)
        {
            Debug.Assert(reader != null, "!reader");

            if (resultSetMethod == null)
                lock (locker)
                    if (resultSetMethod == null)
                        resultSetMethod = GenerateUnprotectedFieldRead<MySqlDataReader, object>(reader, "resultSet");

            return resultSetMethod(reader);
        }
        private static Func<MySqlDataReader, object> resultSetMethod;

        /// <summary>
        /// Make ((ResultSet)<paramref name="resultSet"/>).fields[<paramref name="index"/>] unprotected read.
        /// </summary>
        /// <param name="resultSet"></param>
        /// <param name="index"></param>
        /// <returns>MySqlField</returns>
        public static object/*MySqlField*/fields_index(object/*!*/resultSet, int index)
        {
            Debug.Assert(resultSet != null, "!resultSet");

            if (fields_indexMethod == null)
                lock (locker)
                    if (fields_indexMethod == null)
                        try
                        {
                            Type ResultSetType = resultSet.GetType();
                            var fieldsFld = ResultSetType.GetField("fields", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            Debug.Assert(fieldsFld != null, "!fieldsFld");

                            var getter = new DynamicMethod("get_Fields[]#1", typeof(object), new Type[] { typeof(object), typeof(int) }, true);
                            var il = getter.GetILGenerator();

                            // return <resultSet>.fields[<index>]
                            il.Emit(OpCodes.Ldarg_0);   // <resultSet>
                            il.Emit(OpCodes.Castclass, ResultSetType);
                            il.Emit(OpCodes.Ldfld, fieldsFld);  // .fields
                            il.Emit(OpCodes.Ldarg_1);   // <index>
                            il.Emit(OpCodes.Ldelem, typeof(object));    // []
                            il.Emit(OpCodes.Ret);

                            //
                            fields_indexMethod = (Func<object, int, object>)getter.CreateDelegate(typeof(Func<object, int, object>));
                        }
                        catch
                        {
                            throw;
                        }

            return fields_indexMethod(resultSet, index);
        }
        private static Func<object, int, object> fields_indexMethod;

        /// <summary>
        /// Make ((MySqlField)<paramref name="mysqlfield"/>).colFlags unprotected read.
        /// </summary>
        /// <param name="mysqlfield"></param>
        /// <returns></returns>
        public static ColumnFlags/*ColumnFlags*/colFlags(object/*!*/mysqlfield)
        {
            Debug.Assert(mysqlfield != null, "!mysqlfield");

            if (colFlagsMethod == null)
                lock (locker)
                    if (colFlagsMethod == null)
                        colFlagsMethod = GenerateUnprotectedFieldRead<object, int>(mysqlfield, "colFlags");

            return (ColumnFlags)colFlagsMethod(mysqlfield);
        }
        private static Func<object, int> colFlagsMethod;

        /// <summary>
        /// Make ((MySqlField)<paramref name="mysqlfield"/>).RealTableName unprotected read.
        /// </summary>
        /// <param name="mysqlfield"></param>
        /// <returns></returns>
        public static string/*string*/RealTableName(object/*!*/mysqlfield)
        {
            Debug.Assert(mysqlfield != null, "!mysqlfield");

            if (RealTableNameMethod == null)
                lock (locker)
                    if (RealTableNameMethod == null)
                        RealTableNameMethod = GenerateUnprotectedFieldRead<object, string>(mysqlfield, "RealTableName");

            return RealTableNameMethod(mysqlfield);
        }
        private static Func<object, string> RealTableNameMethod;

        /// <summary>
        /// Make ((MySqlField)<paramref name="mysqlfield"/>).ColumnLength unprotected read.
        /// </summary>
        /// <param name="mysqlfield"></param>
        /// <returns></returns>
        public static int/*int*/ColumnLength(object/*!*/mysqlfield)
        {
            Debug.Assert(mysqlfield != null, "!mysqlfield");

            if (ColumnLengthMethod == null)
                lock (locker)
                    if (ColumnLengthMethod == null)
                        ColumnLengthMethod = GenerateUnprotectedFieldRead<object, int>(mysqlfield, "ColumnLength");

            return ColumnLengthMethod(mysqlfield);
        }
        private static Func<object, int> ColumnLengthMethod;

        /// <summary>
        /// Make ((MySqlField)<paramref name="mysqlfield"/>).maxLength unprotected read.
        /// </summary>
        /// <param name="mysqlfield"></param>
        /// <returns></returns>
        public static int/*int*/maxLength(object/*!*/mysqlfield)
        {
            Debug.Assert(mysqlfield != null, "!mysqlfield");

            if (maxLengthMethod == null)
                lock (locker)
                    if (maxLengthMethod == null)
                        maxLengthMethod = GenerateUnprotectedFieldRead<object, int>(mysqlfield, "maxLength");

            return maxLengthMethod(mysqlfield);
        }
        private static Func<object, int> maxLengthMethod;

        /// <summary>
        /// Make ((MySqlField)<paramref name="mysqlfield"/>).IsTextField call.
        /// </summary>
        /// <param name="mysqlfield"></param>
        /// <returns></returns>
        public static bool/*bool*/IsTextField(object/*!*/mysqlfield)
        {
            Debug.Assert(mysqlfield != null, "!mysqlfield");

            if (IsTextFieldMethod == null)
            {
                lock (locker)
                {
                    if (IsTextFieldMethod == null)
                        try
                        {
                            Type MySqlFieldType = mysqlfield.GetType();
                            var IsTextFieldProp = MySqlFieldType.GetProperty("IsTextField", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            Debug.Assert(IsTextFieldProp != null, "!IsTextField");
                            Debug.Assert(IsTextFieldProp.PropertyType == typeof(bool), "IsTextField expected to be bool");

                            var getter = new DynamicMethod("get_IsTextField#1", typeof(bool), new Type[] { typeof(object) }, true);
                            var il = getter.GetILGenerator();

                            // return <mysqlfield>.IsTextField;
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Castclass, MySqlFieldType); // cast to MySqlField
                            il.Emit(OpCodes.Call, IsTextFieldProp.GetGetMethod());
                            il.Emit(OpCodes.Ret);

                            //
                            IsTextFieldMethod = (Func<object, bool>)getter.CreateDelegate(typeof(Func<object, bool>));
                        }
                        catch
                        {
                            throw;
                        }
                }
            }
            return IsTextFieldMethod(mysqlfield);
        }
        private static Func<object, bool> IsTextFieldMethod;

        public static int GetColumnSize(object/*!*/mysqlfield)
        {
            return IsTextField(mysqlfield) ? (ColumnLength(mysqlfield) / maxLength(mysqlfield)) : ColumnLength(mysqlfield);
        }
    }

    /// <summary>
    /// Copied from <c>MySql.Data.MySqlClient</c>.
    /// </summary>
    public enum ColumnFlags
    {
        /// <summary></summary>
        NOT_NULL = 1,
        /// <summary></summary>
        PRIMARY_KEY,
        /// <summary></summary>
        UNIQUE_KEY = 4,
        /// <summary></summary>
        MULTIPLE_KEY = 8,
        /// <summary></summary>
        BLOB = 16,
        /// <summary></summary>
        UNSIGNED = 32,
        /// <summary></summary>
        ZERO_FILL = 64,
        /// <summary></summary>
        BINARY = 128,
        /// <summary></summary>
        ENUM = 256,
        /// <summary></summary>
        AUTO_INCREMENT = 512,
        /// <summary></summary>
        TIMESTAMP = 1024,
        /// <summary></summary>
        SET = 2048,
        /// <summary></summary>
        NUMBER = 32768
    }
}
