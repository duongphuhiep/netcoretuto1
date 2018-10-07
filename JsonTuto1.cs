using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NetCoreTuto.Tuto1 {
    public static class JsonTuto1 {
        public static void create_json_with_dynamic_object () {
                var response = new {
                dataset = new [] {
                    new {
                        records = new[] {
                            new {
                                svrIp = "192.168.200.13",
                                dbName = "mb_dev_hiep",
                                message = "1 row(s) affected",
                            }
                        },
                        datatype = new {
                            svrIp = "text",
                            dbName = "text",
                            message = "text"
                        }
                    },
                    new {
                        records = new[] {
                            new {
                                svrIp = "192.168.200.13",
                                dbName = "mb_dev_hiep",
                                message = "1 row(s) affected",
                            }
                        },
                        datatype = new {
                            svrIp = "text",
                            dbName = "text",
                            message = "text"
                        }
                    }
                },
                errors = new[] {
                    new {
                        svrIp = "192.168.200.13",
                        dbName = "mb_dev_test001atos",
                        code = "42000",
                        msg = "SQLSTATE[42000]: [Microsoft][ODBC Driver 11 for SQL Server][SQL Server]error is raised"
                    }
                }
            };
            
            Log.Information(JsonConvert.SerializeObject(response));
        }
    }
}