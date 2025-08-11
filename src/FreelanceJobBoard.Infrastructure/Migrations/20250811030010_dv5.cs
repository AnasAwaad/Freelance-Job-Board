using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dv5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix any existing ContractVersions that might be missing initial versions
            migrationBuilder.Sql(@"
                -- Create initial versions for contracts that don't have any versions
                INSERT INTO ContractVersions (
                    ContractId, 
                    VersionNumber, 
                    Title, 
                    Description, 
                    PaymentAmount, 
                    PaymentType, 
                    Deliverables, 
                    TermsAndConditions, 
                    AdditionalNotes, 
                    CreatedByUserId, 
                    CreatedByRole, 
                    CreatedOn, 
                    IsCurrentVersion, 
                    ChangeReason, 
                    IsActive
                )
                SELECT 
                    c.Id as ContractId,
                    1 as VersionNumber,
                    CONCAT('Initial Contract for ', COALESCE(j.Title, 'Project')) as Title,
                    COALESCE(j.Description, 'Project work as described in the original proposal') as Description,
                    c.PaymentAmount,
                    COALESCE(c.AgreedPaymentType, 'Fixed') as PaymentType,
                    COALESCE(p.CoverLetter, 'Work as agreed upon') as Deliverables,
                    'Standard terms and conditions apply' as TermsAndConditions,
                    'Initial contract version created from accepted proposal' as AdditionalNotes,
                    'system' as CreatedByUserId,
                    'System' as CreatedByRole,
                    c.CreatedOn,
                    1 as IsCurrentVersion,
                    'Initial contract creation' as ChangeReason,
                    1 as IsActive
                FROM Contracts c
                INNER JOIN Proposals p ON c.ProposalId = p.Id
                INNER JOIN Jobs j ON p.JobId = j.Id
                WHERE c.Id NOT IN (SELECT DISTINCT ContractId FROM ContractVersions WHERE ContractVersions.ContractId = c.Id)
                    AND c.IsActive = 1;

                -- Ensure each contract has exactly one current version
                UPDATE ContractVersions 
                SET IsCurrentVersion = 0 
                WHERE Id NOT IN (
                    SELECT Id FROM (
                        SELECT MAX(Id) as Id
                        FROM ContractVersions 
                        WHERE IsActive = 1
                        GROUP BY ContractId
                    ) AS latest_versions
                );

                UPDATE ContractVersions 
                SET IsCurrentVersion = 1 
                WHERE Id IN (
                    SELECT Id FROM (
                        SELECT MAX(Id) as Id
                        FROM ContractVersions 
                        WHERE IsActive = 1
                        GROUP BY ContractId
                    ) AS latest_versions
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove system-created initial versions
            migrationBuilder.Sql(@"
                DELETE FROM ContractVersions 
                WHERE CreatedByUserId = 'system' 
                    AND CreatedByRole = 'System' 
                    AND ChangeReason = 'Initial contract creation';
            ");
        }
    }
}
