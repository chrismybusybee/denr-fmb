namespace FMB_CIS.Models
{
    public class DashboardView
    {
        public int UserActiveCount { get; set; } = 0;
        public int UserChangeRequestCount { get; set; } = 0;
        public int OfficeTypesCount { get; set; } = 0;
        public int OfficesCount { get; set; } = 0;
        public int UserTypesCount { get; set; } = 0;
        public int AccessRightsCount { get; set; } = 0;
        public int WorkflowsCount { get; set; } = 0;
        public int AnnouncementsCount { get; set; } = 0;
        public int EmailsCount { get; set; } = 0;
        public int ChecklistsCount { get; set; } = 0;
        //view for applicants (number of application applied by a certain applicant)
        public int SellerPendingCount { get; set; } = 0;
        public int SellerApprovedCount { get; set; } = 0;
        public int SellerDeclinedCount { get; set; } = 0;
        public int SellerTotalCount { get; set; } = 0;
        public int OwnerPendingCount { get; set; } = 0;
        public int OwnerApprovedCount { get; set; } = 0;
        public int OwnerDeclinedCount { get; set; } = 0;
        public int OwnerTotalCount { get; set; } = 0;
        public int ImporterPendingCount { get; set; } = 0;
        public int ImporterApprovedCount { get; set; } = 0;
        public int ImporterDeclinedCount { get; set; } = 0;
        public int ImporterTotalCount { get; set; } = 0;

        //view for officers (number of applications that an officer can see)
        public int officerViewSellerPendingCount { get; set; } = 0;
        public int officerViewSellerApprovedCount { get; set; } = 0;
        public int officerViewSellerDeclinedCount { get; set; } = 0;
        public int officerViewSellerTotalCount { get; set; } = 0;
        public int officerViewOwnerPendingCount { get; set; } = 0;
        public int officerViewOwnerApprovedCount { get; set; } = 0;
        public int officerViewOwnerDeclinedCount { get; set; } = 0;
        public int officerViewOwnerTotalCount { get; set; } = 0;
        public int officerViewImporterPendingCount { get; set; } = 0;
        public int officerViewImporterApprovedCount { get; set; } = 0;
        public int officerViewImporterDeclinedCount { get; set; } = 0;
        public int officerViewImporterTotalCount { get; set; } = 0;
    }
}
