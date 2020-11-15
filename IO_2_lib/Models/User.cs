using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_2_lib.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public virtual AuthKey AuthKey { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }

        public User()
        {
            Tasks = new List<Task>();
        }
    }
}
