﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BenDing.Domain.Models.Dto.JsonEntity;
using BenDing.Domain.Models.Dto.OutpatientDepartment;
using BenDing.Domain.Models.Dto.Resident;
using BenDing.Domain.Models.Dto.Web;
using BenDing.Domain.Models.Entitys;
using BenDing.Domain.Models.Enums;
using BenDing.Domain.Models.Params;
using BenDing.Domain.Models.Params.Base;
using BenDing.Domain.Models.Params.OutpatientDepartment;
using BenDing.Domain.Models.Params.Resident;
using BenDing.Domain.Models.Params.SystemManage;
using BenDing.Domain.Models.Params.UI;
using BenDing.Domain.Models.Params.Web;
using BenDing.Domain.Xml;
using BenDing.Repository.Interfaces.Web;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NFine.Code;
using SqlSugar;

namespace BenDing.Repository.Providers.Web
{
    public class HisSqlRepository : IHisSqlRepository
    {
        private readonly IMedicalInsuranceSqlRepository _baseSqlServerRepository;
        private readonly ISystemManageRepository _iSystemManageRepository;
        private readonly string _connectionString;
        private readonly Log _log;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iBaseSqlServerRepository"></param>
        /// <param name="isystemManageRepository"></param>

        public HisSqlRepository(IMedicalInsuranceSqlRepository iBaseSqlServerRepository, ISystemManageRepository isystemManageRepository)
        {
            _baseSqlServerRepository = iBaseSqlServerRepository;
            _iSystemManageRepository = isystemManageRepository;
            _log = LogFactory.GetLogger("ini".GetType().ToString());
            string conStr = ConfigurationManager.ConnectionStrings["NFineDbContext"].ToString();
            _connectionString = !string.IsNullOrWhiteSpace(conStr) ? conStr : throw new ArgumentNullException(nameof(conStr));

        }
        /// <summary>
        /// 更新医疗机构
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public void ChangeOrg(UserInfoDto userInfo, List<OrgDto> param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {

                sqlConnection.Open();
                if (param.Any())
                {
                    IDbTransaction transaction = sqlConnection.BeginTransaction();
                    try
                    {

                        string strSql = $"update [dbo].[HospitalOrganization] set DeleteTime=getDate(),IsDelete=1,DeleteUserId='{userInfo.UserId}'";

                        if (param.Any())
                        {

                            sqlConnection.Execute(strSql, null, transaction);

                            string insterCount = null;
                            foreach (var itmes in param)
                            {
                                string insterSql = $@"
                                insert into [dbo].[HospitalOrganization](id,HospitalId,HospitalName,HospitalAddr,ContactPhone,PostalCode,ContactPerson,CreateTime,IsDelete,DeleteTime,CreateUserId)
                                values('{Guid.NewGuid()}','{itmes.Id}','{itmes.HospitalName}','{itmes.HospitalAddr}','{itmes.ContactPhone}','{itmes.PostalCode}','{itmes.ContactPerson}',
                                    getDate(),0,null,'{userInfo.UserId}');";
                                insterCount += insterSql;
                            }
                            sqlConnection.Execute(insterCount, null, transaction);
                            transaction.Commit();

                        }

                    }
                    catch (Exception exception)
                    {

                        transaction.Rollback();
                        throw new Exception(exception.Message);

                    }
                }

                sqlConnection.Close();


            }


        }
        /// <summary>
        /// 添加三大目录
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="param"></param>
        /// <param name="type"></param>
        public int AddCatalog(UserInfoDto userInfo, List<CatalogDto> param, CatalogTypeEnum type)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                string insterCount = null;
                var paramNew =new List<CatalogDto>();
                try
                {
                    if (param.Any())
                    {
                        var directoryCodeList = CommonHelp.ListToStr(param.Select(d => d.DirectoryCode).ToList());
                        string strSql =
                            $@" select [DirectoryCode] from [dbo].[HospitalThreeCatalogue] where [IsDelete]=0 
                               and [OrganizationCode]='{userInfo.OrganizationCode}' and [DirectoryCode] in({directoryCodeList})";
                        var data = sqlConnection.Query<string>(strSql).ToList();
                        paramNew = data.Any() ? param.Where(c => !data.Contains(c.DirectoryCode)).ToList() : param;
                        foreach (var itmes in paramNew)
                        {
                            string insterSql = $@"
                                    insert into [dbo].[HospitalThreeCatalogue]([id],[DirectoryCode],[DirectoryName],[MnemonicCode],[DirectoryCategoryCode],[DirectoryCategoryName],[Unit],[Specification],[formulation],
                                    [ManufacturerName],[remark],DirectoryCreateTime,CreateTime,IsDelete,CreateUserId,FixedEncoding,OrganizationCode,OrganizationName)
                                    values('{Guid.NewGuid()}','{itmes.DirectoryCode}','{CommonHelp.FilterSqlStr(itmes.DirectoryName)}','{CommonHelp.FilterSqlStr(itmes.MnemonicCode)}',{Convert.ToInt16(type)},'{itmes.DirectoryCategoryName}','{itmes.Unit}','{CommonHelp.FilterSqlStr(itmes.Specification)}','{itmes.Formulation}',
                                   '{itmes.ManufacturerName}','{itmes.Remark}', '{itmes.DirectoryCreateTime}',getDate(),0,'{userInfo.UserId}','{ CommonHelp.GuidToStr(itmes.DirectoryCode)}','{userInfo.OrganizationCode}','{userInfo.OrganizationName}');";
                            insterCount += insterSql;
                        }

                        if (!string.IsNullOrWhiteSpace(insterCount))
                        {
                            sqlConnection.Execute(insterCount);
                        }
                        sqlConnection.Close();
                    }

                }
                catch (Exception e)
                {
                    _log.Debug(insterCount);
                    throw new Exception(e.Message);
                }

                return paramNew.Count;
            }


        }
        /// <summary>
        /// 基层端三大目录查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<QueryCatalogDto>> QueryCatalog(QueryCatalogUiParam param)
        {
            List<QueryCatalogDto> dataList;
            var resultData = new Dictionary<int, List<QueryCatalogDto>>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    querySql = $@"
                             select Id, DirectoryCode ,DirectoryName,MnemonicCode,DirectoryCategoryName,Unit,Specification
                             ,Formulation,ManufacturerName,Remark,DirectoryCreateTime from [dbo].[HospitalThreeCatalogue] where IsDelete=0 and OrganizationCode='{param.OrganizationCode}'";
                    string countSql = $@"select count(*) from [dbo].[HospitalThreeCatalogue] where IsDelete=0 and OrganizationCode='{param.OrganizationCode}'";
                    string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(param.DirectoryCategoryCode))
                    {
                        whereSql += $" and DirectoryCategoryCode='{param.DirectoryCategoryCode}'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.DirectoryCode))
                    {
                        whereSql += $" and DirectoryCode='{param.DirectoryCode}'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.DirectoryName))
                    {
                        whereSql += $"  and DirectoryName like '%{param.DirectoryName}%'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.ManufacturerName))
                    {
                        whereSql += $"  and ManufacturerName like '%{param.ManufacturerName}%'";
                    }
                    if (param.Limit != 0 && param.Page > 0)
                    {
                        var skipCount = param.Limit * (param.Page - 1);
                        querySql += whereSql + " order by CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.Limit + " ROWS ONLY;";
                    }
                    string executeSql = countSql + whereSql + ";" + querySql;

                    var result = sqlConnection.QueryMultiple(executeSql);

                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<QueryCatalogDto>()

                                select t).ToList();

                    resultData.Add(totalPageCount, dataList);
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
        /// 三大目录列表查询
        /// </summary>
        /// <param name="directoryCodeList"></param>
        /// <param name="organizationCode"></param>
        /// <returns></returns>
        public List<QueryCatalogDto> QueryCatalogList(List<string> directoryCodeList,string organizationCode)
        {
            List<QueryCatalogDto> dataList;
          
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    querySql = $@"
                             select Id, DirectoryCode ,DirectoryName,MnemonicCode,DirectoryCategoryName,Unit,Specification
                             ,Formulation,ManufacturerName,Remark,DirectoryCreateTime from [dbo].[HospitalThreeCatalogue] 
                          where IsDelete=0 and OrganizationCode='{organizationCode}'";

                    var result = sqlConnection.QueryMultiple(querySql);

                 
                    dataList = (from t in result.Read<QueryCatalogDto>()

                                select t).ToList();

                    if (directoryCodeList.Any())
                    {
                        querySql += $" and DirectoryCode in ({CommonHelp.ListToStr(directoryCodeList)})";
                    }
                    sqlConnection.Close();
                    return dataList;

                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }


            }
        }

        /// <summary>
        /// 删除三大目录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int DeleteCatalog(UserInfoDto user, int param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {//update [dbo].[HospitalThreeCatalogue] set IsDelete=1,DeleteUserId='{user.UserId}',DeleteTime=getDate()  where DirectoryCategoryCode='{param.ToString()}' and [OrganizationCode]='{user.OrganizationCode}' and IsDelete=0 
                sqlConnection.Open();
                string strSql = $"delete [dbo].[HospitalThreeCatalogue] where DirectoryCategoryCode='{param.ToString()}' and [OrganizationCode]='{user.OrganizationCode}'";
                var num = sqlConnection.Execute(strSql);
                sqlConnection.Close();
                return num;
            }
        }
        /// <summary>
        /// 获取三大项目最新时间
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string GetTime(int num, UserInfoDto user)
        {
            string result;
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                string strSql = $"select MAX(DirectoryCreateTime) from [dbo].[HospitalThreeCatalogue] where IsDelete=0 and OrganizationCode='{user.OrganizationCode}' and  DirectoryCategoryCode={num}";
                var timeMax = sqlConnection.QueryFirst<string>(strSql);

                result = timeMax;
                sqlConnection.Close();
            }

            return result;

        }
        /// <summary>
        /// 下载医保数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Int64 MedicalInsuranceDownloadIcd10(DataTable dt, string userId)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                string sqlStr = $"update [dbo].[ICD10] set IsDelete=1,UpdateTime=GETDATE(),UpdateUserId='{userId}'  where [IsMedicalInsurance]=1 ";
                sqlConnection.Execute(sqlStr);
                sqlConnection.Close();
            }

            var addIcd10Data = new List<ICD10InfoDto>();
            int totalNum = 0;
            foreach (DataRow dr in dt.Rows)
            {
                var item = new ICD10InfoDto
                {
                    DiseaseCoding = dr["编码"].ToString(),
                    DiseaseName = dr["名称"].ToString(),
                   MnemonicCode = dr["拼音助记码"].ToString()

                };
                addIcd10Data.Add(item);
                if (addIcd10Data.Count() >= 300)
                {
                    SaveMedicalInsuranceICD10(addIcd10Data, userId);
                    totalNum += addIcd10Data.Count();
                    addIcd10Data.Clear();
                }
            }
            //执行剩余的数据
            if (addIcd10Data.Any())
            {
                SaveMedicalInsuranceICD10(addIcd10Data, userId);
                totalNum += addIcd10Data.Count();
            }

            return totalNum;
        }
        //300行一次保存医保ICD10
        private void SaveMedicalInsuranceICD10(List<ICD10InfoDto> param, string userId)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insterCount = null;
                try
                {
                    sqlConnection.Open();
                    if (param.Any())
                    {

                        if (param.Any())
                        {
                            foreach (var itmes in param)
                            {
                                string insterSql = $@"
                                        insert into [dbo].[ICD10]([id],[DiseaseCoding],[DiseaseName],[MnemonicCode],[Remark],[DiseaseId],
                                          Icd10CreateTime, CreateTime,CreateUserId,IsDelete,IsMedicalInsurance)
                                        values('{Guid.NewGuid()}','{itmes.DiseaseCoding}','{itmes.DiseaseName}','{itmes.MnemonicCode}','{itmes.Remark}','{itmes.DiseaseId}','{itmes.Icd10CreateTime}',
                                        getDate(),'{userId}',0,1);";

                                insterCount += insterSql;
                            }
                            sqlConnection.Execute(insterCount);
                            sqlConnection.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Debug(insterCount);
                    throw new Exception(e.Message);
                }



            }
        }
        /// <summary>
        /// ICD10获取最新时间
        /// </summary>
        /// <returns></returns>
        public string GetICD10Time()
        {
            string result;
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                string strSql = $"select MAX(CreateTime) from [dbo].[ICD10] where IsDelete=0  and IsMedicalInsurance=0  ";
                var timeMax = sqlConnection.QueryFirst<string>(strSql);

                result = timeMax;
                sqlConnection.Close();
            }

            return result;

        }
        /// <summary>
        /// 添加ICD10
        /// </summary>
        /// <param name="param"></param>
        /// <param name="user"></param>
        public void AddICD10(List<ICD10InfoDto> param, UserInfoDto user)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insterCount = null;
                try
                {
                    sqlConnection.Open();
                    if (param.Any())
                    {
                        //获取唯一编码
                        var catalogDtoIdList = param.Select(c => c.DiseaseId).ToList();
                        var ids = CommonHelp.ListToStr(catalogDtoIdList);
                        string sqlStr = $"select DiseaseCoding  from [dbo].[ICD10]  where DiseaseCoding  in({ids}) and IsDelete=0 and IsMedicalInsurance=0";
                        var idListNew = sqlConnection.Query<string>(sqlStr);
                        //排除已有项目 
                        var listNew = idListNew as string[] ?? idListNew.ToArray();
                        var paramNew = listNew.Any() ? param.Where(c => !listNew.Contains(c.DiseaseId)).ToList()
                            : param;
                        if (paramNew.Any())
                        {
                            foreach (var itmes in paramNew)
                            {
                                string insterSql = $@"
                                        insert into [dbo].[ICD10]([id],[DiseaseCoding],[DiseaseName],[MnemonicCode],[Remark],[DiseaseId],
                                          Icd10CreateTime, CreateTime,CreateUserId,IsDelete,IsMedicalInsurance)
                                        values('{Guid.NewGuid()}','{itmes.DiseaseCoding}','{itmes.DiseaseName}','{itmes.MnemonicCode}','{itmes.Remark}','{itmes.DiseaseId}','{itmes.Icd10CreateTime}',
                                        getDate(),'{user.UserId}',0,0);";

                                insterCount += insterSql;
                            }
                            sqlConnection.Execute(insterCount);
                            sqlConnection.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Debug(insterCount);
                    throw new Exception(e.Message);
                }



            }
        }

        /// <summary>
        /// ICD10查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public  List<QueryICD10InfoDto> QueryAllICD10()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = @"select a.DiseaseId,b.DiseaseCoding,b.DiseaseName  from [dbo].[ICD10] as a,[dbo].[ICD10] as b where 
                         a.IsDelete=0 and a.IsMedicalInsurance=0 and a.DiseaseCoding=b.DiseaseCoding and a.DiseaseName=b.DiseaseName
                          and b.IsMedicalInsurance=1 and b.IsDelete=0";
                   
                    var data = sqlConnection.Query<QueryICD10InfoDto>(strSql);
                    sqlConnection.Close();
                    return data.ToList();
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
            }
        }

        /// <summary>
        /// ICD10查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<QueryICD10InfoDto>> QueryICD10(QueryICD10UiParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                List<QueryICD10InfoDto> dataList;
                var dataListNew = new List<QueryICD10InfoDto>();
                var resultData = new Dictionary<int, List<QueryICD10InfoDto>>();
                string executeSql = null;
                try
                {
                    sqlConnection.Open();
                    string querySql = $@"
                             select  [id],[DiseaseCoding],[DiseaseName] ,[MnemonicCode],[Remark] ,[CreateTime],DiseaseId from [dbo].[ICD10]  where IsDelete=0 and IsMedicalInsurance={param.IsMedicalInsurance}";
                    string countSql = $@"select  count(*) from [dbo].[ICD10]  where IsDelete=0  and IsMedicalInsurance={param.IsMedicalInsurance}";

                    string regexstr = @"[\u4e00-\u9fa5]";
                    string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(param.DiseaseCoding))
                    {
                        whereSql += $" and DiseaseCoding ='{param.DiseaseCoding}'";
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(param.DiseaseName))
                        {
                            if (Regex.IsMatch(param.DiseaseName, regexstr))
                            {
                                whereSql += " and DiseaseName like '%" + param.DiseaseName + "%'";
                            }
                            else
                            {
                                whereSql += " and MnemonicCode like '%" + param.DiseaseName + "%'";
                            }
                        }


                    }


                    if (param.Limit != 0 && param.Page > 0)
                    {
                        var skipCount = param.Limit * (param.Page - 1);
                        querySql += whereSql + " order by CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.Limit + " ROWS ONLY;";
                    }
                    executeSql = countSql + whereSql + ";" + querySql;
                    var result = sqlConnection.QueryMultiple(executeSql);
                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<QueryICD10InfoDto>()
                                select t).ToList();
                    if (param.IsMedicalInsurance == 0)
                    {
                        var diseaseIdList = dataList.Select(c => c.DiseaseId).ToList();
                        string strlist = CommonHelp.ListToStr(diseaseIdList);
                        string sqlPairCode = $@"select [DiseaseId],[ProjectName],[ProjectCode],[PairCodeUserName],[CreateTime] from [dbo].[ICD10PairCode] 
                         where [State] = 1 and [IsDelete] = 0 and [DiseaseId] in({strlist})";
                        var data = sqlConnection.Query<ICD10PairCodeDto>(sqlPairCode).ToList();
                        if (data.Any())
                        {
                            foreach (var item in dataList)
                            {
                                var queryPairCode = data.FirstOrDefault(c => c.DiseaseId == item.DiseaseId);
                                var pairCode = new QueryICD10InfoDto()
                                {
                                    Id = item.Id,
                                    DiseaseCoding = item.DiseaseCoding,
                                    DiseaseName = item.DiseaseName,
                                    MnemonicCode = item.MnemonicCode,
                                    ProjectCode = queryPairCode != null ? queryPairCode.ProjectCode : item.ProjectCode,
                                    ProjectName = queryPairCode != null ? queryPairCode.ProjectName : item.ProjectName,
                                    DiseaseId = item.DiseaseId,
                                    PairCodeTime = queryPairCode?.CreateTime,
                                    PairCodeUserName = queryPairCode?.PairCodeUserName

                                };

                                dataListNew.Add(pairCode);
                            }
                        }
                        else
                        {
                            dataListNew = dataList;
                        }
                    }
                    else
                    {
                        dataListNew = dataList;
                    }

                    resultData.Add(totalPageCount, dataListNew);
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
        /// ICD10明细查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<QueryICD10InfoDto> QueryICD10Detail(List<string> param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = @"select   DiseaseCoding, DiseaseName from [dbo].[ICD10]  where  IsDelete=0  and IsMedicalInsurance=0";
                    if (param != null && param.Any())
                    {
                        var idlist = CommonHelp.ListToStr(param);
                        strSql += $" and DiseaseCoding in({idlist})";
                    }
                    var data = sqlConnection.Query<QueryICD10InfoDto>(strSql);
                    sqlConnection.Close();
                    return data.ToList();
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
            }
        }
       
        /// <summary>
        /// ICD10 对码
        /// </summary>
        /// <param name="param"></param>
        public void Icd10PairCode(Icd10PairCodeParam  param )
        {

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string sqlStr = null;
                try
                {
                    sqlConnection.Open();
                    string idlist="";
                    if (param.DataList.Any())
                    {
                         idlist = CommonHelp.ListToStr(param.DataList.Select(c=>c.DiseaseId).ToList());
                        sqlStr =
                            $@"update [dbo].[ICD10PairCode] set [IsDelete]=1,[DeleteTime]=GETDATE(),[DeleteUserId]='{param.User.UserId}'
                              where [DiseaseId] in({idlist}) 
                             ";
                        sqlConnection.Execute(sqlStr);
                        string sqlStrNew = null;
                        foreach (var item in param.DataList)
                        {
                            sqlStrNew += $@" insert into  [dbo].[ICD10PairCode] 
                            ([Id],[DiseaseId],[ProjectName],[ProjectCode],
                             [State],[CreateTime],[CreateUserId],[IsDelete],[PairCodeUserName])
                            values
                            ('{Guid.NewGuid()}','{item.DiseaseId}','{item.ProjectName}','{item.ProjectCode}',
                             1,GETDATE(),'{param.User.UserId}',0,'{param.User.UserName}');";

                        }
                        sqlConnection.Execute(sqlStrNew);
                    }

                   

                    sqlConnection.Close();


                }
                catch (Exception e)
                {
                    _log.Debug(sqlStr);
                    throw new Exception(e.Message);
                }



            }
        }
        /// <summary>
        /// 保存门诊病人信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        public void SaveOutpatient(UserInfoDto user, BaseOutpatientInfoDto param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $@"
                   INSERT INTO [dbo].[outpatient](
                   Id,[PatientName],[IdCardNo],[PatientSex],[BusinessId],[OutpatientNumber],[VisitDate]
                   ,[DepartmentId],[DepartmentName],[DiagnosticDoctor],[DiagnosticJson]
                   ,[Operator] ,[MedicalTreatmentTotalCost],[Remark],[ReceptionStatus],[FixedEncoding]
                   ,[CreateTime],[DeleteTime],OrganizationCode,OrganizationName,CreateUserId,IsDelete)
                   VALUES('{param.Id}','{param.PatientName}','{param.IdCardNo}','{param.PatientSex}','{param.BusinessId}','{param.OutpatientNumber}','{param.VisitDate}'
                         ,'{param.DepartmentId}','{param.DepartmentName}','{param.DiagnosticDoctor}','{param.DiagnosticJson}' 
                        ,'{param.Operator}','{param.MedicalTreatmentTotalCost}','{param.Remark}','{param.ReceptionStatus}','{CommonHelp.GuidToStr(param.BusinessId)}'
                         ,getDate(),null,'{user.OrganizationCode}','{user.OrganizationName}','{user.UserId}',0
                    );";
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
        /// 查询门诊病人明细
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<BaseOutpatientDetailDto>>  QueryOutpatientDetail(QueryOutpatientDetailParam param)
        {
            var resultData = new Dictionary<int, List<BaseOutpatientDetailDto>>();
            var dataList = new List<BaseOutpatientDetailDto>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string executeSql = null;
               
                try
                {
                    sqlConnection.Open();
                    string querySql = $"select  * from [dbo].[OutpatientFee] where IsDelete=0 ";
                    string countSql = $@"select  count(*) from [dbo].[OutpatientFee] where IsDelete=0";
                    string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(param.Id))
                        whereSql += $" and id='{param.Id}'";
                    if (!string.IsNullOrWhiteSpace(param.PatientId))
                        whereSql += $" and PatientId='{param.PatientId}'";
                    if (!string.IsNullOrWhiteSpace(param.OutpatientNo))
                        whereSql += $" and OutpatientNo='{param.OutpatientNo}'";
                    if (!string.IsNullOrWhiteSpace(param.DirectoryName))
                        whereSql += $" and DirectoryName='{param.DirectoryName}'";
                    if (param.rows != 0 && param.Page > 0)
                    {
                        var skipCount = param.rows * (param.Page - 1);
                        querySql += whereSql + " order by CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.rows + " ROWS ONLY;";
                    }
                    executeSql = countSql + whereSql + ";" + querySql;
                    var result = sqlConnection.QueryMultiple(executeSql);
                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<BaseOutpatientDetailDto>()
                        select t).ToList();
                    resultData.Add(totalPageCount, dataList);
                    //resultData = sqlConnection.Query<BaseOutpatientDetailDto>(strSql).ToList();
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
        /// 查询门诊List
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<BaseOutpatientDetailDto> QueryOutpatientDetailList(QueryOutpatientDetailListParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    string querySql = $"select  * from [dbo].[OutpatientFee] where IsDelete=0 ";
                    if (!string.IsNullOrWhiteSpace(param.PatientId))
                        querySql += $" and PatientId='{param.PatientId}'";
                    if (!string.IsNullOrWhiteSpace(param.OutpatientNo))
                        querySql += $" and OutpatientNo='{param.OutpatientNo}'";
                    if (param.IsAdjustmentDifferenceValue==1)
                        querySql += " and IsAdjustmentDifference=1";
                    
                    var data = sqlConnection.Query<BaseOutpatientDetailDto>(querySql);
                    sqlConnection.Close();
                    return data.ToList();
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
            }
        }
        /// <summary>
        /// 更新门诊病人
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        public void UpdateOutpatient(UserInfoDto user, UpdateOutpatientParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    if (!string.IsNullOrWhiteSpace(param.SettlementTransactionId))
                    {
                        strSql = $@"update [dbo].[Outpatient] set [UpdateUserId]='{user.UserId}',[UpdateTime]=getDate(),
                                SettlementTransactionId='{param.SettlementTransactionId}' where Id='{param.Id.ToString()}'";
                    }
                    if (!string.IsNullOrWhiteSpace(param.SettlementCancelTransactionId))
                    {
                        strSql = $@"update [dbo].[Outpatient] set [SettlementCancelUserId]='{user.UserId}',[SettlementCancelTime]=getDate(),
                                SettlementCancelTransactionId='{param.SettlementCancelTransactionId}' where Id='{param.Id.ToString()}'";
                    }

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
        /// 保存住院结算
        /// </summary>
        /// <param name="param"></param>
        public void SaveInpatientSettlement(SaveInpatientSettlementParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();

                    strSql = $@"update [dbo].[Inpatient] set [UpdateUserId]='{param.User.UserId}',[UpdateTime]=getDate(),LeaveHospitalDate='{param.LeaveHospitalDate}',
                                   LeaveHospitalDiagnosisJson='{param.LeaveHospitalDiagnosisJson}',LeaveHospitalDepartmentId='{param.LeaveHospitalDepartmentId}',
                                   LeaveHospitalDepartmentName='{param.LeaveHospitalDepartmentName}',LeaveHospitalBedNumber='{param.LeaveHospitalBedNumber}',
                                   LeaveHospitalDiagnosticDoctor='{param.LeaveHospitalDiagnosticDoctor}',LeaveHospitalOperator='{param.LeaveHospitalOperator}'
                                   where Id='{param.Id.ToString()}'";

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
        /// 查询门诊病人信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public QueryOutpatientDto QueryOutpatient(QueryOutpatientParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $"select top 1 * from [dbo].[Outpatient] where IsDelete=0 and BusinessId='{param.BusinessId}'";
                    if (!string.IsNullOrWhiteSpace(param.Id))
                    {
                        strSql += $" and id='{param.Id}'";
                    }

                    var data = sqlConnection.QueryFirstOrDefault<QueryOutpatientDto>(strSql);
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
        /// 保存门诊病人明细
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        public void SaveOutpatientDetail(UserInfoDto user, List<BaseOutpatientDetailDto> param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insertSql = null;
                try
                {
                    sqlConnection.Open();

                    if (param.Any())
                    {

                        var outpatientNum = CommonHelp.ListToStr(param.Select(c => c.DetailId).ToList());
                        var paramFirst = param.FirstOrDefault();
                        if (paramFirst != null)
                        {
                            string strSql =
                                $@" select [DetailId],[DataSort] from [dbo].[OutpatientFee] where [OutpatientNo]='{paramFirst.OutpatientNo}'
                                 and [DetailId] in({outpatientNum})";
                            var data = sqlConnection.Query<InpatientInfoDetailQueryDto>(strSql).ToList();
                            int sort = 0;
                            List<BaseOutpatientDetailDto> paramNew;
                            if (data.Any())
                            {    //获取最大排序号
                                sort = data.Select(c => c.DataSort).Max();
                                var costDetailIdList = data.Select(c => c.DetailId).ToList();
                                //排除已包含的明细id
                                paramNew = param.Where(c => !costDetailIdList.Contains(c.DetailId)).ToList();
                            }
                            else
                            {
                                paramNew = param.OrderBy(d => d.BillTime).ToList();
                            }
                            foreach (var item in paramNew)
                            {
                                sort++;
                                var businessTime = item.BillTime.Substring(0, 10) + " 00:00:00.000";
                                string str = $@"INSERT INTO [dbo].[OutpatientFee](
                               id,[OutpatientNo] ,[DetailId] ,[DirectoryName],[DirectoryCode] ,[DirectoryCategoryName] ,[DirectoryCategoryCode]
                               ,[Unit] ,[Formulation] ,[Specification] ,[UnitPrice],[Quantity],[Amount] ,[Dosage] ,[Usage] ,[MedicateDays]
		                       ,[HospitalPricingUnit] ,[IsImportedDrugs] ,[DrugProducingArea] ,[RecipeCode]  ,[CostDocumentType] ,[BillDepartment]
			                   ,[BillDepartmentId] ,[BillDoctorName],[BillDoctorId] ,[BillTime] ,[OperateDepartmentName],[OperateDepartmentId]
                               ,[OperateDoctorName] ,[OperateDoctorId],[OperateTime] ,[PrescriptionDoctor] ,[Operators],[PracticeDoctorNumber]
                               ,[CostWriteOffId],[OrganizationCode],[OrganizationName] ,[CreateTime] ,[IsDelete],[DeleteTime],CreateUserId
                               ,DataSort,UploadMark,RecipeCodeFixedEncoding,BillDoctorIdFixedEncoding,BusinessTime,MedicalInsuranceProjectCode,NotUploadMark,PatientId)
                           VALUES('{Guid.NewGuid()}','{item.OutpatientNo}','{item.DetailId}','{item.DirectoryName}','{item.DirectoryCode}','{item.DirectoryCategoryName}','{item.DirectoryCategoryCode}'
                                 ,'{item.Unit}','{item.Formulation}','{item.Specification}',{item.UnitPrice},{item.Quantity},{CommonHelp.ValueToDouble(item.Amount)},'{item.Dosage}','{item.Usage}','{item.MedicateDays}',
                                 '{item.HospitalPricingUnit}','{item.IsImportedDrugs}','{item.DrugProducingArea}','{item.RecipeCode}','{item.CostDocumentType}','{item.BillDepartment}'
                                 ,'{item.BillDepartmentId}','{item.BillDoctorName}','{item.BillDoctorId}','{item.BillTime}','{item.OperateDepartmentName}','{item.OperateDepartmentId}'
                                 ,'{item.OperateDoctorName}','{item.OperateDoctorId}','{item.OperateTime}','{item.PrescriptionDoctor}','{item.Operators}','{item.PracticeDoctorNumber}'
                                 ,'{item.CostWriteOffId}','{item.OrganizationCode}','{item.OrganizationName}',getDate(),0,null,'{user.UserId}'
                                 ,{sort},0,'null','{item.BillDoctorId}','{businessTime}','{item.MedicalInsuranceProjectCode}',{item.NotUploadMark},'{item.PatientId}'
                                 );";
                                insertSql += str;
                            }

                            if (paramNew.Count > 0)
                            {
                                sqlConnection.Execute(insertSql);


                            }
                        }
                    }
                    sqlConnection.Close();
                }
                catch (Exception e)
                {
                    _log.Debug(insertSql);
                    throw new Exception(e.Message);
                }


            }
        }
        /// <summary>
        /// 更新门诊费用上传明细
        /// </summary>
        /// <param name="user"></param>
        /// <param name="outpatientNo"></param>
        public void UpdateOutpatientDetail(UserInfoDto user, string outpatientNo)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string insertSql = null;
                try
                {
                    sqlConnection.Open();

                    insertSql = $@"update [dbo].[OutpatientFee] set [UploadMark]=1 ,[UploadTime]=getdate(),
                        [UploadUserId] = '{user.UserId}',[UploadUserName]='{user.UserName}' where [Isdelete] = 0 and [OutpatientNo]='{outpatientNo}'";
                    sqlConnection.Close();
                }
                catch (Exception e)
                {
                    _log.Debug(insertSql);
                    throw new Exception(e.Message);
                }


            }
        }
        /// <summary>
        /// 保存住院病人明细
        /// </summary>
        /// <param name="param"></param>
        public void SaveInpatientInfoDetail(SaveInpatientInfoDetailParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {

                sqlConnection.Open();

                if (param.DataList.Any())
                {
                    string insertSql = null;
                    try
                    {

                        var outpatientNum = CommonHelp.ListToStr(param.DataList.Select(c => c.DetailId).ToList());
                        var paramFirst = param.DataList.FirstOrDefault();
                        if (paramFirst != null)
                        {
                            string strSql =
                                $@" select [DetailId],[DataSort] from [dbo].[HospitalizationFee] where [HospitalizationId]='{param.HospitalizationId}'
                                 and [DetailId] in({outpatientNum}) and IsDelete=0";
                            var data = sqlConnection.Query<InpatientInfoDetailQueryDto>(strSql).ToList();
                            int sort = 0;
                            List<InpatientInfoDetailDto> paramNew;
                            if (data.Any())
                            {    //获取最大排序号
                                sort = data.Select(c => c.DataSort).Max();
                                var costDetailIdList = data.Select(c => c.DetailId).ToList();
                                //排除已包含的明细id
                                paramNew = param.DataList.Where(c => !costDetailIdList.Contains(c.DetailId)).ToList();
                            }
                            else
                            {
                                paramNew = param.DataList.OrderBy(d => d.BillTime).ToList();
                            }
                            foreach (var item in paramNew)
                            {
                                sort++;
                                var businessTime = item.BillTime.Substring(0, 10) + " 00:00:00.000";
                                string str = $@"INSERT INTO [dbo].[HospitalizationFee]
                                           ([Id],[HospitalizationId],[DetailId],[DocumentNo],[BillDepartment] ,[DirectoryName],[DirectoryCode]
                                           ,[ProjectCode] ,[Formulation],[Specification],[UnitPrice],[Usage] ,[Quantity],[Amount],[DocumentType]
                                           ,[BillDepartmentId] ,[BillDoctorId] ,[BillDoctorName] ,[Dosage] ,[Unit]
                                           ,[OperateDepartmentName],[OperateDepartmentId],[OperateDoctorName],[OperateDoctorId] ,[DoorEmergencyFeeMark]
                                           ,[HospitalAuditMark],[BillTime],[OutHospitalInspectMark] ,[OrganizationCode] ,[OrganizationName]
                                           ,[UploadMark] ,[DataSort] ,[AdjustmentDifferenceValue],[BusinessTime],[CreateTime],[CreateUserId])
                                           VALUES('{Guid.NewGuid()}','{param.HospitalizationId}','{item.DetailId}','{item.DocumentNo}','{item.BillDepartment}','{item.DirectoryName}','{item.DirectoryCode}',
                                                  '{item.ProjectCode}','{item.Formulation}','{item.Specification}',{item.UnitPrice},'{item.Usage}',{item.Quantity},{CommonHelp.ValueToDouble(item.Amount)},'{item.DocumentType}',
                                                  '{item.BillDepartmentId}','{item.BillDoctorId}','{item.BillDoctorName}','{item.Dosage}','{item.Unit}',
                                                  '{item.OperateDepartmentName}','{item.OperateDepartmentId}','{item.OperateDoctorName}','{item.OperateDoctorId}','{item.DoorEmergencyFeeMark}',
                                                  '{item.HospitalAuditMark}','{item.BillTime}','{item.OutHospitalInspectMark}','{param.User.OrganizationCode}','{param.User.OrganizationName}',
                                                  0,{sort},0,'{businessTime}',getDate(),'{param.User.UserId}');";
                                insertSql += str;
                            }

                            if (paramNew.Count > 0)
                            {
                                sqlConnection.Execute(insertSql);


                            }
                        }
                    }
                    catch (Exception exception)
                    {


                        _log.Debug(insertSql);
                        throw new Exception(exception.Message);
                    }
                }
                sqlConnection.Close();

            }
        }
        /// <summary>
        /// 住院病人明细查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<QueryInpatientInfoDetailDto> InpatientInfoDetailQuery(InpatientInfoDetailQueryParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try  
                {
                    sqlConnection.Open();
                    strSql = @"select   HospitalizationId as BusinessId, * from [dbo].[HospitalizationFee]  where  IsDelete=0 ";
                    if (param.IdList != null && param.IdList.Any())
                    {
                        var idlist = CommonHelp.ListToStr(param.IdList);
                        strSql += $" and Id in({idlist})";
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(param.BusinessId))
                        {
                            strSql += $@" and HospitalizationId ='{param.BusinessId}' ";
                        }
                    }

                    if (param.NotUploadMark)
                    {
                        strSql += "  and NotUploadMark is null";
                    }

                    if (param.UploadMark != null)
                    {
                        strSql += $" and UploadMark ={param.UploadMark}";
                    }

                    var data = sqlConnection.Query<QueryInpatientInfoDetailDto>(strSql);
                    sqlConnection.Close();
                    return data.ToList();
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
            }
        }
      

        /// <summary>
        /// 批量审核数据
        /// </summary>
        public void BatchExamineData(BatchExamineDataParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $@"update [dbo].[HospitalizationFee] set ApprovalMark={param.BatchExamineSign}, [ApprovalUserName]='{param.User.UserName}',
                       [ApprovalTime]=GETDATE(),[ApprovalUserId]='{param.User.UserId}' where IsDelete=0 and HospitalizationId='{param.BusinessId}'";
                    if (param.DataIdList != null && param.DataIdList.Any())
                    {
                        var idlist = CommonHelp.ListToStr(param.DataIdList);
                        strSql += $" and Id in({idlist}) ";
                        var data = sqlConnection.Execute(strSql);
                    }
                  
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
        /// 住院清单查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<QueryHospitalizationFeeDto>> QueryHospitalizationFee(QueryHospitalizationFeeUiParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var threeDirectoryListNew = new List<QueryHospitalizationFeeDto>();
                var dataListNew = new List<QueryHospitalizationFeeDto>();
                var resultData = new Dictionary<int, List<QueryHospitalizationFeeDto>>();

                string queryExamineSql = $@"
                    select [ProjectCode] from [dbo].[MedicalInsuranceProject] where [ProjectCode] in 
                    (select ProjectCode from [dbo].[HospitalizationFee] where  HospitalizationId='{param.BusinessId}' and IsDelete=0) 
                    and IsDelete=0 and RestrictionSign='1'";
                sqlConnection.Open();

                string querySql = $@"
                             select * from [dbo].[HospitalizationFee] 
                             where HospitalizationId='{param.BusinessId}' and IsDelete=0 ";
                string countSql = $@"select COUNT(*) from [dbo].[HospitalizationFee] 
                              where HospitalizationId='{param.BusinessId}' and IsDelete=0 ";

                string whereSql = "";

                //是否上传标志
                if (!string.IsNullOrWhiteSpace(param.UploadMark))
                    whereSql += $"  and UploadMark='{param.UploadMark}'";
             
                //药品名称模糊查询
                if (!string.IsNullOrWhiteSpace(param.DirectoryName))
                    whereSql += " and DirectoryName like '" + param.DirectoryName + "%'";
                //时间查询
                var billTime = CommonHelp.GetBillTime(param.BillTime);
                if (!string.IsNullOrWhiteSpace(param.BillTime))
                    whereSql += $" and BillTime between '{billTime.StartTime}' and '{billTime.EndTime}'";

                 if (param.IsExamine == 1)
                 {//限制医保编码
                     var idList=new List<string>(); 
                     var examineList = sqlConnection.Query<string>(queryExamineSql).ToList();
                     if (examineList.Any())
                     {
                         idList = examineList;
                     }
                     else
                     {
                        idList.Add(Guid.NewGuid().ToString());
                     }
                     var idListStr = CommonHelp.ListToStr(idList);
                     whereSql += $" and ProjectCode in ({idListStr})";
                  }
                //if (param.UploadMark == "1")
                        //{
                        //    whereSql +=" and NotUploadMark is null";
                        //}

                        if (param.Limit != 0 && param.Page > 0)
                {
                    var skipCount = param.Limit * (param.Page - 1);
                    querySql += whereSql + " order by CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.Limit + " ROWS ONLY;";
                }
                string executeSql = countSql + whereSql + ";" + querySql;
                var result = sqlConnection.QueryMultiple(executeSql);
                int totalPageCount = result.Read<int>().FirstOrDefault();
                var dataList = (from t in result.Read<QueryHospitalizationFeeDto>()
                                select new QueryHospitalizationFeeDto
                                {
                                    Id = t.Id,
                                    Amount = t.Amount,
                                    BillTime = t.BillTime,
                                    BillDepartment = t.BillDepartment,
                                    DirectoryCode = t.DirectoryCode,
                                    DirectoryName = t.DirectoryName,
                                    UnitPrice = t.UnitPrice,
                                    UploadUserName = t.UploadUserName,
                                    Quantity = t.Quantity,
                                    RecipeCode = t.RecipeCode,
                                    Specification = t.Specification,
                                    OperateDoctorName = t.OperateDoctorName,
                                    UploadMark = t.UploadMark,
                                    AdjustmentDifferenceValue = t.AdjustmentDifferenceValue,
                                    UploadAmount = t.UploadAmount,
                                    UploadTime = t.UploadTime,
                                    OrganizationCode = t.OrganizationCode,
                                    BatchNumber = t.BatchNumber,
                                    DetailId = t.DetailId,
                                    ApprovalMark=t.ApprovalMark,
                                    ApprovalUserName=t.ApprovalUserName,
                                    NotUploadMark=t.NotUploadMark
                                }
                    ).ToList();
                if (dataList.Any())
                {
                    var organizationCode = dataList.Select(d => d.OrganizationCode).FirstOrDefault();
                    var directoryCodeList = dataList.Select(c => c.DirectoryCode).ToList();
                    var queryPairCodeParam = new QueryMedicalInsurancePairCodeParam()
                    {
                        DirectoryCodeList = directoryCodeList,
                        OrganizationCode = organizationCode
                    };
                    //获取医保对码数据
                    var pairCodeData = _baseSqlServerRepository.QueryMedicalInsurancePairCode(queryPairCodeParam);
                    //获取医院等级
                    var gradeData = _iSystemManageRepository.QueryHospitalOrganizationGrade(organizationCode);
                    //获取入院病人登记信息
                    var residentInfoData = _baseSqlServerRepository.QueryMedicalInsuranceResidentInfo(
                             new QueryMedicalInsuranceResidentInfoParam() { BusinessId = param.BusinessId });
                    if (pairCodeData != null)
                    {
                        foreach (var c in dataList)
                        {
                            var itemPairCode = pairCodeData
                                   .FirstOrDefault(d => d.DirectoryCode == c.DirectoryCode);
                            var item = new QueryHospitalizationFeeDto
                            {
                                Id = c.Id,
                                Amount = c.Amount,
                                BillTime = c.BillTime,
                                BillDepartment = c.BillDepartment,
                                DirectoryCode = c.DirectoryCode,
                                DirectoryName = c.DirectoryName,
                                UnitPrice = c.UnitPrice,
                                UploadUserName = c.UploadUserName,
                                Quantity = c.Quantity,
                                RecipeCode = c.RecipeCode,
                                Specification = c.Specification,
                                OperateDoctorName = c.OperateDoctorName,
                                UploadMark = c.UploadMark,
                                AdjustmentDifferenceValue = c.AdjustmentDifferenceValue,
                                DirectoryCategoryCode = itemPairCode != null ? ((CatalogTypeEnum)Convert.ToInt32(itemPairCode.DirectoryCategoryCode)).ToString() : null,
                                BlockPrice = itemPairCode != null ? GetBlockPrice(itemPairCode, gradeData.OrganizationGrade) : 0,
                                ProjectCode = itemPairCode?.ProjectCode,
                                ProjectName = itemPairCode?.ProjectName,
                                ProjectLevel = itemPairCode != null ? ((ProjectLevel)Convert.ToInt32(itemPairCode.ProjectLevel)).ToString() : null,
                                ProjectCodeType = itemPairCode != null ? ((ProjectCodeType)Convert.ToInt32(itemPairCode.ProjectCodeType)).ToString() : null,
                                SelfPayProportion = (residentInfoData != null && itemPairCode != null)
                                    ? GetSelfPayProportion(itemPairCode, residentInfoData)
                                    : 0,
                                UploadAmount = c.UploadAmount,
                                OrganizationCode = c.OrganizationCode,
                                UploadTime = c.UploadTime,
                                BatchNumber = c.BatchNumber,
                                DetailId = c.DetailId,
                                RestrictionSign = itemPairCode?.RestrictionSign,
                                ApprovalMark = c.ApprovalMark,
                                ApprovalUserName = c.ApprovalUserName,
                                Manufacturer = itemPairCode?.Manufacturer,
                                NotUploadMark = c.NotUploadMark
                            };
                    
                            if (item.RestrictionSign == "1")
                            {
                                item.DirectoryName = "【限】" + item.DirectoryName;
                            }

                            dataListNew.Add(item);

                        }
                    }
                    else
                    {
                        dataListNew = dataList;
                    }
                    var directoryCodeListData = dataListNew.Select(d => d.DirectoryCode).ToList();
                    string threeSqlStr =
                        $@"select [DirectoryCode],[ManufacturerName] From [dbo].[HospitalThreeCatalogue] 
                               where IsDelete=0 and OrganizationCode='{organizationCode}'";
                    if (directoryCodeListData.Any())
                    {
                        threeSqlStr += $@" and DirectoryCode in ({CommonHelp.ListToStr(directoryCodeListData)})";
                    }

                 
                    var threeDirectoryData = sqlConnection.Query<HospitalThreeCatalogueManufacturerNameDto>(threeSqlStr).ToList();

                    if (threeDirectoryData.Any())
                    {
                        foreach (var itemData in dataListNew)
                        {
                            var itemPairCode = threeDirectoryData
                                .FirstOrDefault(d => d.DirectoryCode == itemData.DirectoryCode);
                            var itemDataNew = itemData;
                            itemDataNew.ManufacturerName = itemPairCode?.ManufacturerName;
                            threeDirectoryListNew.Add(itemDataNew);
                        }
                    }
                    else
                    {
                        threeDirectoryListNew = dataListNew;
                    }
                }
                sqlConnection.Close();
                resultData.Add(totalPageCount, threeDirectoryListNew);
                return resultData;

            }
        }

     
        /// <summary>
                /// 住院病人查询
                /// </summary>
                /// <param name="param"></param>
                /// <returns></returns>
      public QueryInpatientInfoDto QueryInpatientInfo(QueryInpatientInfoParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $"select top 1 * from [dbo].[Inpatient] where IsDelete=0 and BusinessId='{param.BusinessId}'";
                    var data = sqlConnection.QueryFirstOrDefault<QueryInpatientInfoDto>(strSql);
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
        /// 获取所有未传费用的住院病人
        /// </summary>
        /// <returns></returns>
        public List<QueryAllHospitalizationPatientsDto> QueryAllHospitalizationPatients(PrescriptionUploadAutomaticParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {

                    sqlConnection.Open();

                    if (param.IsTodayUpload)
                    {
                        string day = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00.000";
                        strSql = $@"select a.OrganizationCode,a.HospitalizationNo,a.BusinessId,b.InsuranceType from [dbo].[Inpatient]  as a 
                                inner join [dbo].[MedicalInsurance] as b on b.BusinessId=a.BusinessId
                                where a.IsDelete=0 and b.IsDelete=0 and a.HospitalizationId 
                                in(select HospitalizationId from [dbo].[HospitalizationFee] where  BusinessTime='{day}' and IsDelete=0 and UploadMark=0 Group by HospitalizationId)";
                    }
                    else
                    {
                        strSql = $@"
                               select  a.BusinessId,a.[HospitalizationNo],a.[PatientName],a.AdmissionDate,a.[IdCardNo],b.[InsuranceType] 
                             from [dbo].[Inpatient] as a inner join [dbo].[MedicalInsurance]  as b
                             on a.BusinessId=b.BusinessId 
                             where a.IsDelete=0 and b.IsDelete=0  and a.IsCanCelHospitalized is null
                             and b.MedicalInsuranceState<5 and a.OrganizationCode='{param.OrganizationCode}' and 
                             b.OrganizationCode='{param.OrganizationCode}'";
                        //if string.is
                    }
                    var data = sqlConnection.Query<QueryAllHospitalizationPatientsDto>(strSql);
                    sqlConnection.Close();
                    return data.ToList();

                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
            }
        }
        /// <summary>
        /// 保存HIS系统中科室、医师、病区、床位的基本信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public int SaveInformationInfo(UserInfoDto user, List<InformationDto> param, InformationParam info)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    int count = 0;
                    sqlConnection.Open();
                    if (param.Any())
                    {
                        var outpatientNum = CommonHelp.ListToStr(param.Select(c => c.DirectoryCode).ToList());
                        strSql =
                          $@"update [dbo].[HospitalGeneralCatalog] set  [IsDelete] =1 ,DeleteTime=getDate(),DeleteUserId='{user.UserId}' where [IsDelete]=0 
                                and [DirectoryCode] in(" + outpatientNum + ")";
                        sqlConnection.Execute(strSql);
                        string insertSql = "";
                        foreach (var item in param)
                        {

                            string str = $@"INSERT INTO [dbo].[HospitalGeneralCatalog]
                                   (id,DirectoryType,[OrganizationCode],[DirectoryCode],[DirectoryName]
                                   ,[MnemonicCode],[DirectoryCategoryName],[Remark] ,[CreateTime]
		                            ,[IsDelete],[DeleteTime],CreateUserId,FixedEncoding)
                             VALUES ('{Guid.NewGuid()}','{info.DirectoryType}','{info.OrganizationCode}','{item.DirectoryCode}','{item.DirectoryName}',
                                     '{item.MnemonicCode}','{item.DirectoryCategoryName}','{item.Remark}',getDate(),
                                       0, null,'{user.UserId}','{CommonHelp.GuidToStr(item.DirectoryCode)}');";
                            insertSql += str;

                        }
                        count = sqlConnection.Execute(strSql + insertSql);

                        sqlConnection.Close();

                    }
                    return count;
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }

            }
        }
        /// <summary>
        /// 查询HIS系统中科室、医师、病区、床位的基本信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<QueryInformationInfoDto> QueryInformationInfo(InformationParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = @"select Id, [OrganizationCode],[DirectoryType], [DirectoryCode],[DirectoryName],[FixedEncoding],
                                [DirectoryCategoryName],[Remark] from [dbo].[HospitalGeneralCatalog] where IsDelete=0 ";
                    if (!string.IsNullOrWhiteSpace(param.DirectoryName)) strSql += " and DirectoryName like '" + param.DirectoryName + "%'";
                    if (!string.IsNullOrWhiteSpace(param.DirectoryType)) strSql += $" and DirectoryType='{param.DirectoryType}'";
                    var data = sqlConnection.Query<QueryInformationInfoDto>(strSql);
                    sqlConnection.Close();
                    return data.ToList();
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
            }
        }
        /// <summary>
        /// 门诊不传医保查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<OutpatientExclusion>> OutpatientExclusionQuery(OutpatientExclusionQueryParam param)
        {
            List<OutpatientExclusion> dataList;
            var resultData = new Dictionary<int, List<OutpatientExclusion>>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                string whereSql = "";
                try
                {
                    sqlConnection.Open();
                    querySql = $@"select * from [dbo].[OutpatientExclusion] where IsDelete=0 and OrganizationCode='{param.OrganizationCode}'";
                    string countSql = $@"select count(*) from [dbo].[OutpatientExclusion] where IsDelete=0 and OrganizationCode='{param.OrganizationCode}'";
                   
                   
                    if (!string.IsNullOrWhiteSpace(param.DirectoryName))
                    {
                        whereSql += $"  and DirectoryName like '%{param.DirectoryName}%'";
                    }
                   
                    if (param.Limit != 0 && param.Page > 0)
                    {
                        var skipCount = param.Limit * (param.Page - 1);
                        querySql += whereSql + " order by CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.Limit + " ROWS ONLY;";
                    }
                    string executeSql = countSql + whereSql + ";" + querySql;

                    var result = sqlConnection.QueryMultiple(executeSql);

                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<OutpatientExclusion>()

                                select t).ToList();

                    resultData.Add(totalPageCount, dataList);
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
        /// 门诊不传医保查询
        /// </summary>
        /// <param name="organizationCode">组织机构编号</param>
        /// <returns></returns>
        public List<OutpatientExclusion> OutpatientExclusionListQuery(string organizationCode)
        {
          
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                try
                {
                    sqlConnection.Open();
                    querySql = $@"select * from [dbo].[OutpatientExclusion] where IsDelete=0 and OrganizationCode='{organizationCode}'";
                    var result = sqlConnection.Query<OutpatientExclusion>(querySql).ToList();

                    sqlConnection.Close();
                    return result;

                }
                catch (Exception e)
                {
                    _log.Debug(querySql);
                    throw new Exception(e.Message);
                }


            }
        }
        //取消门诊不传医保
        public int OutpatientExclusionCancel(OutpatientExclusionCancelParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $@" update [dbo].[OutpatientExclusion] set IsDelete=1 ,DeleteUserId='{param.UserId}',DeleteTime=getDate() 
                               where Id='{param.Id}'";
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
        //添加门诊不传医保
        public int OutpatientExclusionAdd(OutpatientExclusionAddParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                     strSql = $@" update [dbo].[OutpatientExclusion] set IsDelete=1 ,DeleteUserId='{param.User.UserId}',DeleteTime=getDate() 
                                  where OrganizationCode='{param.User.OrganizationCode}' and IsDelete=0 and DirectoryCode='{param.DirectoryCode}';
                                 insert into [dbo].[OutpatientExclusion]([id],[DirectoryCode],[DirectoryName],[DirectoryCategoryName],
                                 [OrganizationCode],[OrganizationName],CreateTime,CreateUserId,IsDelete,CreateUserName)
                                 values('{Guid.NewGuid()}','{param.DirectoryCode}','{param.DirectoryName}','{param.DirectoryCategoryName}',
                                 '{param.User.OrganizationCode}','{param.User.OrganizationName}',getDate(),'{param.User.UserId}',0,'{param.User.UserName}');";
                    
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
        public int DeleteDatabase(DeleteDatabaseParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $@" update {param.TableName} set IsDelete=1 ,DeleteUserId='{param.User.UserId}',DeleteTime=GETDATE() 
                               where {param.Field}='{param.Value}' and IsDelete=0";
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
        /// 门诊居民报账月报表
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<MedicalExpenseMonthReportDto>> MedicalExpenseMonthReport(MedicalExpenseMonthReportParam param)
        {
            var dataListNew = new List<MedicalExpenseReportDto>();
            var resultData = new Dictionary<int, List<MedicalExpenseMonthReportDto>>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                string whereSql = "";
                try
                {
                    sqlConnection.Open();
                    querySql = @"
							select a.id,a.BusinessId,a.OrganizationCode,a.DiagnosticJson,a.[OrganizationName], a.[PatientName],a.[IdCardNo],a.[Operator],
                             a.[VisitDate],b.CommunityName,b.SettlementUserName,b.[SettlementTime],b.[SelfPayFeeAmount],b.[PatientId],
                             a.[MedicalTreatmentTotalCost],b.[ContactAddress],b.ReimbursementExpensesAmount,b.[CarryOver],b.ContactPhone  
                            from [dbo].[Outpatient] as a  JOIN (select * from [dbo].[MedicalInsurance] where  IsDelete=0 and 
							  [InsuranceType]=342 and [MedicalInsuranceState]=6  
							  and [PatientId] is not null and PatientId<>'') as b  on  a.Id=b.PatientId
							  where a.IsDelete=0 ";
            
                    if (!string.IsNullOrWhiteSpace(param.OrganizationCode))
                        whereSql += $"  and a.OrganizationCode='{param.OrganizationCode}'";

                    if (!string.IsNullOrWhiteSpace(param.Date) )
                    {
                       var monthTime= CommonHelp.GetMonthTime(param.Date);
                        whereSql += $"  and b.SettlementTime>='{monthTime.StartTime}' and b.SettlementTime<='{monthTime.EndTime}'";
                    }

                    
                    string executeSql = querySql + whereSql;

                    var result = sqlConnection.Query<MedicalExpenseReportDto>(executeSql).ToList();

                    if (result.Count > 0)
                    {
                        var idCardNoList = result.GroupBy(g => g.IdCardNo).Where(s => s.Count() > 1).Select(c => c.Key).ToList();
                        var idList = result.Where(c => idCardNoList.Contains(c.IdCardNo)).ToList();
                        var repeatData = MedicalExpenseRepeat(idList);

                        foreach (var item in result)
                        {
                            var repeatValue = repeatData.FirstOrDefault(c => c.Id == item.Id);
                            var itemData = new MedicalExpenseReportDto()
                            {
                                SettlementTime = item.SettlementTime,
                                BusinessId = item.BusinessId,
                                ContactAddress = item.ContactAddress,
                                CarryOver = item.CarryOver,
                                CommunityName = item.CommunityName,
                                ContactPhone = item.ContactPhone,
                                DiagnosticJson = GetDiagnosticContent(item.DiagnosticJson),
                                Id = item.Id,
                                IdCardNo = item.IdCardNo,
                                MedicalTreatmentTotalCost = item.MedicalTreatmentTotalCost,
                                ReimbursementExpensesAmount = item.ReimbursementExpensesAmount,
                                SettlementUserName = item.SettlementUserName,
                                PatientName = item.PatientName,
                                VisitDate = item.VisitDate,
                                OrganizationName = item.OrganizationName,
                                OrganizationCode = item.OrganizationCode,
                                Sign = repeatValue == null ? 1 : 0,
                                Operator = item.Operator,
                                PatientId = item.PatientId

                            };
                            dataListNew.Add(itemData);
                        }

                    }

                 
                    var getListData = MedicalExpenseMonthReportList(dataListNew,param.Date);
                    if (getListData.Any())
                    {
                        var totalData = new MedicalExpenseMonthReportDto()
                        {
                            OrganizationName = getListData.Select(d=>d.OrganizationName).FirstOrDefault(),
                            Day = "合计",
                            Frequency = getListData.Sum(d=>d.Frequency),
                            MedicalTreatmentTotalCost = getListData.Sum(d => d.MedicalTreatmentTotalCost),
                            ReimbursementExpensesAmount = getListData.Sum(d => d.ReimbursementExpensesAmount),

                        };
                        getListData.Add(totalData);
                    }
                

                    resultData.Add(getListData.Count(), getListData);
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
        /// 门诊居民月统计导出
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataTable MedicalExpenseMonthExcel( MedicalExpenseMonthReportParam param)
        {
            var resultData=new DataTable();
            var monthData = MedicalExpenseMonthReport(param);
            var monthList = monthData.Values.FirstOrDefault();
            var monthListData = new List<MedicalExpenseMonthExcelDto>();
            if (monthList != null && monthList.Any())
            {
                monthListData = monthList.Select(d => new MedicalExpenseMonthExcelDto
                {
                    日期 = d.Day,
                    报销金额 = d.ReimbursementExpensesAmount,
                    一般诊疗费人次 = d.Frequency,
                    门诊费用=d.MedicalTreatmentTotalCost

                }).ToList();
            }

            //MedicalExpenseMonthExcelDto
            resultData = ListToDataTable(monthListData);

            return resultData;
        }
        /// <summary>
        /// 门诊居民年统计导出
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataTable MedicalExpenseYearExcel(MedicalExpenseYearReportParam param)
        {
           
            var yearData = MedicalExpenseYearReport(param);
            var yearList = yearData.Values.FirstOrDefault();
             var yearListData = new List< MedicalExpenseYearExcelDto>();
            if (yearList != null && yearList.Any())
            {
                yearListData = yearList.Select(d => new MedicalExpenseYearExcelDto
                {
                    机构名称 = d.OrganizationName,
                    汇总月份 = d.Month,
                    总汇总人次 = d.Frequency

                }).ToList();
            }


            //MedicalExpenseMonthExcelDto
            var resultData = ListToDataTable(yearListData);

            return resultData;
        }

        /// <summary>
        /// 年报表统计
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>

        public Dictionary<int, List<MedicalExpenseYearReportDto>> MedicalExpenseYearReport(
            MedicalExpenseYearReportParam param)
        {
            var resultData = new Dictionary<int, List<MedicalExpenseYearReportDto>>();

            var yearList = new List<MedicalExpenseYearReportDto>();
            int count = 12;
            for (int i = 1; i <= count; i++)
            {
                var day = param.Date + (i >= 10 ? "-" + i : "-0" + i);
                var monthList =  MedicalExpenseMonthReport(new MedicalExpenseMonthReportParam()
                {
                    Date = day,
                    OrganizationCode = param.OrganizationCode
                });
                var monthListData = monthList.Values.FirstOrDefault();
                if (monthListData!=null && monthListData.Any())
                {
                    var yearData = new MedicalExpenseYearReportDto()
                    {
                        OrganizationName = monthListData.Select(d=>d.OrganizationName).FirstOrDefault(),
                        Frequency = monthListData.Where(d=>d.Day=="合计").Select(c=>c.Frequency).FirstOrDefault(),
                        Month = day
                    };
                    yearList.Add(yearData);
                }
            }

            if (yearList.Any())
            {
                var totalYear = new MedicalExpenseYearReportDto()
                {
                    OrganizationName = "",
                    Frequency = yearList.Sum(d=>d.Frequency),
                    Month = "合计"
                };
                yearList.Add(totalYear);
            }

            resultData.Add(yearList.Count(), yearList);
            return resultData;
        }

        /// <summary>
        /// 门诊居民挂号费报销
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Dictionary<int, List<MedicalExpenseReportDto>> MedicalExpenseReport(MedicalExpenseReportParam param)
        {
            List<MedicalExpenseReportDto> dataList;
            var dataListNew = new List<MedicalExpenseReportDto>();
            var resultData = new Dictionary<int, List<MedicalExpenseReportDto>>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                string whereSql = "";
                try
                {
                    sqlConnection.Open();
                    querySql = @"
							select a.id,a.BusinessId,a.OrganizationCode,a.DiagnosticJson,a.[OrganizationName], a.[PatientName],a.[IdCardNo],a.[Operator],
                             a.[VisitDate],b.CommunityName,b.SettlementUserName,b.[SettlementTime],b.[SelfPayFeeAmount],b.[PatientId],
                             a.[MedicalTreatmentTotalCost],b.[ContactAddress],b.ReimbursementExpensesAmount,b.[CarryOver],b.ContactPhone  
                            from [dbo].[Outpatient] as a  JOIN (select * from [dbo].[MedicalInsurance] where  IsDelete=0 and 
							  [InsuranceType]=342 and [MedicalInsuranceState]=6  
							  and [PatientId] is not null and PatientId<>'') as b  on  a.Id=b.PatientId
							  where a.IsDelete=0";
                    string countSql = @"select COUNT(*) from [dbo].[Outpatient] as a  JOIN (select *from [dbo].[MedicalInsurance] where  IsDelete=0 and 
							  [InsuranceType]=342 and [MedicalInsuranceState]=6  
							  and [PatientId] is not null and PatientId<>'') as b  on  a.Id=b.PatientId
							  where a.IsDelete=0";
                    if (!string.IsNullOrWhiteSpace(param.OrganizationCode))
                        whereSql += $"  and a.OrganizationCode='{param.OrganizationCode}'";
                    if (!string.IsNullOrWhiteSpace(param.PatientName))
                        whereSql += $"  and a.PatientName like '%{param.PatientName}%'";
                    if (!string.IsNullOrWhiteSpace(param.OrganizationName))
                        whereSql += $"  and a.OrganizationName like '%{param.OrganizationName}%'";
                    if (!string.IsNullOrWhiteSpace(param.IdCardNo))
                        whereSql += $"  and a.IdCardNo='{param.IdCardNo}'";
                    if (!string.IsNullOrWhiteSpace(param.StartTime) && !string.IsNullOrWhiteSpace(param.EndTime))
                    {
                        whereSql += $"  and b.SettlementTime>='{param.StartTime+ " 00:00:00.000"}' and b.SettlementTime<='{param.EndTime+ " 23:59:59.000"}'";
                    }

                    if (param.rows!= 0 && param.Page > 0)
                    {
                        var skipCount = param.rows * (param.Page - 1);
                        querySql += whereSql + " order by b.SettlementTime asc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.rows + " ROWS ONLY;";
                    }
                    string executeSql = countSql + whereSql + ";" + querySql;

                    var result = sqlConnection.QueryMultiple(executeSql);

                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<MedicalExpenseReportDto>()

                        select t).ToList();
                    if (totalPageCount > 0)
                    {
                        var idCardNoList = dataList.GroupBy(g => g.IdCardNo).Where(s => s.Count() > 1).Select(c=>c.Key).ToList();
                        var idList = dataList.Where(c => idCardNoList.Contains(c.IdCardNo)).ToList();
                        var repeatData = MedicalExpenseRepeat(idList);
                        
                        foreach (var item in dataList)
                        {
                            var repeatValue = repeatData.FirstOrDefault(c => c.Id == item.Id);
                            var itemData = new MedicalExpenseReportDto()
                            {
                                SettlementTime = item.SettlementTime,
                                BusinessId = item.BusinessId,
                                ContactAddress = item.ContactAddress,
                                CarryOver = item.CarryOver,
                                CommunityName = item.CommunityName,
                                ContactPhone = item.ContactPhone,
                                DiagnosticJson = GetDiagnosticContent(item.DiagnosticJson),
                                Id = item.Id,
                                IdCardNo = item.IdCardNo,
                                MedicalTreatmentTotalCost = item.MedicalTreatmentTotalCost,
                                ReimbursementExpensesAmount = item.ReimbursementExpensesAmount,
                                SettlementUserName = item.SettlementUserName,
                                PatientName = item.PatientName,
                                VisitDate = item.VisitDate,
                                OrganizationName = item.OrganizationName,
                                OrganizationCode = item.OrganizationCode,
                                Sign = repeatValue==null?1:0,
                                Operator= item.Operator,
                                PatientId=item.PatientId

                            };
                            dataListNew.Add(itemData);
                        }

                    }

                    resultData.Add(totalPageCount, dataListNew.OrderBy(d=>d.SettlementTime).ToList());
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

        public DataTable MedicalExpenseReportExcel(MedicalExpenseReportUiParam param)
        {
            var resultData = new DataTable();
            List<MedicalExpenseReportDto> dataList;
            var dataListNew = new List<MedicalExpenseReportExcelDto>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string querySql = null;
                string whereSql = "";
                try
                {
                    sqlConnection.Open();
                    querySql = @"
							  select a.id,a.BusinessId,a.OrganizationCode,a.DiagnosticJson,a.[OrganizationName], a.[PatientName],a.[IdCardNo],a.[Operator],
                             a.[VisitDate],b.CommunityName,b.SettlementUserName,b.[SettlementTime],b.[SelfPayFeeAmount],
                             a.[MedicalTreatmentTotalCost],b.[ContactAddress],b.ReimbursementExpensesAmount,b.[CarryOver],b.ContactPhone  from [dbo].[Outpatient] as a  JOIN (select *from [dbo].[MedicalInsurance] where  IsDelete=0 and 
							  [InsuranceType]=342 and [MedicalInsuranceState]=6  
							  and [PatientId] is not null and PatientId<>'') as b  on  a.Id=b.PatientId
							  where a.IsDelete=0
";
                   
                    if (!string.IsNullOrWhiteSpace(param.OrganizationCode))
                        whereSql += $"  and a.OrganizationCode='{param.OrganizationCode}'";
                    if (!string.IsNullOrWhiteSpace(param.PatientName))
                        whereSql += $"  and a.PatientName like '%{param.PatientName}%'";
                    if (!string.IsNullOrWhiteSpace(param.OrganizationName))
                        whereSql += $"  and a.OrganizationName like '%{param.OrganizationName}%'";
                    if (!string.IsNullOrWhiteSpace(param.IdCardNo))
                        whereSql += $"  and a.IdCardNo='{param.IdCardNo}'";
                    if (!string.IsNullOrWhiteSpace(param.StartTime) && !string.IsNullOrWhiteSpace(param.EndTime))
                    {
                        whereSql += $"  and b.SettlementTime>='{param.StartTime + " 00:00:00.000"}' and b.SettlementTime<='{param.EndTime + " 23:59:59.000"}'";
                    }

                    //querySql += whereSql + " order by b.SettlementTime asc";
                    string executeSql = querySql + whereSql;

                     dataList = sqlConnection.Query<MedicalExpenseReportDto>(executeSql).ToList();

                 
                 
                    if (dataList.Any())
                    {
                        var idCardNoList = dataList.GroupBy(g => g.IdCardNo).Where(s => s.Count() > 1).Select(c => c.Key).ToList();
                        var idList = dataList.Where(c => idCardNoList.Contains(c.IdCardNo)).ToList();
                        var repeatData = MedicalExpenseRepeat(idList);

                        foreach (var item in dataList)
                        {
                            var repeatValue = repeatData.FirstOrDefault(c => c.Id == item.Id);
                            var itemData = new MedicalExpenseReportExcelDto()
                            {
                                报销日期 = item.SettlementTime,
                                历年结转 = item.CarryOver,
                                参保地 = item.CommunityName,
                                联系电话 = item.ContactPhone,
                                诊断 = GetDiagnosticContent(item.DiagnosticJson),
                                身份证号 = item.IdCardNo,
                                门诊费用 = item.MedicalTreatmentTotalCost,
                                门诊报销 = item.ReimbursementExpensesAmount,
                                患者姓名 = item.PatientName,
                                就诊日期 = item.VisitDate,
                                就诊机构 = item.OrganizationName,
                                标记 = repeatValue == null ? 1 : 0,
                                经办人 = item.Operator,
                                家庭住址=item.ContactAddress
                            };
                            dataListNew.Add(itemData);
                        }

                    }
                    
                    resultData = ListToDataTable(dataListNew.OrderBy(c=>c.报销日期).ToList());
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
        ///获取门诊居民报账 重复数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<MedicalExpenseReportDto> MedicalExpenseRepeat(List<MedicalExpenseReportDto> paramNew)
        {
            var param = paramNew.OrderBy(c => c.SettlementTime).ToList();
            var resultData = new List<MedicalExpenseReportDto>();
           var getDays= CommonHelp.GetDays();
           if (getDays==null) throw  new Exception("获取报表配置天数失败!!!");


            foreach (var item in param)
            {//
                var startTime = Convert.ToDateTime(item.SettlementTime.ToString("yyyy-MM-dd") + " 00:00:00.000");
                var startEnd = Convert.ToDateTime(item.SettlementTime.AddDays(Convert.ToInt16(getDays)).ToString("yyyy-MM-dd") + " 23:59:59.000");
               // var lastDayOfMonth = LastDayOfMonth(item.VisitDate);
                var idList = resultData.Select(d => d.Id).ToList();
                var itemValueList = param.Where(c =>
                        c.SettlementTime > startTime && c.SettlementTime < startEnd && 
                        c.IdCardNo==item.IdCardNo && 
                        c.OrganizationCode == item.OrganizationCode && !idList.Contains(c.Id))
                    .OrderBy(d=>d.SettlementTime).ToList();

                if (itemValueList.Count > 1)
                {
                    int i = 0;
                    foreach (var itemValue in itemValueList)
                    {   //排除第一个为1,其余为0
                        if (i > 0)
                        {
                            resultData.Add(itemValue);
                        }

                        i++;

                    }

                }
            }

            return resultData;
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="param"></param>
        public void ExecuteSql(string param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = param;
                try
                {
                   
                    if (!string.IsNullOrWhiteSpace(strSql))
                    {
                        sqlConnection.Open();
                        var data = sqlConnection.Execute(strSql);
                        sqlConnection.Close();
                       
                    }

                  
                  
                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }

            }
        }
        /// <summary>
        /// 查询组织机构病人信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<QueryOrganizationInpatientInfoDto>> QueryOrganizationInpatientInfo(QueryOrganizationInpatientInfoParam param )
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var resultData = new Dictionary<int, List<QueryOrganizationInpatientInfoDto>>();
               
                var dataListNew = new List<QueryOrganizationInpatientInfoDto>();
                string executeSql = null;
                try
                {
                    string querySql = $@"select  a.BusinessId,a.[HospitalizationNo],a.[PatientName],a.AdmissionDate,a.[IdCardNo] 
                             from [dbo].[Inpatient] as a inner join [dbo].[MedicalInsurance]  as b
                             on a.BusinessId=b.BusinessId 
                             where a.IsDelete=0 and b.IsDelete=0  and a.IsCanCelHospitalized is null
                             and b.MedicalInsuranceState<5 and a.OrganizationCode='{param.OrganizationCode}' and 
                             b.OrganizationCode='{param.OrganizationCode}'";

                    string countSql = $@"select  COUNT(*) 
                             from [dbo].[Inpatient] as a inner join [dbo].[MedicalInsurance]  as b
                             on a.BusinessId=b.BusinessId 
                             where a.IsDelete=0 and b.IsDelete=0 and a.IsCanCelHospitalized is null 
                             and b.MedicalInsuranceState<5 and a.OrganizationCode='{param.OrganizationCode}' and 
                             b.OrganizationCode='{param.OrganizationCode}'";
                    string regexstr = @"[\u4e00-\u9fa5]";
                    string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(param.SearchKey))
                    {
                        if (Regex.IsMatch(param.SearchKey, regexstr))
                        {
                            whereSql += " and PatientName like '%" + param.SearchKey + "%'";
                        }
                        else
                        {
                            whereSql += " and (HospitalizationNo like '%" + param.SearchKey + "%' or IdCardNo like '%" + param.SearchKey + "%')";
                        }
                    }


                    if (param.Limit != 0 && param.Page > 0)
                    {
                        var skipCount = param.Limit * (param.Page - 1);
                        querySql += whereSql + " order by a.CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.Limit + " ROWS ONLY;";
                    }

                   
                    executeSql = countSql + whereSql + ";" + querySql;
                    if (!string.IsNullOrWhiteSpace(executeSql))
                    {
                        sqlConnection.Open();

                        var result = sqlConnection.QueryMultiple(executeSql);
                       
                        int totalPageCount = result.Read<int>().FirstOrDefault();
                        var  dataList = (from t in result.Read<QueryOrganizationInpatientInfoDto>()
                            select t).ToList();
                        //查询费用明细

                        if (dataList.Any())
                        {
                            var businessIdList = dataList.Select(c => c.BusinessId).ToList();
                            string businessIdStr =  CommonHelp.ListToStr(businessIdList);
                            string queryDetailSql = $@"select  [HospitalizationId] as BusinessId,[DetailId],[UploadMark] from
                                                    [dbo].[HospitalizationFee] where HospitalizationId in ({businessIdStr}) and IsDelete=0";
                            var queryDetailData = sqlConnection.Query<OrganizationInpatientDetailDto>(queryDetailSql).ToList();
                            if (queryDetailData.Any())
                            {
                                foreach (var item in dataList)
                                {
                                    var detailData = queryDetailData.Where(c => c.BusinessId == item.BusinessId)
                                        .ToList();
                                    int allNum=0, notUploadNum=0, uploadNum=0;
                                    if (detailData.Any())
                                    {
                                        allNum = detailData.Count;
                                        uploadNum = 0;
                                        foreach (var c in detailData)
                                        {
                                            if (c.UploadMark == 1) uploadNum++;
                                        }

                                        notUploadNum = allNum - uploadNum;
                                    }

                                    var itemData = new QueryOrganizationInpatientInfoDto
                                    {
                                        BusinessId = item.BusinessId,
                                        IdCardNo = item.IdCardNo,
                                        AdmissionDate = item.AdmissionDate,
                                        
                                        HospitalizationNo = item.HospitalizationNo,
                                        AllNum = allNum,
                                        NotUploadNum = notUploadNum,
                                        UploadNum = uploadNum,
                                        PatientName = item.PatientName

                                    };
                                    dataListNew.Add(itemData);
                                }

                            }

                            if (dataListNew.Count == 0) dataListNew = dataList;
                        }

                        resultData.Add(totalPageCount, dataListNew);
                        sqlConnection.Close();

                    }
                }
                catch (Exception e)
                {
                    _log.Debug(executeSql);
                    throw new Exception(e.Message);
                }

                return resultData;
            }
        }
        /// <summary>
        /// 查询病人信息
        /// </summary>
        public Dictionary<int, List<QueryPatientInfoDto>> QueryPatientInfo(QueryPatientInfoParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                List<QueryPatientInfoDto> dataList;
                var dataListNew =new List<QueryPatientInfoDto>();
                var resultData = new Dictionary<int, List<QueryPatientInfoDto>>();
                string executeSql = null;
                try
                {
                    sqlConnection.Open();
                    string countSql  = $@"
                            select count(*) from [dbo].[Inpatient] as a,[dbo].[MedicalInsurance] as b
                             where a.BusinessId=b.BusinessId and a.IsDelete=0 and b.IsDelete=0
                           and a.[OrganizationCode]='{param.OrganizationCode}'";
                    string querySql = $@"select [PatientName], [IdCardNo],[HospitalizationNo] as NumCode,[MedicalInsuranceState] 
                             ,a.[CreateTime],a.BusinessId as Id from [dbo].[Inpatient] as a,[dbo].[MedicalInsurance] as b
                             where a.BusinessId=b.BusinessId and a.IsDelete=0 and b.IsDelete=0
                              and a.[OrganizationCode]='{param.OrganizationCode}'";

                    if (param.IsOutpatient == "1")
                    {
                        countSql = $@" select  count(*) from [dbo].[Outpatient] as a,[dbo].[MedicalInsurance] as b
                                 where b.PatientId is not null and b.PatientId<>'' and a.id=b.PatientId and a.IsDelete=0 and b.IsDelete=0 
                                  and MedicalInsuranceState=6 and a.[OrganizationCode]='{param.OrganizationCode}'";
                        querySql = $@" select [PatientName], [IdCardNo],[OutpatientNumber] as NumCode,[MedicalInsuranceState] 
                              ,a.[CreateTime],a.BusinessId as Id from [dbo].[Outpatient] as a,[dbo].[MedicalInsurance] as b
                             where b.PatientId is not null and b.PatientId<>'' and a.id=b.PatientId and a.IsDelete=0 and b.IsDelete=0 
                              and MedicalInsuranceState=6 and a.[OrganizationCode]='{param.OrganizationCode}'";
                    }

                    string regexstr = @"[\u4e00-\u9fa5]";
                    string whereSql = "";

                    if (!string.IsNullOrWhiteSpace(param.KeyWord))
                    {
                        if (Regex.IsMatch(param.KeyWord, regexstr))
                        {
                            whereSql += " and a.PatientName like '%" + param.KeyWord + "%'";
                        }
                        else
                        {
                            whereSql += $" and a.IdCardNo ='{param.KeyWord}'";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(param.StartTime) && !string.IsNullOrWhiteSpace(param.EndTime))
                        whereSql += $" and a.CreateTime between '{param.StartTime}' and '{param.EndTime.Trim()+ " 23:59:59.000"}'";
                    if (param.rows != 0 && param.Page > 0)
                    {
                        var skipCount = param.rows * (param.Page - 1);
                        querySql += whereSql + " order by a.CreateTime desc OFFSET " + skipCount + " ROWS FETCH NEXT " + param.rows + " ROWS ONLY;";
                    }
                    executeSql = countSql + whereSql + ";" + querySql;
                    var result = sqlConnection.QueryMultiple(executeSql);
                    int totalPageCount = result.Read<int>().FirstOrDefault();
                    dataList = (from t in result.Read<QueryPatientInfoDto>()
                                select t).ToList();
                    foreach (var item in dataList)
                    {
                        var mumCode = item.NumCode;
                        if (param.IsOutpatient == "1")
                        { //拆分门诊号
                            string[] arr = mumCode.Split('M');
                            mumCode = arr[1];
                        }

                        var itemData = new QueryPatientInfoDto()
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            IdCardNo = item.IdCardNo,
                            NumCode = mumCode,
                            PatientName = item.PatientName,
                            MedicalInsuranceState = ((MedicalInsuranceState)Convert.ToInt32(item.MedicalInsuranceState)).ToString()
                        };
                        dataListNew.Add(itemData);
                    }
                    resultData.Add(totalPageCount, dataListNew);
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
        /// 
        /// </summary>
        /// <param name="organizationCode"></param>
        /// <returns></returns>
        public DataTable MedicalInsurancePairCodeTableData( string organizationCode)
        {
            DataTable table = new DataTable("MyTable");
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string strSql = null;
                try
                {
                    sqlConnection.Open();
                    strSql = $@"select a.[DirectoryCode] as 医院目录编码商品ID,
                    a.DirectoryName as 医院目录名称,a.DirectoryCategoryName as 医院目录类别, [Unit] as
                    医院单位 ,a.Specification as 医院规格,[ManufacturerName] as 医院生产厂家,
                    c.ProjectCode as 医保项目编码,c.[ProjectName] as 医保项目名称,
                    c.ProjectCodeType as 医保项目类别,
                    c.QuasiFontSize as 药品准字号,
                    c.Manufacturer as 医保生产厂家,
                    c.ProjectLeve as 医保报销类别

                    from [dbo].[HospitalThreeCatalogue] as a 
                    left join 
                    (select b.DirectoryCode,c.ProjectCode,[ProjectName],(CASE c.[ProjectCodeType]
                    WHEN '92' THEN '其它全自费'
                    WHEN '11' THEN '西药费'
                    WHEN '12' THEN '中成药'
                    WHEN '13' THEN '中草药'
                    WHEN '21' THEN '检查费'
                    WHEN '22' THEN '治疗费'
                    WHEN '23' THEN '放射费'
                    WHEN '24' THEN '手术费'
                    WHEN '25' THEN '化验费'
                    WHEN '26' THEN '输血费'
                    WHEN '27' THEN '诊疗费'
                    WHEN '28' THEN '核医学'
                    WHEN '29' THEN '输氧费'
                    WHEN '31' THEN '护理费'
                    WHEN '32' THEN '床位费'
                    WHEN '33' THEN '注射费'
                    WHEN '34' THEN '病理费'
                    WHEN '35' THEN '理疗费'
                    WHEN '36' THEN '麻醉费'
                    WHEN '37' THEN '抢救费'
                    WHEN '38' THEN '特殊人员床位费'
                    WHEN '40' THEN '特殊检查费'
                    WHEN '41' THEN '特殊材料'
                    WHEN '50' THEN '特殊治疗费'
                    WHEN '60' THEN '血液蛋白质制品'
                    WHEN '81' THEN '材料费'
                    WHEN '91' THEN '其他费用'
                     END) as ProjectCodeType,
                    (CASE c.[ProjectLevel]
                    WHEN '1' THEN '甲类'
                    WHEN '2' THEN '乙类'
                    ELSE '丙类' END) as ProjectLeve,
                    c.[QuasiFontSize],c.[Manufacturer],
                    c.[ResidentSelfPayProportion]
                    ,c.[WorkersSelfPayProportion]
                    From [dbo].[ThreeCataloguePairCode]  as b 
                    inner join  [dbo].[MedicalInsuranceProject] as c on b.[ProjectCode]=c.[ProjectCode]
                    where  b.IsDelete =0 and 
                    b.OrganizationCode='{organizationCode}'
                    and c.IsDelete=0) as c  on  a.[DirectoryCode]=c.DirectoryCode
                    where  a.IsDelete =0 and 
                    a.OrganizationCode='{organizationCode}'  order by a.DirectoryCategoryName desc ";
                    var data = sqlConnection.ExecuteReader(strSql);
                    table.Load(data);
                    sqlConnection.Close();

                   

                }
                catch (Exception e)
                {
                    _log.Debug(strSql);
                    throw new Exception(e.Message);
                }
                return table;
            }
        }

        //public  void 
        public List<T> QueryDatabase<T>(T t, DatabaseParam param)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                string sqlStr = null;
                try
                {
                    sqlConnection.Open();
                    sqlStr = $@"select * from {param.TableName} where {param.Field}='{param.Value}' and IsDelete=0";
                    var data = sqlConnection.Query<T>(sqlStr).ToList();
                    sqlConnection.Close();
                    return data;
                }
                catch (Exception e)
                {

                    _log.Debug(sqlStr);
                    throw new Exception(e.Message);
                }

            }
        }
        //private  string 
        /// <summary>
        /// 获取诊断内容
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        private string GetDiagnosticContent( string jsonData)
        {
            string resultData = "";
            string jsonStr = "";

            if (jsonData != "[]")
            {
                var data = JsonConvert.DeserializeObject<List<InpatientDiagnosisDataDto>>(jsonData);

                foreach (var item in data)
                {
                    resultData += item.DiseaseName + ",";
                }

                resultData = resultData.Substring(0, resultData.Length - 1);
            }

           
          
            return resultData;
        }

        private decimal GetBlockPrice(QueryMedicalInsurancePairCodeDto param, OrganizationGrade grade)
        {
            decimal resultData = 0;
            if (grade == OrganizationGrade.二级乙等以下) resultData = param.ZeroBlock;
            if (grade == OrganizationGrade.二级乙等) resultData = param.OneBlock;
            if (grade == OrganizationGrade.二级甲等) resultData = param.TwoBlock;
            if (grade == OrganizationGrade.三级乙等) resultData = param.ThreeBlock;
            if (grade == OrganizationGrade.三级甲等) resultData = param.FourBlock;

            return resultData;
        }
        private decimal GetSelfPayProportion(QueryMedicalInsurancePairCodeDto param, MedicalInsuranceResidentInfoDto residentInfo)
        {
            decimal resultData = 0;
            //居民
            if (residentInfo.InsuranceType == "342") resultData = param.ResidentSelfPayProportion;
            //职工
            if (residentInfo.InsuranceType == "310") resultData = param.WorkersSelfPayProportion;
            return resultData;
        }
        /// <summary>
        /// 将泛类型集合List类转换成DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        private DataTable ListToDataTable<T>(List<T> entitys)
        {
            //检查实体集合不能为空
            if (entitys == null || entitys.Count < 1)
            {
                throw new Exception("需转换的集合为空");
            }
            //取出第一个实体的所有Propertie
            Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entityType.GetProperties();

            //生成DataTable的structure
            //生产代码中，应将生成的DataTable结构Cache起来，此处略
            DataTable dt = new DataTable();
            for (int i = 0; i < entityProperties.Length; i++)
            {
                //dt.Columns.Add(entityProperties[i].Name, entityProperties[i].PropertyType);
                dt.Columns.Add(entityProperties[i].Name);
            }
            //将所有entity添加到DataTable中
            foreach (object entity in entitys)
            {
                //检查所有的的实体都为同一类型
                if (entity.GetType() != entityType)
                {
                    throw new Exception("要转换的集合元素类型不一致");
                }
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }

        private List<MedicalExpenseMonthReportDto> MedicalExpenseMonthReportList(List<MedicalExpenseReportDto> param, string date)
        {
            var resultData = new List<MedicalExpenseMonthReportDto>();
            var monthTime = CommonHelp.GetMonthTime(date);
            var maxDay=Convert.ToInt16(monthTime.EndTime.Substring(8, 2)); // 
            for (int i = 1; i <= maxDay; i++)
            {//GetDayTime
                var day = date+(i >= 10 ? "-" + i : "-0" + i);
                var dayTime = CommonHelp.GetDayTime(day);
                var queryData = param.Where(c => c.SettlementTime >=Convert.ToDateTime(dayTime.StartTime) 
                        && c.SettlementTime <= Convert.ToDateTime(dayTime.EndTime) && c.Sign == 1
                                               ) .ToList();
                if (queryData.Any())
                {
                    var item = new MedicalExpenseMonthReportDto()
                    {
                        Day = day,
                        OrganizationName = queryData.Select(d=>d.OrganizationName).FirstOrDefault(),
                        Frequency = queryData.Count(),
                        MedicalTreatmentTotalCost = queryData.Sum(d=>d.MedicalTreatmentTotalCost),
                        ReimbursementExpensesAmount = queryData.Sum(d => d.ReimbursementExpensesAmount)
                    };
                    resultData.Add(item);
                }

              
            }

            return resultData;
        }

        /// <summary>
        /// 取得某月的最后一天
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        private DateTime LastDayOfMonth(DateTime datetime)
        {
            var monthTime = datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
            return Convert.ToDateTime(monthTime.ToString("yyyy-MM-dd") + " 23:59:59.000");
        }

    }
}
