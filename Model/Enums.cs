using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewer.Models
{
    /// <summary>
    /// Types of email that can be sent.
    /// </summary>
    public enum MailType : int
    {
        // ChangeList
        Request = 0,
        Iteration = 1,
        Reminder = 2,
        StatusChange = 3,

        // User
        SignedOff = 4,
        SignedOffWithComments = 5,
        WaitingOnAuthor = 6,
    }

    public enum CommentStatus
    {
        Active,
        Resolved,
        WontFix,
        Closed,
        Canceled,
    }

    public enum ChangeListStatus
    {
        Active,
        Resolved,
        Closed,
        Deleted,
    }

    public enum ReviewerStatus
    {
        NotLookedAtYet,
        Looking,
        SignedOff,
        SignedOffWithComments,
        WaitingOnAuthor,
        Complete,
        Deleted,
    }

    public enum SourceControlAction
    {
        Add = 0,
        Edit = 1,
        Delete = 2,
        Branch = 3,
        Integrate = 4,
        Rename = 5
    };

}
