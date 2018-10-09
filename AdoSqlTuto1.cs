using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreTuto.Tuto1
{
    public class AdoSqlTuto1
    {
        private static readonly HashSet<string> NumberTypeNames = new HashSet<string> { "int", "bigint", "long", "decimal", "money", "commission", "amount" };
        
        public class Response {
            public class Table {
                Dictionary<string, string> datatype = new Dictionary<string, string>();
                JArray records;
            }
            public List<Table> dataset = new List<Table>();
            public JArray errors;
        }
        

        public static void exec_request() 
        {
            CultureInfo newCulture = (CultureInfo) CultureInfo.InvariantCulture.Clone ();
            newCulture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            newCulture.DateTimeFormat.DateSeparator = "-";
            newCulture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd HH:mm:ss,fff";
            newCulture.DateTimeFormat.TimeSeparator = ":";
            newCulture.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss,fff";
            Thread.CurrentThread.CurrentCulture = newCulture;

            var connectionString = "Server=tcp:localhost,1433;Initial Catalog=TuanGiang;User Id=sa; Password=secret; Connection Timeout=2";
            var query = @"
            DECLARE @o1 varchar(max);
            DECLARE @r int;
            EXEC @r = [test001] @p1='hiep', @o1 = @o1 output; --c1,c2 / hiep
            update ClientOrder set qty=qty+1 where id<2;
            SELECT @r as x1, @o1 as x1; --200
            select * from ClientOrder;
                        ";
            //var query = @"DECLARE @o1 varchar(max); update ClientOrder set qty=qty+1 where id<3;";
            var response = new JObject ();

            try {
                int tableId = 0;
                using (var connection = new SqlConnection (connectionString)) {
                    var command = new SqlCommand (query, connection);
                    connection.Open ();

                    using (var reader = command.ExecuteReader ()) {
                        var noname = 1;
                        var dataset = (JArray) response.GetValue ("dataset");
                        if (dataset == null) {
                            dataset = new JArray ();
                            response.Add ("dataset", dataset);
                        }

                        while (reader.HasRows) {
                            var table = (JObject) dataset.FirstOrDefault (x => (int) ((JObject) x).GetValue ("id") == tableId);
                            if (table == null) {
                                table = new JObject ();
                                table.Add ("id", tableId);
                                dataset.Add (table);
                            }

                            var datatype = (JObject) table.GetValue ("datatype");
                            if (datatype == null) {
                                datatype = new JObject ();
                                table.Add ("datatype", datatype);
                            }
                            var schemaTable = reader.GetSchemaTable ();
                            int columnCount = schemaTable.Rows.Count;
                            string[] columnNames = new string[columnCount];
                            string[] columnTypes = new string[columnCount];
                            for (int i = 0; i < columnCount; i++) {
                                var columnInfo = schemaTable.Rows[i];
                                var columnType = (string) columnInfo["DataTypeName"];
                                var columnName = (string) columnInfo["ColumnName"];
                                //if columName is empty => create a name for it
                                if (string.IsNullOrEmpty (columnName)) {
                                    columnName = $"no_name_{noname}";
                                }
                                //if columnName is repeated (2 columns with same name) => change the name to something else
                                if (columnNames.Any (x => x == columnName)) {
                                    int c = 0;
                                    string newColumnName;
                                    do {
                                        c++;
                                        newColumnName = columnName + "_" + c;
                                    }
                                    while (columnNames.Any (x => x == newColumnName));
                                    columnName = newColumnName;
                                }
                                columnTypes[i] = columnType;
                                columnNames[i] = columnName;
                                if (datatype.GetValue (columnName) == null) {
                                    datatype.Add (columnName, NumberTypeNames.Contains (columnType) ? "number" : "text");
                                }
                            }

                            var records = (JArray) table.GetValue ("records");
                            if (records == null) {
                                records = new JArray ();
                                table.Add ("records", records);
                            }

                            while (reader.Read ()) {
                                var record = new JObject ();
                                records.Add (record);
                                object[] cellValues = new object[columnCount];
                                reader.GetValues (cellValues);
                                for (int i = 0; i < columnCount; i++) {
                                    var cellHeaderName = columnNames[i];
                                    var cellType = columnTypes[i];
                                    var cellValue = cellValues[i];
                                    switch (cellType) {
                                        case "int":
                                            record.Add (cellHeaderName, (int) cellValue);
                                            break;
                                        case "bigint":
                                        case "long":
                                            record.Add (cellHeaderName, (long) cellValue);
                                            break;
                                        case "decimal":
                                        case "money":
                                        case "commission":
                                        case "amount":
                                            record.Add (cellHeaderName, (decimal) cellValue);
                                            break;
                                        default:
                                            record.Add (cellHeaderName, cellValue.ToString ());
                                            break;
                                    }
                                }
                            }
                            reader.NextResult ();
                            tableId++;
                        }

                        var recordsAffected = reader.RecordsAffected;
                        if (recordsAffected > 0) {
                            
                            var table = JObject.FromObject (new {
                                datatype = new {
                                        RecordsAffected = "number"
                                    },
                                    records = new [] {
                                        new {
                                            RecordsAffected = recordsAffected
                                        }
                                    }
                            });
                            dataset.Add (table);
                        }
                    }
                }
                Console.WriteLine (response);
            } 
            catch (Exception ex) {
                Console.WriteLine (ex);
            }
        }
    }
}