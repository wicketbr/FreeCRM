using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CRM.EFModels.EFModels;

public partial class EFDataModel : DbContext
{
    public EFDataModel()
    {
    }

    public EFDataModel(DbContextOptions<EFDataModel> options)
        : base(options)
    {
    }

    // {{ModuleItemStart:Appointments}}
    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentNote> AppointmentNotes { get; set; }

    public virtual DbSet<AppointmentService> AppointmentServices { get; set; }

    public virtual DbSet<AppointmentUser> AppointmentUsers { get; set; }
    // {{ModuleItemEnd:Appointments}}

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentGroup> DepartmentGroups { get; set; }

    // {{ModuleItemStart:EmailTemplates}}
    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
    // {{ModuleItemEnd:EmailTemplates}}

    public virtual DbSet<FileStorage> FileStorages { get; set; }

    // {{ModuleItemStart:Invoices}}
    public virtual DbSet<Invoice> Invoices { get; set; }
    // {{ModuleItemEnd:Invoices}}

    // {{ModuleItemStart:Locations}}
    public virtual DbSet<Location> Locations { get; set; }
    // {{ModuleItemEnd:Locations}}

    // {{ModuleItemStart:Payments}}
    public virtual DbSet<Payment> Payments { get; set; }
    // {{ModuleItemEnd:Payments}}

    public virtual DbSet<PluginCache> PluginCaches { get; set; }

    // {{ModuleItemStart:Services}}
    public virtual DbSet<Service> Services { get; set; }
    // {{ModuleItemEnd:Services}}

    public virtual DbSet<Setting> Settings { get; set; }

    // {{ModuleItemStart:Tags}}
    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagItem> TagItems { get; set; }
    // {{ModuleItemEnd:Tags}}

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<UDFLabel> UDFLabels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserInGroup> UserInGroups { get; set; }

    // The OnConfiguring override is only commented out and used to build the migration scripts.
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // => optionsBuilder.UseSqlite("Data Source=C:\\Working\\CRM.db");
    // => optionsBuilder.UseMySQL("Server=localhost;Database=CRM;User=admin;Password=admin");
    // => optionsBuilder.UseNpgsql("Host=localhost;Database=CRM;Username=postgres;Password=admin");
    // => optionsBuilder.UseSqlServer("Data Source=(local);Initial Catalog=CRM;Persist Security Info=True;User ID=sa;Password=saPassword;MultipleActiveResultSets=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // {{ModuleItemStart:Appointments}}
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(e => e.AppointmentId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.BackgroundColor).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.End).HasColumnType("datetime");
            entity.Property(e => e.ForegroundColor).HasMaxLength(100);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Start).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);

            // {{ModuleItemStart:Locations}}
            entity.HasOne(d => d.Location).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK_Appointments_Locations");
            // {{ModuleItemEnd:Locations}}
        });

        modelBuilder.Entity<AppointmentNote>(entity =>
        {
            entity.Property(e => e.AppointmentNoteId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentNotes)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentNotes_Appointments");
        });

        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.Property(e => e.AppointmentServiceId).ValueGeneratedNever();
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Fee).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentServices)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentServices_Appointments");

            // {{ModuleItemStart:Services}}
            entity.HasOne(d => d.Service).WithMany(p => p.AppointmentServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentServices_Services");
            // {{ModuleItemEnd:Services}}
        });

        modelBuilder.Entity<AppointmentUser>(entity =>
        {
            entity.Property(e => e.AppointmentUserId).ValueGeneratedNever();
            entity.Property(e => e.AttendanceCode).HasMaxLength(50);
            entity.Property(e => e.Fees).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentUsers)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentUsers_Appointments");

            entity.HasOne(d => d.User).WithMany(p => p.AppointmentUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentUsers_Users");
        });
        // {{ModuleItemEnd:Appointments}}

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.DepartmentId).ValueGeneratedNever();
            entity.Property(e => e.ActiveDirectoryNames).HasMaxLength(100);
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<DepartmentGroup>(entity =>
        {
            entity.Property(e => e.DepartmentGroupId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DepartmentGroupName).HasMaxLength(200);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
        });

        // {{ModuleItemStart:EmailTemplates}}
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.Property(e => e.EmailTemplateId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(300);
        });
        // {{ModuleItemEnd:EmailTemplates}}

        modelBuilder.Entity<FileStorage>(entity =>
        {
            entity.HasKey(e => e.FileId);

            entity.ToTable("FileStorage");

            entity.HasIndex(e => e.UserId, "IX_FileStorage_UserId");

            entity.Property(e => e.FileId).ValueGeneratedNever();
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Extension).HasMaxLength(15);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.SourceFileId).HasMaxLength(100);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");
            entity.Property(e => e.UploadedBy).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.FileStorages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("IX_FileStorage_UserId");
        });

        // {{ModuleItemStart:Invoices}}
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(e => e.InvoiceId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.InvoiceClosed).HasColumnType("datetime");
            entity.Property(e => e.InvoiceCreated).HasColumnType("datetime");
            entity.Property(e => e.InvoiceDueDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.InvoiceSendDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceSent).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.PONumber).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Total).HasColumnType("decimal(19, 4)");

            // {{ModuleItemStart:Appointments}}
            entity.HasOne(d => d.Appointment).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_Invoices_Appointments");
            // {{ModuleItemEnd:Appointments}}

            entity.HasOne(d => d.Tenant).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Tenants");

            entity.HasOne(d => d.User).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Invoices_Users");
        });

        // {{ModuleItemStart:Locations}}
        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(e => e.LocationId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CalendarBackgroundColor).HasMaxLength(100);
            entity.Property(e => e.CalendarForegroundColor).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
        });
        // {{ModuleItemEnd:Locations}}
        // {{ModuleItemEnd:Invoices}}

        // {{ModuleItemStart:Payments}}
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.PaymentId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.RefundDate).HasColumnType("datetime");
            entity.Property(e => e.Refunded).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.RefundedBy).HasMaxLength(100);

            // {{ModuleItemStart:Invoices}}
            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Invoices");
            // {{ModuleItemEnd:Invoices}}

            entity.HasOne(d => d.Tenant).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Tenants");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Payments_Users");
        });
        // {{ModuleItemEnd:Payments}}

        modelBuilder.Entity<PluginCache>(entity =>
        {
            entity.HasKey(e => e.RecordId);

            entity.ToTable("PluginCache");

            entity.Property(e => e.RecordId).ValueGeneratedNever();
            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Namespace).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.Version).HasMaxLength(100);
        });

        // {{ModuleItemStart:Services}}
        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.ServiceId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Rate).HasColumnType("decimal(19, 4)");
        });
        // {{ModuleItemEnd:Services}}

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK_Settings_1");

            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.SettingName).HasMaxLength(100);
            entity.Property(e => e.SettingType).HasMaxLength(100);
        });

        // {{ModuleItemStart:Tags}}
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(e => e.TagId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<TagItem>(entity =>
        {
            entity.Property(e => e.TagItemId).ValueGeneratedNever();

            entity.HasOne(d => d.Tag).WithMany(p => p.TagItems)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TagItems_Tags");
        });
        // {{ModuleItemEnd:Tags}}

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(e => e.TenantId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TenantCode).HasMaxLength(50);
        });

        modelBuilder.Entity<UDFLabel>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Module).HasMaxLength(20);
            entity.Property(e => e.UDF).HasMaxLength(10);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId, "IX_Users_DepartmentId");

            entity.HasIndex(e => e.TenantId, "IX_Users_TenantId");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastLockoutDate).HasColumnType("datetime");
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastLoginSource).HasMaxLength(50);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UDF01).HasMaxLength(500);
            entity.Property(e => e.UDF02).HasMaxLength(500);
            entity.Property(e => e.UDF03).HasMaxLength(500);
            entity.Property(e => e.UDF04).HasMaxLength(500);
            entity.Property(e => e.UDF05).HasMaxLength(500);
            entity.Property(e => e.UDF06).HasMaxLength(500);
            entity.Property(e => e.UDF07).HasMaxLength(500);
            entity.Property(e => e.UDF08).HasMaxLength(500);
            entity.Property(e => e.UDF09).HasMaxLength(500);
            entity.Property(e => e.UDF10).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("IX_Users_DepartmentId");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IX_Users_TenantId");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.GroupId).ValueGeneratedNever();
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<UserInGroup>(entity =>
        {
            entity.HasIndex(e => e.GroupId, "IX_UserInGroups_GroupId");

            entity.HasIndex(e => e.UserId, "IX_UserInGroups_UserId");

            entity.Property(e => e.UserInGroupId).ValueGeneratedNever();

            entity.HasOne(d => d.Group).WithMany(p => p.UserInGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInGroups_UserGroups");

            entity.HasOne(d => d.User).WithMany(p => p.UserInGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInGroups_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
