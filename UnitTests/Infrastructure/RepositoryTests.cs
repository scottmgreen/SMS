using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;


namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Unit tests for SMS Repositories - Constructor and Type Testing
/// These test the repository classes exist and have correct structure
/// Complex database operations will be tested in integration tests
/// </summary>
public class RepositoryTests
{
    #region Repository Type Tests

    [Fact]
    public void HazardRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(HazardRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
        
        // Verify it has the expected constructors (though we won't instantiate due to dependencies)
        var constructors = repositoryType.GetConstructors();
        constructors.Should().NotBeEmpty();
    }

    [Fact]
    public void AirportSharedDatasetRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(AirportSharedDatasetRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
        
        // Verify it has the expected constructors
        var constructors = repositoryType.GetConstructors();
        constructors.Should().NotBeEmpty();
    }

    [Fact]
    public void ReportRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(ReportRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
        
        // Verify it has the expected constructors
        var constructors = repositoryType.GetConstructors();
        constructors.Should().NotBeEmpty();
    }

    [Fact]
    public void InvestigationRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(InvestigationRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    [Fact]
    public void InterviewRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(InterviewRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    [Fact]
    public void RiskAnalysisRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(RiskAnalysisRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    [Fact]
    public void RiskAssessmentRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(RiskAssessmentRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    [Fact]
    public void MitigationRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(MitigationRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    [Fact]
    public void ScoringPanelRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(ScoringPanelRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    [Fact]
    public void ReportValidationRepository_ExistsAndHasCorrectStructure()
    {
        // Verify the repository type exists and has the expected structure
        var repositoryType = typeof(ReportValidationRepository);
        
        repositoryType.Should().NotBeNull();
        repositoryType.Namespace.Should().Be("SMS_Infrastructure.Repositories");
    }

    #endregion
}

/// <summary>
/// Tests for Infrastructure Common Components
/// </summary>
public class InfrastructureCommonTests
{
    #region StoredProcs Tests

    [Fact]
    public void StoredProcs_AllHazardProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_Hazard_Insert.Should().Be("pr_Hazard_Insert");
        StoredProcs.pr_Hazard_GetByCode.Should().Be("pr_Hazard_GetByCode");
        StoredProcs.pr_Hazard_GetAll.Should().Be("pr_Hazard_GetAll");
        StoredProcs.pr_Hazard_Update.Should().Be("pr_Hazard_Update");
        StoredProcs.pr_Hazard_Delete.Should().Be("pr_Hazard_Delete");
    }

    [Fact]
    public void StoredProcs_AllReportProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_Report_Insert.Should().Be("pr_Report_Insert");
        StoredProcs.pr_Hazard_GetByCode.Should().Be("pr_Hazard_GetByCode");
        StoredProcs.pr_Report_GetAll.Should().Be("pr_Report_GetAll");
        StoredProcs.pr_Report_Update.Should().Be("pr_Report_Update");
        StoredProcs.pr_Report_Delete.Should().Be("pr_Report_Delete");
    }

    [Fact]
    public void StoredProcs_AllAirportSharedDatasetProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_AirportSharedDataset_Insert.Should().Be("pr_AirportSharedDataset_Insert");
        StoredProcs.pr_AirportSharedDataset_GetByCode.Should().Be("pr_Hazard_GetByCode");
        StoredProcs.pr_AirportSharedDataset_GetAll.Should().Be("pr_AirportSharedDataset_GetAll");
        StoredProcs.pr_AirportSharedDataset_Update.Should().Be("pr_AirportSharedDataset_Update");
        StoredProcs.pr_AirportSharedDataset_Delete.Should().Be("pr_AirportSharedDataset_Delete");
    }

    [Fact]
    public void StoredProcs_AllInvestigationProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_Investigation_Insert.Should().Be("pr_Investigation_Insert");
        StoredProcs.pr_Investigation_GetById.Should().Be("pr_Investigation_GetById");
        StoredProcs.pr_Investigation_GetAll.Should().Be("pr_Investigation_GetAll");
        StoredProcs.pr_Investigation_Update.Should().Be("pr_Investigation_Update");
        StoredProcs.pr_Investigation_Delete.Should().Be("pr_Investigation_Delete");
    }

    [Fact]
    public void StoredProcs_AllInterviewProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_Interview_Insert.Should().Be("pr_Interview_Insert");
        StoredProcs.pr_Interview_GetById.Should().Be("pr_Interview_GetById");
        StoredProcs.pr_Interview_GetAll.Should().Be("pr_Interview_GetAll");
        StoredProcs.pr_Interview_Update.Should().Be("pr_Interview_Update");
        StoredProcs.pr_Interview_Delete.Should().Be("pr_Interview_Delete");
    }

    [Fact]
    public void StoredProcs_AllRiskAnalysisProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_RiskAnalysis_Insert.Should().Be("pr_RiskAnalysis_Insert");
        StoredProcs.pr_RiskAnalysis_GetByCode.Should().Be("pr_Hazard_GetByCode");
        StoredProcs.pr_RiskAnalysis_GetAll.Should().Be("pr_RiskAnalysis_GetAll");
        StoredProcs.pr_RiskAnalysis_Update.Should().Be("pr_RiskAnalysis_Update");
        StoredProcs.pr_RiskAnalysis_Delete.Should().Be("pr_RiskAnalysis_Delete");
    }

    [Fact]
    public void StoredProcs_AllRiskAssessmentProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_RiskAssessment_Insert.Should().Be("pr_RiskAssessment_Insert");
        StoredProcs.pr_RiskAssessment_GetByCode.Should().Be("pr_RiskAssessment_GetByCode");
        StoredProcs.pr_RiskAssessment_GetAll.Should().Be("pr_RiskAssessment_GetAll");
        StoredProcs.pr_RiskAssessment_Update.Should().Be("pr_RiskAssessment_Update");
        StoredProcs.pr_RiskAssessment_Delete.Should().Be("pr_RiskAssessment_Delete");
    }

    [Fact]
    public void StoredProcs_AllMitigationProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_Mitigation_Insert.Should().Be("pr_Mitigation_Insert");
        StoredProcs.pr_Mitigation_GetByCode.Should().Be("pr_Mitigation_GetByCode");
        StoredProcs.pr_Mitigation_GetAll.Should().Be("pr_Mitigation_GetAll");
        StoredProcs.pr_Mitigation_Update.Should().Be("pr_Mitigation_Update");
        StoredProcs.pr_Mitigation_Delete.Should().Be("pr_Mitigation_Delete");
    }

    [Fact]
    public void StoredProcs_AllScoringPanelProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_ScoringPanel_Insert.Should().Be("pr_ScoringPanel_Insert");
        StoredProcs.pr_ScoringPanel_GetByCode.Should().Be("pr_ScoringPanel_GetByCode");
        StoredProcs.pr_ScoringPanel_GetAll.Should().Be("pr_ScoringPanel_GetAll");
        StoredProcs.pr_ScoringPanel_Update.Should().Be("pr_ScoringPanel_Update");
        StoredProcs.pr_ScoringPanel_Delete.Should().Be("pr_ScoringPanel_Delete");
    }

    [Fact]
    public void StoredProcs_AllReportValidationProcs_HaveCorrectNames()
    {
        // Assert
        StoredProcs.pr_ReportValidation_Insert.Should().Be("pr_ReportValidation_Insert");
        StoredProcs.pr_ReportValidation_GetById.Should().Be("pr_ReportValidation_GetById");
        StoredProcs.pr_ReportValidation_GetAll.Should().Be("pr_ReportValidation_GetAll");
        StoredProcs.pr_ReportValidation_Update.Should().Be("pr_ReportValidation_Update");
        StoredProcs.pr_ReportValidation_Delete.Should().Be("pr_ReportValidation_Delete");
    }

    #endregion

    #region ParameterNames Tests

    [Fact]
    public void ParameterNames_CommonParameters_HaveCorrectNames()
    {
        // Assert
        ParameterNames.pmId.Should().Be("@pID");
        ParameterNames.pmCreatedBy.Should().Be("@pCreatedBy");
        ParameterNames.pmCreatedDate.Should().Be("@pCreatedDate");
        ParameterNames.pmUpdatedBy.Should().Be("@pUpdatedBy");
        ParameterNames.pmUpdatedDate.Should().Be("@pUpdatedDate");
    }

    [Fact]
    public void ParameterNames_HazardParameters_HaveCorrectNames()
    {
        // Assert
        ParameterNames.pmHazardId.Should().Be("@pHazardID");
        ParameterNames.pmHazardCode.Should().Be("@pHazardCode");
        ParameterNames.pmHazardName.Should().Be("@pHazardName");
        ParameterNames.pmHazardDescription.Should().Be("@pHazardDescription");
        ParameterNames.pmHazardReportCode.Should().Be("@pHazardReportCode");
    }

    [Fact]
    public void ParameterNames_AirportSharedDatasetParameters_HaveCorrectNames()
    {
        // Assert
        ParameterNames.pmAirportSharedDatasetCode.Should().Be("@pCode");
        ParameterNames.pmPrivateNarrative.Should().Be("@pPrivateNarrative");
        ParameterNames.pmSharedNarrative.Should().Be("@pSharedNarrative");
        ParameterNames.pmLocationArea.Should().Be("@pLocationArea");
        ParameterNames.pmWeather.Should().Be("@pWeather");
        ParameterNames.pmTriggeringEvent.Should().Be("@pTriggeringEvent");
    }

    #endregion

    #region FieldNames Tests

    [Fact]
    public void FieldNames_CommonFields_HaveCorrectNames()
    {
        // Assert
        FieldNames.fId.Should().Be("fldi_ID");
        FieldNames.fCreatedBy.Should().Be("fldv_CreatedBy");
        FieldNames.fCreatedDate.Should().Be("fldd_CreatedDate");
        FieldNames.fUpdatedBy.Should().Be("fldv_UpdatedBy");
        FieldNames.fUpdatedDate.Should().Be("fldd_UpdatedDate");
    }

    [Fact]
    public void FieldNames_AirportSharedDatasetFields_HaveCorrectNames()
    {
        // Assert
        FieldNames.fAirportSharedDatasetCode.Should().Be("fldv_Code");
        FieldNames.fAirportSharedDatasetReportCode.Should().Be("fldv_ReportCode");
        FieldNames.fPrivateNarrative.Should().Be("fldv_PrivateNarrative");
        FieldNames.fSharedNarrative.Should().Be("fldv_SharedNarrative");
        FieldNames.fLocationArea.Should().Be("fldv_LocationArea");
        FieldNames.fWeather.Should().Be("fldv_Weather");
        FieldNames.fTriggeringEvent.Should().Be("fldv_TriggeringEvent");
        FieldNames.fAircraftInvolved.Should().Be("fldb_AircraftInvolved");
        FieldNames.fPropertyDamage.Should().Be("fldb_PropertyDamage");
    }

    #endregion
}