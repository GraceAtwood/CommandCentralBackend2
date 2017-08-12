using CommandCentral.Authentication;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Muster;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using CommandCentral.Framework.Data;
using CommandCentral.PreDefs;
using CommandCentral.Utilities.Types;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class TestDatabaseBuilder
    {

        public static void BuildDatabase()
        {

            var rawConnectionString = ConfigurationUtility.Configuration.GetConnectionString("Main");

            var connectionString = new MySqlConnectionStringBuilder(rawConnectionString);
            var connectionStringWithoutDatabase = new MySqlConnectionStringBuilder(rawConnectionString);
            var database = connectionStringWithoutDatabase.Database;
            connectionStringWithoutDatabase.Database = null;

            MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase.GetConnectionString(true), $"DROP DATABASE IF EXISTS {database}");
            MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase.GetConnectionString(true), $"CREATE DATABASE {database}");

            SessionManager.Schema.Create(true, true);

            //Entities.Watchbill.WatchAssignment.WatchAssignmentMapping.UpdateForeignKeyRule();
            AddAPIKey();

            PreDefUtility.PersistPreDef<WatchQualification>();
            PreDefUtility.PersistPreDef<Sex>();
            PreDefUtility.PersistPreDef<PhoneNumberType>();
            PreDefUtility.PersistPreDef<Paygrade>();
            PreDefUtility.PersistPreDef<AccountabilityType>();
            PreDefUtility.PersistPreDef<DutyStatus>();
            PreDefUtility.PersistPreDef<AccountHistoryType>();

            CreateUICs();
            CreateDesignations();

            CreateCommands(1, 1);
            CreateDepartments(2, 4);
            CreateDivisions(2, 4);

            CreateDeveloper();
            CreateUsers(30);
        }

        private static void AddAPIKey()
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {

                SessionManager.CurrentSession.Save(new APIKey
                {
                    ApplicationName = "Command Central Official Frontend",
                    Id = Guid.Parse("90FDB89F-282B-4BD6-840B-CEF597615728")
                });

                SessionManager.CurrentSession.Save(new APIKey
                {
                    ApplicationName = "Swagger Documentation Page",
                    Id = Guid.Parse("E28235AC-57A1-42AC-AA85-1547B755EA7E")
                });

                transaction.Commit();
            }
        }

        private static void CreateUICs()
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                for (int x = 0; x < Random.GetRandomNumber(5, 10); x++)
                {
                    SessionManager.CurrentSession.Save(new UIC
                    {
                        Value = Random.RandomString(5),
                        Description = Random.RandomString(8),
                        Id = Guid.NewGuid()
                    });

                }

                transaction.Commit();
            }
        }

        private static void CreateDesignations()
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                for (int x = 0; x < Random.GetRandomNumber(5, 10); x++)
                {
                    SessionManager.CurrentSession.Save(new Designation
                    {
                        Value = Random.RandomString(3),
                        Description = Random.RandomString(8),
                        Id = Guid.NewGuid()
                    });

                }

                transaction.Commit();
            }
        }

        private static void CreateCommands(int min, int max)
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                for (int x = 0; x < Random.GetRandomNumber(2, 4); x++)
                {
                    var command = new Command
                    {
                        Description = Random.RandomString(8),
                        Name = x.ToString(),
                        Id = Guid.NewGuid(),
                        Address = "123 Happy St.",
                        City = "Happyville",
                        Country = "USA",
                        State = "Texas",
                        ZipCode = "55555",
                        MusterStartHour = 16
                    };

                    DateTime startTime;
                    if (DateTime.UtcNow.Hour < command.MusterStartHour)
                        startTime = DateTime.UtcNow.Date.AddDays(-1).AddHours(command.MusterStartHour);
                    else
                        startTime = DateTime.UtcNow.Date.AddHours(command.MusterStartHour);

                    var cycle = new MusterCycle
                    {
                        Command = command,
                        Id = Guid.NewGuid(),
                        Range = new TimeRange
                        {
                            Start = startTime,
                            End = startTime.AddDays(1)
                        }
                    };

                    command.CurrentMusterCycle = cycle;

                    SessionManager.CurrentSession.SaveOrUpdate(command);
                }

                transaction.Commit();
            }
        }

        private static void CreateDepartments(int min, int max)
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var commands = SessionManager.CurrentSession.QueryOver<Command>().List();

                foreach (var command in commands)
                {
                    for (int x = 0; x < Random.GetRandomNumber(min, max); x++)
                    {
                        var dep = new Department
                        {
                            Command = command,
                            Description = Random.RandomString(8),
                            Name = $"{command.Name}.{x.ToString()}",
                            Id = Guid.NewGuid()
                        };

                        command.Departments.Add(dep);

                        SessionManager.CurrentSession.Update(command);
                    }
                }

                transaction.Commit();
            }
        }

        private static void CreateDivisions(int min, int max)
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var departments = SessionManager.CurrentSession.QueryOver<Department>().List();

                foreach (var department in departments)
                {
                    for (int x = 0; x < Random.GetRandomNumber(min, max); x++)
                    {
                        var div = new Division
                        {
                            Department = department,
                            Description = Random.RandomString(8),
                            Name = $"{department.Name}.{x.ToString()}",
                            Id = Guid.NewGuid()
                        };

                        department.Divisions.Add(div);
                        SessionManager.CurrentSession.Update(department);
                    }
                }

                transaction.Commit();
            }
        }

        private static Dictionary<string, int> emailAddresses = new Dictionary<string, int>();

        private static Person CreatePerson(Division division,
            UIC uic, string lastName, string username, IEnumerable<PermissionGroup> permissionGroups,
            IEnumerable<WatchQualification> watchQuals, Paygrade paygrade, Designation designation)
        {
            var person = new Person()
            {
                Id = Guid.NewGuid(),
                LastName = lastName,
                MiddleName = division.Name,
                Division = division,
                UIC = uic,
                SSN = Random.GenerateSSN(),
                DoDId = Random.GenerateDoDId(),
                IsClaimed = true,
                Username = username,
                PasswordHash = Authentication.PasswordHash.CreateHash("a"),
                Sex = ReferenceListHelper<Sex>.Random(1).First(),
                DateOfBirth = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12), Random.GetRandomNumber(1, 28)),
                DateOfArrival = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12), Random.GetRandomNumber(1, 28)),
                EAOS = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12), Random.GetRandomNumber(1, 28)),
                PRD = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12), Random.GetRandomNumber(1, 28)),
                Paygrade = paygrade,
                DutyStatus = ReferenceListHelper<DutyStatus>.Random(1).First(),
                WatchQualifications = watchQuals.ToList(),
                PermissionGroups = permissionGroups.ToList(),
                Designation = designation
            };

            person.FirstName = String.Join("__", person.GetHighestAccessLevels().Select(x => $"{x.Key.ToString().Substring(0, 2)}_{x.Value.ToString().Substring(0, 3)}"));

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
                    IsReleasableOutsideCoC = true,
                    IsPreferred = true,
                    Person = person
                } };

            person.AccountHistory = new List<AccountHistoryEvent>
            {
                new AccountHistoryEvent
                {
                    AccountHistoryEventType = ReferenceListHelper<AccountHistoryType>.Find("Creation"),
                    EventTime = DateTime.UtcNow,
                    Person = person,
                    Id = Guid.NewGuid()
                }
            };

            return person;
        }

        private static void CreateDeveloper()
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var command = SessionManager.CurrentSession.QueryOver<Command>().CacheMode(NHibernate.CacheMode.Ignore).List().First();
                var department = command.Departments.First();
                var division = department.Divisions.First();
                var uic = ReferenceListHelper<UIC>.Random(1).First();

                var person = CreatePerson(division, uic, "developer", "dev",
                    new[] { PermissionsCache.PermissionGroupsCache["Developers"] },
                    ReferenceListHelper<WatchQualification>.All(), ReferenceListHelper<Paygrade>.Find("E5"), ReferenceListHelper<Designation>.Random(1).First());

                SessionManager.CurrentSession.Save(person);

                SessionManager.CurrentSession.Update(person);

                transaction.Commit();
            }
        }

        private static void CreateUsers(int sailorsPerDivision)
        {
            int created = 0;
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var paygrades = ReferenceListHelper<Paygrade>.All().Where(x => (x.Value.Contains("E") && !x.Value.Contains("O")) || (x.Value.Contains("O") && !x.Value.Contains("C")));

                var divPermGroups = Authorization.PermissionsCache.PermissionGroupsCache.Values
                    .Where(x => x.AccessLevels.Values.Any(y => y == ChainOfCommandLevels.Division) && x.IsMemberOfChainOfCommand).ToList();
                var depPermGroups = Authorization.PermissionsCache.PermissionGroupsCache.Values
                    .Where(x => x.AccessLevels.Values.Any(y => y == ChainOfCommandLevels.Department) && x.IsMemberOfChainOfCommand).ToList();
                var comPermGroups = Authorization.PermissionsCache.PermissionGroupsCache.Values
                    .Where(x => x.AccessLevels.Values.Any(y => y == ChainOfCommandLevels.Command) && x.IsMemberOfChainOfCommand).ToList();

                foreach (var command in SessionManager.CurrentSession.QueryOver<Command>().List())
                {
                    foreach (var department in command.Departments)
                    {
                        foreach (var division in department.Divisions)
                        {

                            //Add Sailors
                            for (int x = 0; x < sailorsPerDivision; x++)
                            {
                                var paygrade = paygrades.Shuffle().First();
                                var uic = ReferenceListHelper<UIC>.Random(1).First();

                                List<WatchQualification> quals = new List<WatchQualification>();
                                List<PermissionGroup> permGroups = new List<PermissionGroup>();

                                var permChance = Random.GetRandomNumber(0, 100);

                                if (!paygrade.IsCivilianPaygrade())
                                {
                                    if (permChance >= 0 && permChance < 60)
                                    {
                                        //Users
                                    }
                                    else if (permChance >= 60 && permChance < 80)
                                    {
                                        //Division leadership
                                        permGroups.AddRange(divPermGroups.Shuffle().Take(Random.GetRandomNumber(1, divPermGroups.Count)));
                                    }
                                    else if (permChance >= 80 && permChance < 90)
                                    {
                                        //Dep leadership
                                        permGroups.AddRange(depPermGroups.Shuffle().Take(Random.GetRandomNumber(1, depPermGroups.Count)));
                                    }
                                    else if (permChance >= 90 && permChance < 95)
                                    {
                                        //Com leadership
                                        permGroups.AddRange(comPermGroups.Shuffle().Take(Random.GetRandomNumber(1, comPermGroups.Count)));
                                    }
                                    else if (permChance >= 95 && permChance <= 100)
                                    {
                                        permGroups.Add(Authorization.PermissionsCache.PermissionGroupsCache["Admin"]);
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

                                var person = CreatePerson(division, uic, "user" + created.ToString(), "user" + created.ToString(), permGroups, quals, paygrade, ReferenceListHelper<Designation>.Random(1).First());

                                SessionManager.CurrentSession.Save(person);
                                
                                created++;

                            }
                        }
                    }
                }

                transaction.Commit();

            }
        }
    }
}
