using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using CommandCentral.Framework.Data;
using CommandCentral.Entities;
using CommandCentral.Entities.Muster;
using CommandCentral.Utilities.Types;

namespace CommandCentral.CronOperations
{
    public class MusterRegistry : Registry
    {
        public MusterRegistry()
        {
            SetupMuster();
        }

        private void SetupMuster()
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var commands = SessionManager.CurrentSession.QueryOver<Command>().Future();

                foreach (var command in commands)
                {
                    if (command.CurrentMusterCycle == null)
                        throw new ArgumentNullException(nameof(command.CurrentMusterCycle));

                    if (DateTime.UtcNow >= command.CurrentMusterCycle.Range.End)
                    {
                        //We need to rollover the current muster cycle.  We let the command handle its own muster rollover.
                        command.RolloverCurrentMusterCycle(null);
                    }

                    Events.EventManager.OnMusterOpened(new Events.Args.MusterOpenedEventArgs
                    {
                        MusterCycle = command.CurrentMusterCycle
                    });

                    SessionManager.CurrentSession.Update(command);

                    Schedule(() => DoRolloverForCommand(command.Id)).ToRunEvery(1).Days().At(command.MusterStartHour, 0);
                }

                transaction.Commit();
            }
        }

        private void DoRolloverForCommand(Guid commandId)
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var command = SessionManager.CurrentSession.Get<Command>(commandId);

                if (command == null)
                    throw new ArgumentNullException(nameof(commandId), $"The command identified by the id {commandId} does not exist in the database.  Occurred in the cron operation '{nameof(DoRolloverForCommand)}'.");

                if (command.CurrentMusterCycle == null)
                    throw new ArgumentNullException(nameof(command.CurrentMusterCycle));

                if (DateTime.UtcNow >= command.CurrentMusterCycle.Range.End)
                {
                    //We need to rollover the current muster cycle.  We let the command handle its own muster rollover.
                    command.RolloverCurrentMusterCycle(null);
                }

                Events.EventManager.OnMusterOpened(new Events.Args.MusterOpenedEventArgs
                {
                    MusterCycle = command.CurrentMusterCycle
                });

                SessionManager.CurrentSession.Update(command);

                transaction.Commit();
            }
        }
    }
}
