using System;
using System.Collections.Generic;
using System.Net;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Utilities;
using RestSharp;

namespace CommandCentral.Test
{
    public static class WatchbillTester
    {
        static RestClient client = new RestClient("https://localhost:1113/api");
        static List<DTOs.WatchShiftType.Get> shiftTypes = new List<DTOs.WatchShiftType.Get>();

        private static DTOs.Person.Get developer = null;
        
        public static void StartTest()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;


            var request = TestUtils.CreateRequest("persons/me", Method.GET);
            developer = client.Execute<DTOs.Person.Get>(request).Data;
            
            AddShiftTypes();

            Console.ReadKey();
        }

        private static void AddShiftTypes()
        {
            shiftTypes.Add(AddShiftType("CDO", WatchQualifications.CDO));
            shiftTypes.Add(AddShiftType("OOD", WatchQualifications.OOD));
            shiftTypes.Add(AddShiftType("JOOD", WatchQualifications.JOOD));
            shiftTypes.Add(AddShiftType("CDO Super", WatchQualifications.CDO));
            shiftTypes.Add(AddShiftType("OOD Super", WatchQualifications.OOD));
            shiftTypes.Add(AddShiftType("JOOD Super", WatchQualifications.JOOD));
        }

        private static DTOs.WatchShiftType.Get AddShiftType(string name, WatchQualifications qual)
        {
            var request = TestUtils.CreateRequest("watchshifttypes", Method.POST);
            request.AddJsonBody(new DTOs.WatchShiftType.Post
            {
                Description = name,
                Name = name,
                Qualification = qual,
                Command = developer.Command
            });
            return client.Execute<DTOs.WatchShiftType.Get>(request).Data;
        }
    }
}