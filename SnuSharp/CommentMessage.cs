﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnuSharp
{
    [DataContract]
    public class CommentMessage : Message
    {
        [JsonProperty("link_title")]
        public string LinkTitle { get; set; }
        [JsonProperty("likes")]
        public bool? Likes { get; set; }
    }
}