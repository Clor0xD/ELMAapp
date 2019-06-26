using System;

namespace ELMAapp.Models
{
    public class Documents
    {
        public virtual int ID { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Author { get; set; }
        public virtual string BinaryFile { get; set; }
    }
}