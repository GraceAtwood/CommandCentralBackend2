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
                    SetupOrRolloverCurrentMusterCycle(command);
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

        private void SetupOrRolloverCurrentMusterCycle(Command command)
        {
            //If the command's current muster cycle is null, then we need to give it a new muster cycle.
            if (command.CurrentMusterCycle == null)
            {
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
            }
            else
            {
                //OK, so here we have an existing muster cycle on this command.
                //We need to check a few things because we don't know anything about it.
                //First we need to know if it should've been rolled over, meaning the application was offline during a rollover time.
                if (DateTime.UtcNow >= command.CurrentMusterCycle.Range.End)
                {
                    //We need to rollover the current muster cycle.  We let the command handle its own muster rollover.
                    command.RolloverCurrentMusterCycle(null);
                }
            }
        }

        private void DoRolloverForCommand(Guid commandId)
        {
            using (var transaction = SessionManager.CurrentSession.BeginTransaction())
            {
                var command = SessionManager.CurrentSession.Get<Command>(commandId);

                if (command == null)
                    throw new ArgumentNullException(nameof(commandId), $"The command identified by the id {commandId} does not exist in the database.  Occurred in the cron operation '{nameof(DoRolloverForCommand)}'.");

                SetupOrRolloverCurrentMusterCycle(command);
            }
        }
    }
}
