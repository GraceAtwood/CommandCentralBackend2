using CommandCentral.Authentication;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework.Data;
using CommandCentral.PreDefs;
using CommandCentral.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestFixture]
    public class IntegrationTests
    {
        [TestCase("localhost", "test_database", "root", "password", true)]
        public void MainIntegrationTest(string server, string database, string username, string password, bool rebuild)
        {
            var connectionStringWithDatabase = $"server={server};database={database};user={username};password={password};";
            var connectionStringWithoutDatabase = $"server={server};user={username};password={password};";

            var result = MySql.Data.MySqlClient.MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase,
                $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{database}'");

            if (result == null && !rebuild)
            {
                throw new Exception("nope");
            }

            if (rebuild)
            {
                MySql.Data.MySqlClient.MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase, $"DROP DATABASE IF EXISTS {database}");
                MySql.Data.MySqlClient.MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase, $"CREATE DATABASE {database}");

                SessionManager.Schema.Create(true, true);

                Entities.Watchbill.WatchAssignment.WatchAssignmentMapping.UpdateForeignKeyRule();
                AddAPIKey();

                PreDefUtility.PersistPreDef<WatchQualification>();
                PreDefUtility.PersistPreDef<Sex>();
                PreDefUtility.PersistPreDef<WatchbillStatus>();
                PreDefUtility.PersistPreDef<WatchShiftType>();
                PreDefUtility.PersistPreDef<WatchEligibilityGroup>();
                PreDefUtility.PersistPreDef<WatchAssignmentState>();
                PreDefUtility.PersistPreDef<PhoneNumberType>();
                PreDefUtility.PersistPreDef<Paygrade>();
                PreDefUtility.PersistPreDef<MusterStatus>();
                PreDefUtility.PersistPreDef<DutyStatus>();
                PreDefUtility.PersistPreDef<AccountHistoryType>();

                using (var transaction = SessionManager.CurrentSession.BeginTransaction())
                {
                    for (int x = 0; x < Utilities.GetRandomNumber(5, 10); x++)
                    {
                        SessionManager.CurrentSession.Save(new UIC
                        {
                            Value = Utilities.RandomString(5),
                            Description = Utilities.RandomString(8),
                            Id = Guid.NewGuid()
                        });

                    }

                    transaction.Commit();
                }

                using (var transaction = SessionManager.CurrentSession.BeginTransaction())
                {
                    for (int x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                    {
                        SessionManager.CurrentSession.Save(new Command
                        {
                            Description = Utilities.RandomString(8),
                            Value = x.ToString(),
                            Id = Guid.NewGuid()
                        });
                    }

                    transaction.Commit();
                }

                using (var transaction = SessionManager.CurrentSession.BeginTransaction())
                {
                    var commands = SessionManager.CurrentSession.QueryOver<Command>().List();

                    foreach (var command in commands)
                    {
                        for (int x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                        {
                            var dep = new Department
                            {
                                Command = command,
                                Description = Utilities.RandomString(8),
                                Value = $"{command.Value}.{x.ToString()}",
                                Id = Guid.NewGuid()
                            };

                            command.Departments.Add(dep);

                            SessionManager.CurrentSession.Update(command);
                        }
                    }

                    transaction.Commit();
                }

                using (var transaction = SessionManager.CurrentSession.BeginTransaction())
                {
                    var departments = SessionManager.CurrentSession.QueryOver<Department>().List();

                    foreach (var department in departments)
                    {
                        for (int x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                        {
                            var div = new Division
                            {
                                Department = department,
                                Description = Utilities.RandomString(8),
                                Value = $"{department.Value}.{x.ToString()}",
                                Id = Guid.NewGuid()
                            };

                            department.Divisions.Add(div);
                            SessionManager.CurrentSession.Update(department);
                        }
                    }

                    transaction.Commit();
                }

                CreateDeveloper();
                CreateUsers();
            }
        }

        public static void AddAPIKey()
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {

                SessionManager.CurrentSession.Save(new APIKey
                {
                    ApplicationName = "Command Central Official Frontend",
                    Id = Guid.Parse("90FDB89F-282B-4BD6-840B-CEF597615728")
                });

                transaction.Commit();
            }
        }

        
    }
}

