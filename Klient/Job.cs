using System;
using System.Text.RegularExpressions;

namespace Klient
{
    class Job
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string CreateTime { get; }
        public string UpdateTime { get; }

        public Job(int id, string name, string des, string createTime, string updateTime)
        {
            this.Id = id;
            this.Name = name;
            this.Description = des;
            this.CreateTime = createTime;
            this.UpdateTime = updateTime;
        }

        public static Job Parse(string text)
        {
            Regex regex = new Regex(@"Id:([0-9]+)[\s]*Name:([\w]+[\w\s]*)Description:([\w]+[\w\s]*)CreateTime:([0-9.]+ [0-9:]+)[\s]*UpdateTime:([0-9.]+ [0-9:]+)[\s]*");
            var match = regex.Match(text);

            if (match.Success)
                return new Job(Int32.Parse(match.Groups[1].Value.Trim()), match.Groups[2].Value.Trim(), match.Groups[3].Value.Trim(), match.Groups[4].Value.Trim(), match.Groups[5].Value.Trim());

            return null;
        }
    }
}