using CommandCentral.Authentication;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Muster;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities.Types;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Entities.CollateralDutyTracking;
using NHibernate.Linq;

namespace CommandCentral.Utilities
{
    public static class TestDatabaseBuilder
    {
        public static void BuildDatabase()
        {
            var rawConnectionString = ConfigurationUtility.Configuration.GetConnectionString("Main");

            var connectionStringWithoutDatabase = new MySqlConnectionStringBuilder(rawConnectionString);
            var database = connectionStringWithoutDatabase.Database;
            connectionStringWithoutDatabase.Database = null;

            MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase.GetConnectionString(true),
                $"DROP DATABASE IF EXISTS {database}");
            MySqlHelper.ExecuteScalar(connectionStringWithoutDatabase.GetConnectionString(true),
                $"CREATE DATABASE {database}");

            SessionManager.Schema.Execute(true, true, false);

            AddAPIKey();
        }

        public static void InsertTestData(int commands, int departmentsPerCommand, int divisionsPerDepartment,
            int personsPerDivision)
        {
            CreateUICs();
            CreateDesignations();

            CreateCommands(commands);
            CreateDepartments(departmentsPerCommand);
            CreateDivisions(divisionsPerDepartment);

            CreateCollateralDuties();
            
            CreateDeveloper();
            CreateUsers(personsPerDivision);
        }

        private static void CreateCollateralDuties()
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                foreach (var command in SessionManager.GetCurrentSession().Query<Command>())
                {
                    foreach (var chainOfCommand in (ChainsOfCommand[]) Enum.GetValues(typeof(ChainsOfCommand)))
                    {
                        var duty = new CollateralDuty
                        {
                            ChainOfCommand = chainOfCommand,
                            Command = command,
                            Id = Guid.NewGuid(),
                            Name = chainOfCommand.ToString()
                        };

                        SessionManager.GetCurrentSession().Save(duty);
                    }
                }
                
                transaction.Commit();
            }
        }

        private static void AddAPIKey()
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                SessionManager.GetCurrentSession().Save(new APIKey
                {
                    ApplicationName = "Command Central Official Frontend",
                    Id = Guid.Parse("90FDB89F-282B-4BD6-840B-CEF597615728")
                });

                SessionManager.GetCurrentSession().Save(new APIKey
                {
                    ApplicationName = "Swagger Documentation Page",
                    Id = Guid.Parse("E28235AC-57A1-42AC-AA85-1547B755EA7E")
                });

                transaction.Commit();
            }
        }

        private static void CreateUICs()
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                for (var x = 0; x < Random.GetRandomNumber(5, 10); x++)
                {
                    SessionManager.GetCurrentSession().Save(new UIC
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
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                for (var x = 0; x < Random.GetRandomNumber(5, 10); x++)
                {
                    SessionManager.GetCurrentSession().Save(new Designation
                    {
                        Value = Random.RandomString(3),
                        Description = Random.RandomString(8),
                        Id = Guid.NewGuid()
                    });
                }

                transaction.Commit();
            }
        }

        private static void CreateCommands(int count)
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                for (var x = 0; x < count; x++)
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
                        MusterStartHour = 16,
                        TimeZoneId = TimeZoneInfo.GetSystemTimeZones().Shuffle().First().Id
                    };

                    var startTime = DateTime.UtcNow.Hour < command.MusterStartHour
                        ? DateTime.UtcNow.Date.AddDays(-1).AddHours(command.MusterStartHour)
                        : DateTime.UtcNow.Date.AddHours(command.MusterStartHour);

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

                    SessionManager.GetCurrentSession().SaveOrUpdate(command);
                }

                transaction.Commit();
            }
        }

        private static void CreateDepartments(int count)
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                var commands = SessionManager.GetCurrentSession().Query<Command>().ToList();

                foreach (var command in commands)
                {
                    for (var x = 0; x < count; x++)
                    {
                        var dep = new Department
                        {
                            Command = command,
                            Description = Random.RandomString(8),
                            Name = $"{command.Name}.{x}",
                            Id = Guid.NewGuid()
                        };

                        command.Departments.Add(dep);
                    }
                }

                transaction.Commit();
            }
        }

        private static void CreateDivisions(int count)
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                var departments = SessionManager.GetCurrentSession().Query<Department>().ToList();

                foreach (var department in departments)
                {
                    for (var x = 0; x < count; x++)
                    {
                        var div = new Division
                        {
                            Department = department,
                            Description = Random.RandomString(8),
                            Name = $"{department.Name}.{x}",
                            Id = Guid.NewGuid()
                        };

                        department.Divisions.Add(div);
                    }
                }

                transaction.Commit();
            }
        }

        private static readonly Dictionary<string, int> _emailAddresses = new Dictionary<string, int>();

        private static Person CreatePerson(Guid id, Division division,
            UIC uic, string lastName, string username, IEnumerable<WatchQualifications> watchQuals, Paygrades paygrade,
            Designation designation)
        {
            var person = new Person
            {
                Id = id,
                LastName = lastName,
                MiddleName = division.Name,
                Division = division,
                UIC = uic,
                SSN = Random.GenerateSSN(),
                DoDId = Random.GenerateDoDId(),
                Sex = Random.GetRandomEnumValue<Sexes>(),
                DateOfBirth = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12),
                    Random.GetRandomNumber(1, 28), 0, 0, 0, DateTimeKind.Utc),
                DateOfArrival = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12),
                    Random.GetRandomNumber(1, 28), 0, 0, 0, DateTimeKind.Utc),
                EAOS = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12),
                    Random.GetRandomNumber(1, 28), 0, 0, 0, DateTimeKind.Utc),
                PRD = new DateTime(Random.GetRandomNumber(1970, 2000), Random.GetRandomNumber(1, 12),
                    Random.GetRandomNumber(1, 28), 0, 0, 0, DateTimeKind.Utc),
                Paygrade = paygrade,
                DutyStatus = Random.GetRandomEnumValue<DutyStatuses>(),
                WatchQualifications = watchQuals.ToList(),
                Designation = designation
            };

            person.FirstName = String.Join("__",
                person.GetHighestAccessLevels().Select(x =>
                    $"{x.Key.ToString().Substring(0, 2)}_{x.Value.ToString().Substring(0, 3)}"));

            var emailAddress = $"{person.FirstName}.{person.MiddleName[0]}.{person.LastName}.mil@mail.mil";

            if (_emailAddresses.ContainsKey(emailAddress))
            {
                _emailAddresses[emailAddress]++;
            }
            else
            {
                _emailAddresses.Add(emailAddress, 1);
            }

            emailAddress =
                $"{person.FirstName}.{person.MiddleName[0]}.{person.LastName}{_emailAddresses[emailAddress]}.mil@mail.mil";

            person.EmailAddresses = new List<EmailAddress>
            {
                new EmailAddress
                {
                    Address = emailAddress,
                    Id = Guid.NewGuid(),
                    IsReleasableOutsideCoC = true,
                    IsPreferred = true,
                    Person = person
                }
            };

            person.AccountHistory = new List<AccountHistoryEvent>
            {
                new AccountHistoryEvent
                {
                    AccountHistoryEventType = AccountHistoryTypes.Created,
                    EventTime = DateTime.UtcNow,
                    Person = person,
                    Id = Guid.NewGuid()
                }
            };

            return person;
        }

        private static void CreateDeveloper()
        {
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                var command = SessionManager.GetCurrentSession().Query<Command>().CacheMode(NHibernate.CacheMode.Ignore)
                    .First();
                var department = command.Departments.First();
                var division = department.Divisions.First();
                var uic = SessionManager.GetCurrentSession().Query<UIC>().ToList().Shuffle().First();

                var person = CreatePerson(Guid.Parse("b2db659d-4998-40a2-8962-e6eb05326ea5"), division, uic,
                    "developer", "dev", (WatchQualifications[]) Enum.GetValues(typeof(WatchQualifications)),
                    Paygrades.E5, SessionManager.GetCurrentSession().Query<Designation>().ToList().Shuffle().First());

                foreach (var duty in SessionManager.GetCurrentSession().Query<CollateralDuty>().Where(x => x.Command == person.Division.Department.Command))
                {
                    var membership = new CollateralDutyMembership
                    {
                        CollateralDuty = duty,
                        Id = Guid.NewGuid(),
                        Level = ChainOfCommandLevels.Command,
                        Person = person,
                        Role = CollateralRoles.Primary
                    };

                    SessionManager.GetCurrentSession().Save(membership);
                }
                
                SessionManager.GetCurrentSession().Save(person);

                transaction.Commit();
            }
        }

        private static void CreateUsers(int sailorsPerDivision)
        {
            var created = 0;
            using (var transaction = SessionManager.GetCurrentSession().BeginTransaction())
            {
                var paygrades = PaygradeUtilities.EnlistedPaygrades;

                foreach (var command in SessionManager.GetCurrentSession().Query<Command>())
                {
                    foreach (var department in command.Departments)
                    {
                        foreach (var division in department.Divisions)
                        {
                            //Add Sailors
                            for (var x = 0; x < sailorsPerDivision; x++)
                            {
                                var paygrade = paygrades.Shuffle().First();
                                var uic = SessionManager.GetCurrentSession().Query<UIC>().ToList().Shuffle().First();

                                var quals = ((WatchQualifications[]) Enum.GetValues(typeof(WatchQualifications)))
                                    .ToList();

                                if (paygrade.IsOfficerPaygrade())
                                {
                                    quals.Add(WatchQualifications.CDO);
                                }
                                else if (paygrade.IsEnlistedPaygrade())
                                {
                                    if (paygrade.IsChief())
                                    {
                                        quals.Add(WatchQualifications.CDO);
                                    }
                                    else
                                    {
                                        if (paygrade.IsPettyOfficer())
                                        {
                                            quals.Add(WatchQualifications.OOD);
                                            quals.Add(WatchQualifications.JOOD);
                                        }
                                        else if (paygrade.IsSeaman())
                                        {
                                            quals.Add(WatchQualifications.JOOD);
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

                                var person = CreatePerson(Guid.NewGuid(), division, uic, "user" + created,
                                    "user" + created, quals, paygrade,
                                    SessionManager.GetCurrentSession().Query<Designation>().ToList().Shuffle().First());

                                SessionManager.GetCurrentSession().Save(person);

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