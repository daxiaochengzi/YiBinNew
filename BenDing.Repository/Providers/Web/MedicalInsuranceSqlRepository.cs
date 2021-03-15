﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BenDing.Domain.Models.Dto.Resident;
using BenDing.Domain.Models.Dto.Web;
using BenDing.Domain.Models.Enums;
using BenDing.Domain.Models.Params.OutpatientDepartment;
using BenDing.Domain.Models.Params.Resident;
using BenDing.Domain.Models.Params.UI;
using BenDing.Domain.Models.Params.Web;
using BenDing.Domain.Xml;
using BenDing.Repository.Interfaces.Web;
using Dapper;
using NFine.Code;

namespace BenDing.Repository.Providers.Web
{
    public class MedicalInsuranceSqlRepository : IMedicalInsuranceSqlRepository
    {
        private readonly string _connectionString;
        private readonly Log _log;
        private readonly ISystemManageRepository _iSystemManageRepository;
        /// <summary>
        /// 构造函数
        /// </summary>
        public MedicalInsuranceSqlRepository(ISystemManageRepository iSystemManageRepository)
        {
            _iSystemManageRepository = iSystemManageRepository;
            string conStr = ConfigurationManager.ConnectionStrings["NFineDbContext"].ToString();
            _log = LogFactory.GetLogger("ini".GetType().ToString());
            _connectionString = !string.IsNullOrWhiteSpace(conStr)
                ? conStr
                : throw new ArgumentNullException(nameof(conStr));

        }

        /// <summary>
        ///  更新医保病人结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int UpdateMedicalInsuranceResidentSettlement(UpdateMedicalInsuranceResidentSettlementParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.IsHisUpdateState)
                    {
                        strSql = $@" update MedicalInsurance set [MedicalInsuranceState]={(int)param.MedicalInsuranceState} where Id='{param.Id}' ";
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(param.CancelTransactionId))
                        {
                            strSql = $@" update MedicalInsurance set SettlementUserId='{param.UserId}',SettlementTime=NULL,
                                    SettlementCancelTime=GETDATE(),[MedicalInsuranceState]={(int)param.MedicalInsuranceState}, 
                                    SettlementCancelUserId='{param.UserId}',CancelTransactionId='{param.CancelTransactionId}',
                                    CancelSettlementRemarks='{param.CancelSettlementRemarks}',WorkersStrokeCardNo=NULL
                                    where Id='{param.Id}' ";
                        }
                        else if (!string.IsNullOrWhiteSpace(param.PreSettlementTransactionId))
                        {
                            strSql = $@" update MedicalInsurance set PreSettlementUserId='{param.UserId}',PreSettlementTime=GETDATE(),MedicalInsuranceState={(int)param.MedicalInsuranceState},
                                    OtherInfo='{param.OtherInfo}',MedicalInsuranceAllAmount={param.MedicalInsuranceAllAmount},
                                    SelfPayFeeAmount= {param.SelfPayFeeAmount},ReimbursementExpensesAmount={param.ReimbursementExpensesAmount},
                                    SettlementNo='{param.SettlementNo}',PreSettlementTransactionId='{param.PreSettlementTransactionId}'
                                    where Id='{param.Id}' ";

                        }
                        else if (!string.IsNullOrWhiteSpace(param.SettlementTransactionId))
                        {
                            strSql = $@" update MedicalInsurance set SettlementUserId='{param.UserId}',SettlementTime=GETDATE(),MedicalInsuranceState={(int)param.MedicalInsuranceState},
                                    OtherInfo='{param.OtherInfo}',MedicalInsuranceAllAmount={param.MedicalInsuranceAllAmount},CarryOver={param.CarryOver},NotStatisticsMedicalExpenses={param.NotStatisticsMedicalExpenses}
                                    SelfPayFeeAmount= {param.SelfPayFeeAmount},ReimbursementExpensesAmount={param.ReimbursementExpensesAmount},PatientId='{param.PatientId}',
                                    SettlementNo='{param.SettlementNo}',SettlementTransactionId='{param.SettlementTransactionId}',SettlementType='{param.SettlementType}'
                                    where Id='{param.Id}' ";
                        }
                        else if (!string.IsNullOrWhiteSpace(param.WorkersStrokeCardNo))
                        {
                            strSql = $@" update MedicalInsurance set WorkersStrokeCardNo='{param.WorkersStrokeCardNo}',WorkersStrokeTime=GETDATE(),MedicalInsuranceState={(int)param.MedicalInsuranceState},
                                    WorkersStrokeCardInFo='{param.WorkersStrokeCardInfo}',
                                    SelfPayFeeAmount= {param.SelfPayFeeAmount},ReimbursementExpensesAmount={param.ReimbursementExpensesAmount}
                                    where Id='{param.Id}' ";
                        }
                       
                    }

                    var data = sqlConnection.Execute(strSql);
                    sqlConnection.Close();
                    return data;
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }

            }
        }
        /// <summary>
        /// 更新刷卡结算
        /// </summary>
        /// <param name="param"></param>
        public void UpdateMedicalInsuranceCardSettlement(UpdateMedicalInsuranceCardSettlementParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql =
                        $@"update [dbo].[MedicalInsurance] set [WorkersStrokeCardInfo]='{param.WorkersStrokeCardInfo}',[WorkersStrokeCardNo]='{param.WorkersStrokeCardNo}',
                        [WorkersStrokeTime]=getDate() where BusinessId='{param.BusinessId}' and isDelete=0";

                  sqlConnection.Execute(strSql);
                    sqlConnection.Close();
                  
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }

            }
        }

        /// <summary>
        /// 医保信息保存
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public void SaveMedicalInsurance(UserInfoDto user, MedicalInsuranceDto param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.MedicalInsuranceState == MedicalInsuranceState.HisHospitalized)
                    {
                        strSql = $@"update [dbo].[MedicalInsurance] set 
                    MedicalInsuranceState={(int)param.MedicalInsuranceState}
                    where [Id]='{param.Id}'";
                    }
                    else if (param.IsModify)
                    {

                        strSql = $@"update [dbo].[MedicalInsurance] set 
                        AdmissionInfoJson='{param.AdmissionInfoJson}'
                        where [Id]='{param.Id}' and OrganizationCode='{user.OrganizationCode}'";

                    }
                    else
                    {
                        strSql = $@"INSERT INTO [dbo].[MedicalInsurance]([Id],[BusinessId],[InsuranceNo],[MedicalInsuranceAllAmount],[IdentityMark],[AfferentSign]
                               ,[AdmissionInfoJson],[ReimbursementExpensesAmount] ,[SelfPayFeeAmount],[OtherInfo],[MedicalInsuranceHospitalizationNo],[IsBirthHospital]
		                       ,[CreateTime],[IsDelete] ,OrganizationCode,CreateUserId,OrganizationName,InsuranceType,MedicalInsuranceState,SettlementUserName,CommunityName,ContactPhone,ContactAddress)
                           VALUES('{param.Id}','{param.BusinessId}','{param.InsuranceNo}', 0,'{param.IdentityMark}','{param.AfferentSign}',
                                 '{param.AdmissionInfoJson}',0,0,NULL,'{param.MedicalInsuranceHospitalizationNo}',{param.IsBirthHospital},
                                 GETDATE(),0,'{user.OrganizationCode}','{user.UserId}','{user.OrganizationName }',{param.InsuranceType},
                                  {(int)param.MedicalInsuranceState},'{user.UserName}','{param.CommunityName}','{param.ContactPhone}','{param.ContactAddress}');";
                        strSql = $"update [dbo].[MedicalInsurance] set [IsDelete]=1,DeleteUserId='{user.UserId}',DeleteTime=GETDATE() where [BusinessId]='{param.BusinessId}' and [PatientId] is null;" + strSql;

                    }

                    sqlConnection.Execute(strSql);
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }


            }
        }
        /// <summary>
        /// 医保病人信息查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public MedicalInsuranceResidentInfoDto QueryMedicalInsuranceResidentInfo(
            QueryMedicalInsuranceResidentInfoParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = @" SELECT [Id]
                              ,[BusinessId]
                              ,[InsuranceNo]
                              ,[MedicalInsuranceAllAmount]
                              ,[MedicalInsuranceHospitalizationNo]
                              ,[SelfPayFeeAmount]
                              ,[AdmissionInfoJson]
                              ,[ReimbursementExpensesAmount]
                              ,[OtherInfo]
                              ,[OrganizationCode]
                              ,[OrganizationName]
                              ,[InsuranceType]
                              ,[SettlementNo]
                              ,[MedicalInsuranceState]
                              ,[WorkersStrokeCardNo]
                              ,[IsBirthHospital]
                              ,[IdentityMark]
                              ,[AfferentSign]
                              ,[SettlementType]
                              ,[PatientId]
                            FROM [dbo].[MedicalInsurance]
                            where  IsDelete=0";
                    if (!string.IsNullOrWhiteSpace(param.DataId))
                        strSql += $" and Id='{param.DataId}'";
                    if (!string.IsNullOrWhiteSpace(param.BusinessId))
                        strSql += $" and BusinessId='{param.BusinessId}'";
                    if (!string.IsNullOrWhiteSpace(param.OrganizationCode))
                        strSql += $" and OrganizationCode='{param.OrganizationCode}'";
                    if (!string.IsNullOrWhiteSpace(param.SettlementNo))
                        strSql += $" and SettlementNo='{param.SettlementNo}'";
                    if (param.MedicalInsuranceState > 0)
                    {
                        strSql += $" and MedicalInsuranceState={param.MedicalInsuranceState}";
                    }

                    var data = sqlConnection.QueryFirstOrDefault<MedicalInsuranceResidentInfoDto>(strSql);
                    sqlConnection.Close();

                    return data;
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }

            }
        }
        /// <summary>
        /// 医保中心端项目下载
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<ResidentProjectDownloadRow>> QueryProjectDownload(QueryProjectUiParam param)
        {
            List<ResidentProjectDownloadRow> dataList;
            var resultData = new Dictionary<int, List<ResidentProjectDownloadRow>>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string executeSql = null;
                try
                {
                    sqlConnection.Open();
                    string querySql = @"
                             select [Id],[ProjectCode],[ProjectName],[ProjectCodeType],Unit,MnemonicCode,Formulation,ProjectLevel,
                             Manufacturer,QuasiFontSize,Specification,Remark,NewCodeMark,NewUpdateTime,CreateTime,[StartTime],[EndTime],
                             ZeroBlock,OneBlock,TwoBlock,ThreeBlock,FourBlock from [dbo].[MedicalInsuranceProject] 
                             where  IsDelete=0 and EffectiveSign=1";
                    string countSql = @"select count(*) from [dbo].[MedicalInsuranceProject] where  IsDelete=0  and EffectiveSign=1";
                    string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(param.ProjectCodeType))
                    {   //西药
                        if (param.ProjectCodeType == "1")
                        {
                            whereSql += $" and ProjectCodeType in('11','12')";
                        }//中药
                        if (param.ProjectCodeType == "0")
                        {
                            whereSql += $" and ProjectCodeType in('13')";
                        }//耗材
                        if (param.ProjectCodeType == "3")
                        {
                            whereSql += $" and ProjectCodeType in('41','81','91','92')";
                        }
                        //诊疗
                        if (param.ProjectCodeType == "2")
                        {
                            whereSql += $" and ProjectCodeType not in('41','81','91','92')";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(param.ProjectCode))
                    {
                        whereSql += $" and ProjectCode='{param.ProjectCode}'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.ProjectName))
                    {
                        whereSql += $"  and ProjectName like '%{param.ProjectName}%'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.Manufacturer))
                    {
                        whereSql += $"  and Manufacturer like '%{param.Manufacturer}%'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.QuasiFontSize))
                    {
                        whereSql += $"  and QuasiFontSize like '%{param.QuasiFontSize}%' ";
                    }
                    
                    if (param.Limit != 0 && param.Page > 0)
                    {
                        var skipCount = param.Limit * (param.Page - 1);
                        querySql += whereSql + " order by CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.Limit + " ROWS ONLY;";
                    }
                    executeSql = countSql + whereSql + ";" + querySql;

                    var result = sqlConnection.QueryMultiple(executeSql);

                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<ResidentProjectDownloadRow>()
                                select    new ResidentProjectDownloadRow
                                {
                                    ProjectName = t.ProjectName,
                                    ProjectCode = t.ProjectCode,
                                    Id = t.Id,
                                    MnemonicCode = t.MnemonicCode,
                                    Formulation = t.Formulation,
                                    Remark = t.Remark,
                                    Specification = t.Specification,
                                    Manufacturer = t.Manufacturer,
                                    ProjectCodeType = ((ProjectCodeType)Convert.ToInt32(t.ProjectCodeType)).ToString(),
                                    ProjectLevel = ((ProjectLevel)Convert.ToInt32(t.ProjectLevel)).ToString(),
                                    NewCodeMark = t.NewCodeMark=="1"?"是":"否",
                                    NewUpdateTime =t.NewUpdateTime,
                                    QuasiFontSize = t.QuasiFontSize,
                                    RestrictionSign = t.RestrictionSign,
                                    LimitPaymentScope = t.LimitPaymentScope,
                                    Unit = t.Unit,
                                    CreateTime=t.CreateTime,
                                    EndTime = t.EndTime,
                                    StartTime = t.StartTime,
                                    ZeroBlock= CommonHelp.ValueToFour(t.ZeroBlock),
                                    OneBlock = CommonHelp.ValueToFour(t.OneBlock),
                                    TwoBlock = CommonHelp.ValueToFour(t.TwoBlock),
                                    ThreeBlock = CommonHelp.ValueToFour(t.ThreeBlock),
                                    FourBlock = CommonHelp.ValueToFour(t.FourBlock)
                                }
                            ).ToList();
                    resultData.Add(totalPageCount, dataList);
                    sqlConnection.Close();
                    return resultData;

                }
                catch (Exception e)
                {
                    _log.Debug(executeSql);
                    throw new Exception(e.Message);
                }

            }




        }
        /// <summary>
        /// 医保项目下载
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Int32 ProjectDownload(UserInfoDto user, List<ResidentProjectDownloadRowDataRowDto> param)
        {


            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insertSql = null;
                try
                {
                    int result = 0;
                    sqlConnection.Open();
                    if (param.Any())
                    {
                        //CommonHelp.ListToStr(param.Select(c => c.ProjectCode).ToList());
                        foreach (var item in param)
                        {//判断日期格式是否正确
                            if (!string.IsNullOrWhiteSpace(item.NewUpdateTime))
                            {
                                var projectName = FilteSqlStr(item.ProjectName);
                                insertSql += $@"INSERT INTO [dbo].[MedicalInsuranceProject]
                               (id,[ProjectCode],[ProjectName] ,[ProjectCodeType] ,[ProjectLevel],[WorkersSelfPayProportion]
                               ,[Unit],[MnemonicCode] ,[Formulation],[ResidentSelfPayProportion],[RestrictionSign]
                               ,[ZeroBlock],[OneBlock],[TwoBlock],[ThreeBlock],[FourBlock],[EffectiveSign],[ResidentOutpatientSign]
                               ,[ResidentOutpatientBlock],[Manufacturer] ,[QuasiFontSize] ,[Specification],[Remark],[NewCodeMark]
                               ,[NewUpdateTime],[StartTime] ,[EndTime],[LimitPaymentScope],[CreateTime],[CreateUserId],[IsDelete]
                               )
                              VALUES('{Guid.NewGuid()}','{item.ProjectCode}','{projectName}','{item.ProjectCodeType}','{item.ProjectLevel}',{CommonHelp.ValueToDecimal(item.WorkersSelfPayProportion)}
                                      ,'{item.Unit}','{item.MnemonicCode}', '{item.Formulation}',{CommonHelp.ValueToDecimal(item.ResidentSelfPayProportion)},'{item.RestrictionSign}'
                                      ,{CommonHelp.ValueToDecimal(item.ZeroBlock)},{CommonHelp.ValueToDecimal(item.OneBlock)},{CommonHelp.ValueToDecimal(item.TwoBlock)},{CommonHelp.ValueToDecimal(item.ThreeBlock)},{CommonHelp.ValueToDecimal(item.FourBlock)},'{item.EffectiveSign}','{item.ResidentOutpatientSign}'
                                      ,{CommonHelp.ValueToDecimal(item.ResidentOutpatientBlock)},'{item.Manufacturer}','{item.QuasiFontSize}','{item.Specification}','{item.Remark}','{item.NewCodeMark}'
                                      ,'{ DateTime.ParseExact(item.NewUpdateTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss")}',
                                      NULL,NULL,'{item.LimitPaymentScope}',GETDATE(),'{user.UserId}',0
                                   );";
                            }


                        }
                        result = sqlConnection.Execute(insertSql);
                        sqlConnection.Close();

                    }
                    return result;
                }
                catch (Exception e)
                {
                    _log.Debug(insertSql);
                    throw new Exception(e.Message);
                }


            }


        }
        /// <summary>
        /// 医保药品导入
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Int64 DrugCatalogImportExcel(DataTable dt, string userId)
        {
            _log.Debug(dt.Rows.Count);

           var drugCatalogData = new List<ResidentProjectDownloadRowDataRowDto>();
            int totalNum = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrWhiteSpace(CommonHelp.FilterSqlStr(dr["项目编码"].ToString())))
                {
                    //string endTime = dr["终止日期"].ToString();
                    //if (!string.IsNullOrWhiteSpace(endTime))
                    //{
                    //    if (Convert.ToDateTime(endTime) < DateTime.Now.AddMonths(-1))
                    //    {
                    //        effectiveSign = "0";
                    //    }
                    //}

                    //var item = new ResidentProjectDownloadRowDataRowDto
                    //{
                    //    EffectiveSign ="1",
                    //    RestrictionSign= dr["限制药标志"].ToString()=="1"?"1":"0",
                    //    ProjectCode = CommonHelp.FilterSqlStr(dr["本位码"].ToString()),
                    //    ProjectName = CommonHelp.FilterSqlStr(dr["项目名称"].ToString()),
                    //    StartTime = dr["开始日期"].ToString(),
                    //    EndTime = dr["结束日期"].ToString(),
                    //    ProjectLevel = dr["项目等级"].ToString(),
                    //    WorkersSelfPayProportion = CommonHelp.getNum(dr["职工自付比例"].ToString()),
                    //    ResidentSelfPayProportion = CommonHelp.getNum(dr["居民自付比例"].ToString()),
                    //    Formulation = dr["剂型"].ToString(),
                    //    Specification = dr["规格"].ToString(),
                    //    Manufacturer = dr["生产厂家"].ToString(),
                    //    LimitPaymentScope = CommonHelp.FilterSqlStr(dr["限制支付范围"].ToString()),
                    //    ZeroBlock = CommonHelp.getNum(dr["二乙以下限价"].ToString()),
                    //     OneBlock = CommonHelp.getNum(dr["二级乙等限价"].ToString()),
                    //     TwoBlock = CommonHelp.getNum(dr["二级甲等限价"].ToString()),
                    //     ThreeBlock = CommonHelp.getNum(dr["三级乙等限价"].ToString()),
                    //     FourBlock =CommonHelp.getNum(dr["三级甲等限价"].ToString()),
                    //     NewCodeMark = dr["是否新码"].ToString()== "新码"?"1":"0",
                    //     MnemonicCode = CommonHelp.FilterSqlStr(dr["拼音助记码"].ToString()),
                    //     QuasiFontSize = CommonHelp.FilterSqlStr(dr["批准文号"].ToString()),
                    //     ProjectCodeType = CommonHelp.FilterSqlStr(dr["医保分类"].ToString()),
                    //     Unit = CommonHelp.FilterSqlStr(dr["单位"].ToString()),
                    //     ProjectBigType=CommonHelp.FilterSqlStr(dr["目录大类"].ToString()),// 1 药品 2 诊疗 3 材料 4 其他
                    //     NewUpdateTime= dr["更新日期"].ToString(),
                    //     Remark = dr["备注"].ToString(),

                    //};

                    var item = new ResidentProjectDownloadRowDataRowDto
                    {
                    
                        ProjectCode = CommonHelp.FilterSqlStr(dr["项目编码"].ToString()),
                        ProjectName = CommonHelp.FilterSqlStr(dr["项目名称"].ToString()),
                        ProjectBigType = CommonHelp.FilterSqlStr(dr["目录大类"].ToString()),// 1 药品 2 诊疗 3 材料 4 其他
                        ProjectLevel = dr["项目等级"].ToString(),
                        ProjectCodeType = CommonHelp.FilterSqlStr(dr["医保分类"].ToString()),
                        WorkersSelfPayProportion = CommonHelp.getNum(dr["职工自付比例"].ToString()),
                        ResidentSelfPayProportion = CommonHelp.getNum(dr["居民自付比例"].ToString()),
                        Unit = CommonHelp.FilterSqlStr(dr["单位"].ToString()),
                        Specification = dr["规格"].ToString(),
                        Formulation = dr["剂型"].ToString(),
                        QuasiFontSize = CommonHelp.FilterSqlStr(dr["批准文号"].ToString()),
                        RestrictionSign = dr["限制药标志"].ToString() == "1" ? "1" : "0",
                        LimitPaymentScope = CommonHelp.FilterSqlStr(dr["限制支付范围"].ToString()),
                        MnemonicCode = CommonHelp.FilterSqlStr(dr["拼音助记码"].ToString()),
                        ZeroBlock = CommonHelp.getNum(dr["二乙以下限价"].ToString()),
                        OneBlock = CommonHelp.getNum(dr["二级乙等限价"].ToString()),
                        TwoBlock = CommonHelp.getNum(dr["二级甲等限价"].ToString()),
                        ThreeBlock = CommonHelp.getNum(dr["三级乙等限价"].ToString()),
                        FourBlock = CommonHelp.getNum(dr["三级甲等限价"].ToString()),
                        Manufacturer = dr["生产厂家"].ToString(),
                        NewCodeMark = dr["是否新码"].ToString() == "新码" ? "1" : "0",
                        EffectiveSign = dr["有效标志"].ToString(),
                        StartTime = dr["开始日期"].ToString(),
                        EndTime = dr["结束日期"].ToString(),
                        NewUpdateTime = dr["更新日期"].ToString(),
                        Remark = dr["备注"].ToString(),
                    };

                    drugCatalogData.Add(item);

                }

                if (drugCatalogData.Count() >= 300)
                {
                    SaveDrugCatalog(drugCatalogData, userId);
                    totalNum += drugCatalogData.Count();
                    drugCatalogData = new List<ResidentProjectDownloadRowDataRowDto>();
                }
            }
            //执行剩余的数据
            if (drugCatalogData.Any())
            {
                SaveDrugCatalog(drugCatalogData, userId);
                totalNum += drugCatalogData.Count();
            }
         

            return totalNum;
        }
        private void SaveDrugCatalog(List<ResidentProjectDownloadRowDataRowDto> param, string userId)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insterSql = null;
                string insterCount = null;
                try
                {
                    sqlConnection.Open();
                    if (param.Any())
                    {

                        foreach (var item in param)
                        {
                          
                            var projectName = FilteSqlStr(item.ProjectName);
                            insterSql = $@"INSERT INTO [dbo].[MedicalInsuranceProjectBak]
                               (id,[ProjectCode],[ProjectName] ,[ProjectCodeType] ,[ProjectLevel],[WorkersSelfPayProportion]
                               ,[Unit],[MnemonicCode] ,[Formulation],[ResidentSelfPayProportion],[RestrictionSign]
                               ,[ZeroBlock],[OneBlock],[TwoBlock],[ThreeBlock],[FourBlock],[EffectiveSign],[ResidentOutpatientSign]
                               ,[ResidentOutpatientBlock],[Manufacturer] ,[QuasiFontSize] ,[Specification],[Remark],[NewCodeMark]
                               ,[NewUpdateTime],[StartTime] ,[EndTime],[LimitPaymentScope],[CreateTime],[CreateUserId],[IsDelete],[ProjectBigType]
                               )
                              VALUES('{Guid.NewGuid()}','{item.ProjectCode}','{projectName}','{item.ProjectCodeType}','{item.ProjectLevel}',{CommonHelp.ValueToDecimal(item.WorkersSelfPayProportion)}
                                      ,'{item.Unit}','{item.MnemonicCode}', '{item.Formulation}',{CommonHelp.ValueToDecimal(item.ResidentSelfPayProportion)},'{item.RestrictionSign}'
                                      ,{CommonHelp.ValueToDecimal(item.ZeroBlock)},{CommonHelp.ValueToDecimal(item.OneBlock)},{CommonHelp.ValueToDecimal(item.TwoBlock)},{CommonHelp.ValueToDecimal(item.ThreeBlock)},{CommonHelp.ValueToDecimal(item.FourBlock)},'{item.EffectiveSign}','{item.ResidentOutpatientSign}'
                                      ,{CommonHelp.ValueToDecimal(item.ResidentOutpatientBlock)},'{item.Manufacturer}','{item.QuasiFontSize}','{item.Specification}','{item.Remark}','{item.NewCodeMark}'
                                      ,'{item.NewUpdateTime}','{item.StartTime}','{item.EndTime}','{item.LimitPaymentScope}',GETDATE(),'{userId}',0,'{item.ProjectBigType}'
                                   );";
                            insterCount += insterSql;
                        }
                        sqlConnection.Execute(insterCount);
                        sqlConnection.Close();
                    }
                }
                catch (Exception e)
                {
                    _log.Debug(insterCount);
                    _log.Error(e.Message);
                    throw new Exception(e.Message);
                }



            }
        }
        /// <summary>
        /// 获取更新最新时间
        /// </summary>
        /// <returns></returns>
        public string ProjectDownloadTimeMax()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                string insertSql = "select max(NewUpdateTime) from [dbo].[MedicalInsuranceProject]";
                string result = sqlConnection.QueryFirstOrDefault(insertSql);
                sqlConnection.Close();
                return result;
            }
        }
        /// <summary>
        /// 医保对码
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public void MedicalInsurancePairCode(MedicalInsurancePairCodesUiParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insertSql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.PairCodeList.Any())
                    {
                        string updateSql = "";
                        var directoryCodeList = param.PairCodeList.Select(d => d.DirectoryCode.ToString()).ToList();
                        var updateDirectoryCode = CommonHelp.ListToStr(directoryCodeList);
                        //更新对码
                        if (directoryCodeList.Any())
                        {
                            updateSql += $@"update [dbo].[ThreeCataloguePairCode] set [IsDelete]=1,
                             [DeleteUserId]='{param.UserId}',DeleteTime=GETDATE() where [DirectoryCode] in ({updateDirectoryCode})
                              and OrganizationCode='{param.OrganizationCode}' and [IsDelete]=0";
                            sqlConnection.Execute(updateSql);

                        }
                        var insertParam = param.PairCodeList.ToList();
                        //新增对码
                        if (insertParam.Any())
                        {
                            foreach (var items in insertParam)
                            {
                                insertSql += $@"insert into [dbo].[ThreeCataloguePairCode]
                                           ([Id],[OrganizationCode],[OrganizationName],[DirectoryCategoryCode],[State],[ProjectCode],
                                            [FixedEncoding],[CreateTime],[IsDelete],[CreateUserId],[UploadState],[DirectoryCode]) values (
                                           '{Guid.NewGuid()}','{param.OrganizationCode}','{param.OrganizationName}',
                                           (select top 1 [DirectoryCategoryCode] from [dbo].[HospitalThreeCatalogue]
                                            where [DirectoryCode]='{items.DirectoryCode}' and [IsDelete]=0 and [OrganizationCode]='{param.OrganizationCode}'),0,'{items.ProjectCode}',
                                           '{BitConverter.ToInt64(Guid.Parse(items.DirectoryCode).ToByteArray(), 0)}',GETDATE(),0,'{param.UserId}',0,'{items.DirectoryCode}')";

                            }
                            sqlConnection.Execute(insertSql);
                            sqlConnection.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Debug(insertSql);
                    throw new Exception(e.Message);
                }
            }
        }
        /// <summary>
        /// 医保对码查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<QueryMedicalInsurancePairCodeDto> QueryMedicalInsurancePairCode(
            QueryMedicalInsurancePairCodeParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string sqlStr = null;
                try
                {
                    var resultData = new List<QueryMedicalInsurancePairCodeDto>();
                    sqlConnection.Open();
                    if (param.DirectoryCodeList.Any())
                    {
                        var updateId = CommonHelp.ListToStr(param.DirectoryCodeList);
                        sqlStr = $@"
                            select a.FixedEncoding,a.DirectoryCode,b.ProjectCode,b.ProjectCodeType,
                            b.ProjectName,b.ProjectLevel,b.Formulation,b.RestrictionSign,b.Specification,b.Unit,a.DirectoryCategoryCode,
                            b.OneBlock,b.TwoBlock,b.ThreeBlock,b.FourBlock,b.Manufacturer,b.LimitPaymentScope,b.NewCodeMark,
                            b.ResidentOutpatientBlock,b.ResidentOutpatientSign,b.ResidentSelfPayProportion,b.WorkersSelfPayProportion
                            from [dbo].[ThreeCataloguePairCode] as a
                            inner join [dbo].[MedicalInsuranceProject] as b on b.ProjectCode=a.ProjectCode
                            where a.OrganizationCode='{param.OrganizationCode}' and a.[DirectoryCode] in({updateId})
                             and a.IsDelete=0  and b.IsDelete=0";// and b.EffectiveSign=1 
                        resultData = sqlConnection.Query<QueryMedicalInsurancePairCodeDto>(sqlStr).ToList();
                        sqlConnection.Close();
                    }
                    return resultData;
                }
                catch (Exception e)
                {
                    _log.Debug(sqlStr);
                    throw new Exception(e.Message);
                }

            }
        }
    
      

        /// <summary>
        /// 目录对照中心
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<DirectoryComparisonManagementDto>> DirectoryComparisonManagement(
            DirectoryComparisonManagementUiParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string executeSql = null;
                try
                {
                    sqlConnection.Open();
                    string querySql = "";
                    string countSql = "";
                    string whereSql = "";
                    var resultData = new Dictionary<int, List<DirectoryComparisonManagementDto>>();
                    if (param.State == 1)
                    {
                        whereSql = $@" where  a.OrganizationCode ='{param.OrganizationCode}' and not exists(select b.FixedEncoding from  [dbo].[ThreeCataloguePairCode] as b 
                                where b.OrganizationCode='{param.OrganizationCode}' and b.FixedEncoding=a.FixedEncoding and b.IsDelete=0 )";
                        querySql = @"select  a.[DirectoryCode],a.[DirectoryName],a.[MnemonicCode],a.[DirectoryCategoryCode],
                                a.[DirectoryCategoryName],a.[Unit],a.[Formulation],a.[Specification],a.ManufacturerName,a.FixedEncoding
                                 from [dbo].[HospitalThreeCatalogue]  as a ";
                        countSql = @"select COUNT(*) from [dbo].[HospitalThreeCatalogue] as a  ";
                    }
                    else if (param.State == 2)
                    {
                        querySql =
                            $@"select b.Id, a.[DirectoryCode],a.[DirectoryName],a.[MnemonicCode],a.[DirectoryCategoryCode],c.[Manufacturer],
                             a.[DirectoryCategoryName],a.[Unit],a.[Formulation],a.[Specification],a.ManufacturerName,a.FixedEncoding,b.CreateUserId as PairCodeUser ,
                             c.ProjectCode,c.ProjectName,c.QuasiFontSize,c.LimitPaymentScope,b.CreateTime as PairCodeTime,c.ProjectLevel,c.ProjectCodeType
                             from [dbo].[HospitalThreeCatalogue]  as a  join  [dbo].[ThreeCataloguePairCode] as b
                             on b.[FixedEncoding]=a.FixedEncoding join [dbo].[MedicalInsuranceProject] as c
                             on b.ProjectCode=c.ProjectCode
                             where b.OrganizationCode ='{param.OrganizationCode}' and b.IsDelete=0 and a.OrganizationCode ='{param.OrganizationCode}'";
                        countSql = $@"select COUNT(*)
                             from [dbo].[HospitalThreeCatalogue]  as a  join  [dbo].[ThreeCataloguePairCode] as b
                             on b.[FixedEncoding]=a.FixedEncoding join [dbo].[MedicalInsuranceProject] as c
                             on b.ProjectCode=c.ProjectCode
                             where b.OrganizationCode ='{param.OrganizationCode}' and b.IsDelete=0 and a.OrganizationCode ='{param.OrganizationCode}'";
                    }
                    else if (param.State == 0)
                    {
                        querySql =
                            $@"select b.Id, a.[DirectoryCode],a.[DirectoryName],a.[MnemonicCode],a.[DirectoryCategoryCode],c.[Manufacturer],
                            a.[DirectoryCategoryName],a.[Unit],a.[Formulation],a.[Specification],a.ManufacturerName,a.FixedEncoding,b.CreateUserId as PairCodeUser ,
                            c.ProjectCode,c.ProjectName,c.QuasiFontSize,c.LimitPaymentScope,b.CreateTime as PairCodeTime,c.ProjectLevel,c.ProjectCodeType
                             from [dbo].[HospitalThreeCatalogue]  as a  left join (select * from [dbo].[ThreeCataloguePairCode] where OrganizationCode ='{param.OrganizationCode}'  and IsDelete=0) as b
                             on b.[FixedEncoding]=a.FixedEncoding left join [dbo].[MedicalInsuranceProject] as c
                             on b.ProjectCode=c.ProjectCode
                             where a.IsDelete=0 and  a.OrganizationCode ='{param.OrganizationCode}' ";
                        countSql = $@"select COUNT(*)
                              from [dbo].[HospitalThreeCatalogue]  as a  left join  (select * from [dbo].[ThreeCataloguePairCode] where OrganizationCode ='{param.OrganizationCode}'  and IsDelete=0) as b
                             on b.[FixedEncoding]=a.FixedEncoding left join [dbo].[MedicalInsuranceProject] as c
                             on b.ProjectCode=c.ProjectCode
                             where a.IsDelete=0 and a.OrganizationCode ='{param.OrganizationCode}'";
                    }

                    if (!string.IsNullOrWhiteSpace(param.DirectoryCode))
                    {
                        whereSql += $" and a.DirectoryCode='{param.DirectoryCode}'";
                    }

                    if (!string.IsNullOrWhiteSpace(param.DirectoryCategoryCode))
                    {
                        whereSql += $" and a.DirectoryCategoryCode='{param.DirectoryCategoryCode}'";
                    }

                    if (!string.IsNullOrWhiteSpace(param.DirectoryName))
                    {
                        whereSql += $"  and a.DirectoryName like '%{ param.DirectoryName}%'";
                    }

                    if (param.Limit != 0 && param.Page > 0)
                    {
                        var skipCount = param.Limit * (param.Page - 1);
                        querySql += whereSql + " order by a.CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " +
                                    param.Limit + " ROWS ONLY;";
                    }
                    executeSql = countSql + whereSql + ";" + querySql;
                    var result = sqlConnection.QueryMultiple(executeSql);
                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    var hospitalOperatorAll = _iSystemManageRepository.QueryHospitalOperatorAll();
                    var dataList = (from t in result.Read<DirectoryComparisonManagementDto>()
                                    select new DirectoryComparisonManagementDto
                                    {
                                        Id = t.Id,
                                        DirectoryCode = t.DirectoryCode,
                                        DirectoryCategoryName = t.DirectoryCategoryName,
                                        DirectoryName = t.DirectoryName,
                                        FixedEncoding = t.FixedEncoding,
                                        Formulation = t.Formulation,
                                        LimitPaymentScope = t.LimitPaymentScope,
                                        ManufacturerName = t.ManufacturerName,
                                        MnemonicCode = t.MnemonicCode,
                                        PairCodeTime = t.PairCodeTime,
                                        ProjectName = t.ProjectName,
                                        ProjectCode = t.ProjectCode,
                                        Manufacturer = t.Manufacturer,
                                        ProjectLevel = t.ProjectLevel != null
                                            ? ((ProjectLevel)Convert.ToInt32(t.ProjectLevel)).ToString()
                                            : t.ProjectLevel,
                                        ProjectCodeType = t.ProjectCodeType != null
                                            ? ((ProjectCodeType)Convert.ToInt32(t.ProjectCodeType)).ToString()
                                            : t.ProjectCodeType,
                                        QuasiFontSize = t.QuasiFontSize,
                                        Unit = t.Unit,
                                        Specification = t.Specification,
                                        Remark = t.Remark,
                                        PairCodeUser = t.PairCodeUser != null
                                            ? hospitalOperatorAll.Where(c => c.HisUserId == t.PairCodeUser).Select(d => d.HisUserName)
                                                .FirstOrDefault()
                                            : t.PairCodeUser,
                                    }).ToList();
                    resultData.Add(totalPageCount, dataList);
                    sqlConnection.Close();
                    return resultData;
                }
                catch (Exception e)
                {
                    _log.Debug(executeSql);
                    throw new Exception(e.Message);
                }
            }
        }
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="user"></param>
        public void HospitalThreeCatalogBatchUpload(UserInfoDto user)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    querySql = $@"select  id,DirectoryCode from [dbo].[HospitalThreeCatalogue] 
                               where IsDelete=0 ";
                   
                    var data = sqlConnection.Query<HospitalThreeCatalogBatchUploadDto>(querySql).ToList();

                    if (data.Any())
                    {
                        var dataNew = data.Select(c => new HospitalThreeCatalogBatchUploadDto
                        {
                            Id = c.Id,
                            DirectoryCode = c.DirectoryCode,
                            FixedEncoding = BitConverter.ToInt64(Guid.Parse(c.DirectoryCode).ToByteArray(), 0).ToString()
                        }).ToList();

                        var sql = $@"update [dbo].[HospitalThreeCatalogue] set FixedEncoding=@FixedEncoding 
                         where  id=@Id;";
                        var res = sqlConnection.Execute(sql, dataNew);
                        sql =
                            $@"update [dbo].[ThreeCataloguePairCode] SET  UploadState=1, UpdateUserId='{user.UserId}', UpdateTime=GETDATE(),
                           FixedEncoding = b.FixedEncoding FROM [dbo].[ThreeCataloguePairCode] as a , [dbo].[HospitalThreeCatalogue]  as b
                          WHERE a.DirectoryCode = B.DirectoryCode and OrganizationCode='{user.OrganizationCode}'";
                        sqlConnection.Execute(sql);

                    }
                  

                    sqlConnection.Close();
                  
                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }
            }
        }

        /// <summary>
        /// 添加对码还未上传基层
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<QueryThreeCataloguePairCodeUploadDto> ThreeCataloguePairCodeUpload(UpdateThreeCataloguePairCodeUploadParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.ProjectCodeList!=null && param.ProjectCodeList.Any())
                    {
                        var projectCodeList = CommonHelp.ListToStr(param.ProjectCodeList);

                        querySql = $@"select  a.[Id],a.[ProjectCode],a.[ProjectName],[ProjectCodeType],ProjectLevel,
                                    [RestrictionSign],[Remark],b.DirectoryCode,[DirectoryCategoryCode]
                                    from [dbo].[MedicalInsuranceProject] as a inner join [dbo].[ThreeCataloguePairCode] as b
                                    on a.ProjectCode=b.ProjectCode where b.UploadState=0 and a.ProjectCode in({projectCodeList})
                                    and a.IsDelete=0 and b.IsDelete=0 and OrganizationCode='{param.User.OrganizationCode}'";
                    }
                    else
                    {
                        querySql = $@"select a.[Id],a.[ProjectCode],a.[ProjectName],[ProjectCodeType],ProjectLevel,
                                    [RestrictionSign],[Remark],b.DirectoryCode,[DirectoryCategoryCode]
                                    from [dbo].[MedicalInsuranceProject] as a inner join [dbo].[ThreeCataloguePairCode] as b
                                    on a.ProjectCode=b.ProjectCode where b.UploadState=0 
                                    and a.IsDelete=0 and b.IsDelete=0 and OrganizationCode='{param.User.OrganizationCode}'";
                    }
                    var data = sqlConnection.Query<QueryThreeCataloguePairCodeUploadDto>(querySql).ToList();
                    sqlConnection.Close();
                    return data;
                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }
            }
        }


        /// <summary>
        /// his上传更新数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int UpdateThreeCataloguePairCodeUpload(UpdateThreeCataloguePairCodeUploadParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.ProjectCodeList.Any())
                    {
                        var projectCodeId = CommonHelp.ListToStr(param.ProjectCodeList);
                        querySql = $@"update [dbo].[ThreeCataloguePairCode] set UploadState=1 where  ProjectCode in({projectCodeId}) and
                                 UploadState=0 and OrganizationCode='{param.User.OrganizationCode}' and IsDelete=0";
                    }
                    else
                    {
                        querySql = $@"update [dbo].[ThreeCataloguePairCode] set UploadState=1 where 
                                 UploadState=0 and OrganizationCode='{param.User.OrganizationCode}' and IsDelete=0";
                    }
                    var resultData = sqlConnection.Execute(querySql);
                    sqlConnection.Close();
                    return resultData;
                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }
            }
        }
        /// <summary>
        /// 处方上传数据更新
        /// </summary>
        /// <param name="param"></param>
        /// <param name="batchConfirmFail"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int UpdateHospitalizationFee(List<UpdateHospitalizationFeeParam> param, bool batchConfirmFail, UserInfoDto user)
        {
            int resultData = 0;
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.Any())
                    {
                        if (batchConfirmFail == false)
                        {
                            foreach (var item in param)
                            {
                                querySql +=
                                    $@"update [dbo].[HospitalizationFee] set [UploadMark]=1,BatchNumber='{item.BatchNumber}',UploadAmount={item.UploadAmount},
                               TransactionId='{item.TransactionId}',UploadUserId='{user.UserId}',UploadUserName='{user.UserName}',UploadTime=GETDATE()
                               where id='{item.Id}' and IsDelete=0;";
                            }
                        }
                        else //更新批次确认失败
                        {
                            var updateId = CommonHelp.ListToStr(param.Select(c => c.Id.ToString()).ToList());
                            querySql = $"update [dbo].[HospitalizationFee] set [UploadMark]=0 where id in ({updateId});";

                        }
                        resultData = sqlConnection.Execute(querySql);
                        sqlConnection.Close();
                    }
                    return resultData;
                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }


            }


        }
        /// <summary>
        /// 不传医保
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cancel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int NotUploadMark(List<string> param, bool cancel, UserInfoDto user)
        {
            int resultData = 0;
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    if (param.Any())
                    {
                        var updateId = CommonHelp.ListToStr(param);
                        if (cancel == false)
                        {
                            querySql +=
                                $@"update [dbo].[HospitalizationFee] set [UploadMark]=1,[NotUploadMark]=1,
                                UploadUserId='{user.UserId}',UploadUserName='{user.UserName}',UploadTime=GETDATE()
                               where id in ({updateId}) and IsDelete=0;";
                        }
                        else 
                        {
                            //var updateId = CommonHelp.ListToStr(param.Select(c => c.Id.ToString()).ToList());
                            querySql = $"update [dbo].[HospitalizationFee] set [UploadMark]=0,[NotUploadMark]=NULL where id in ({updateId});";

                        }
                        resultData = sqlConnection.Execute(querySql);
                        sqlConnection.Close();
                    }
                    return resultData;
                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }


            }


        }
        public string FilteSqlStr(string str)
        {

            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            str = str.Replace("&", "&amp");
            str = str.Replace("<", "&lt");
            str = str.Replace(">", "&gt");
            str = str.Replace("delete", "");
            str = str.Replace("update", "");
            str = str.Replace("insert", "");

            return str;
        }
    }
}
