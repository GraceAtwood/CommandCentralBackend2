using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Utilities.Types
{
    /// <summary>
    /// The interface that makes an object commentable.
    /// </summary>
    public interface ICommentable
    {
        /// <summary>
        /// The comments.
        /// </summary>
        IList<Entities.Comment> Comments { get; set; }
    }
}
