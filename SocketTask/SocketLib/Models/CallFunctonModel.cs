﻿namespace SocketLib.Models
{
    public class CallFunctonModel
    {
        public string hubName { get; set; }
        public string functionName { get; set; }
        public object[] parameters { get; set; }
    }
}