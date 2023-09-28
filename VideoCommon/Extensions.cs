using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DatabaseProcessor {
    public static class Extensions {

        public static DbParameter CreateParameter(this DbCommand command, string name, DbType @type) {
            var parm = command.CreateParameter();
            parm.DbType = @type;
            parm.ParameterName = name;
            return parm;
        }

        public static DbParameter CreateParameter(this DbCommand command, string name, DbType @type, object value, int size) {
            var parm = command.CreateParameter();
            parm.DbType = @type;
            parm.ParameterName = name;
            parm.Value = value;
            parm.Size = size;
            command.Parameters.Add(parm);
            return parm;
        }

        public static DbParameter CreateParameter(this DbCommand command, string name, DbType @type, object value) {
            var parm = command.CreateParameter();
            parm.DbType = @type;
            parm.ParameterName = name;
            parm.Value = value;            
            command.Parameters.Add(parm);
            return parm;
        }

        public static T ReturnNullValue<T>() {
            switch (typeof(T).Name) {
                case "Int32":
                case "Int16":
                case "Byte":
                    return (T)Convert.ChangeType(0, typeof(T));
                default: return default(T);
            }
        }

        public static string GetStr(this DbDataReader reader, string name) {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<string>();
            return reader.GetString(col);
        }

        public static int GetInt(this DbDataReader reader, string name) {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<int>();
            return reader.GetInt32(col);
        }

        public static long GetLong(this DbDataReader reader, string name)
        {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<int>();
            return reader.GetInt64(col);
        }

        public static short GetShort(this DbDataReader reader, string name) {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<short>();
            return reader.GetInt16(col);
        }

        public static byte GetB(this DbDataReader reader, string name) {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<byte>();
            return reader.GetByte(col);
        }

        public static bool GetBool(this DbDataReader reader, string name) {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<bool>();
            return reader.GetBoolean(col);
        }

        public static DateTime GetDT(this DbDataReader reader, string name)
        {
            int col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col))
                return ReturnNullValue<DateTime>();
            return reader.GetDateTime(col);
        }

        public static string Quote(this object value, bool mustQuote = true) {
            if (mustQuote)
                return $"'{value}'";
            return value.ToString();
        }

        public static string ToCamel(this string value) {
            if (value.Length == 0)
                return string.Empty;
            if (value.Length < 2)
                return value.ToUpper();

            return $"{char.ToUpper(value[0])}{value.Substring(1).ToLowerInvariant()}";
        }

        public static void SqlEscape(this object[] values) {
            for (int i = 0; i < values.Length; i++) {
                if (values[i] is DBNull)
                    values[i] = "NULL";
                values[i] = values[i].ToString().Replace("\'", "\'\'");
            }
        }
    }
}
