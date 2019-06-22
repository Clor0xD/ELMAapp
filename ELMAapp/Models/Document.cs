using System;

namespace ELMAapp.Models
{
    public class Document
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Author { get; set; }
        public string BinaryFile { get; set; }
    }
}