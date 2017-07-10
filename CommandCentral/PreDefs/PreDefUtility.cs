﻿using CommandCentral.Framework.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.PreDefs
{
    public static class PreDefUtility
    {

        public static ConcurrentBag<IPreDef> Predefs = new ConcurrentBag<IPreDef>();

        static PreDefUtility()
        {
            var test = Assembly.GetExecutingAssembly().GetName().Name;

            var preDefNames = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(x => x.StartsWith($"{Assembly.GetExecutingAssembly().GetName().Name}.PreDefs") && x.EndsWith(".cc"));

            foreach (var resourceName in preDefNames)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();

                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);

                        var fullName = jObject.Value<string>(nameof(IPreDef.TypeFullName));

                        var type = Assembly.GetExecutingAssembly().GetType(fullName, true);

                        Predefs.Add((IPreDef)jObject.ToObject(typeof(PreDefOf<>).MakeGenericType(type)));

                        //TODO Logging.Log.Info("Loaded PreDef for type {0}".With(fullName));
                    }
                }
            }
        }

        public static void PersistPreDef<T>() where T : class
        {
            var predef = (PreDefOf<T>)Predefs.FirstOrDefault(x => x.TypeFullName == typeof(T).FullName) ??
                throw new Exception($"{typeof(T).FullName} does not exist.");

            PersistPreDef(predef);
        }

        public static void PersistPreDef<T>(PreDefOf<T> preDef) where T : class
        {
            using (var transaction = DataProvider.CurrentSession.BeginTransaction())
            {
                foreach (var item in preDef.Definitions)
                {
                    DataProvider.CurrentSession.Save(item);
                }

                transaction.Commit();
            }
        }
    }
}
