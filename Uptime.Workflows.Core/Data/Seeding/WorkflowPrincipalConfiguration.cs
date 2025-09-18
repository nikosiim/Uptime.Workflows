using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data.Seeding;

public class WorkflowPrincipalConfiguration : IEntityTypeConfiguration<WorkflowPrincipal>
{
    public void Configure(EntityTypeBuilder<WorkflowPrincipal> builder)
    {
        builder.HasData(
            new WorkflowPrincipal { 
                Id = 1, 
                ExternalId = "S-1-5-21-10001", 
                Name = "Klient Üks", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "klient1@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 2, 
                ExternalId = "S-1-5-21-10002", 
                Name = "Klient Kaks", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "klient2@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 3, 
                ExternalId = "S-1-5-21-10003", 
                Name = "Klient Kolm", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "klient3@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 4, 
                ExternalId = "S-1-5-21-10004", 
                Name = "Klient Neli", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "klient4@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 5, 
                ExternalId = "S-1-5-21-10005", 
                Name = "Klient Viis", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "klient5@example.com" 
            },

            new WorkflowPrincipal { 
                Id = 6, 
                ExternalId = "S-1-5-21-10006", 
                Name = "Marika Oja", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "marika.oja@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 7, 
                ExternalId = "S-1-5-21-10007", 
                Name = "Jana Pärn", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "jana.parn@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 8, 
                ExternalId = "S-1-5-21-10008", 
                Name = "Piia Saar", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "piia.saar@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 9, 
                ExternalId = "S-1-5-21-10009", 
                Name = "Urve Oja", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "urve.oja@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 10, 
                ExternalId = "S-1-5-21-10010", 
                Name = "Peeter Sepp", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "peeter.sepp@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 11, 
                ExternalId = "S-1-5-21-10011", 
                Name = "Markus Lepik", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "markus.lepik@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 12, 
                ExternalId = "S-1-5-21-10012", 
                Name = "Marta Laine", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "marta.laine@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 13, 
                ExternalId = "S-1-5-21-10013", 
                Name = "Anton Rebane", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "anton.rebane@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 14, 
                ExternalId = "S-1-5-21-10014", 
                Name = "Signe Kask", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "signe.kask@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 15, 
                ExternalId = "S-1-5-21-10015", 
                Name = "Riin Koppel", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "riin.koppel@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 16, 
                ExternalId = "S-1-5-21-10016", 
                Name = "Lauri Saar", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "lauri.saar@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 17, 
                ExternalId = "S-1-5-21-10017", 
                Name = "Viljar Laine", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "viljar.laine@example.com" 
            },
            new WorkflowPrincipal { 
                Id = 18, 
                ExternalId = "S-1-5-21-10018", 
                Name = "Kristina Kroon", 
                Type = PrincipalType.User, 
                Source = "Windows", 
                Email = "kristina.kroon@example.com" 
            }
        );
    }
}