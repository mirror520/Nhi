using System;

namespace Nhi
{
    public class Result<T>
    {
        public string Status { get; set; }
        public string[] Info { get; set; }
        public T Data { get; set;}
        public DateTime Time { get; set; }
    }
}