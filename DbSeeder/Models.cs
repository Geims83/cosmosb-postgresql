using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSeeder
{
    internal class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }

    }

    internal class Event
    {
        internal int EventId { get; set; }
        internal int UserId { get; set; }
        internal int Type { get; set; }
        internal DateTime Date { get; set; }
    }

    internal class Speech
    {
        internal int SpeechId { get; set; }
        internal int UserId { get; set; }
        internal DateTime Date { get; set; }
        internal string Text { get; set; }
    }
}

