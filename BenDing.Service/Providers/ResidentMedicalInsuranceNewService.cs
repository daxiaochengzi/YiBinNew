﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.JsonEntity;
using BenDing.Domain.Models.Dto.Resident;
using BenDing.Domain.Models.Dto.Web;
using BenDing.Domain.Models.Enums;
using BenDing.Domain.Models.HisXml;
using BenDing.Domain.Models.Params.Base;
using BenDing.Domain.Models.Params.Resident;
using BenDing.Domain.Models.Params.SystemManage;
using BenDing.Domain.Models.Params.UI;
using BenDing.Domain.Models.Params.Web;
using BenDing.Domain.Xml;
using BenDing.Repository.Interfaces.Web;
using BenDing.Service.Interfaces;
using Newtonsoft.Json;

namespace BenDing.Service.Providers
{
   public class ResidentMedicalInsuranceNewService: IResidentMedicalInsuranceNewService
    {
        private readonly IResidentMedicalInsuranceRepository _residentMedicalInsuranceRepository;
        private readonly IWebBasicRepository _webBasicRepository;
        private readonly IHisSqlRepository _hisSqlRepository;
        private readonly IMedicalInsuranceSqlRepository _medicalInsuranceSqlRepository;
        private readonly ISystemManageRepository _systemManageRepository;
        private readonly IWebServiceBasicService _webserviceBasicService;

        public ResidentMedicalInsuranceNewService(
            IResidentMedicalInsuranceRepository residentMedicalInsuranceRepository,
            IWebBasicRepository webBasicRepository,
            IHisSqlRepository hisSqlRepository,
            IMedicalInsuranceSqlRepository medicalInsuranceSqlRepository,
            ISystemManageRepository systemManageRepository,
            IWebServiceBasicService serviceBasicService)
        {
            _residentMedicalInsuranceRepository = residentMedicalInsuranceRepository;
            _webBasicRepository = webBasicRepository;
            _hisSqlRepository = hisSqlRepository;
            _medicalInsuranceSqlRepository = medicalInsuranceSqlRepository;
            _systemManageRepository = systemManageRepository;
            _webserviceBasicService = serviceBasicService;
        }
        /// <summary>
        /// 获取居民入院登记参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public ResidentHospitalizationRegisterParam GetResidentHospitalizationRegisterParam(ResidentHospitalizationRegisterUiParam param)
        {
            //his登陆
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            var infoData = new GetInpatientInfoParam()
            {
                User = userBase,
                BusinessId = param.BusinessId,
            };
            //获取医保病人
            var inpatientData = _webserviceBasicService.GetInpatientInfo(infoData);

            var iniParam = new ResidentHospitalizationRegisterParam();
                var diagnosisData = CommonHelp.GetDiagnosis(param.DiagnosisList);
                iniParam.AdmissionMainDiagnosisIcd10 = diagnosisData.AdmissionMainDiagnosisIcd10;
                iniParam.DiagnosisIcd10Two = diagnosisData.DiagnosisIcd10Two;
                iniParam.DiagnosisIcd10Three = diagnosisData.DiagnosisIcd10Three;
                iniParam.AdmissionMainDiagnosis = diagnosisData.DiagnosisDescribe;
                iniParam.IdentityMark = param.IdentityMark;
                iniParam.AfferentSign = param.AfferentSign;
                iniParam.MedicalCategory = param.MedicalCategory;
                iniParam.FetusNumber = param.FetusNumber;
                iniParam.HouseholdNature = param.HouseholdNature;
                iniParam.AdmissionDate = Convert.ToDateTime(inpatientData.AdmissionDate).ToString("yyyyMMdd");
                iniParam.InpatientDepartmentCode = inpatientData.InDepartmentName;
                iniParam.BedNumber = inpatientData.AdmissionBed;
                iniParam.HospitalizationNo = inpatientData.HospitalizationNo;
                iniParam.Operators = inpatientData.AdmissionOperator;
                iniParam.InsuranceType = param.InsuranceType;
                return iniParam;
            
        }
        /// <summary>
        /// 居民入院登记
        /// </summary>
        /// <param name="param"></param>
        public void HospitalizationRegister(ResidentHospitalizationRegisterUiParam param)
        { //his登陆
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            var infoData = new GetInpatientInfoParam()
            {
                User = userBase,
                BusinessId = param.BusinessId,
            };
            //获取医保病人
            var inpatientData = _webserviceBasicService.GetInpatientInfo(infoData);
            var registerParam = GetResidentHospitalizationRegisterParam(param);
            var residentData = JsonConvert.DeserializeObject<ResidentHospitalizationRegisterDto>(param.SettlementJson);
       
            var saveData = new MedicalInsuranceDto
            {
                AdmissionInfoJson = JsonConvert.SerializeObject(registerParam),
                BusinessId = param.BusinessId,
                Id = Guid.NewGuid(),
                InsuranceType = 342,
                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceInpatientNo,
                MedicalInsuranceState = MedicalInsuranceState.MedicalInsuranceHospitalized,
                AfferentSign = param.AfferentSign,
                IdentityMark = param.IdentityMark
            };
            //保存中间库
            _medicalInsuranceSqlRepository.SaveMedicalInsurance(userBase, saveData);
            //回参构建
            var xmlData = new HospitalizationRegisterXml()
            {
                MedicalInsuranceType = "10",
                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceInpatientNo,
                InsuranceNo = null,
            };
            var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
            var saveXml = new SaveXmlDataParam()
            {
                User = userBase,
                MedicalInsuranceBackNum = "CXJB002",
                MedicalInsuranceCode = "21",
                BusinessId = param.BusinessId,
                BackParam = strXmlBackParam
            };
            //存基层
            _webBasicRepository.SaveXmlData(saveXml);
            //更新中间库
            saveData.MedicalInsuranceState = MedicalInsuranceState.HisHospitalized;
            _medicalInsuranceSqlRepository.SaveMedicalInsurance(userBase, saveData);
            //日志
            var logParam = new AddHospitalLogParam
            {
                User = userBase,
                RelationId = saveData.Id,
                JoinOrOldJson = JsonConvert.SerializeObject(param),
                BusinessId = param.BusinessId,
                ReturnOrNewJson = JsonConvert.SerializeObject(residentData),
                Remark = "医保入院登记;TransactionId:" + userBase.TransKey
            };
            _systemManageRepository.AddHospitalLog(logParam);
            //保存入院数据
            infoData.IsSave = true;
            _webserviceBasicService.GetInpatientInfo(infoData);

        }
        /// <summary>
        /// 获取居民入院登记修改参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public HospitalizationModifyParam GetHospitalizationModifyParam(HospitalizationModifyUiParam  param)
        {
            //主诊断
            //医保修改
            var modifyParam = new HospitalizationModifyParam()
            {
                AdmissionDate = Convert.ToDateTime(param.AdmissionDate).ToString("yyyyMMdd"),
                BedNumber = param.BedNumber,
                BusinessId = param.BusinessId,
                FetusNumber = param.FetusNumber,
                HouseholdNature = param.HouseholdNature,
                MedicalInsuranceHospitalizationNo = param.MedicalInsuranceHospitalizationNo,
                TransKey = param.TransKey,
                UserId = param.UserId,
                InpatientDepartmentCode = param.InpatientDepartmentCode,
                HospitalizationNo = CommonHelp.GuidToStr(param.BusinessId)
            };
            var diagnosisData = CommonHelp.GetDiagnosis(param.DiagnosisList);
            modifyParam.AdmissionMainDiagnosisIcd10 = diagnosisData.AdmissionMainDiagnosisIcd10;
            modifyParam.DiagnosisIcd10Two = diagnosisData.DiagnosisIcd10Two;
            modifyParam.DiagnosisIcd10Three = diagnosisData.DiagnosisIcd10Three;
            return modifyParam;
        }
        /// <summary>
        /// 修改居民入院登记
        /// </summary>
        /// <param name="param"></param>
        public void HospitalizationModify(HospitalizationModifyUiParam param)
        {//his登陆
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            var modifyParam = GetHospitalizationModifyParam(param);
            var paramStr = "";
            var queryData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(
                new QueryMedicalInsuranceResidentInfoParam
                {
                    BusinessId = param.BusinessId,
                    OrganizationCode = userBase.OrganizationCode
                });
            if (!string.IsNullOrWhiteSpace(queryData.AdmissionInfoJson))
            {
                var data =
                    JsonConvert.DeserializeObject<QueryMedicalInsuranceDetailDto>(queryData
                        .AdmissionInfoJson);
                if (!string.IsNullOrWhiteSpace(modifyParam.AdmissionDate))
                    data.AdmissionDate = modifyParam.AdmissionDate;
                if (!string.IsNullOrWhiteSpace(modifyParam.AdmissionMainDiagnosis))
                    data.AdmissionMainDiagnosis = modifyParam.AdmissionMainDiagnosis;
                if (!string.IsNullOrWhiteSpace(modifyParam.AdmissionMainDiagnosisIcd10))
                    data.AdmissionMainDiagnosisIcd10 = modifyParam.AdmissionMainDiagnosisIcd10;
                if (!string.IsNullOrWhiteSpace(modifyParam.DiagnosisIcd10Three))
                    data.DiagnosisIcd10Three = modifyParam.DiagnosisIcd10Three;
                if (!string.IsNullOrWhiteSpace(modifyParam.DiagnosisIcd10Two))
                    data.DiagnosisIcd10Two = modifyParam.DiagnosisIcd10Two;
                if (!string.IsNullOrWhiteSpace(param.FetusNumber))
                    data.FetusNumber = param.FetusNumber;
                if (!string.IsNullOrWhiteSpace(param.HouseholdNature))
                    data.HouseholdNature = param.HouseholdNature;
                if (!string.IsNullOrWhiteSpace(param.InpatientDepartmentCode))
                    data.InpatientDepartmentCode = param.InpatientDepartmentCode;
                if (!string.IsNullOrWhiteSpace(param.BedNumber))
                    data.BedNumber = param.BedNumber;
                data.Id = queryData.Id;
                paramStr = JsonConvert.SerializeObject(data);
            }

            var saveData = new MedicalInsuranceDto
            {
                AdmissionInfoJson = paramStr,
                BusinessId = queryData.BusinessId,
                Id = queryData.Id,
                IsModify = true,
                MedicalInsuranceHospitalizationNo = queryData.MedicalInsuranceHospitalizationNo
            };
            _medicalInsuranceSqlRepository.SaveMedicalInsurance(userBase, saveData);
            //日志
            var logParam = new AddHospitalLogParam
            {
                User = userBase,
                RelationId = queryData.Id,
                JoinOrOldJson = queryData.AdmissionInfoJson,
                ReturnOrNewJson = paramStr,
                BusinessId = param.BusinessId,
                Remark = "医保入院登记修改"
            };
            _systemManageRepository.AddHospitalLog(logParam);
        }
        /// <summary>
        /// 获取预结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public HospitalizationPresettlementParam GetHospitalizationPreSettlement(HospitalizationPreSettlementUiParam param)
        {
            HospitalizationPresettlementDto resultData = null;
            //获取操作人员信息
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode
            };
            var infoData = new GetInpatientInfoParam()
            {
                User = userBase,
                BusinessId = param.BusinessId,
            };
            //获取his预结算
            var hisPreSettlementData = _webserviceBasicService.GetHisHospitalizationPreSettlement(infoData);
            var preSettlementData = hisPreSettlementData.PreSettlementData.FirstOrDefault();
            //获取医保病人信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
            return  new HospitalizationPresettlementParam()
            {
                LeaveHospitalDate = Convert.ToDateTime(preSettlementData.EndDate).ToString("yyyyMMdd"),
                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo
            };
        }
        /// <summary>
        /// 居民住院预结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public HospitalizationPresettlementDto HospitalizationPreSettlement(HospitalizationPreSettlementUiParam param)
        {
            HospitalizationPresettlementDto resultData = null;
            //获取操作人员信息
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode
            };
            var infoData = new GetInpatientInfoParam()
            {
                User = userBase,
                BusinessId = param.BusinessId,
            };
            //获取his预结算
            var hisPreSettlementData = _webserviceBasicService.GetHisHospitalizationPreSettlement(infoData);
            var preSettlementData = hisPreSettlementData.PreSettlementData.FirstOrDefault();
            //获取医保病人信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);

            var dataIni = JsonConvert.DeserializeObject<HospitalizationPresettlementJsonDto>(param.SettlementJson);
            var data = AutoMapper.Mapper.Map<HospitalizationPresettlementDto>(dataIni);
            resultData = data ?? throw new Exception("居民预结算返回结果有误!!!");
            //报销金额 =统筹支付+补充险支付+生育补助+民政救助+民政重大疾病救助+精准扶贫+民政优抚+其它支付
            decimal reimbursementExpenses =
                data.BasicOverallPay + data.SupplementPayAmount + data.BirthAAllowance +
                data.CivilAssistancePayAmount + data.CivilAssistanceSeriousIllnessPayAmount +
                data.AccurateAssistancePayAmount + data.CivilServicessistancePayAmount +
                data.OtherPaymentAmount;
            resultData.ReimbursementExpenses = reimbursementExpenses;
            var updateParam = new UpdateMedicalInsuranceResidentSettlementParam()
            {
                ReimbursementExpensesAmount = CommonHelp.ValueToDouble(reimbursementExpenses),
                SelfPayFeeAmount = data.CashPayment,
                OtherInfo = JsonConvert.SerializeObject(data),
                Id = residentData.Id,
                UserId = param.UserId,
                SettlementNo = data.DocumentNo,
                MedicalInsuranceAllAmount = data.TotalAmount,
                PreSettlementTransactionId = param.TransKey,
                MedicalInsuranceState = MedicalInsuranceState.MedicalInsurancePreSettlement
            };
            //存入中间库
            _medicalInsuranceSqlRepository.UpdateMedicalInsuranceResidentSettlement(updateParam);
            var logParam = new AddHospitalLogParam()
            {
                JoinOrOldJson = JsonConvert.SerializeObject(param),
                ReturnOrNewJson = JsonConvert.SerializeObject(data),
                BusinessId = param.BusinessId,
                User = userBase,
                Remark = "居民住院病人预结算"
            };
            _systemManageRepository.AddHospitalLog(logParam);
            return resultData;
        }

        /// <summary>
        /// 获取居民出院结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public LeaveHospitalSettlementParam GetHospitalizationSettlement(LeaveHospitalSettlementUiParam param)
        {
            // 获取操作人员信息
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode
            };
            userBase.TransKey = param.TransKey;
            var infoData = new GetInpatientInfoParam()
            {
                User = userBase,
                BusinessId = param.BusinessId,
            };
            //获取his结算
            var hisSettlement = _webserviceBasicService.GetHisHospitalizationSettlement(infoData);
            //获取医保病人信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
            if (residentData.MedicalInsuranceState != MedicalInsuranceState.MedicalInsurancePreSettlement) throw new Exception("当前病人未办理预结算,不能办理结算!!!");
            if (residentData.MedicalInsuranceState == MedicalInsuranceState.HisSettlement) throw new Exception("当前病人已办理医保结算,不能办理再次结算!!!");

            var inpatientInfoParam = new QueryInpatientInfoParam() { BusinessId = param.BusinessId };
            //获取住院病人
            var inpatientInfoData = _hisSqlRepository.QueryInpatientInfo(inpatientInfoParam);
            if (inpatientInfoData == null) throw new Exception("该病人未在中心库中,请检查是否办理医保入院!!!");

            var settlementParam = new LeaveHospitalSettlementParam()
            {
                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                LeaveHospitalDate = Convert.ToDateTime(hisSettlement.LeaveHospitalDate).ToString("yyyyMMdd"),
                UserId = hisSettlement.LeaveHospitalOperator,
                LeaveHospitalInpatientState = param.LeaveHospitalInpatientState,
            };
            //获取诊断
            var diagnosisData = CommonHelp.LeaveHospitalDiagnosis(param.DiagnosisList);
            settlementParam.LeaveHospitalMainDiagnosisIcd10 = diagnosisData.AdmissionMainDiagnosisIcd10;
            settlementParam.LeaveHospitalDiagnosisIcd10Two = diagnosisData.DiagnosisIcd10Two;
            settlementParam.LeaveHospitalDiagnosisIcd10Three = diagnosisData.DiagnosisIcd10Three;
            settlementParam.LeaveHospitalMainDiagnosis = diagnosisData.DiagnosisDescribe;
            var logParam = new AddHospitalLogParam()
            {
                JoinOrOldJson = JsonConvert.SerializeObject(param),
                ReturnOrNewJson = JsonConvert.SerializeObject(settlementParam),
                User = userBase,
                BusinessId = param.BusinessId,
                Remark = "获取住院结算参数"
            };
            _systemManageRepository.AddHospitalLog(logParam);
            return settlementParam;
        }
        /// <summary>
        /// 居民住院结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public HospitalizationPresettlementDto LeaveHospitalSettlement(LeaveHospitalSettlementUiParam param)
        {
            // 获取操作人员信息
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode
            };
            userBase.TransKey = param.TransKey;
            var infoData = new GetInpatientInfoParam()
            {
                User = userBase,
                BusinessId = param.BusinessId,
            };
            //获取his结算
            var hisSettlement = _webserviceBasicService.GetHisHospitalizationSettlement(infoData);
            //获取医保病人信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
            if (residentData.MedicalInsuranceState != MedicalInsuranceState.MedicalInsurancePreSettlement) throw new Exception("当前病人未办理预结算,不能办理结算!!!");
            if (residentData.MedicalInsuranceState == MedicalInsuranceState.HisSettlement) throw new Exception("当前病人已办理医保结算,不能办理再次结算!!!");

            var inpatientInfoParam = new QueryInpatientInfoParam() { BusinessId = param.BusinessId };
            //获取住院病人
            var inpatientInfoData = _hisSqlRepository.QueryInpatientInfo(inpatientInfoParam);
            if (inpatientInfoData == null) throw new Exception("该病人未在中心库中,请检查是否办理医保入院!!!");

            var settlementParam = new LeaveHospitalSettlementParam()
            {
                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                LeaveHospitalDate = Convert.ToDateTime(hisSettlement.LeaveHospitalDate).ToString("yyyyMMdd"),
                UserId = hisSettlement.LeaveHospitalOperator,
                LeaveHospitalInpatientState = param.LeaveHospitalInpatientState,
            };
            //获取诊断
            var diagnosisData = CommonHelp.GetDiagnosis(param.DiagnosisList);
            settlementParam.LeaveHospitalMainDiagnosisIcd10 = diagnosisData.AdmissionMainDiagnosisIcd10;
            settlementParam.LeaveHospitalDiagnosisIcd10Two = diagnosisData.DiagnosisIcd10Two;
            settlementParam.LeaveHospitalDiagnosisIcd10Three = diagnosisData.DiagnosisIcd10Three;
            settlementParam.LeaveHospitalMainDiagnosis = diagnosisData.DiagnosisDescribe;
           
            var dataIni = JsonConvert.DeserializeObject<HospitalizationPresettlementJsonDto>(param.SettlementJson);
           var data = AutoMapper.Mapper.Map<HospitalizationPresettlementDto>(dataIni);
            //报销金额 =统筹支付+补充险支付+生育补助+民政救助+民政重大疾病救助+精准扶贫+民政优抚+其它支付
            decimal reimbursementExpenses =
                data.BasicOverallPay + data.SupplementPayAmount + data.BirthAAllowance +
                data.CivilAssistancePayAmount + data.CivilAssistanceSeriousIllnessPayAmount +
                data.AccurateAssistancePayAmount + data.CivilServicessistancePayAmount +
                data.OtherPaymentAmount;
            data.ReimbursementExpenses = reimbursementExpenses;
           
            if (data == null) throw new Exception("居民住院结算反馈失败");
            var updateData = new UpdateMedicalInsuranceResidentSettlementParam()
            {
                UserId = userBase.UserId,
                ReimbursementExpensesAmount = CommonHelp.ValueToDouble(data.ReimbursementExpenses),
                SelfPayFeeAmount = data.CashPayment,
                OtherInfo = JsonConvert.SerializeObject(data),
                Id = residentData.Id,
                SettlementNo = data.DocumentNo,
                MedicalInsuranceAllAmount = data.TotalAmount,
                SettlementTransactionId = userBase.UserId,
                MedicalInsuranceState = MedicalInsuranceState.MedicalInsuranceSettlement
            };
            //存入中间层
            _medicalInsuranceSqlRepository.UpdateMedicalInsuranceResidentSettlement(updateData);
            //添加日志
            var logParam = new AddHospitalLogParam()
            {
                JoinOrOldJson = JsonConvert.SerializeObject(param),
                ReturnOrNewJson = JsonConvert.SerializeObject(data),
                User = userBase,
                Remark = "医保居民住院结算",
                RelationId = residentData.Id,
                BusinessId = param.BusinessId,
            };
            _systemManageRepository.AddHospitalLog(logParam);

            decimal insuranceBalance = !string.IsNullOrWhiteSpace(param.InsuranceBalance) == true
                ? Convert.ToDecimal(param.InsuranceBalance) : 0;
            var cashPayment = CommonHelp.ValueToDouble(hisSettlement.AllAmount - reimbursementExpenses);
            // 回参构建
            var xmlData = new HospitalSettlementXml()
            {

                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                CashPayment = data.CashPayment,
                SettlementNo = data.DocumentNo,
                PaidAmount = data.PaidAmount,
                AllAmount = data.TotalAmount,
                PatientName = inpatientInfoData.PatientName,
                AccountBalance = insuranceBalance,
                AccountAmountPay = 0,
                InsuredStatus = "342"
            };


            var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
            var saveXml = new SaveXmlDataParam()
            {
                User = userBase,
                MedicalInsuranceBackNum = data.DocumentNo,
                MedicalInsuranceCode = "41",
                BusinessId = param.BusinessId,
                BackParam = strXmlBackParam
            };
            //结算存基层
            _webBasicRepository.SaveXmlData(saveXml);

            //添加日志
       
            _systemManageRepository.AddHospitalLog(new AddHospitalLogParam()
            {
                JoinOrOldJson = JsonConvert.SerializeObject(xmlData),
                ReturnOrNewJson = "",
                User = userBase,
                Remark = "基层居民住院结算",
                RelationId = residentData.Id,
                BusinessId = param.BusinessId
            });
            var updateParamData = new UpdateMedicalInsuranceResidentSettlementParam()
            {
                UserId = param.UserId,
                Id = residentData.Id,
                MedicalInsuranceState = MedicalInsuranceState.HisSettlement,
                IsHisUpdateState = true
            };

            //  更新中间层
            _medicalInsuranceSqlRepository.UpdateMedicalInsuranceResidentSettlement(updateParamData);
            //结算后保存信息
            var saveParam = AutoMapper.Mapper.Map<SaveInpatientSettlementParam>(hisSettlement);
            saveParam.Id = (Guid)inpatientInfoData.Id;
            saveParam.User = userBase;
            saveParam.LeaveHospitalDiagnosisJson = JsonConvert.SerializeObject(param.DiagnosisList);
            _hisSqlRepository.SaveInpatientSettlement(saveParam);
            return data;
        }

        /// <summary>
        /// 获取医保上传数据参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public GetPrescriptionUploadParam GetPrescriptionUploadParam(PrescriptionUploadUiParam param, UserInfoDto user)
        {
            //处方上传解决方案
            //1.判断是id上传还是单个用户上传
            //3.获取医院等级判断金额是否符合要求
            //4.数据上传
            //4.1 id上传
            //4.1.2 获取医院等级判断金额是否符合要求
            //4.1.3 数据上传
            //4.1.3.1 数据上传失败,数据回写到日志
            //4.1.3.2 数据上传成功,保存批次号，数据回写至基层
            //4.2   单个病人整体上传
            //4.2.2 获取医院等级判断金额是否符合要求
            //4.2.3 数据上传
            //4.2.3.1 数据上传失败,数据回写到日志
            var isOrganizationCodeUpload = true;
            var isDataIdList = false;
            var resultData = new GetPrescriptionUploadParam();
            var resultUpload = new RetrunPrescriptionUploadDto();
            var uploadList=new List<PrescriptionUploadParam>(); 
            List<QueryInpatientInfoDetailDto> queryData;
            var queryParam = new InpatientInfoDetailQueryParam();
            //1.判断是id上传还是单个用户上传
            if (param.DataIdList != null && param.DataIdList.Any())
            {
                queryParam.IdList = param.DataIdList;
                queryParam.UploadMark = 0;
                isOrganizationCodeUpload = false;
                isDataIdList = true;
            }
            else
            {
                queryParam.BusinessId = param.BusinessId;
                queryParam.UploadMark = 0;
            }

            queryParam.NotUploadMark = true;

            //获取病人明细
            queryData = _hisSqlRepository.InpatientInfoDetailQuery(queryParam);
            if (queryData.Any())
            {
                var queryBusinessId = (!string.IsNullOrWhiteSpace(queryParam.BusinessId))
                    ? param.BusinessId
                    : queryData.Select(c => c.BusinessId).FirstOrDefault();
                var medicalInsuranceParam = new QueryMedicalInsuranceResidentInfoParam()
                { BusinessId = queryBusinessId };
                //获取病人医保信息
                var medicalInsurance =
                    _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(medicalInsuranceParam);
                if (medicalInsurance == null)
                {
                    if (!string.IsNullOrWhiteSpace(queryParam.BusinessId))
                    {
                        resultUpload.Msg += "病人未办理医保入院";
                    }
                    else
                    {
                        throw new Exception("病人未办理医保入院");
                    }
                }
                else
                {

                    var queryPairCodeParam = new QueryMedicalInsurancePairCodeParam()
                    {
                        DirectoryCodeList = queryData.Select(d => d.DirectoryCode).Distinct().ToList(),
                        OrganizationCode = user.OrganizationCode,
                    };
                    //获取医保对码数据
                    var queryPairCode =
                        _medicalInsuranceSqlRepository.QueryMedicalInsurancePairCode(queryPairCodeParam);
                    ////处方上传数据金额验证
                    //var validData = PrescriptionDataUnitPriceValidation(queryData, queryPairCode, user);
                    //var validDataList = validData.Values.FirstOrDefault();
                    //错误提示信息
                    //var validDataMsg = queryData.Keys.FirstOrDefault();
                    //if (!string.IsNullOrWhiteSpace(validDataMsg))
                    //{
                    //    resultUpload.Msg += validDataMsg;
                    //}

                    //获取处方上传入参
                    var paramIni = GetPrescriptionUploadParam(queryData, queryPairCode, user,
                        medicalInsurance.InsuranceType, isOrganizationCodeUpload, isDataIdList);
                    //医保住院号
                    paramIni.MedicalInsuranceHospitalizationNo = medicalInsurance.MedicalInsuranceHospitalizationNo;
                 
                    int num = paramIni.RowDataList.Count;
                    resultUpload.Count = num;
                    int a = 0;
                    int limit = 40; //限制条数
                    var count = Convert.ToInt32(num / limit) + ((num % limit) > 0 ? 1 : 0);
                    var idList = new List<Guid>();
                    while (a < count)
                    {
                        //排除已上传数据

                        var rowDataListAll = paramIni.RowDataList.Where(d => !idList.Contains(d.Id))
                            .OrderBy(c => c.PrescriptionSort).ToList();
                        var sendList = rowDataListAll.Take(limit).Select(s => s.Id).ToList();
                        ////新的数据上传参数
                        //var uploadDataParam = paramIni;
                        //uploadDataParam.RowDataList 
                        var uploadRowParam = new PrescriptionUploadParam();
                        uploadRowParam.MedicalInsuranceHospitalizationNo = paramIni.MedicalInsuranceHospitalizationNo;
                        uploadRowParam.Operators = paramIni.Operators;
                        uploadRowParam.RowDataList = rowDataListAll.Where(c => sendList.Contains(c.Id)).ToList();
                        ////数据上传
                        uploadList.Add(uploadRowParam);
                        //var uploadData = PrescriptionUploadData(uploadDataParam, param.BusinessId, user);
                        //if (uploadData.ReturnState != "1")
                        //{
                        //    resultUpload.Msg += uploadData.ReturnState;
                        //}
                        //else
                        //{
                        //    //更新数据上传状态
                        //    idList.AddRange(sendList);
                        //    //获取总行数
                        //resultUpload.Num += sendList.Count();
                        //}
                        idList.AddRange(sendList);
                        resultUpload.Num += sendList.Count();
                        a++;
                    }

                }
            }

            resultData.RetrunUpload = resultUpload;
            resultData.UploadList = uploadList;
            return resultData;

        }
        /// <summary>
        /// 不传医保
        /// </summary>
        public void NotUploadMark(NotUploadMarkUiParam param)
        {// 获取操作人员信息
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode
            };
            //获取医保病人信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
         
            var inpatientDetial= _hisSqlRepository.InpatientInfoDetailQuery(new InpatientInfoDetailQueryParam()
            {
                IdList = param.IdList,
            });
            var detailId = inpatientDetial.FirstOrDefault()?.DetailId;
            var idListNew=new List<string>(){ detailId };
            //InpatientInfoDetailQuery
            if (param.IsCancel == false)
            {
                var rowXml = idListNew.Select(c => new HospitalizationFeeUploadRowXml() { SerialNumber = c }).ToList();
                //回参
                var xmlData = new HospitalizationFeeUploadXml()
                {
                    MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                    RowDataList = rowXml,
                };
                var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
                param.TransKey = param.BusinessId;
                var saveXml = new SaveXmlDataParam()
                {
                    User = userBase,
                    MedicalInsuranceBackNum = "CXJB004",
                    MedicalInsuranceCode = "31",
                    BusinessId = param.BusinessId,
                    BackParam = strXmlBackParam
                };
                //存基层
                _webBasicRepository.SaveXmlData(saveXml);
               
            }
            else
            {
                // 回参构建
                var xmlData = new HospitalizationFeeUploadCancelXml()
                {
                    MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                    RowDataList = idListNew.Select(c => new HospitalizationFeeUploadRowXml() { SerialNumber =c }).ToList()
                };
                var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
                var saveXml = new SaveXmlDataParam()
                {
                    User = userBase,
                    MedicalInsuranceBackNum = "CXJB005",
                    MedicalInsuranceCode = "32",
                    BusinessId = param.BusinessId,
                    BackParam = strXmlBackParam
                };
                //存基层
                _webBasicRepository.SaveXmlData(saveXml);
               
            }
            var logParam = new AddHospitalLogParam
            {
                User = userBase,
                RelationId = Guid.Parse(param.BusinessId),
                JoinOrOldJson = JsonConvert.SerializeObject(param.IdList),
                ReturnOrNewJson = "",
                BusinessId = param.BusinessId,
                Remark = param.IsCancel == true ? "取消不传医保" : "不传医保"
            };
            _systemManageRepository.AddHospitalLog(logParam);
            _medicalInsuranceSqlRepository.NotUploadMark(param.IdList, param.IsCancel, userBase);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int PrescriptionUploadUpdateData(PrescriptionUploadUpdateDataParam param )
        {
            var rowXml = param.UploadData.RowDataList.Select(c => new HospitalizationFeeUploadRowXml() { SerialNumber = c.DetailId }).ToList();
            //交易码
            var transactionId = Guid.NewGuid().ToString("N");
            //添加批次
            var updateFeeParam = param.UploadData.RowDataList.Select(d => new UpdateHospitalizationFeeParam
            {
                Id = d.Id,
                BatchNumber = param.ProjectBatch,
                TransactionId = transactionId,
                UploadAmount = d.Amount
            }).ToList();
            _medicalInsuranceSqlRepository.UpdateHospitalizationFee(updateFeeParam, false, param.User);
            //回参
            var xmlData = new HospitalizationFeeUploadXml()
            {
                MedicalInsuranceHospitalizationNo = param.UploadData.MedicalInsuranceHospitalizationNo,
                RowDataList = rowXml,
            };
            var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
            param.User.TransKey = param.BusinessId;
            var saveXml = new SaveXmlDataParam()
            {
                User =param.User,
                MedicalInsuranceBackNum = "CXJB004",
                MedicalInsuranceCode = "31",
                BusinessId = param.BusinessId,
                BackParam = strXmlBackParam
            };
            //存基层
            _webBasicRepository.SaveXmlData(saveXml);
            var batchConfirmParam = new BatchConfirmParam()
            {
                ConfirmType = 1,
                MedicalInsuranceHospitalizationNo = param.UploadData.MedicalInsuranceHospitalizationNo,
                BatchNumber = param.ProjectBatch,
                Operators = CommonHelp.GuidToStr(param.User.UserId)
            };
            

            return param.UploadData.RowDataList.Count();
        }
        /// <summary>
        /// 取消医保出院结算
        /// </summary>
        /// <param name="param"></param>
        /// <param name="infoParam"></param>
        /// <returns></returns>
        public void LeaveHospitalSettlementCancel(LeaveHospitalSettlementCancelParam param,
            LeaveHospitalSettlementCancelInfoParam infoParam)
        {
            if (param.CancelLimit == "1")
            {
              
                var updateParam = new UpdateMedicalInsuranceResidentSettlementParam()
                {
                    UserId = infoParam.User.UserId,
                    Id = infoParam.Id,
                    CancelTransactionId = infoParam.User.TransKey,
                    MedicalInsuranceState = MedicalInsuranceState.MedicalInsurancePreSettlement
                };
                //存入中间层
                _medicalInsuranceSqlRepository.UpdateMedicalInsuranceResidentSettlement(updateParam);
                //添加日志
                var logParam = new AddHospitalLogParam()
                {
                    JoinOrOldJson = JsonConvert.SerializeObject(param),
                    User = infoParam.User,
                    Remark = "医保居民住院结算取消",
                    RelationId = infoParam.Id,
                    BusinessId = infoParam.BusinessId,
                };
                _systemManageRepository.AddHospitalLog(logParam);
                //回参构建
                var xmlData = new HospitalSettlementCancelXml()
                {
                    SettlementNo= param.SettlementNo,
                };
                var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
                var saveXml = new SaveXmlDataParam()
                {
                    User = infoParam.User,
                    MedicalInsuranceBackNum = "CXJB011",
                    MedicalInsuranceCode = "42",
                    BusinessId = infoParam.BusinessId,
                    BackParam = strXmlBackParam
                };
                //存基层
                _webBasicRepository.SaveXmlData(saveXml);
                //添加日志

                _systemManageRepository.AddHospitalLog(new AddHospitalLogParam()
                {
                    JoinOrOldJson = JsonConvert.SerializeObject(xmlData),
                    User = infoParam.User,
                    Remark = "基层居民住院结算取消",
                    RelationId = infoParam.Id,
                    BusinessId = infoParam.BusinessId,
                });
            }
            else if (param.CancelLimit == "2") //取消入院登记并删除资料
            {
              
                //回参构建
                var xmlData = new HospitalizationRegisterCancelXml()
                {
                    MedicalInsuranceHospitalizationNo = param.MedicalInsuranceHospitalizationNo
                };
                var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
                var saveXml = new SaveXmlDataParam()
                {
                    User = infoParam.User,
                    MedicalInsuranceBackNum = "CXJB004",
                    MedicalInsuranceCode = "22",
                    BusinessId = infoParam.BusinessId,
                    BackParam = strXmlBackParam
                };
                //存基层
                _webBasicRepository.SaveXmlData(saveXml);
                //添加日志
                var logParam = new AddHospitalLogParam()
                {
                    JoinOrOldJson = JsonConvert.SerializeObject(param),
                    User = infoParam.User,
                    Remark = "基层取消入院登记",
                    RelationId = infoParam.Id,
                    BusinessId = infoParam.BusinessId,
                };
                _systemManageRepository.AddHospitalLog(logParam);
                _hisSqlRepository.ExecuteSql($"update [dbo].[Inpatient] set [IsCanCelHospitalized]=1 where BusinessId='{infoParam.BusinessId}' and IsDelete=0");
            }
           
        }
        ///  <summary>
        /// 处方上传数据金额验证
        ///  </summary>
        ///  <param name="param"></param>
        /// <param name="pairCode"></param>
        /// <param name="user"></param>
        ///  <returns></returns>
        private Dictionary<string, List<QueryInpatientInfoDetailDto>> PrescriptionDataUnitPriceValidation(
            List<QueryInpatientInfoDetailDto> param,
            List<QueryMedicalInsurancePairCodeDto> pairCode, UserInfoDto user)
        {

            var resultData = new Dictionary<string, List<QueryInpatientInfoDetailDto>>();
            var dataList = new List<QueryInpatientInfoDetailDto>();
            //获取医院等级
            var gradeData = _systemManageRepository.QueryHospitalOrganizationGrade(user.OrganizationCode);
            var grade = gradeData.OrganizationGrade;
            string msg = "";
            foreach (var item in param)
            {
                var queryData = pairCode.FirstOrDefault(c => c.DirectoryCode == item.DirectoryCode);
                if (queryData != null)
                {
                    decimal queryAmount = 0;
                    if (grade == OrganizationGrade.二级乙等以下) queryAmount = queryData.ZeroBlock;
                    if (grade == OrganizationGrade.二级乙等) queryAmount = queryData.OneBlock;
                    if (grade == OrganizationGrade.二级甲等) queryAmount = queryData.TwoBlock;
                    if (grade == OrganizationGrade.三级乙等) queryAmount = queryData.ThreeBlock;
                    if (grade == OrganizationGrade.三级甲等)
                    {
                        queryAmount = queryData.FourBlock;
                    }

                    //限价大于零判断
                    if (queryAmount > 0)
                    {
                        if (item.Amount < queryAmount)
                        {
                            dataList.Add(item);
                        }
                        else
                        {
                            msg += item.DirectoryCode + ",";
                        }
                    }
                    else
                    {
                        dataList.Add(item);
                    }
                }
            }

            msg = msg != "" ? msg + "金额超出限制等级" : "";
            resultData.Add(msg, dataList);
            return resultData;

        }
        /// <summary>
        /// 获取处方上传入参
        /// </summary>
        /// <returns></returns>
        private PrescriptionUploadParam GetPrescriptionUploadParam(List<QueryInpatientInfoDetailDto> param,
            List<QueryMedicalInsurancePairCodeDto> pairCodeList, UserInfoDto user, string insuranceType,
            bool isOrganizationCodeUpload,bool isDataIdList )
        {

            var resultData = new PrescriptionUploadParam();

            resultData.Operators = CommonHelp.GuidToStr(user.UserId);
            var rowDataList = new List<PrescriptionUploadRowParam>();
            foreach (var item in param)
            {
                var pairCodeData = pairCodeList.FirstOrDefault(c => c.DirectoryCode == item.DirectoryCode);

                if (pairCodeData != null)
                {
                    //自付金额
                    decimal residentSelfPayProportion = 0;
                    if (insuranceType == "342") //居民   
                    {
                        residentSelfPayProportion = CommonHelp.ValueToDouble(
                            item.Amount *
                            pairCodeData.ResidentSelfPayProportion);
                      
                       
                    }

                    if (insuranceType == "310") //职工
                    {
                        residentSelfPayProportion = CommonHelp.ValueToDouble(
                            item.Amount  * pairCodeData.WorkersSelfPayProportion);
                        
                    }

                    var rowData = new PrescriptionUploadRowParam()
                    {
                        ColNum = 0,
                        PrescriptionNum = item.DocumentNo,
                        PrescriptionSort = item.DataSort,
                        ProjectCode = pairCodeData.ProjectCode,
                        FixedEncoding = pairCodeData.FixedEncoding,
                        DirectoryDate = CommonHelp.FormatDateTime(item.BillTime),
                        DirectorySettlementDate = CommonHelp.FormatDateTime(item.BillTime),
                        ProjectCodeType = pairCodeData.ProjectCodeType,
                        ProjectName = pairCodeData.ProjectName,
                        ProjectLevel = pairCodeData.ProjectLevel,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        Amount = item.Amount,
                        
                        ResidentSelfPayProportion = residentSelfPayProportion, //自付金额计算
                        Formulation = pairCodeData.Formulation,
                        Dosage = (!string.IsNullOrWhiteSpace(item.Dosage))
                            ? CommonHelp.ValueToDouble(Convert.ToDecimal(item.Dosage))
                            : 0,
                        UseFrequency = "0",
                        Usage = item.Usage,
                        Specification = item.Specification,
                        Unit = item.Unit,
                        UseDays = 0,
                        Remark = "",
                        DetailId = item.DetailId,
                        DoctorJobNumber = item.OperateDoctorId,
                        Id = item.Id,
                        //LimitApprovalDate = item.ApprovalTime,
                        //LimitApprovalUser = item.ApprovalUserName,
                        //LimitApprovalMark = item.ApprovalMark.ToString(),
                        LimitApprovalRemark = ""
                    };
                 
                    //是否是限制使用药品
                    if (pairCodeData.RestrictionSign == "1")
                    {
                        if (!string.IsNullOrWhiteSpace(item.ApprovalTime))
                        {
                            rowData.LimitApprovalDate = Convert.ToDateTime(item.ApprovalTime).ToString("yyyyMMddHHmmss");
                            rowData.LimitApprovalUser = item.ApprovalUserName;
                            rowData.LimitApprovalMark = item.ApprovalMark.ToString();
                            rowDataList.Add(rowData);
                        }
                    }
                    else
                    {
                        rowDataList.Add(rowData);
                    }

                }
                //else
                //{
                //    if (isDataIdList)
                //    {
                //       throw new Exception("商品id:"+item.DetailId+"项目名称:"+ item.DirectoryName+"未医保对码!!!");
                //    }
                //}


            }

            resultData.RowDataList = rowDataList;
            return resultData;

        }
        /// <summary>
        /// 获取处方取消上传参数
        /// </summary>
        /// <param name="param"></param>
        public DeletePrescriptionUploadParam GetDeletePrescriptionUploadParam(BaseUiBusinessIdDataParam param)
        {
            var resultData = new DeletePrescriptionUploadParam();
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            //获取医保病人信息
            var residentDataParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode,
            };
            List<QueryInpatientInfoDetailDto> queryData;
            //获取病人明细
            var queryDataDetail = _hisSqlRepository.InpatientInfoDetailQuery
                (new InpatientInfoDetailQueryParam() { BusinessId = param.BusinessId ,NotUploadMark = true});
            //获取选择
            queryData = param.DataIdList != null ? queryDataDetail.Where(c => param.DataIdList.Contains(c.Id.ToString())).ToList() : queryDataDetail;
            //获取病人医保信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(residentDataParam);
            if (queryData.Any())
            {
                //获取已上传数据、
                var uploadDataId = queryData.Where(c => c.UploadMark == 1).Select(d => d.Id).ToList();
                var batchNumberList = queryData.Where(c => c.UploadMark == 1).GroupBy(d => d.BatchNumber)
                    .Select(b => b.Key).ToList();
                if (batchNumberList.Any())
                {
                    var deleteParam = new DeletePrescriptionUploadParam()
                    {
                        BatchNumber = string.Join(",", batchNumberList.ToArray()),
                        MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo
                    };
                    resultData = deleteParam;
                }

                
            }

            return resultData;
        }
        /// <summary>
        ///处方取消上传
        /// </summary>
        /// <param name="param"></param>
        public void DeletePrescriptionUpload(BaseUiBusinessIdDataParam param)
        {
            var userBase = _webserviceBasicService.GetUserBaseInfo(param.UserId);
            userBase.TransKey = param.TransKey;
            //获取医保病人信息
            var residentDataParam = new QueryMedicalInsuranceResidentInfoParam()
            {
                BusinessId = param.BusinessId,
                OrganizationCode = userBase.OrganizationCode,
            };
            List<QueryInpatientInfoDetailDto> queryData;
            //获取病人明细
            var queryDataDetail = _hisSqlRepository.InpatientInfoDetailQuery
                (new InpatientInfoDetailQueryParam() { BusinessId = param.BusinessId });
            //获取选择
            queryData = param.DataIdList != null ? queryDataDetail.Where(c => param.DataIdList.Contains(c.Id.ToString())).ToList() : queryDataDetail;
            //获取病人医保信息
            var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(residentDataParam);
            if (queryData.Any())
            {
                //获取已上传数据、
                var uploadDataId = queryData.Where(c => c.UploadMark == 1).Select(d => d.Id).ToList();
                var batchNumberList = queryData.Where(c => c.UploadMark == 1).GroupBy(d => d.BatchNumber).Select(b => b.Key).ToList();
                if (batchNumberList.Any())
                {
                    var deleteParam = new DeletePrescriptionUploadParam()
                    {
                        BatchNumber = string.Join(",", batchNumberList.ToArray()),
                        MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo
                    };

                    //医保执行
                    //_residentMedicalInsuranceRepository.DeletePrescriptionUpload(deleteParam, uploadDataId, userBase);
                    //取消医保上传状态
                    var updateFeeParam =
                        uploadDataId.Select(c => new UpdateHospitalizationFeeParam { Id = c })
                            .ToList();
                    _medicalInsuranceSqlRepository.UpdateHospitalizationFee(updateFeeParam, true, userBase);
                    //日志
                    var joinJson = JsonConvert.SerializeObject(queryData.Select(c => c.DetailId).ToList());
                    var logParam = new AddHospitalLogParam
                    {
                        User = userBase,
                        RelationId =Guid.Parse(param.BusinessId) ,
                        JoinOrOldJson = joinJson,
                        ReturnOrNewJson = "",
                        BusinessId = param.BusinessId,
                        Remark = "医保取消处方明细id执行成功"
                    };
                    _systemManageRepository.AddHospitalLog(logParam);
                    // 回参构建
                    var xmlData = new HospitalizationFeeUploadCancelXml()
                    {
                        MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                        RowDataList = queryData.Select(c => new HospitalizationFeeUploadRowXml() { SerialNumber = c.DetailId }).ToList()
                    };
                    var strXmlBackParam = XmlSerializeHelper.HisXmlSerialize(xmlData);
                    var saveXml = new SaveXmlDataParam()
                    {
                        User = userBase,
                        MedicalInsuranceBackNum = "CXJB005",
                        MedicalInsuranceCode = "32",
                        BusinessId = param.BusinessId,
                        BackParam = strXmlBackParam
                    };
                    //存基层
                    _webBasicRepository.SaveXmlData(saveXml);
                    //日志
                    logParam.Remark = "基层取消处方明细id执行成功";
                    _systemManageRepository.AddHospitalLog(logParam);
                }
            }
            else
            {
                throw new Exception("未获取到医保退处方数据,请核实数据的正确性!!!");
            }
        }
       

        //PrescriptionUploadUiParam
    }
}
