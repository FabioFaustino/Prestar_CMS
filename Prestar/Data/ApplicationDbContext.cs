using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Prestar.Models;

namespace Prestar.Data
{
    /// <summary>
    /// Application Database Context for Prestar
    /// Inherits from IdentityDbContext<User>
    /// </summary>
    /// <see cref="IdentityDbContext"/>
    /// <seealso cref="User"/>
    public class ApplicationDbContext : IdentityDbContext<User>
    {

        /// <summary>
        /// Constructor of the ApplicationDbContext class that receives one parameter
        /// and initializes him.
        /// </summary>
        /// <param name="options">Database context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// On model creating, it defines how the different entities work with each other
        /// </summary>
        /// <param name="modelBuilder">
        /// Provides a simple API surface for configuring a IMutableModel that defines the 
        /// shape of your entities, the relationships between them, and how they map to 
        /// the database.
        /// </param>
        /// <see cref="ModelBuilder"/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Handler)
                .WithMany()
                .HasForeignKey(r => r.RequestHandlerID);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Requisitioner)
                .WithMany()
                .HasForeignKey(r => r.RequisitionerID);

            modelBuilder.Entity<Service>()
                .HasOne<User>(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserID);

            modelBuilder.Entity<Service>()
                .HasOne<ServiceCategory>(s => s.ServiceCategory)
                .WithMany()
                .HasForeignKey(s => s.ServiceCategoryID);

            modelBuilder.Entity<ServiceCategory>()
                .HasMany(b => b.ServiceCategories)
                .WithOne();

            modelBuilder.Entity<ServiceCategory>()
                .Navigation(b => b.ServiceCategories)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<ServiceRequisition>()
                .HasOne<User>(sr => sr.Requisitioner)
                .WithMany()
                .HasForeignKey(sr => sr.RequisitionerID);

            modelBuilder.Entity<ServiceRequisition>()
                .HasOne<Service>(sr => sr.Service)
                .WithMany()
                .HasForeignKey(sr => sr.ServiceID);

            modelBuilder.Entity<ServiceRequisition>()
                .HasOne<Service>(sr => sr.Service)
                .WithMany()
                .HasForeignKey(sr => sr.ServiceID);

            modelBuilder.Entity<Notification>()
                .HasOne<User>(sr => sr.User)
                .WithMany()
                .HasForeignKey(sr => sr.DestinaryID);

            modelBuilder.Entity<New>()
                .HasOne<User>(n => n.Writter)
                .WithMany()
                .HasForeignKey(n => n.WriterID);

            modelBuilder.Entity<Section>()
                .HasOne(m => m.UserManual)
                .WithMany(c => c.Sections)
                .HasForeignKey(m => m.UserManualID);

            modelBuilder.Entity<Enrollment>()
                .HasOne(m => m.Formation)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(m => m.FormationID);

            modelBuilder.Entity<Enrollment>()
                .HasOne(m => m.Registered)
                .WithMany()
                .HasForeignKey(m => m.RegisteredID);

            modelBuilder.Entity<PrivacyPolicySection>()
                .HasOne(pps => pps.User)
                .WithMany()
                .HasForeignKey(pps => pps.LastUpdateUserID);

            modelBuilder.Entity<About>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.LastUpdateUserID);

            modelBuilder.Entity<TermsAndConditionsSection>()
                .HasOne(tcs => tcs.User)
                .WithMany()
                .HasForeignKey(tcs => tcs.LastUpdateUserID);

        }

        /// <summary>
        /// Database Set of Service
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Service> Service { get; set; }

        /// <summary>
        /// Database Set of ServiceCategory
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<ServiceCategory> ServiceCategory { get; set; }

        /// <summary>
        /// Database Set of Request
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Request> Request { get; set; }

        /// <summary>
        /// Database Set of Complaint
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Complaint> Complaint { get; set; }

        /// <summary>
        /// Database Set of ServiceRequisition
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<ServiceRequisition> ServiceRequisition { get; set; }

        /// <summary>
        /// Database Set of CommentAndEvaluation
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<CommentAndEvaluation> CommentAndEvaluation { get; set; }

        /// <summary>
        /// Database Set of Gamification
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Gamification> Gamification { get; set; }

        /// <summary>
        /// Database Set of Notification
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Notification> Notification { get; set; }

        /// <summary>
        /// Database Set of New
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<New> New { get; set; }

        /// <summary>
        /// Database Set of Section
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Section> Section { get; set; }

        /// <summary>
        /// Database Set of UserManual
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<UserManual> UserManual { get; set; }

        /// <summary>
        /// Database Set of Enrollment
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Enrollment> Enrollment { get; set; }

        /// <summary>
        /// Database Set of Formation
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Formation> Formation { get; set; }

        /// <summary>
        /// Database Set of PrivacyPolicySection
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<PrivacyPolicySection> PrivacyPolicySection { get; set; }

        /// <summary>
        /// Database Set of About
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<About> About { get; set; }

        /// <summary>
        /// Database Set of TermsAndConditionsSection
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<TermsAndConditionsSection> TermsAndConditionsSection { get; set; }

        /// <summary>
        /// Database Set of TermsAndConditionsSection
        /// </summary>
        /// <see cref="DbSet{TEntity}"/>
        public DbSet<Prestar.Models.ServiceImage> ServiceImage { get; set; }
    }
}
