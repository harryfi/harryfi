using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MasterOnline.Models;
using Microsoft.Web.Redis;
using Newtonsoft.Json;

namespace MasterOnline.ViewModels
{
    public class AccountUserViewModel
    {
        public Account Account { get; set; }
        public User User { get; set; }
        public Admin Admin { get; set; }
        public List<User> ListUser { get; set; } = new List<User>();
        public List<SecUser> ListSec { get; set; } = new List<SecUser>();
        public List<string> Errors { get; set; } = new List<string>();

        //add by nurul 1/3/2019
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        //end add by nurul 1/3/2019
    }

    public class JsonSerializer : ISerializer
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public byte[] Serialize(object data)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, _settings));
        }

        public object Deserialize(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), _settings);
        }
    }
}