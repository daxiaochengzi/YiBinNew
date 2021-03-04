﻿using BenDing.Domain.Models.Dto.Web;
using BenDing.Domain.Models.Params.Web;
using System;
using System.Collections.Generic;
using System.Data;
using BenDing.Domain.Models.Dto.JsonEntity;
using BenDing.Domain.Models.Dto.OutpatientDepartment;
using BenDing.Domain.Models.Params.Base;
using BenDing.Domain.Models.Params.OutpatientDepartment;
using BenDing.Domain.Models.Params.Resident;
using BenDing.Domain.Models.Params.SystemManage;
using BenDing.Domain.Models.Params.UI;

namespace BenDing.Service.Interfaces
{
    public interface IWebServiceBasicService
    {/// <summary>
     /// 获取验证码
     /// </summary>
     /// <param name="tradeCode"></param>
     /// <param name="inputParameter"></param>
     /// <returns></returns>
        UserInfoDto GetVerificationCode(string tradeCode, string inputParameter);
        /// <summary>
        /// 获取住院病人
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        InpatientInfoDto GetInpatientInfo(GetInpatientInfoParam param);

      
        /// <summary>
        /// 获取机构
        /// </summary>
        /// <param name="userinfo"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Int32 GetOrg(UserInfoDto userinfo, string name);
        /// <summary>
        /// 获取三大目录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        string GetCatalog(UserInfoDto user, CatalogParam param);
        /// <summary>
        /// 下载医保icd10
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Int64 MedicalInsuranceDownloadIcd10(DataTable dt, string userId);
        /// <summary>
        /// Icd10对码
        /// </summary>
        /// <param name="param"></param>
        void Icd10PairCode(Icd10PairCodeParam param);
        /// <summary>
        /// 删除三大目录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        string DeleteCatalog(UserInfoDto user, int catalog);
        /// <summary>
        /// 医保项目Excel导入
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Int64 DrugCatalogImportExcel(DataTable dt, string userId);
        /// <summary>
        /// 获取门诊病人
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        BaseOutpatientInfoDto GetOutpatientPerson(GetOutpatientPersonParam param);
        /// <summary>
        /// 门诊结算取消
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
         HisHospitalizationSettlementCancelInfoJsonDto GetOutpatientSettlementCancel(SettlementCancelParam param);

        /// <summary>
        /// 获取门诊病人明细
        /// </summary>
     
        /// <param name="param"></param>
        /// <returns></returns>
        List<BaseOutpatientDetailDto> GetOutpatientDetailPerson(OutpatientDetailParam param);
        /// <summary>
        /// 获取门诊医保出参
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        OutpatientMedicalInsuranceInputDto OutpatientMedicalInsuranceInput(OutpatientDetailParam param);
        
        /// <summary>
        /// 获取门诊病人结算单据号
        /// </summary>
        /// <returns></returns>

         string GetOutpatientSettlementNo(GetOutpatientSettlementNoParam param);
        /// <summary>
        /// 获取住院病人明细
        /// </summary>
        /// <param name="user"></param>
        /// <param name="businessId"></param>
        /// <returns></returns>
        List<InpatientInfoDetailDto> GetInpatientInfoDetail(UserInfoDto user, string businessId);
        /// <summary>
        /// 医保信息保存
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        void MedicalInsuranceSave(UserInfoDto user, MedicalInsuranceParam param);

        /// <summary>
        ///  获取HIS系统中科室、医师、病区、床位的基本信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        List<InformationDto> SaveInformation(UserInfoDto user, InformationParam param);
        /// <summary>
        /// 获取ICD10
        /// </summary>
        /// <param name="user"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        string GetIcd10(UserInfoDto user, CatalogParam param);
        void GetXmlData(MedicalInsuranceXmlDto param);
        UserInfoDto GetUserBaseInfo(string param);
        /// <summary>
        /// 三大目录对码信息回写至基层系统
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        int ThreeCataloguePairCodeUpload(UpdateThreeCataloguePairCodeUploadParam param);
        /// <summary>
        /// 门诊对码
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
         Dictionary<int, List<OutpatientPairCodeQueryDto>> OutpatientPairCodeQuery(
            OutpatientPairCodeUiParam param);
        /// <summary>
        /// 住院医保查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        QueryMedicalInsuranceDetailInfoDto QueryMedicalInsuranceDetail(
            QueryMedicalInsuranceUiParam param);
        /// <summary>
        /// 住院结算
        /// </summary>
        /// <param name="param"></param>
        PatientLeaveHospitalInfoDto GetHisHospitalizationSettlement(GetInpatientInfoParam param);
        /// <summary>
        /// 获取his住院预结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
         HisHospitalizationPreSettlementJsonDto GetHisHospitalizationPreSettlement(GetInpatientInfoParam param);

        /// <summary>
        /// 获取基础取消结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
         HisHospitalizationSettlementCancelInfoJsonDto GetHisHospitalizationSettlementCancel(
            SettlementCancelParam param);
    }
}
