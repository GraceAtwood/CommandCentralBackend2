using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using CommandCentral.Framework.Data;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Authorization;
using CommandCentral.Authorization.Rules;
using FluentValidation.Results;
using CommandCentral.Entities.Muster;
using NHibernate.Linq;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single person and all their properties and data access methods.
    /// </summary>
    [HasPermissions]
    public class Person : Entity, IHazComments
    {

        #region Properties

        #region Main Properties

        /// <summary>
        /// The person's last name.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string LastName { get; set; }

        /// <summary>
        /// The person's first name.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string FirstName { get; set; }

        /// <summary>
        /// The person's middle name.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string MiddleName { get; set; }

        /// <summary>
        /// The person's SSN.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanReturnIfSelf]
        [CanReturnIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual string SSN { get; set; }

        /// <summary>
        /// The person's DoD Id which allows us to communicate with other systems about this person.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanReturnIfSelf]
        [CanReturnIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual string DoDId { get; set; }

        /// <summary>
        /// The person's suffix.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string Suffix { get; set; }

        /// <summary>
        /// The person's date of birth.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        [CanReturnIfSelf]
        [CanReturnIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual DateTime DateOfBirth { get; set; }

        /// <summary>
        /// The person's age.  0 if the date of birth isn't set.
        /// </summary>
        [CanNeverEdit]
        public virtual int Age
        {
            get
            {
                if (DateOfBirth == default(DateTime))
                    return 0;

                if (DateTime.Today.Month < DateOfBirth.Month ||
                    DateTime.Today.Month == DateOfBirth.Month &&
                    DateTime.Today.Day < DateOfBirth.Day)
                {
                    return DateTime.Today.Year - DateOfBirth.Year - 1;
                }

                return DateTime.Today.Year - DateOfBirth.Year;
            }
        }

        /// <summary>
        /// The person's sex.
        /// </summary>
        [CanEditIfSelf]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual Sex Sex { get; set; }

        /// <summary>
        /// Stores the person's ethnicity.
        /// </summary>
        [CanEditIfSelf]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual Ethnicity Ethnicity { get; set; }

        /// <summary>
        /// The person's religious preference
        /// </summary>
        [CanEditIfSelf]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanReturnIfSelf]
        [CanReturnIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual ReligiousPreference ReligiousPreference { get; set; }

        /// <summary>
        /// The person's paygrade (e5, O1, O5, CWO2, GS1,  etc.)
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual Paygrade Paygrade { get; set; }

        /// <summary>
        /// The person's Designation (CTI2, CTR1, 1114, Job title)
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual Designation Designation { get; set; }

        /// <summary>
        /// The person's division
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual Division Division { get; set; }

        /// <summary>
        /// Readonly. Returns Division.Department
        /// </summary>
        public virtual Department Department => Division?.Department;

        /// <summary>
        /// Readonly. Returns Department.Command
        /// </summary>
        public virtual Command Command => Department?.Command;

        #endregion

        #region Work Properties

        /// <summary>
        /// The person's NECs.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual IList<NECInfo> NECs { get; set; } = new List<NECInfo>();

        /// <summary>
        /// The person's supervisor
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string Supervisor { get; set; }

        /// <summary>
        /// The person's work center.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string WorkCenter { get; set; }

        /// <summary>
        /// The room in which the person works.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string WorkRoom { get; set; }

        /// <summary>
        /// A free form text field intended to let the client store the shift of a person - however the client wants to do that.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string Shift { get; set; }

        /// <summary>
        /// The person's duty status
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual DutyStatus DutyStatus { get; set; }

        /// <summary>
        /// The person's UIC
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual UIC UIC { get; set; }

        /// <summary>
        /// The date/time that the person arrived at the command.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual DateTime DateOfArrival { get; set; }

        /// <summary>
        /// The client's job title.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfSelf]
        public virtual string JobTitle { get; set; }

        /// <summary>
        /// The date/time of the end of active obligatory service (EAOS) for the person.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual DateTime? EAOS { get; set; }

        /// <summary>
        /// The member's projected rotation date.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual DateTime? PRD { get; set; }

        /// <summary>
        /// The date/time that the client left/will leave the command.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual DateTime? DateOfDeparture { get; set; }

        /// <summary>
        /// The person's watch qualification.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfInChainOfCommand(ChainsOfCommand.QuarterdeckWatchbill, ChainOfCommandLevels.Division)]
        public virtual IList<WatchQualification> WatchQualifications { get; set; }

        /// <summary>
        /// The person's status periods which describe projected locations and duty locations.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfInChainOfCommand(ChainsOfCommand.QuarterdeckWatchbill, ChainOfCommandLevels.Division)]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Muster, ChainOfCommandLevels.Division)]
        public virtual IList<StatusPeriod> StatusPeriods { get; set; }

        /// <summary>
        /// The type of billet this person is assigned to.
        /// </summary>
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual BilletAssignment BilletAssignment { get; set; }

        #endregion

        #region Contacts Properties

        /// <summary>
        /// The email addresses of this person.
        /// </summary>
        [CanEditIfSelf]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        public virtual IList<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// The Phone Numbers of this person.
        /// </summary>
        public virtual IList<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// The Physical Addresses of this person
        /// </summary>
        public virtual IList<PhysicalAddress> PhysicalAddresses { get; set; }

        #endregion

        #region Account

        /// <summary>
        /// A boolean indicating whether or not this account has been claimed.
        /// </summary>
        [HiddenFromPermissions]
        public virtual bool IsClaimed { get; set; }

        /// <summary>
        /// The client's username.
        /// </summary>
        [CanReturnIfSelf]
        [CanEditIfSelf]
        public virtual string Username { get; set; }

        /// <summary>
        /// The client's hashed password.
        /// </summary>
        [HiddenFromPermissions]
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// The list of the person's permissions.  This is not persisted in the database.  Only the names are.
        /// </summary>
        [HiddenFromPermissions]
        public virtual IList<PermissionGroup> PermissionGroups { get; set; } = new List<PermissionGroup>();

        /// <summary>
        /// A list containing account history events, these are events that track things like login, password reset, etc.
        /// </summary>
        [CanReturnIfSelf]
        [CanNeverEdit]
        [CanReturnIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Command)]
        public virtual IList<AccountHistoryEvent> AccountHistory { get; set; }

        /// <summary>
        /// A list containing all changes that have ever occurred to the profile.
        /// </summary>
        [CanNeverEdit]
        public virtual IList<Change> Changes { get; set; }

        /// <summary>
        /// The list of those events to which this person is subscribed.
        /// </summary>
        [CanEditIfSelf]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Main, ChainOfCommandLevels.Division)]
        [CanEditIfInChainOfCommand(ChainsOfCommand.QuarterdeckWatchbill, ChainOfCommandLevels.Division)]
        [CanEditIfInChainOfCommand(ChainsOfCommand.Muster, ChainOfCommandLevels.Division)]
        public virtual IDictionary<SubscribableEvents, ChainOfCommandLevels> SubscribedEvents { get; set; }

        /// <summary>
        /// The list of comments.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a friendly name for this user in the form: {LastName}, {FirstName} {MiddleName}
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{LastName}, {FirstName} {MiddleName}";
        }

        #endregion

        #region Helper Methods

        public virtual Dictionary<ChainsOfCommand, Dictionary<ChainOfCommandLevels, List<Person>>> GetChainOfCommand()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a boolean indicating if this person is in the same command as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameCommandAs(Person person)
        {
            if (person == null || Division.Department.Command == null || person.Division.Department.Command == null)
                return false;

            return Command.Id == person.Command.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command and department as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDepartmentAs(Person person)
        {
            if (person == null || Department == null || person.Department == null)
                return false;

            return IsInSameCommandAs(person) && Department.Id == person.Department.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command, department, and division as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDivisionAs(Person person)
        {
            if (person == null || Division == null || person.Division == null)
                return false;

            return IsInSameDepartmentAs(person) && Division.Id == person.Division.Id;
        }

        /// <summary>
        /// Determines if a person can access comments.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonAccessComments(Person person)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps a person to the database.
        /// </summary>
        public class PersonMapping : ClassMap<Person>
        {
            /// <summary>
            /// Maps a person to the database.
            /// </summary>
            public PersonMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Ethnicity).Nullable();
                References(x => x.ReligiousPreference).Nullable();
                References(x => x.Designation).Not.Nullable();
                References(x => x.Division).Not.Nullable();
                References(x => x.UIC).Nullable();
                References(x => x.Paygrade).Not.Nullable();
                References(x => x.DutyStatus).Not.Nullable();
                References(x => x.Sex).Not.Nullable();
                References(x => x.BilletAssignment);

                Map(x => x.LastName).Not.Nullable();
                Map(x => x.FirstName).Not.Nullable();
                Map(x => x.MiddleName);
                Map(x => x.SSN).Not.Nullable().Unique();
                Map(x => x.DoDId).Not.Nullable().Unique();
                Map(x => x.DateOfBirth).Not.Nullable();
                Map(x => x.Supervisor);
                Map(x => x.WorkCenter);
                Map(x => x.WorkRoom);
                Map(x => x.Shift);
                Map(x => x.DateOfArrival).Not.Nullable();
                Map(x => x.JobTitle);
                Map(x => x.EAOS).CustomType<UtcDateTimeType>();
                Map(x => x.PRD).CustomType<UtcDateTimeType>();
                Map(x => x.DateOfDeparture).CustomType<UtcDateTimeType>();
                Map(x => x.IsClaimed).Not.Nullable().Default(false.ToString());
                Map(x => x.Username).Unique();
                Map(x => x.PasswordHash).Unique();
                Map(x => x.Suffix);

                HasMany(x => x.NECs).Cascade.All();
                HasMany(x => x.AccountHistory).Cascade.All();
                HasMany(x => x.Changes).Cascade.All().Inverse();
                HasMany(x => x.EmailAddresses).Cascade.All();
                HasMany(x => x.PhoneNumbers).Cascade.All();
                HasMany(x => x.PhysicalAddresses).Cascade.All();
                HasMany(x => x.StatusPeriods).Cascade.All().KeyColumn("Person_id");

                HasManyToMany(x => x.WatchQualifications);

                HasMany(x => x.SubscribedEvents)
                    .AsMap<string>(index =>
                        index.Column("ChangeEvent").Type<GenericEnumMapper<SubscribableEvents>>(), element =>
                        element.Column("Level").Type<GenericEnumMapper<ChainOfCommandLevels>>())
                    .Cascade.All();

                HasMany(x => x.PermissionGroups).Table("persontopermissiongroups").Component(x =>
                {
                    x.Map(y => y.Name);
                });
            }
        }

        /// <summary>
        /// Validates a person object.
        /// </summary>
        public class Validator : AbstractValidator<Person>
        {
            /// <summary>
            /// Validates a person object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.LastName).NotEmpty().Length(1, 255)
                    .WithMessage("The last name must not be left blank and must not exceed 255 characters.");
                RuleFor(x => x.FirstName).NotEmpty().Length(1, 255)
                    .WithMessage("The first name must not be left blank and must not exceed 255 characters.");
                RuleFor(x => x.MiddleName).Length(0, 255)
                    .WithMessage("The middle name must not exceed 255 characters.");
                RuleFor(x => x.Suffix).Length(0, 255)
                    .WithMessage("The suffix must not exceed 255 characters.");
                RuleFor(x => x.SSN).NotEmpty()
                    .Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                        .WithMessage("The SSN must be valid and contain only numbers.")
                    .Must((person, ssn) => SessionManager.GetCurrentSession().Query<Person>().Count(x => x.SSN == ssn && x.Id != person.Id) == 0)
                        .WithMessage("That ssn exists on another profile.  SSNs must be unique.");
                RuleFor(x => x.DoDId).NotEmpty().Length(10)
                    .Must(x => x.All(Char.IsDigit))
                        .WithMessage("All characters of a DoD id must be digits.")
                    .Must((person, dodId) => SessionManager.GetCurrentSession().Query<Person>().Count(x => x.DoDId == dodId && x.Id != person.Id) == 0)
                        .WithMessage("That DoD id exists on another profile.  DoD Ids must be unique.");
                RuleFor(x => x.DateOfBirth).NotEmpty()
                    .WithMessage("The DOB must not be left blank.");
                RuleFor(x => x.Sex).NotEmpty().Must(x => ReferenceListHelper.IdExists<Sex>(x.Id))
                    .WithMessage("The sex must not be left blank.");
                RuleFor(x => x.Command).NotEmpty()
                    .WithMessage("A person must have a command.  If you are trying to indicate this person left the command, please set his or her duty status to 'LOSS'.");
                RuleFor(x => x.Department).NotEmpty()
                    .WithMessage("A person must have a department.  If you are trying to indicate this person left the command, please set his or her duty status to 'LOSS'.");
                RuleFor(x => x.Division).NotEmpty()
                    .WithMessage("A person must have a division.  If you are trying to indicate this person left the command, please set his or her duty status to 'LOSS'.");
                RuleFor(x => x.Ethnicity).Must(x =>
                    {
                        if (x != null && !ReferenceListHelper.IdExists<Ethnicity>(x.Id))
                            return false;

                        return true;
                    })
                    .WithMessage("Your ethnicity was not found.");
                RuleFor(x => x.ReligiousPreference).Must(x =>
                    {
                        if (x != null && !ReferenceListHelper.IdExists<Ethnicity>(x.Id))
                            return false;

                        return true;
                    })
                    .WithMessage("Your religious preference was not found.");
                RuleFor(x => x.Designation).Must(x => ReferenceListHelper.IdExists<Designation>(x.Id))
                    .WithMessage("Your designation was not found.");
                RuleFor(x => x.Division).Must(x => SessionManager.GetCurrentSession().Query<Division>().Count(div => div.Id == x.Id) == 1)
                    .WithMessage("Your division was not found.");
                RuleFor(x => x.Supervisor).Length(0, 255)
                    .WithMessage("The supervisor field may not be longer than 255 characters.");
                RuleFor(x => x.WorkCenter).Length(0, 255)
                    .WithMessage("The work center field may not be longer than 255 characters.");
                RuleFor(x => x.WorkRoom).Length(0, 255)
                    .WithMessage("The work room field may not be longer than 255 characters.");
                RuleFor(x => x.Shift).Length(0, 255)
                    .WithMessage("The shift field may not be longer than 255 characters.");
                RuleFor(x => x.UIC).Must(x => ReferenceListHelper.IdExists<UIC>(x.Id))
                    .WithMessage("Your uic was not found.");
                RuleFor(x => x.JobTitle).Length(0, 255)
                    .WithMessage("The job title may not be longer than 255 characters.");
                
                // If you add more EmailAddresses Rules, it may be necessary to call Person validation in the
                // EmailAddress POST and PUT endpoints. Right now, this rule is covered in logic just before the
                // transaction, so validation isn't called, as it's pointless extra effort.
                RuleFor(x => x.EmailAddresses).Must(x => x.Count(y => y.IsPreferred) <= 1)
                    .WithMessage("Only one email address may be marked as 'Preferred'.");
            }
        }
    }
}
