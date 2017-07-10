using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using System.Globalization;
using NHibernate;
using NHibernate.Type;
using CommandCentral.Authorization;
using CommandCentral.Utilities.Types;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using CommandCentral.Enums;

namespace CommandCentral.Entities.Watchbill
{
    /// <summary>
    /// Describes a single watchbill, which is a collection of watch days, shifts in those days, and inputs.
    /// </summary>
    public class Watchbill : IEntity
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watchbill.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The free text title of this watchbill.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The person who created this watchbill.  This is expected to often be the command watchbill coordinator.
        /// </summary>
        public virtual Person CreatedBy { get; set; }

        /// <summary>
        /// Represents the current state of the watchbill.  Different states should trigger different actions.
        /// </summary>
        public virtual WatchbillStatus CurrentState { get; set; }

        /// <summary>
        /// Indicates the last time the state of this watchbill was changed.
        /// </summary>
        public virtual DateTime LastStateChange { get; set; }

        /// <summary>
        /// Contains a reference to the person who caused the last state change.
        /// </summary>
        public virtual Person LastStateChangedBy { get; set; }

        /// <summary>
        /// The list of all watch shifts that exist on this watchbill.
        /// </summary>
        public virtual IList<WatchShift> WatchShifts { get; set; } = new List<WatchShift>();

        /// <summary>
        /// The min and max dates of the watchbill.
        /// </summary>
        public virtual TimeRange Range { get; set; }

        /// <summary>
        /// The collection of requirements.  This is how we know who needs to provide inputs and who is available to be on this watchbill.
        /// </summary>
        public virtual IList<WatchInputRequirement> InputRequirements { get; set; } = new List<WatchInputRequirement>();

        /// <summary>
        /// The command at which this watchbill was created.
        /// </summary>
        public virtual ReferenceLists.Command Command { get; set; }

        /// <summary>
        /// This is how the watchbill knows the pool of people to use when assigning inputs, and assigning watches.  
        /// <para />
        /// The eligibility group also determines the type of watchbill.
        /// </summary>
        public virtual WatchEligibilityGroup EligibilityGroup { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new watchbill, setting all collection to empty.
        /// </summary>
        public Watchbill()
        {
        }

        #endregion

        #region Methods
        

        /// <summary>
        /// Returns all those input requirements a person is responsible for.  Meaning those requirements that are in a person's chain of command.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual IEnumerable<WatchInputRequirement> GetInputRequirementsPersonIsResponsibleFor(Person person)
        {
            if (!person.PermissionGroupNames.Any())
                return new List<WatchInputRequirement>();

            var resolvedPermissions = person.ResolvePermissions(null);

            var highestLevelForWatchbill = resolvedPermissions.HighestLevels[this.EligibilityGroup.OwningChainOfCommand];

            if (highestLevelForWatchbill == ChainOfCommandLevels.None)
                return new List<WatchInputRequirement>();

            switch (highestLevelForWatchbill)
            {
                case ChainOfCommandLevels.Command:
                    {
                        return this.InputRequirements.Where(x => x.Person.IsInSameCommandAs(person));
                    }
                case ChainOfCommandLevels.Department:
                    {
                        return this.InputRequirements.Where(x => x.Person.IsInSameDepartmentAs(person));
                    }
                case ChainOfCommandLevels.Division:
                    {
                        return this.InputRequirements.Where(x => x.Person.IsInSameDivisionAs(person));
                    }
                case ChainOfCommandLevels.Self:
                    {
                        return this.InputRequirements.Where(x => x.Person.Id == person.Id);
                    }
                case ChainOfCommandLevels.None:
                    {
                        return new List<WatchInputRequirement>();
                    }
                default:
                    {
                        throw new NotImplementedException("Fell to the default case in the chain of command switch of the LoadInputRequirementsResponsibleFor endpoint.");
                    }
            }
        }

        /// <summary>
        /// Returns true if the watchbill is in a state that allows editing of the structure of the watchbill.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanEditStructure()
        {
            return CurrentState == ReferenceLists.ReferenceListHelper<WatchbillStatus>.Find("Initial") || CurrentState == ReferenceLists.ReferenceListHelper<WatchbillStatus>.Find("Open for Inputs");
        }

        /// <summary>
        /// Sends an email to each person in the el group who is also a member of this watchbill's chain of command.
        /// <para/>
        /// The email contains a list of all those personnel who the given person is responsible for in terms of watch inputs.
        /// </summary>
        public static void SendWatchInputRequirementsAlertEmail(Guid watchbillId)
        {
            using (var session = DataProvider.CurrentSession)
            {
                var watchbill = session.Get<Watchbill>(watchbillId) ??
                    throw new Exception("A watchbill was loaded that no longer exists.");

                //We need to find each person who is in this watchbill's chain of command, and then iterate over each one, sending emails to each with the peopel they are responsible for.
                foreach (var person in watchbill.EligibilityGroup.EligiblePersons)
                {
                    var requirementsResponsibleFor = watchbill.GetInputRequirementsPersonIsResponsibleFor(person);

                    if (requirementsResponsibleFor.Any())
                    {
                        var model = new Email.Models.WatchInputRequirementsEmailModel
                        {
                            Person = person,
                            Watchbill = watchbill,
                            PersonsWithoutInputs = requirementsResponsibleFor.Where(x => !x.IsAnswered).Select(x => x.Person)
                        };

                        var emailAddresses = person.EmailAddresses.Where(x => x.IsDodEmailAddress);

                        Email.EmailInterface.CCEmailMessage
                            .CreateDefault()
                            .To(emailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, person.ToString())))
                            .CC(Email.EmailInterface.CCEmailMessage.DeveloperAddress)
                            .Subject("Watch Input Requirements")
                            .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.WatchInputRequirements_HTML.html", model)
                            .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
                    }
                }
            }
        }

        #endregion

        #region Startup / Cron Alert Methods

        /// <summary>
        /// Sets up the watch alerts.  This is basically just a recurring cron method that looks to see if someone has watch coming up and if they do, sends them a notification.
        /// </summary>
        public static void SetupAlerts()
        {
            FluentScheduler.JobManager.AddJob(() => SendWatchAlerts(), s => s.ToRunEvery(1).Hours().At(0));

            //Here, we're also going to set up any watch input requirements alerts we need for each watchbill that is in the open for inputs state.
            using (var session = DataProvider.CurrentSession)
            {
                var watchbills = session.QueryOver<Watchbill>().Where(x => x.CurrentState.Id == ReferenceLists.ReferenceListHelper<WatchbillStatus>.Find("Open for Inputs").Id).List();

                foreach (var watchbill in watchbills)
                {
                    //We now need to register the job that will send emails every day to alert people to the inputs they are responsible for.
                    FluentScheduler.JobManager.AddJob(() => SendWatchInputRequirementsAlertEmail(watchbill.Id), s => s.WithName(watchbill.Id.ToString()).ToRunEvery(1).Days().At(4, 0));
                }
            }
        }

        /// <summary>
        /// Checks if alerts have been sent for upcoming watch assignments, and sends them if they haven't.
        /// </summary>
        private static void SendWatchAlerts()
        {
            using (var session = DataProvider.CurrentSession)
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var assignments = session.QueryOver<WatchAssignment>().Where(x => x.NumberOfAlertsSent != 2).List();

                        var hourRange = new Itenso.TimePeriod.TimeRange(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
                        var dayRange = new Itenso.TimePeriod.TimeRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

                        foreach (var assignment in assignments)
                        {
                            if (assignment.NumberOfAlertsSent == 0)
                            {
                                if (dayRange.IntersectsWith(new Itenso.TimePeriod.TimeRange(assignment.WatchShift.Range.Start)))
                                {
                                    var model = new Email.Models.UpcomingWatchEmailModel
                                    {
                                        WatchAssignment = assignment
                                    };

                                    var addresses = assignment.PersonAssigned.EmailAddresses
                                        .Where(x => x.IsPreferred)
                                        .Select(x => new System.Net.Mail.MailAddress(x.Address, assignment.PersonAssigned.ToString()));

                                    Email.EmailInterface.CCEmailMessage
                                        .CreateDefault()
                                        .To(addresses)
                                        .CC(Email.EmailInterface.CCEmailMessage.DeveloperAddress)
                                        .Subject("Upcoming Watch")
                                        .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.UpcomingWatch_HTML.html", model)
                                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                                    assignment.NumberOfAlertsSent++;

                                    session.Update(assignment);
                                }
                            }
                            else if (assignment.NumberOfAlertsSent == 1)
                            {
                                if (hourRange.IntersectsWith(new Itenso.TimePeriod.TimeRange(assignment.WatchShift.Range.Start)))
                                {
                                    var model = new Email.Models.UpcomingWatchEmailModel
                                    {
                                        WatchAssignment = assignment
                                    };

                                    var addresses = assignment.PersonAssigned.EmailAddresses
                                        .Where(x => x.IsPreferred)
                                        .Select(x => new System.Net.Mail.MailAddress(x.Address, assignment.PersonAssigned.ToString()));

                                    Email.EmailInterface.CCEmailMessage
                                        .CreateDefault()
                                        .To(addresses)
                                        .CC(Email.EmailInterface.CCEmailMessage.DeveloperAddress)
                                        .Subject("Upcoming Watch")
                                        .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.UpcomingWatch_HTML.html", model)
                                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                                    assignment.NumberOfAlertsSent++;

                                    session.Update(assignment);
                                }
                            }
                            else
                            {
                                throw new NotImplementedException("How the fuck did we get here?");
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchbillMapping : ClassMap<Watchbill>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchbillMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.CreatedBy).Not.Nullable();
                References(x => x.CurrentState).Not.Nullable();
                References(x => x.Command).Not.Nullable();
                References(x => x.LastStateChangedBy).Not.Nullable();
                References(x => x.EligibilityGroup).Not.Nullable();

                HasMany(x => x.WatchShifts).Cascade.AllDeleteOrphan();
                HasMany(x => x.InputRequirements).Cascade.AllDeleteOrphan();

                Map(x => x.Title).Not.Nullable();
                Map(x => x.LastStateChange).Not.Nullable();

                Component(x => x.Range, x =>
                {
                    x.Map(y => y.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                    x.Map(y => y.End).Not.Nullable().CustomType<UtcDateTimeType>();
                });

                Cache.IncludeAll().ReadWrite();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchbillValidator : AbstractValidator<Watchbill>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchbillValidator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(1, 50);

                RuleFor(x => x.CreatedBy).NotEmpty();
                RuleFor(x => x.CurrentState).NotEmpty();
                RuleFor(x => x.Command).NotEmpty();
                RuleFor(x => x.LastStateChange).NotEmpty();
                RuleFor(x => x.LastStateChangedBy).NotEmpty();
                RuleFor(x => x.EligibilityGroup).NotEmpty();

                RuleFor(x => x.WatchShifts).SetCollectionValidator(new WatchShift.WatchShiftValidator());
                RuleFor(x => x.InputRequirements).SetCollectionValidator(new WatchInputRequirement.WatchInputRequirementValidator());
                RuleFor(x => x.Range).Must(x => x.Start <= x.End);

#pragma warning disable CS0618 // Type or member is obsolete
                Custom(watchbill =>
                {
                    var shiftsByType = watchbill.WatchShifts.GroupBy(x => x.ShiftType);

                    List<string> errorElements = new List<string>();

                    //Make sure that none of the shifts overlap.
                    foreach (var group in shiftsByType)
                    {
                        var shifts = group.ToList();
                        foreach (var shift in shifts)
                        {
                            var shiftRange = new Itenso.TimePeriod.TimeRange(shift.Range.Start, shift.Range.End, false);
                            foreach (var otherShift in shifts.Where(x => x.Id != shift.Id))
                            {
                                var otherShiftRange = new Itenso.TimePeriod.TimeRange(otherShift.Range.Start, otherShift.Range.End, false);
                                if (shiftRange.OverlapsWith(otherShiftRange))
                                {
                                    errorElements.Add($"{group.Key} shifts: {String.Join(" ; ", otherShiftRange.ToString())}");
                                }
                            }
                        }
                    }

                    var watchbillTimeRange = new Itenso.TimePeriod.TimeRange(watchbill.Range.Start, watchbill.Range.End, true);

                    if (errorElements.Any())
                    {
                        string str = $"One or more shifts with the same type overlap:  {String.Join(" | ", errorElements)}";
                        return new FluentValidation.Results.ValidationFailure(nameof(watchbill.WatchShifts), str);
                    }

                    return null;
                });
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

    }
}
