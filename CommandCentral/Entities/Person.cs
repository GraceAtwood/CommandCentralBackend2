using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using CommandCentral.Framework.Data;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentValidation.Results;
using CommandCentral.Entities.Muster;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single person and all their properties and data access methods.
    /// </summary>
    public class Person : CommentableEntity
    {
        #region Properties

        #region Main Properties

        /// <summary>
        /// The person's last name.
        /// </summary>
        public virtual string LastName { get; set; }

        /// <summary>
        /// The person's first name.
        /// </summary>
        public virtual string FirstName { get; set; }

        /// <summary>
        /// The person's middle name.
        /// </summary>
        public virtual string MiddleName { get; set; }

        /// <summary>
        /// The person's DoD Id which allows us to communicate with other systems about this person.
        /// </summary>
        public virtual string DoDId { get; set; }

        /// <summary>
        /// The person's suffix.
        /// </summary>
        public virtual string Suffix { get; set; }

        /// <summary>
        /// The person's date of birth.
        /// </summary>
        public virtual DateTime DateOfBirth { get; set; }

        /// <summary>
        /// The person's age.  0 if the date of birth isn't set.
        /// </summary>
        public virtual int Age
        {
            get
            {
                if (DateOfBirth == default)
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
        public virtual Sexes Sex { get; set; }

        /// <summary>
        /// Stores the person's ethnicity.
        /// </summary>
        public virtual Ethnicity Ethnicity { get; set; }

        /// <summary>
        /// The person's religious preference
        /// </summary>
        public virtual ReligiousPreference ReligiousPreference { get; set; }

        /// <summary>
        /// The person's paygrade (e5, O1, O5, CWO2, GS1,  etc.)
        /// </summary>
        public virtual Paygrades Paygrade { get; set; }

        /// <summary>
        /// The person's Designation (CTI2, CTR1, 1114, Job title)
        /// </summary>
        public virtual Designation Designation { get; set; }

        /// <summary>
        /// The person's division
        /// </summary>
        public virtual Division Division { get; set; }

        #endregion

        #region Work Properties

        /// <summary>
        /// The person's NECs.
        /// </summary>
        public virtual IList<NECInfo> NECs { get; set; } = new List<NECInfo>();

        /// <summary>
        /// The person's supervisor
        /// </summary>
        public virtual string Supervisor { get; set; }

        /// <summary>
        /// The person's work center.
        /// </summary>
        public virtual string WorkCenter { get; set; }

        /// <summary>
        /// The room in which the person works.
        /// </summary>
        public virtual string WorkRoom { get; set; }

        /// <summary>
        /// A free form text field intended to let the client store the shift of a person - however the client wants to do that.
        /// </summary>
        public virtual string Shift { get; set; }

        /// <summary>
        /// The person's duty status
        /// </summary>
        public virtual DutyStatuses DutyStatus { get; set; }

        /// <summary>
        /// The person's UIC
        /// </summary>
        public virtual UIC UIC { get; set; }

        /// <summary>
        /// The date/time that the person arrived at the command.
        /// </summary>
        public virtual DateTime DateOfArrival { get; set; }

        /// <summary>
        /// The client's job title.
        /// </summary>
        public virtual string JobTitle { get; set; }

        /// <summary>
        /// The date/time of the end of active obligatory service (EAOS) for the person.
        /// </summary>
        public virtual DateTime? EAOS { get; set; }

        /// <summary>
        /// The member's projected rotation date.
        /// </summary>
        public virtual DateTime? PRD { get; set; }

        /// <summary>
        /// The date/time that the client left/will leave the command.
        /// </summary>
        public virtual DateTime? DateOfDeparture { get; set; }

        /// <summary>
        /// The person's watch qualification.
        /// </summary>
        public virtual IList<WatchQualifications> WatchQualifications { get; set; }

        /// <summary>
        /// The person's status periods which describe projected locations and duty locations.
        /// </summary>
        public virtual IList<StatusPeriod> StatusPeriods { get; set; }

        /// <summary>
        /// The type of billet this person is assigned to.
        /// </summary>
        public virtual BilletAssignments BilletAssignment { get; set; }

        #endregion

        #region Contacts Properties

        /// <summary>
        /// The email addresses of this person.
        /// </summary>
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
        /// The collection of all the col duty rules this person has.
        /// </summary>
        public virtual IList<CollateralDutyMembership> CollateralDutyMemberships { get; set; }

        /// <summary>
        /// A list of the submodules this person can access.
        /// </summary>
        public virtual IList<SpecialPermissions> SpecialPermissions { get; set; } = new List<SpecialPermissions>();

        /// <summary>
        /// A list containing account history events, these are events that track things like login, password reset, etc.
        /// </summary>
        public virtual IList<AccountHistoryEvent> AccountHistory { get; set; }

        /// <summary>
        /// A list containing all changes that have ever occurred to the profile.
        /// </summary>
        public virtual IList<Change> Changes { get; set; }

        /// <summary>
        /// The list of those events to which this person is subscribed.
        /// </summary>
        public virtual IDictionary<SubscribableEvents, ChainOfCommandLevels> SubscribedEvents { get; set; }

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

        /// <summary>
        /// Returns a boolean indicating if this person is in the same command as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameCommandAs(Person person)
        {
            if (person == null || Division.Department.Command == null || person.Division.Department.Command == null)
                return false;

            return Division.Department.Command.Id == person.Division.Department.Command.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command and department as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDepartmentAs(Person person)
        {
            if (person == null || Division.Department == null || person.Division.Department == null)
                return false;

            return IsInSameCommandAs(person) && Division.Department.Id == person.Division.Department.Id;
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

                Map(x => x.LastName).Not.Nullable();
                Map(x => x.FirstName).Not.Nullable();
                Map(x => x.MiddleName);
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
                Map(x => x.Suffix);
                Map(x => x.Paygrade).Not.Nullable().CustomType<GenericEnumMapper<Paygrades>>();
                Map(x => x.Sex).Not.Nullable();
                Map(x => x.DutyStatus).Not.Nullable();
                Map(x => x.BilletAssignment);

                HasMany(x => x.CollateralDutyMemberships).Cascade.All();
                HasMany(x => x.NECs).Cascade.All();
                HasMany(x => x.AccountHistory).Cascade.All();
                HasMany(x => x.Changes).Cascade.All().Inverse();
                HasMany(x => x.EmailAddresses).Cascade.All();
                HasMany(x => x.PhoneNumbers).Cascade.All();
                HasMany(x => x.PhysicalAddresses).Cascade.All();
                HasMany(x => x.StatusPeriods).Cascade.All().KeyColumn("Person_id");
                HasMany(x => x.WatchQualifications)
                    .Cascade.All()
                    .Table("watchqualificationstoperson")
                    .Element("WatchQualification");

                HasMany(x => x.SubscribedEvents)
                    .AsMap<string>(index =>
                        index.Column("ChangeEvent").Type<GenericEnumMapper<SubscribableEvents>>(), element =>
                        element.Column("Level").Type<GenericEnumMapper<ChainOfCommandLevels>>())
                    .Cascade.All();

                HasMany(x => x.SpecialPermissions)
                    .Cascade.All()
                    .Table("specialpermissionstopersons")
                    .Element("SpecialPermission");
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
                RuleFor(x => x.DoDId).NotEmpty().Length(10)
                    .Must(x => x.All(Char.IsDigit))
                    .WithMessage("All characters of a DoD id must be digits.")
                    .Must((person, dodId) =>
                        SessionManager.GetCurrentSession().Query<Person>()
                            .Count(x => x.DoDId == dodId && x.Id != person.Id) == 0)
                    .WithMessage("That DoD id exists on another profile.  DoD Ids must be unique.");
                RuleFor(x => x.DateOfBirth).NotEmpty()
                    .WithMessage("The DOB must not be left blank.");
                RuleFor(x => x.Sex).NotEmpty()
                    .WithMessage("The sex must not be left blank.");
                RuleFor(x => x.Division).NotEmpty()
                    .WithMessage("A person must have a division.  If you are trying to indicate this person left " +
                                 "the command, please set his or her duty status to 'LOSS'.");
                RuleFor(x => x.Division).Must(x =>
                        SessionManager.GetCurrentSession().Query<Division>().Count(div => div.Id == x.Id) == 1)
                    .WithMessage("Your division was not found.");
                RuleFor(x => x.Supervisor).Length(0, 255)
                    .WithMessage("The supervisor field may not be longer than 255 characters.");
                RuleFor(x => x.WorkCenter).Length(0, 255)
                    .WithMessage("The work center field may not be longer than 255 characters.");
                RuleFor(x => x.WorkRoom).Length(0, 255)
                    .WithMessage("The work room field may not be longer than 255 characters.");
                RuleFor(x => x.Shift).Length(0, 255)
                    .WithMessage("The shift field may not be longer than 255 characters.");
                RuleFor(x => x.JobTitle).Length(0, 255)
                    .WithMessage("The job title may not be longer than 255 characters.");

                // If you add more EmailAddresses Rules, it may be necessary to call Person validation in the
                // EmailAddress POST and PUT endpoints. Right now, this rule is covered in logic just before the
                // transaction, so validation isn't called, as it's pointless extra effort.
                When(x => x.EmailAddresses != null, () =>
                {
                    RuleFor(x => x.EmailAddresses).Must(x => x.Count(y => y.IsPreferred) <= 1)
                        .WithMessage("Only one email address may be marked as 'Preferred'.");
                });
            }
        }

        public class Contract : RulesContract<Person>
        {
            public Contract()
            {
                //Can edit if in chain of command or self, everyone can return.
                RulesFor(
                        x => x.FirstName,
                        x => x.LastName,
                        x => x.MiddleName,
                        x => x.Ethnicity,
                        x => x.ReligiousPreference,
                        x => x.JobTitle,
                        x => x.Supervisor,
                        x => x.Suffix,
                        x => x.WorkCenter,
                        x => x.WorkRoom,
                        x => x.Sex,
                        x => x.Shift)
                    .CanEdit((editor, subject) =>
                        editor.IsInChainOfCommand(subject, ChainsOfCommand.Main) || editor == subject)
                    .CanReturn((editor, subject) => true);

                //Can edit if in chain of command, everyone can return.
                RulesFor(
                        x => x.DateOfArrival,
                        x => x.DateOfBirth,
                        x => x.DateOfDeparture,
                        x => x.BilletAssignment,
                        x => x.Designation,
                        x => x.DutyStatus,
                        x => x.EAOS,
                        x => x.NECs,
                        x => x.Paygrade,
                        x => x.PRD,
                        x => x.UIC)
                    .CanEdit((editor, subject) => editor.IsInChainOfCommand(subject, ChainsOfCommand.Main))
                    .CanReturn((editor, subjecct) => true);

                //Can edit if in chain of command, everyone can return.
                RulesFor(x => x.Division)
                    .CanEdit((editor, subject) =>
                        editor.IsInChainOfCommand(subject, ChainsOfCommand.Main, ChainsOfCommand.Muster))
                    .CanReturn((editor, subject) => true);

                //Can edit if in chain of command, everyone can return.
                RulesFor(x => x.StatusPeriods)
                    .CanEdit((editor, subject) =>
                        editor.IsInChainOfCommand(subject, ChainsOfCommand.Main, ChainsOfCommand.Muster,
                            ChainsOfCommand.QuarterdeckWatchbill))
                    .CanReturn((editor, subject) => true);
                
                //Can edit if in chain of command, everyone can return.
                RulesFor(x => x.WatchQualifications)
                    .CanEdit((editor, subject) =>
                        editor.IsInChainOfCommand(subject, ChainsOfCommand.Main, ChainsOfCommand.QuarterdeckWatchbill))
                    .CanReturn((editor, subject) => true);

                //can edit if in chain of command, can see if in coc or self.
                RulesFor(
                        x => x.DoDId)
                    .CanEdit((editor, subject) => editor.IsInChainOfCommand(subject, ChainsOfCommand.Main))
                    .CanReturn((editor, subject) =>
                        editor.IsInChainOfCommand(subject, ChainsOfCommand.Main) || editor == subject);

                //No one can edit, everyone can see
                RulesFor(
                        x => x.AccountHistory,
                        x => x.Age,
                        x => x.Changes,
                        x => x.Id)
                    .CanEdit((editor, subject) => false)
                    .CanReturn((editor, subject) => true);

                RulesFor(
                        x => x.EmailAddresses,
                        x => x.PhoneNumbers,
                        x => x.PhysicalAddresses)
                    .CanEdit((editor, subject) =>
                        editor.IsInChainOfCommand(subject, ChainsOfCommand.Main) || editor == subject)
                    .CanReturn((editor, subject) =>
                        throw new NotImplementedException("Return is controlled by inidividual objects."));

                //Not supported edit, all can see
                RulesFor(
                        x => x.CollateralDutyMemberships,
                        x => x.Comments
                    )
                    .CanEdit((editor, subject) =>
                        throw new NotImplementedException(
                            "The edit rules for this property are implemented in their controllers or elsewhere due to logic that the rules contracts can not support."))
                    .CanReturn((editor, subject) => true);
            }
        }
    }
}