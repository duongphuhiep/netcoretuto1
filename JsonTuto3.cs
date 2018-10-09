using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreTuto.Tuto1
{
    public class JsonTuto3
    {
        public class Response {
            public class Table {
                public JObject datatype = new JObject();
                public List<JObject> records = new List<JObject>();
            }
            public List<Table> dataset = new List<Table>();
            public List<JObject> errors = new List<JObject>();
        }
        
        public static void create_json_mixed() {
            var response = new Response();
            var dataset = response.dataset;
            {
                var table1 = new Response.Table();
                dataset.Add(table1);
                var records = table1.records;
                var datatype = table1.datatype;
                
                var r = new JObject();
                records.Add(r);
                r.Add("svrIp", "192.168.200.13");
                r.Add("dbName", "mb_dev_hiep");
                r.Add("message", "1 row(s) affected");
                
                datatype.Add("svrIp", "text");
                datatype.Add("dbName", "text");
                datatype.Add("message", "text");
            }
            {
                var table1 = new Response.Table();
                dataset.Add(table1);
            }
            
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            Console.WriteLine(json);
            
        }
    }
}