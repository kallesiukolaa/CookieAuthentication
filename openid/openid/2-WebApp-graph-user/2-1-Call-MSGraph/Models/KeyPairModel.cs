using System;

namespace WebApp_OpenIDConnect_DotNet_graph.Models
{
    public class KeyPairModel
    {
        public int KeyID {get; set;}
        public string KeyName { get; set; }

        public string KeyDescription { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
    }
}
