using BankProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<ChequeBookRequest> ChequeBookRequests { get; set; }
    public DbSet<Contacts> Contacts { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Transfer> Transfers { get; set; } // Added DbSet for Transfer

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Account entity
        modelBuilder.Entity<Account>()
            .HasKey(a => a.AccountId); // Primary key is AccountId

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.AccountNumber)
            .IsUnique(); // Ensure AccountNumber is unique

        // Configure Transfer entity
        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.SenderAccount)
            .WithMany() // No collection navigation on Account for transfers
            .HasPrincipalKey(a => a.AccountNumber) // Use AccountNumber as the principal key
            .HasForeignKey(t => t.SenderAccountNumber) // Link Transfer's SenderAccountNumber to AccountNumber
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.ReceiverAccount)
            .WithMany() // No collection navigation on Account for transfers
            .HasPrincipalKey(a => a.AccountNumber) // Use AccountNumber as the principal key
            .HasForeignKey(t => t.ReceiverAccountNumber) // Link Transfer's ReceiverAccountNumber to AccountNumber
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

        // Configure Bank relationships
        modelBuilder.Entity<Bank>()
            .HasMany(b => b.Accounts)
            .WithOne(a => a.Bank)
            .HasForeignKey(a => a.BankId);

        // Configure Account relationships
        

        modelBuilder.Entity<Account>()
            .HasMany(a => a.ChequeBookRequests)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId);

        // Configure Contacts relationships
        modelBuilder.Entity<Contacts>()
            .HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Contacts>()
            .HasOne(c => c.Contact)
            .WithMany()
            .HasForeignKey(c => c.ContactUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure Messages relationships to avoid multiple cascade delete paths
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete
    }
}
