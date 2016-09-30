﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMS.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TMSEntities : DbContext
    {
        public TMSEntities()
            : base("name=TMSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<BusinessRule> BusinessRules { get; set; }
        public virtual DbSet<BusinessRuleCondition> BusinessRuleConditions { get; set; }
        public virtual DbSet<BusinessRuleNotification> BusinessRuleNotifications { get; set; }
        public virtual DbSet<BusinessRuleTrigger> BusinessRuleTriggers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Impact> Impacts { get; set; }
        public virtual DbSet<KnowledgeBase> KnowledgeBases { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<PriorityMatrixItem> PriorityMatrixItems { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }
        public virtual DbSet<TicketHistory> TicketHistories { get; set; }
        public virtual DbSet<Urgency> Urgencies { get; set; }
    }
}
