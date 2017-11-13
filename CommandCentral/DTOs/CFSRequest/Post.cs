using System;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.DTOs.CFSRequest
{
    public class Post : Put
    {
        public Guid Person { get; set; }

        public CFSRequestType RequestType { get; set; }
    }
}