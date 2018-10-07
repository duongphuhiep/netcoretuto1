using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreTuto.Tuto1
{
    public class JsonTuto2
    {
        void create_json_with_newtonsoft_linq_object() {
            var response = new JObject();
            JArray dataset = new JArray();
            JArray errors = new JArray();
            response.Add("dataset", dataset);
            response.Add("errors", errors);
            {
                var table1 = new JObject();
                dataset.Add(table1);
                var records = new JObject();
                var datatype = new JObject();
                table1.Add("records", records);
                table1.Add("datatype", datatype);
                
                records.Add("svrIp", "192.168.200.13");
                records.Add("dbName", "mb_dev_hiep");
                records.Add("message", "1 row(s) affected");
                
                datatype.Add("svrIp", "text");
                datatype.Add("dbName", "text");
                datatype.Add("message", "text");
            }
            {
                var table2 = new JObject();
                dataset.Add(table2);
                table2.Add("records", new JObject());
                table2.Add("datatype", new JObject());
            }
            var error1 = new JObject();
            errors.Add(error1);
            
            string json = response.ToString(Formatting.Indented);
            
        }
    }
}