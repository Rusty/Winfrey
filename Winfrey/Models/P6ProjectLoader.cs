using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Winfrey.Models.P6ProjectLoader;

namespace Winfrey.Models
{
    public class P6ProjectLoader
    {
        private static string _server { get; set; }
        private static string _cookie { get; set; }

        private static string _username { get; set; }
        private static string _password { get; set; }
        private static string _database { get; set; }
        private static string _projectId { get; set; }

        public Project LoadProject(string projectId)
        {
            _projectId = projectId;
            _server = "http://20.162.220.223:8206/p6ws/restapi";
            _username = "admin";
            _password = "Pav09061";
            _database = "PrimaVeraTestDB";

            var project = new Project();

            if (Login())
            {
                var projs = GetProject(projectId);
                var activities = GetActivities(projectId);
                var rels = GetRelationships(projectId);

                var p = projs[0];
                project.StartDate = DateTime.Parse(p.StartDate);
                project.DataDate = DateTime.Parse(p.DataDate);

                foreach (var activity in activities) 
                {
                    var t = new Task();
                    t.Id = activity.Id;
                    t.Name = activity.Name;
                    t.p6ES = DateTime.Parse(activity.EarlyStartDate);
                    t.p6EF = DateTime.Parse(activity.EarlyFinishDate);
                    t.p6LS = DateTime.Parse(activity.LateStartDate);
                    t.p6LF = DateTime.Parse(activity.LateFinishDate);
                    t.p6Critical = activity.IsCritical;
                    t.RemaningDuration = activity.RemainingDuration;

                    project.Tasks[t.Id] = t;
                }

                foreach (var r in rels)
                {
                    var link = new Link();
                    link.Lag = r.Lag;
                    link.PrecedingTaskId = r.PredecessorActivityId;
                    link.SucceedingTaskId = r.SuccessorActivityId;
                    link.Type = r.LinkType();

                    project.Links.Add(link);
                }


            }

            return project;
        }

        public bool Login()
        {
            try
            {
                
                var url = $"{_server}/login?DatabaseName={_database}";

                //string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{creds.UserName}:{creds.Password}"));
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                //httpRequest.Headers["Authorization"] = "Basic " + credentials;
                httpRequest.Headers["Username"] = _username;
                httpRequest.Headers["Password"] = _password;


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _cookie = httpResponse.Headers["Set-Cookie"];
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("An error occured");
                        Debug.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }
        public List<P6Relationship> GetRelationships(string projectId)
        {
            var data = GetUrl($"{_server}/relationship?Filter=PredecessorProjectObjectId%20IN%28{projectId}%29&Fields=Lag%2CObjectId%2CPredecessorActivityId%2CPredecessorProjectObjectId%2CSuccessorActivityId%2CSuccessorProjectId%2CType");
            var relationships = new List<P6Relationship>();
            if (data != null && data["data"] != null)
            {
                foreach (var item in data["data"])
                {
                    relationships.Add(DeserializeJSON<P6Relationship>(item.ToString()));
                }
            }
            return relationships;
        }

        public List<P6Actvity> GetActivities(string projectId)
        {
            var data = GetUrl($"{_server}/activity?Filter=ProjectObjectId%20IN%28{projectId}%29&Fields=Id%2CName%2CObjectId%2CProjectObjectId%2CActualStartDate%2CActualFinishDate%2CEarlyStartDate%2CEarlyFinishDate%2CLateStartDate%2CLateFinishDate%2cRemainingDuration");
            var activities = new List<P6Actvity>();
            if (data != null && data["data"] != null)
            {
                foreach (var item in data["data"])
                {
                    activities.Add(DeserializeJSON<P6Actvity>(item.ToString()));
                }
            }
            return activities;
        }

        public List<P6Project> GetProject(string projectId)
        {
            var data = GetUrl($"{_server}/project?Filter=ObjectId%20IN%20%28{projectId}%29&Fields=Name%2CObjectId%2CStartDate%2CDataDate%2CParentEPSObjectId");
            
             var projects = new List<P6Project>();
                if (data != null && data["data"] != null)
                {
                    foreach (var item in data["data"])
                    {
                        projects.Add(DeserializeJSON<P6Project>(item.ToString()));
                    }
                }
                return projects;
        }

        private JObject GetUrl(string url)
        {
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";
                httpRequest.Headers["Cookie"] = _cookie;

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var result_json = result.ToString();
                        if (result_json.StartsWith("[") && result_json.EndsWith("]"))
                        {
                            result_json = "{\"data\":" + result_json + "}";
                        }
                        var JSON = JObject.Parse(result_json);
                        return JSON;
                    }
                    else
                    {
                        Debug.WriteLine("An error occured");
                        Debug.WriteLine(url);
                        Debug.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get data");
                Debug.WriteLine(url);
                Debug.WriteLine(ex);
            }
            return null;
        }

        public static T DeserializeJSON<T>(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                ms.Position = 0;
                T obj = (T)serializer.ReadObject(ms);
                if (obj is IPostDeserializeAction<T>)
                    ((IPostDeserializeAction<T>)obj).OnPostDeserialization(obj);
                return obj;
            }
        }
        public interface IPostDeserializeAction<T>
        {
            void OnPostDeserialization(T model);
        }


        public class P6Project
        {/*
          * ActivityDefaultActivityType,ActivityDefaultCalendarName,ActivityDefaultCalendarObjectId,
          * ActivityDefaultCostAccountObjectId,ActivityDefaultDurationType,ActivityDefaultPercentCompleteType,
          * ActivityDefaultPricePerUnit,ActivityDefaultReviewRequired,ActivityIdBasedOnSelectedActivity,
          * ActivityIdIncrement,ActivityIdPrefix,ActivityIdSuffix,ActivityPercentCompleteBasedOnActivitySteps,
          * AddActualToRemaining,AddedBy,AllowNegativeActualUnitsFlag,AllowStatusReview,AnnualDiscountRate,
          * AnticipatedFinishDate,AnticipatedStartDate,AssignmentDefaultDrivingFlag,AssignmentDefaultRateType,
          * CalculateFloatBasedOnFinishDate,CheckOutDate,CheckOutStatus,CheckOutUserObjectId,ComputeTotalFloatType,
          * ContainsSummaryData,ContractManagementGroupName,ContractManagementProjectName,CostQuantityRecalculateFlag,
          * CreateDate,CreateUser,CriticalActivityFloatLimit,CriticalActivityFloatThreshold,CriticalActivityPathType,
          * CriticalFloatThreshold,CurrentBaselineProjectObjectId,CurrentBudget,CurrentVariance,DataDate,DateAdded,
          * DefaultPriceTimeUnits,Description,DiscountApplicationPeriod,DistributedCurrentBudget,EarnedValueComputeType,
          * EarnedValueETCComputeType,EarnedValueETCUserValue,EarnedValueUserPercent,EnablePrimeSycFlag,
          * EnablePublication,EnableSummarization,EtlInterval,FinancialPeriodTemplateId,FinishDate,FiscalYearStartMonth,
          * ForecastFinishDate,ForecastStartDate,GUID,HasFutureBucketData,HistoryInterval,HistoryLevel,Id,
          * IgnoreOtherProjectRelationships,IndependentETCLaborUnits,IndependentETCTotalCost,IntegratedType,
          * IntegratedWBS,IsTemplate,LastApplyActualsDate,LastFinancialPeriodObjectId,LastLevelDate,LastPublishedOn,
          * LastScheduleDate,LastSummarizedDate,LastUpdateDate,LastUpdateUser,Latitude,LevelAllResources,LevelDateFlag,
          * LevelFloatThresholdCount,LevelOuterAssign,LevelOuterAssignPriority,LevelOverAllocationPercent,LevelPriorityList,
          * LevelResourceList,LevelWithinFloat,LevelingPriority,LimitMultipleFloatPaths,LinkActualToActualThisPeriod,
          * LinkPercentCompleteWithActual,LinkPlannedAndAtCompletionFlag,LocationName,LocationObjectId,Longitude,
          * MakeOpenEndedActivitiesCritical,MaximumMultipleFloatPaths,MultipleFloatPathsEnabled,MultipleFloatPathsEndingActivityObjectId,
          * MultipleFloatPathsUseTotalFloat,MustFinishByDate,Name,NetPresentValue,OBSName,OBSObjectId,ObjectId,OriginalBudget,
          * OutOfSequenceScheduleType,OverallProjectScore,OwnerResourceObjectId,ParentEPSId,ParentEPSName,ParentEPSObjectId,
          * PaybackPeriod,PerformancePercentCompleteByLaborUnits,PlannedStartDate,PostResponsePessimisticFinish,
          * PostResponsePessimisticStart,PreResponsePessimisticFinish,PreResponsePessimisticStart,PricePerUnit,
          * PrimaryResourcesCanMarkActivitiesAsCompleted,PrimaryResourcesCanUpdateActivityDates,ProjectForecastStartDate,
          * ProjectScheduleType,PropertyType,ProposedBudget,PublicationPriority,PublishLevel,RelationshipLagCalendar,
          * ResetPlannedToRemainingFlag,ResourceCanBeAssignedToSameActivityMoreThanOnce,ResourceName,
          * ResourcesCanAssignThemselvesToActivities,ResourcesCanAssignThemselvesToActivitiesOutsideOBSAccess,
          * ResourcesCanEditAssignmentPercentComplete,ResourcesCanMarkAssignmentAsCompleted,ResourcesCanStaffRoleAssignment,
          * ResourcesCanViewInactiveActivities,ReturnOnInvestment,ReviewType,RiskExposure,RiskLevel,RiskMatrixName,
          * RiskMatrixObjectId,RiskScore,ScheduleWBSHierarchyType,ScheduledFinishDate,SourceProjectObjectId,StartDate,
          * StartToStartLagCalculationType,Status,StatusReviewerName,StatusReviewerObjectId,StrategicPriority,
          * SummarizeResourcesRolesByWBS,SummarizeToWBSLevel,SummarizedDataDate,SummaryAccountingVarianceByCost,
          * SummaryAccountingVarianceByLaborUnits,SummaryActivityCount,SummaryActualDuration,SummaryActualExpenseCost,
          * SummaryActualFinishDate,SummaryActualLaborCost,SummaryActualLaborUnits,SummaryActualMaterialCost,
          * SummaryActualNonLaborCost,SummaryActualNonLaborUnits,SummaryActualStartDate,SummaryActualThisPeriodCost,
          * SummaryActualThisPeriodLaborCost,SummaryActualThisPeriodLaborUnits,SummaryActualThisPeriodMaterialCost,
          * SummaryActualThisPeriodNonLaborCost,SummaryActualThisPeriodNonLaborUnits,SummaryActualTotalCost,
          * SummaryActualValueByCost,SummaryActualValueByLaborUnits,SummaryAtCompletionDuration,SummaryAtCompletionExpenseCost,
          * SummaryAtCompletionLaborCost,SummaryAtCompletionLaborUnits,SummaryAtCompletionMaterialCost,SummaryAtCompletionNonLaborCost,
          * SummaryAtCompletionNonLaborUnits,SummaryAtCompletionTotalCost,SummaryAtCompletionTotalCostVariance,
          * SummaryBaselineCompletedActivityCount,SummaryBaselineDuration,SummaryBaselineExpenseCost,SummaryBaselineFinishDate,
          * SummaryBaselineInProgressActivityCount,SummaryBaselineLaborCost,SummaryBaselineLaborUnits,SummaryBaselineMaterialCost,
          * SummaryBaselineNonLaborCost,SummaryBaselineNonLaborUnits,SummaryBaselineNotStartedActivityCount,
          * SummaryBaselineStartDate,SummaryBaselineTotalCost,SummaryBudgetAtCompletionByCost,SummaryBudgetAtCompletionByLaborUnits,
          * SummaryCompletedActivityCount,SummaryCostPercentComplete,SummaryCostPercentOfPlanned,SummaryCostPerformanceIndexByCost,
          * SummaryCostPerformanceIndexByLaborUnits,SummaryCostVarianceByCost,SummaryCostVarianceByLaborUnits,SummaryCostVarianceIndex,
          * SummaryCostVarianceIndexByCost,SummaryCostVarianceIndexByLaborUnits,SummaryDurationPercentComplete,
          * SummaryDurationPercentOfPlanned,SummaryDurationVariance,SummaryEarnedValueByCost,SummaryEarnedValueByLaborUnits,
          * SummaryEstimateAtCompletionByCost,SummaryEstimateAtCompletionByLaborUnits,SummaryEstimateAtCompletionHighPercentByLaborUnits,
          * SummaryEstimateAtCompletionLowPercentByLaborUnits,SummaryEstimateToCompleteByCost,SummaryEstimateToCompleteByLaborUnits,
          * SummaryExpenseCostPercentComplete,SummaryExpenseCostVariance,SummaryFinishDateVariance,SummaryInProgressActivityCount,
          * SummaryLaborCostPercentComplete,SummaryLaborCostVariance,SummaryLaborUnitsPercentComplete,SummaryLaborUnitsVariance,
          * SummaryLevel,SummaryMaterialCostPercentComplete,SummaryMaterialCostVariance,SummaryNonLaborCostPercentComplete,
          * SummaryNonLaborCostVariance,SummaryNonLaborUnitsPercentComplete,SummaryNonLaborUnitsVariance,SummaryNotStartedActivityCount,
          * SummaryPerformancePercentCompleteByCost,SummaryPerformancePercentCompleteByLaborUnits,SummaryPlannedCost,SummaryPlannedDuration,
          * SummaryPlannedExpenseCost,SummaryPlannedFinishDate,SummaryPlannedLaborCost,SummaryPlannedLaborUnits,SummaryPlannedMaterialCost,
          * SummaryPlannedNonLaborCost,SummaryPlannedNonLaborUnits,SummaryPlannedStartDate,SummaryPlannedValueByCost,
          * SummaryPlannedValueByLaborUnits,SummaryProgressFinishDate,SummaryRemainingDuration,SummaryRemainingExpenseCost,
          * SummaryRemainingFinishDate,SummaryRemainingLaborCost,SummaryRemainingLaborUnits,SummaryRemainingMaterialCost,
          * SummaryRemainingNonLaborCost,SummaryRemainingNonLaborUnits,SummaryRemainingStartDate,SummaryRemainingTotalCost,
          * SummarySchedulePercentComplete,SummarySchedulePercentCompleteByLaborUnits,SummarySchedulePerformanceIndexByCost,
          * SummarySchedulePerformanceIndexByLaborUnits,SummaryScheduleVarianceByCost,SummaryScheduleVarianceByLaborUnits,
          * SummaryScheduleVarianceIndex,SummaryScheduleVarianceIndexByCost,SummaryScheduleVarianceIndexByLaborUnits,
          * SummaryStartDateVariance,SummaryToCompletePerformanceIndexByCost,SummaryTotalCostVariance,SummaryTotalFloat,
          * SummaryUnitsPercentComplete,SummaryVarianceAtCompletionByLaborUnits,SyncWbsHierarchyFlag,TeamMemberActivityFields,
          * TeamMemberAddNewActualUnits,TeamMemberAssignmentOption,TeamMemberAssignmentProficiencyFlag,TeamMemberCanStatusOtherResources,
          * TeamMemberCanUpdateNotebooks,TeamMemberDisplayBaselineDatesFlag,TeamMemberDisplayPlannedUnits,TeamMemberDisplayTotalFloatFlag,
          * TeamMemberIncludePrimaryResources,TeamMemberReadOnlyActivityFields,TeamMemberResourceAssignmentFields,TeamMemberStepUDFViewableFields,
          * TeamMemberStepsAddDeletable,TeamMemberViewableFields,TotalBenefitPlan,TotalBenefitPlanTally,TotalFunding,TotalSpendingPlan,
          * TotalSpendingPlanTally,UnallocatedBudget,UndistributedCurrentVariance,UnifierCBSTasksOnlyFlag,UnifierDataMappingName,
          * UnifierDeleteActivitiesFlag,UnifierEnabledFlag,UnifierProjectName,UnifierProjectNumber,UnifierScheduleSheetName,
          * UnitPerTimeOvertimeFactor,UseExpectedFinishDates,UseProjectBaselineForEarnedValue,WBSCategoryObjectId,WBSCodeSeparator,
          * WBSHierarchyLevels,WBSMilestonePercentComplete,WBSObjectId,WebSiteRootDirectory,WebSiteURL
          * */

            public string ParentEPSObjectId { get; set; }
            public string ObjectId { get; set; }
            public string Name { get; set; }
            public string StartDate { get; set; }
            public string DataDate { get; set; }
        }

        public class P6Actvity
        {
            /*
             AccountingVariance,AccountingVarianceLaborUnits,ActivityOwnerUserId,ActualDuration,ActualExpenseCost,ActualFinishDate,
            ActualLaborCost,ActualLaborUnits,ActualMaterialCost,ActualNonLaborCost,ActualNonLaborUnits,ActualStartDate,ActualThisPeriodLaborCost,
            ActualThisPeriodLaborUnits,ActualThisPeriodMaterialCost,ActualThisPeriodNonLaborCost,ActualThisPeriodNonLaborUnits,ActualTotalCost,
            ActualTotalUnits,AtCompletionDuration,AtCompletionExpenseCost,AtCompletionLaborCost,AtCompletionLaborUnits,AtCompletionLaborUnitsVariance,
            AtCompletionMaterialCost,AtCompletionNonLaborCost,AtCompletionNonLaborUnits,AtCompletionTotalCost,AtCompletionTotalUnits,
            AtCompletionVariance,AutoComputeActuals,Baseline1Duration,Baseline1FinishDate,Baseline1PlannedDuration,Baseline1PlannedExpenseCost,
            Baseline1PlannedLaborCost,Baseline1PlannedLaborUnits,Baseline1PlannedMaterialCost,Baseline1PlannedNonLaborCost,
            Baseline1PlannedNonLaborUnits,Baseline1PlannedTotalCost,Baseline1StartDate,Baseline2Duration,Baseline2FinishDate,
            Baseline2PlannedDuration,Baseline2PlannedExpenseCost,Baseline2PlannedLaborCost,Baseline2PlannedLaborUnits,
            Baseline2PlannedMaterialCost,Baseline2PlannedNonLaborCost,Baseline2PlannedNonLaborUnits,Baseline2PlannedTotalCost,
            Baseline2StartDate,Baseline3Duration,Baseline3FinishDate,Baseline3PlannedDuration,Baseline3PlannedExpenseCost,
            Baseline3PlannedLaborCost,Baseline3PlannedLaborUnits,Baseline3PlannedMaterialCost,Baseline3PlannedNonLaborCost,
            Baseline3PlannedNonLaborUnits,Baseline3PlannedTotalCost,Baseline3StartDate,BaselineDuration,BaselineFinishDate,
            BaselinePlannedDuration,BaselinePlannedExpenseCost,BaselinePlannedLaborCost,BaselinePlannedLaborUnits,
            BaselinePlannedMaterialCost,BaselinePlannedNonLaborCost,BaselinePlannedNonLaborUnits,BaselinePlannedTotalCost,
            BaselineStartDate,BudgetAtCompletion,CBSCode,CBSId,CBSObjectId,CalendarName,CalendarObjectId,CostPercentComplete,
            CostPercentOfPlanned,CostPerformanceIndex,CostPerformanceIndexLaborUnits,CostVariance,CostVarianceIndex,
            CostVarianceIndexLaborUnits,CostVarianceLaborUnits,CreateDate,CreateUser,DataDate,Duration1Variance
            Duration2Variance,Duration3Variance,DurationPercentComplete,DurationPercentOfPlanned,DurationType,
            DurationVariance,EarlyFinishDate,EarlyStartDate,EarnedValueCost,EarnedValueLaborUnits,EstimateAtCompletionCost,
            EstimateAtCompletionLaborUnits,EstimateToComplete,EstimateToCompleteLaborUnits,EstimatedWeight,ExpectedFinishDate,
            ExpenseCost1Variance,ExpenseCost2Variance,ExpenseCost3Variance,ExpenseCostPercentComplete,ExpenseCostVariance,
            ExternalEarlyStartDate,ExternalLateFinishDate,Feedback,FinancialPeriodTmplId,FinishDate,FinishDate1Variance,
            FinishDate2Variance,FinishDate3Variance,FinishDateVariance,FloatPath,FloatPathOrder,FreeFloat,GUID,HasFutureBucketData,
            Id,IsBaseline,IsCritical,IsLongestPath,IsNewFeedback,IsStarred,IsTemplate,IsWorkPackage,LaborCost1Variance,
            LaborCost2Variance,LaborCost3Variance,LaborCostPercentComplete,LaborCostVariance,LaborUnits1Variance,LaborUnits2Variance,
            LaborUnits3Variance,LaborUnitsPercentComplete,LaborUnitsVariance,LastUpdateDate,LastUpdateUser,LateFinishDate,
            LateStartDate,LevelingPriority,LocationName,LocationObjectId,MaterialCost1Variance,MaterialCost2Variance,
            MaterialCost3Variance,MaterialCostPercentComplete,MaterialCostVariance,MaximumDuration,MinimumDuration,MostLikelyDuration,
            Name,NonLaborCost1Variance,NonLaborCost2Variance,NonLaborCost3Variance,NonLaborCostPercentComplete,NonLaborCostVariance,
            NonLaborUnits1Variance,NonLaborUnits2Variance,NonLaborUnits3Variance,NonLaborUnitsPercentComplete,NonLaborUnitsVariance,
            NotesToResources,ObjectId,OwnerIDArray,OwnerNamesArray,PercentComplete,PercentCompleteType,PerformancePercentComplete,
            PerformancePercentCompleteByLaborUnits,PhysicalPercentComplete,PlannedDuration,PlannedExpenseCost,PlannedFinishDate,
            PlannedLaborCost,PlannedLaborUnits,PlannedMaterialCost,PlannedNonLaborCost,PlannedNonLaborUnits,PlannedStartDate,
            PlannedTotalCost,PlannedTotalUnits,PlannedValueCost,PlannedValueLaborUnits,PostRespCriticalityIndex,PostResponsePessimisticFinish,
            PostResponsePessimisticStart,PreRespCriticalityIndex,PreResponsePessimisticFinish,PreResponsePessimisticStart,
            PrimaryConstraintDate,PrimaryConstraintType,PrimaryResourceId,PrimaryResourceName,PrimaryResourceObjectId,ProjectFlag,ProjectId,
            ProjectName,ProjectNameSepChar,ProjectObjectId,ProjectProjectFlag,RemainingDuration,RemainingEarlyFinishDate,
            RemainingEarlyStartDate,RemainingExpenseCost,RemainingFloat,RemainingLaborCost,RemainingLaborUnits,RemainingLateFinishDate,
            RemainingLateStartDate,RemainingMaterialCost,RemainingNonLaborCost,RemainingNonLaborUnits,RemainingTotalCost,RemainingTotalUnits,
            ResumeDate,ReviewFinishDate,ReviewRequired,ReviewStatus,SchedulePercentComplete,SchedulePerformanceIndex,
            SchedulePerformanceIndexLaborUnits,ScheduleVariance,ScheduleVarianceIndex,ScheduleVarianceIndexLaborUnits,
            ScheduleVarianceLaborUnits,ScopePercentComplete,SecondaryConstraintDate,SecondaryConstraintType,StartDate,
            StartDate1Variance,StartDate2Variance,StartDate3Variance,StartDateVariance,Status,StatusCode,SuspendDate,
            TaskStatusCompletion,TaskStatusDates,TaskStatusIndicator,ToCompletePerformanceIndex,TotalCost1Variance,TotalCost2Variance,
            TotalCost3Variance,TotalCostVariance,TotalFloat,TotalPastPeriodEarnedValueCostBCWP,TotalPastPeriodEarnedValueLaborUnits,
            TotalPastPeriodExpenseCost,TotalPastPeriodLaborCost,TotalPastPeriodLaborUnits,TotalPastPeriodMaterialCost,
            TotalPastPeriodNonLaborCost,TotalPastPeriodNonLaborUnits,TotalPastPeriodPlannedValueCost,TotalPastPeriodPlannedValueLaborUnits,
            Type,UnitsPercentComplete,UnreadCommentCount,WBSCode,WBSName,WBSNamePath,WBSObjectId,WBSPath,WorkPackageId,WorkPackageName
             */
            public string ObjectId { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string EarlyStartDate { get; set; }
            public string EarlyFinishDate { get; set; }
            public string LateStartDate { get; set; }
            public string LateFinishDate { get; set; }
            public string ActualStartDate { get; set; }
            public string ActualFinishDate { get; set; }
            public bool IsCritical { get; set; }
            public double RemainingDuration { get; set; }


        }

        public class P6Relationship
        {
            /*
            Aref,
            Arls,
                Comments,
                CreateDate,
                CreateUser,
                Driving,
                IsPredecessorBaseline,
                IsPredecessorTemplate,
                IsSuccessorBaseline,
                IsSuccessorTemplate,
                Lag,
                LastUpdateDate,
                LastUpdateUser,
                ObjectId,
                PredecessorActivityId,
                PredecessorActivityName,
                PredecessorActivityObjectId,
                PredecessorActivityType,
                PredecessorFinishDate,
                PredecessorProjectId,
                PredecessorProjectObjectId,
                PredecessorStartDate,
                PredecessorWbsName,
                SuccessorActivityId,
                SuccessorActivityName,
                SuccessorActivityObjectId,
                SuccessorActivityType,
                SuccessorFinishDate,
                SuccessorProjectId,
                SuccessorProjectObjectId,
                SuccessorStartDate,
                SuccessorWbsName,
                Type*/

            public double Lag { get; set; }
            public string ObjectId { get; set; }
            public string PredecessorActivityId { get; set; }
            public string PredecessorProjectObjectId { get; set; }
            public string SuccessorActivityId { get; set; } 
            public string SuccessorProjectId { get; set; }
            public string Type { get; set; }    

            public Link.linkType LinkType()
            {
                switch (Type) 
                {
                    case "Start to Start":
                        return Link.linkType.SS;
                    case "Finish to Finish":
                        return Link.linkType.FF;
                    case "Start to Finish":
                        return Link.linkType.SF;
                    default:
                        return Link.linkType.FS;
                }
            }

        }
              
    }
}
