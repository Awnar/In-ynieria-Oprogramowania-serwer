using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_2_lib.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual User User { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}:{Id} \n\r" +
                 $"{nameof(Name)}:{Name} \n\r" +
                 $"{nameof(Description)}:{Description} \n\r" +
                 $"User Id: {User.Id} \n\r";
        }
    }
}
