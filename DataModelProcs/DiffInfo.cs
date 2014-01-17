using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeReviewerDataModel.Models;

namespace CodeReviewer.Models
{
    public class FileDiff
    {
        public FileVersion Left { get; set; }
        public FileVersion Right { get; set; }
        public string DiffHtml { get; set; }
    }

    public class Revision
    {
        public int Id { get; set; }
        public string RevisionName { get; set; }
        public List<FileDiff> FileDiffs { get; set; } 
    }

    public class DiffInfo
    {
        public ChangeList ChangeList { get; set; }
        public string DiffHtml { get; set; } // TODO: Remo
        public List<Revision> Revisions { get; set; } 
    }
}