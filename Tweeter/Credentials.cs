﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweeter
{
    public class Credentials
    {
        public string ApiKey { get; set; }
        public string ApiKeySecret { get; set; }

        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public string ConsumerKey => ApiKey;
        public string ConsumerSecret => ApiKeySecret;
    }
}
