using System;

namespace CommandCentral.DTOs.WorkOrder
{
    public class Get : Update
    {
        public Guid Id { get; set; }

        public Get(Entities.BEQ.WorkOrder workOrder)
        {
            Id = workOrder.Id;
            Body = workOrder.Body;
            Location = workOrder.Location;
            RoomLocation = workOrder.RoomLocation?.Id;
        }
    }
}