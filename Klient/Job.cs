using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klient
{
    class Job
    {
        public int id;
        public string name;
        public string des;

        public Job(int id, string name, string des)
        {
            this.id = id;
            this.name = name;
            this.des = des;
        }

        public static Job Parse(string text)
        {
            Regex regex = new Regex(@"Id:([0-9]+)[\s]*Name:([\w]+[\w\s]*)Description:([\w]+[\w\s]*)");
            var match = regex.Match(text);

            if (match.Success)
               return new Job(Int32.Parse(match.Groups[1].Value.Trim()), match.Groups[2].Value.Trim(), match.Groups[3].Value.Trim());

            return null;
        }
    }
}
