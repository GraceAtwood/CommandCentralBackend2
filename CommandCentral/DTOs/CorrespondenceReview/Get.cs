using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceReview
{
    public class Get
    {
        public Guid Id { get; set; }
        public Guid Reviewer { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime TimeRouted { get; set; }
        public DateTime? TimeReviewed { get; set; }
        public bool IsReviewed { get; set; }
        public bool? IsRecommended { get; set; }
        public string Body { get; set; }
        public Guid? NextReview { get; set; }
        public Guid CorrespondenceItem { get; set; }
        public bool IsFinal { get; set; }
        public Guid RoutedBy { get; set; }

        public Get(Entities.Correspondence.CorrespondenceReview item)
        {
            Id = item.Id;
            Reviewer = item.Reviewer.Id;
            ReviewedBy = item.ReviewedBy?.Id;
            IsReviewed = item.IsReviewed;
            IsRecommended = item.IsRecommended;
            Body = item.Body;
            NextReview = item.NextReview?.Id;
            CorrespondenceItem = item.CorrespondenceItem.Id;
            IsFinal = item.IsFinal;
            RoutedBy = item.RoutedBy.Id;
            TimeRouted = item.TimeRouted;
            TimeReviewed = item.TimeReviewed;
        }
    }
}
