//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SurveyForm.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class TemplateQuestion
    {
        public int TemplateQuestionId { get; set; }
        public int TemplateId { get; set; }
        public int QuestionId { get; set; }
    
        public virtual Question Question { get; set; }
        public virtual Template Template { get; set; }
    }
}