using System;
using FluentScheduler;
using CommandCentral.Framework.Data;
using CommandCentral.Entities;

namespace CommandCentral.CronOperations
{
    /// <summary>
    /// Contains all of the start up and cron operation registrations for the muster module.
    /// </summary>
    public class MusterRegistry : Registry
    {
        /// <summary>
        /// Initializes this registry.
        /// </summary>
        public MusterRegistry()
        {
            SetupMuster();
        }

        /// <summary>
        /// Walks through each command in the database, rolling over any muster cycle that needs it, and registering the muster cycle for rollover at its proper hour.
        /// </summary>
        private void SetupMuster()
        {
            using (var transaction = SessionManager.CurrentSession().BeginTransaction())
            {
                var commands = SessionManager.CurrentSession().QueryOver<Command>().Future();

                foreach (var command in commands)
                {
                    if (command.CurrentMusterCycle == null)
                        throw new ArgumentNullException(nameof(command.CurrentMusterCycle));

                    if (DateTime.UtcNow >= command.CurrentMusterCycle.Range.End)
                    {
                        //We need to rollover the current muster cycle.  We let the command handle its own muster rollover.
                        command.RolloverCurrentMusterCycle(null);
                    }

                    Events.EventManager.OnMusterOpened(new Events.Args.MusterCycleEventArgs
                    {
                        MusterCycle = command.CurrentMusterCycle
                    }, this);

                    SessionManager.CurrentSession().Update(command);

                    Schedule(() => DoRolloverForCommand(command.Id)).ToRunEvery(1).Days().At(command.MusterStartHour, 0);
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Rolls over the muster cycle for the given command.
        /// </summary>
        /// <param name="commandId"></param>
        private void DoRolloverForCommand(Guid commandId)
        {
            using (var transaction = SessionManager.CurrentSession().BeginTransaction())
            {
                var command = SessionManager.CurrentSession().Get<Command>(commandId);

                if (command == null)
                    throw new ArgumentNullException(nameof(commandId), $"The command identified by the id {commandId} does not exist in the database.  Occurred in the cron operation '{nameof(DoRolloverForCommand)}'.");

                if (command.CurrentMusterCycle == null)
                    throw new ArgumentNullException(nameof(command.CurrentMusterCycle));

                if (DateTime.UtcNow >= command.CurrentMusterCycle.Range.End)
                {
                    //We need to rollover the current muster cycle.  We let the command handle its own muster rollover.
                    command.RolloverCurrentMusterCycle(null);
                }

                Events.EventManager.OnMusterOpened(new Events.Args.MusterCycleEventArgs
                {
                    MusterCycle = command.CurrentMusterCycle
                }, this);

                SessionManager.CurrentSession().Update(command);

                transaction.Commit();
            }
        }
    }
}
