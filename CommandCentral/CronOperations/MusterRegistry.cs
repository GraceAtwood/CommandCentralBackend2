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

        }

        private void SetupMuster()
        {
            using (var transaction = SessionManager.CurrentSession)
            {
                var commands = SessionManager.CurrentSession.QueryOver<Command>().Future();

                foreach (var command in commands)
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

                        SessionManager.CurrentSession.Save(cycle);
                        SessionManager.CurrentSession.Update(command);
                    }
                    else
                    {
                        //OK, so here we have an existing muster cycle on this command.
                        //We need to check a few things because we don't know anything about it.
                        //First we need to know if it should've been rolled over, meaning the application was offline during a rollover time.
                        if (DateTime.UtcNow >= command.CurrentMusterCycle.Range.End)
                        {
                            //We need to rollover the current muster cycle.  We let the command handle its own muster rollover.
                        }

                    }

                }
            }
        }
    }
}
