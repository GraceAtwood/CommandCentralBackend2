using CommandCentral.Authentication;
using CommandCentral.Authorization.Groups;
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
        [TestCase("localhost", "test_database", "anguslmm", "applew", true)]
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

                DataProvider.ConnectionString = connectionStringWithDatabase;
                DataProvider.ForceFactoryInitialization();

                DataProvider.Schema.Create(true, true);

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

                using (var transaction = DataProvider.CurrentSession.BeginTransaction())
                {
                    for (int x = 0; x < Utilities.GetRandomNumber(5, 10); x++)
                    {
                        DataProvider.CurrentSession.Save(new UIC
                        {
                            Value = Utilities.RandomString(5),
                            Description = Utilities.RandomString(8),
                            Id = Guid.NewGuid()
                        });

                    }

                    transaction.Commit();
                }

                using (var transaction = DataProvider.CurrentSession.BeginTransaction())
                {
                    for (int x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                    {
                        DataProvider.CurrentSession.Save(new Command
                        {
                            Description = Utilities.RandomString(8),
                            Value = x.ToString(),
                            Id = Guid.NewGuid()
                        });
                    }

                    transaction.Commit();
                }

                using (var transaction = DataProvider.CurrentSession.BeginTransaction())
                {
                    var commands = DataProvider.CurrentSession.QueryOver<Command>().List();

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

                            DataProvider.CurrentSession.Update(command);
                        }
                    }

                    transaction.Commit();
                }

                using (var transaction = DataProvider.CurrentSession.BeginTransaction())
                {
                    var departments = DataProvider.CurrentSession.QueryOver<Department>().List();

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
                            DataProvider.CurrentSession.Update(department);
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
            using (var transaction = DataProvider.CurrentSession.BeginTransaction())
            {

                DataProvider.CurrentSession.Save(new APIKey
                {
                    ApplicationName = "Command Central Official Frontend",
                    Id = Guid.Parse("90FDB89F-282B-4BD6-840B-CEF597615728")
                });

                transaction.Commit();
            }
        }

        static Dictionary<string, int> emailAddresses = new Dictionary<string, int>();

        public Person CreatePerson(Command command, Department department, Division division,
            UIC uic, string lastName, string username, IEnumerable<PermissionGroup> permissionGroups,
            IEnumerable<WatchQualification> watchQuals, Paygrade paygrade)
        {
            var person = new Person()
            {
                Id = Guid.NewGuid(),
                LastName = lastName,
                MiddleName = division.Value,
                Command = command,
                Department = department,
                Division = division,
                UIC = uic,
                SSN = Utilities.GenerateSSN(),
                DoDId = Utilities.GenerateDoDId(),
                IsClaimed = true,
                Username = username,
                PasswordHash = Authentication.PasswordHash.CreateHash("a"),
                Sex = ReferenceListHelper<Sex>.Random(1).First(),
                DateOfBirth = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                DateOfArrival = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                EAOS = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                PRD = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                Paygrade = paygrade,
                DutyStatus = ReferenceListHelper<DutyStatus>.Random(1).First(),
                PermissionGroupNames = permissionGroups.Select(x => x.GroupName).ToList(),
                WatchQualifications = watchQuals.ToList()
            };

            var resolvedPermissions = person.ResolvePermissions(null);
            person.FirstName = String.Join("__", resolvedPermissions.HighestLevels.Select(x => $"{x.Key.ToString().Substring(0, 2)}_{x.Value.ToString().Substring(0, 3)}"));

            var emailAddress = $"{person.FirstName}.{person.MiddleName[0]}.{person.LastName}.mil@mail.mil";

            if (emailAddresses.ContainsKey(emailAddress))
            {
                emailAddresses[emailAddress]++;
            }
            else
            {
                emailAddresses.Add(emailAddress, 1);
            }

            emailAddress = $"{person.FirstName}.{person.MiddleName[0]}.{person.LastName}{emailAddresses[emailAddress]}.mil@mail.mil";

            person.EmailAddresses = new List<EmailAddress> { new EmailAddress
                {
                    Address = emailAddress,
                    Id = Guid.NewGuid(),
                    IsContactable = true,
                    IsPreferred = true
                } };

            person.AccountHistory = new List<AccountHistoryEvent>
            {
                new AccountHistoryEvent
                {
                    AccountHistoryEventType = ReferenceListHelper<AccountHistoryType>.Find("Creation"),
                    EventTime = DateTime.UtcNow
                }
            };

            return person;
        }

        public void CreateDeveloper()
        {
            using (var transaction = DataProvider.CurrentSession.BeginTransaction())
            {
                var command = DataProvider.CurrentSession.QueryOver<Command>().CacheMode(NHibernate.CacheMode.Ignore).List().First();
                var department = command.Departments.First();
                var division = department.Divisions.First();
                var uic = ReferenceListHelper<UIC>.Random(1).First();

                var eligibilityGroup = ReferenceListHelper<WatchEligibilityGroup>.Find("Quarterdeck");

                var person = CreatePerson(command, department, division, uic, "developer", "dev",
                    new[] { new Authorization.Groups.Definitions.Developers() },
                    ReferenceListHelper<WatchQualification>.All(), ReferenceListHelper<Paygrade>.Find("E5"));

                DataProvider.CurrentSession.Save(person);

                eligibilityGroup.EligiblePersons.Add(person);

                DataProvider.CurrentSession.Update(person);

                transaction.Commit();
            }
        }

        public void CreateUsers()
        {
            int created = 0;
            using (var transaction = DataProvider.CurrentSession.BeginTransaction())
            {
                var paygrades = ReferenceListHelper<Paygrade>.All().Where(x => (x.Value.Contains("E") && !x.Value.Contains("O")) || (x.Value.Contains("O") && !x.Value.Contains("C")));

                var eligibilityGroup = ReferenceListHelper<WatchEligibilityGroup>.Find("Quarterdeck");

                var divPermGroups = PermissionGroup.AllPermissionGroups.Where(y => y.AccessLevel == ChainOfCommandLevels.Division).ToList();
                var depPermGroups = PermissionGroup.AllPermissionGroups.Where(y => y.AccessLevel == ChainOfCommandLevels.Department).ToList();
                var comPermGroups = PermissionGroup.AllPermissionGroups.Where(y => y.AccessLevel == ChainOfCommandLevels.Command).ToList();

                foreach (var command in DataProvider.CurrentSession.QueryOver<Command>().List())
                {
                    foreach (var department in command.Departments)
                    {
                        foreach (var division in department.Divisions)
                        {

                            //Add Sailors
                            for (int x = 0; x < 30; x++)
                            {
                                var paygrade = paygrades.Shuffle().First();
                                var uic = ReferenceListHelper<UIC>.Random(1).First();

                                List<WatchQualification> quals = new List<WatchQualification>();
                                List<PermissionGroup> permGroups = new List<PermissionGroup>();

                                var permChance = Utilities.GetRandomNumber(0, 100);

                                if (!paygrade.IsCivilianPaygrade())
                                {
                                    if (permChance >= 0 && permChance < 60)
                                    {
                                        //Users
                                    }
                                    else if (permChance >= 60 && permChance < 80)
                                    {
                                        //Division leadership
                                        permGroups.AddRange(divPermGroups.Shuffle().Take(Utilities.GetRandomNumber(1, divPermGroups.Count)));
                                    }
                                    else if (permChance >= 80 && permChance < 90)
                                    {
                                        //Dep leadership
                                        permGroups.AddRange(depPermGroups.Shuffle().Take(Utilities.GetRandomNumber(1, depPermGroups.Count)));
                                    }
                                    else if (permChance >= 90 && permChance < 95)
                                    {
                                        //Com leadership
                                        permGroups.AddRange(comPermGroups.Shuffle().Take(Utilities.GetRandomNumber(1, comPermGroups.Count)));
                                    }
                                    else if (permChance >= 95 && permChance <= 100)
                                    {
                                        permGroups.Add(new Authorization.Groups.Definitions.Admin());
                                    }
                                }

                                if (paygrade.IsOfficerPaygrade())
                                {
                                    quals.Add(ReferenceListHelper<WatchQualification>.Find("CDO"));
                                }
                                else if (paygrade.IsEnlistedPaygrade())
                                {
                                    if (paygrade.IsChief())
                                    {
                                        quals.Add(ReferenceListHelper<WatchQualification>.Find("CDO"));
                                    }
                                    else
                                    {
                                        if (paygrade.IsPettyOfficer())
                                        {
                                            quals.AddRange(ReferenceListHelper<WatchQualification>.FindAll("OOD", "JOOD"));
                                        }
                                        else if (paygrade.IsSeaman())
                                        {
                                            quals.Add(ReferenceListHelper<WatchQualification>.Find("JOOD"));
                                        }
                                        else
                                        {
                                            throw new Exception("We shouldn't be here...");
                                        }
                                    }
                                }
                                else if (paygrade.IsCivilianPaygrade())
                                {
                                    //Do nothing for now
                                }
                                else
                                {
                                    throw new Exception($"An unknown paygrade was found! {paygrade}");
                                }

                                var person = CreatePerson(command, department, division, uic, "user" + created.ToString(), "user" + created.ToString(), permGroups, quals, paygrade);

                                DataProvider.CurrentSession.Save(person);

                                if (!paygrade.IsCivilianPaygrade())
                                {
                                    eligibilityGroup.EligiblePersons.Add(person);
                                }

                                created++;

                            }
                        }
                    }
                }

                DataProvider.CurrentSession.Update(eligibilityGroup);

                transaction.Commit();

            }
        }
    }
}

