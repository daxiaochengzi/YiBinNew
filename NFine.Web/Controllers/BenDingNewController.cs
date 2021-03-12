﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BenDing.Domain.Models.Dto.JsonEntity;
using BenDing.Domain.Models.Dto.OutpatientDepartment;
using BenDing.Domain.Models.Dto.Resident;
using BenDing.Domain.Models.Dto.Web;
using BenDing.Domain.Models.Enums;
using BenDing.Domain.Models.Params;
using BenDing.Domain.Models.Params.Base;
using BenDing.Domain.Models.Params.OutpatientDepartment;
using BenDing.Domain.Models.Params.Resident;
using BenDing.Domain.Models.Params.SystemManage;
using BenDing.Domain.Models.Params.UI;
using BenDing.Domain.Models.Params.Web;
using BenDing.Domain.Models.Params.Workers;
using BenDing.Domain.Xml;
using BenDing.Repository.Interfaces.Web;
using BenDing.Service.Interfaces;
using Newtonsoft.Json;
using NFine.Domain._03_Entity.BenDingManage;

namespace NFine.Web.Controllers
{    /// <summary>
/// 本鼎插件模式新接口
/// </summary>
    public class BenDingNewController : ApiController
    {
        private readonly IWebBasicRepository _webServiceBasicRepository;
        private readonly IHisSqlRepository _hisSqlRepository;
        private readonly IWebServiceBasicService _webServiceBasicService;
        private readonly IMedicalInsuranceSqlRepository _medicalInsuranceSqlRepository;
        private readonly ISystemManageRepository _systemManageRepository;
        private readonly IResidentMedicalInsuranceRepository _residentMedicalInsuranceRepository;
        private readonly IResidentMedicalInsuranceNewService _residentMedicalInsuranceNewService;
        private readonly IResidentMedicalInsuranceService _residentMedicalInsuranceService;
        private readonly IOutpatientDepartmentService _outpatientDepartmentService;
        private readonly IOutpatientDepartmentNewService _outpatientDepartmentNewService;
        private readonly IOutpatientDepartmentRepository _outpatientDepartmentRepository;
        private readonly IWorkerMedicalInsuranceService _workerMedicalInsuranceService;
        private readonly IWorkerMedicalInsuranceNewService _workerMedicalInsuranceNewService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insuranceRepository"></param>
        /// <param name="webServiceBasicService"></param>
        /// <param name="medicalInsuranceSqlRepository"></param>
        /// <param name="hisSqlRepository"></param>
        /// <param name="manageRepository"></param>
        /// <param name="residentMedicalInsuranceService"></param>
        /// <param name="webServiceBasicRepository"></param>
        /// <param name="outpatientDepartmentService"></param>
        /// <param name="outpatientDepartmentRepository"></param>
        /// <param name="workerMedicalInsuranceService"></param>
        /// <param name="outpatientDepartmentNewService"></param>
        /// <param name="residentMedicalInsuranceNewService"></param>
        /// <param name="workerMedicalInsuranceNewService"></param>

        public BenDingNewController(IResidentMedicalInsuranceRepository insuranceRepository,
            IWebServiceBasicService webServiceBasicService,
            IMedicalInsuranceSqlRepository medicalInsuranceSqlRepository,
            IHisSqlRepository hisSqlRepository,
            ISystemManageRepository manageRepository,
            IResidentMedicalInsuranceService residentMedicalInsuranceService,
            IWebBasicRepository webServiceBasicRepository,
            IOutpatientDepartmentService outpatientDepartmentService,
            IOutpatientDepartmentRepository outpatientDepartmentRepository,
            IWorkerMedicalInsuranceService workerMedicalInsuranceService,
            IOutpatientDepartmentNewService outpatientDepartmentNewService,
            IResidentMedicalInsuranceNewService residentMedicalInsuranceNewService,
            IWorkerMedicalInsuranceNewService workerMedicalInsuranceNewService
            )
        {
            _webServiceBasicService = webServiceBasicService;
            _medicalInsuranceSqlRepository = medicalInsuranceSqlRepository;
            _residentMedicalInsuranceRepository = insuranceRepository;
            _hisSqlRepository = hisSqlRepository;
            _systemManageRepository = manageRepository;
            _residentMedicalInsuranceService = residentMedicalInsuranceService;
            _webServiceBasicRepository = webServiceBasicRepository;
            _outpatientDepartmentService = outpatientDepartmentService;
            _outpatientDepartmentRepository = outpatientDepartmentRepository;
            _workerMedicalInsuranceService = workerMedicalInsuranceService;
            _outpatientDepartmentNewService = outpatientDepartmentNewService;
            _residentMedicalInsuranceNewService = residentMedicalInsuranceNewService;
            _workerMedicalInsuranceNewService = workerMedicalInsuranceNewService;
            
        }

        #region 门诊
        /// <summary>
        /// 获取普通门诊结算入参
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetOutpatientDepartmentCostInputParam([FromBody] OutpatientPlanBirthSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                userBase.TransKey = param.TransKey;
                var data = _outpatientDepartmentNewService.GetOutpatientDepartmentCostInputParam(new GetOutpatientPersonParam()
                {
                    User = userBase,
                    UiParam = param,
                    IdentityMark = param.IdentityMark,
                    AfferentSign = param.AfferentSign,

                });
                y.Data = XmlSerializeHelper.XmlSerialize(data);

            });
        }
        /// <summary>
        /// 获取门诊计划生育预结算参数 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetOutpatientPlanBirthPreSettlementParam([FromBody] OutpatientPlanBirthPreSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
             
                var data = _outpatientDepartmentNewService.GetOutpatientPlanBirthPreSettlementParam(param);
                y.Data = XmlSerializeHelper.XmlSerialize(data);

            });
        }
        /// <summary>
        /// 获取门诊计划生育结算参数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetOutpatientPlanBirthSettlementParam([FromBody]OutpatientPlanBirthSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.GetOutpatientPlanBirthSettlementParam(param);
                y.Data = XmlSerializeHelper.XmlSerialize(data);

            });
        }
        /// <summary>
        /// 获取门诊月结参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetMonthlyHospitalizationParam([FromBody] MonthlyHospitalizationUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.GetMonthlyHospitalizationParam(param);
                y.Data = XmlSerializeHelper.XmlSerialize(data);

            });
        }
        /// <summary>
        /// 获取门诊月结取消参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData GetMonthlyHospitalizationCancelUiParam([FromUri] GetMonthlyHospitalizationCancelUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.GetMonthlyHospitalizationCancelUiParam(param);
                y.Data = XmlSerializeHelper.XmlSerialize(data);

            });
        }
        /// <summary>
        /// 门诊费用结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientDepartmentCostInput([FromBody]OutpatientPlanBirthSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                userBase.TransKey = param.TransKey;
                //门诊计划生育
                if (param.IsBirthHospital == 1)
                {
                    var settlementData = _outpatientDepartmentNewService.OutpatientPlanBirthSettlement(param);
                    y.Data = new OutpatientCostReturnDataDto()
                    {
                       
                        SelfPayFeeAmount = settlementData.CashPayment,
                        PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(settlementData))
                    };

                }
                else
                {   //普通门诊
                    var data = _outpatientDepartmentNewService.OutpatientDepartmentCostInput(new GetOutpatientPersonParam()
                    {
                        User = userBase,
                        UiParam = param,
                        IdentityMark = param.IdentityMark,
                        AfferentSign = param.AfferentSign,
                        InsuranceType = param.InsuranceType,
                        AccountBalance = param.AccountBalance,
                        SettlementXml = param.SettlementJson,


                    });
                    if (data == null) throw new Exception("获取门诊结算反馈数据失败!!!");

                    y.Data = new OutpatientCostReturnDataDto()
                    {
                        ReimbursementExpensesAmount = data.ReimbursementExpensesAmount,
                        SelfPayFeeAmount = data.SelfPayFeeAmount,
                        PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(data))
                    };
                }

            });

        }
        /// <summary>
        /// 门诊计划生育预结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientPlanBirthPreSettlement([FromBody]OutpatientPlanBirthPreSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                userBase.TransKey = param.TransKey;
               
                var settlementData = _outpatientDepartmentNewService.OutpatientPlanBirthPreSettlement(param);
                var accountPayment = settlementData.AccountPayment + settlementData.CivilServantsSubsidies +
                                settlementData.CivilServantsSubsidy + settlementData.OtherPaymentAmount +
                                settlementData.BirthAllowance + settlementData.SupplementPayAmount;

                var cashPayment = settlementData.TotalAmount - accountPayment;
                y.Data = new OutpatientCostReturnDataDto()
                {
                    SelfPayFeeAmount = cashPayment<0?0: cashPayment,
                    PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(settlementData))
                };

            });

        }
        /// <summary>
        /// 获取取消门诊费用结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData GetCancelOutpatientDepartmentCostParam([FromUri]CancelOutpatientDepartmentCostUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data=_outpatientDepartmentNewService.GetCancelOutpatientDepartmentCostParam(param);
                y.Data = data;
            });

        }
        /// <summary>
        /// 获取取消入院登记信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetAdmissionRegistrationCancelInfo([FromBody]UiBaseDataParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                userBase.TransKey = param.TransKey;
                var resultData = new AdmissionRegistrationCancelInfoDto();
             
                var infoData = new GetInpatientInfoParam()
                {
                    User = userBase,
                    BusinessId = param.BusinessId,
                };
                //获取病人信息
                var inpatientData = _webServiceBasicService.GetInpatientInfo(infoData);
                var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId,
                    OrganizationCode = userBase.OrganizationCode
                };
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
                if (residentData == null) throw new Exception("当前病人未办理医保入院登记,不能取消入院登记!!!");
                if (residentData.MedicalInsuranceState == MedicalInsuranceState.HisSettlement)
                {
                    throw new Exception("当前病人已办理医保结算,请先取消结算,再取消入院登记!!!");
                }
                resultData.InpatientInfo = inpatientData;
                resultData.InsuranceType = residentData.InsuranceType;
                resultData.IsBirthHospital = residentData.IsBirthHospital;
                y.Data = resultData;
            });

        }
        
        /// <summary>
        /// 取消门诊费用结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData CancelOutpatientDepartmentCost([FromUri]CancelOutpatientDepartmentCostUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
               _outpatientDepartmentNewService.CancelOutpatientDepartmentCost(param);
            });

        }
        
        /// <summary>
        ///查询门诊费用结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData QueryOutpatientDepartmentCost([FromUri]UiBaseDataParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var resultData = new QueryOutpatientDepartmentCostDataDto();
              
                var baseUser = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                baseUser.TransKey = param.TransKey;
                var paramIni = new SettlementCancelParam
                {
                    User = baseUser,
                    BusinessId = param.BusinessId
                };
             var outpatientSettlementNo=  _webServiceBasicService.GetOutpatientSettlementNo(new GetOutpatientSettlementNoParam()
                {
                    BusinessId = param.BusinessId,
                    User = baseUser
                });
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId,
                    SettlementNo = outpatientSettlementNo,
                    MedicalInsuranceState = 6
                });
                var cancelSettlementData = _webServiceBasicService.GetOutpatientSettlementCancel(paramIni);
                var queryData = _hisSqlRepository.QueryOutpatient(new QueryOutpatientParam()
                {
                    BusinessId = param.BusinessId,
                    Id = residentData.PatientId
                });
                if (queryData == null) throw new Exception("获取门诊结算病人失败!!!");

                //获取门诊病人信息
                resultData.DepartmentName = queryData.DepartmentName;
                resultData.DiagnosticDoctor = queryData.DiagnosticDoctor;
                resultData.IdCardNo = queryData.IdCardNo;
                resultData.Operator = cancelSettlementData.CancelOperator;
                resultData.PatientName = queryData.PatientName;
                resultData.OutpatientNumber = queryData.OutpatientNumber;
              
                string[] arr = queryData.OutpatientNumber.Split('M');
                string mz = arr[1];
                resultData.OutpatientNumber = mz;
                resultData.VisitDate = queryData.VisitDate;
                //医保门诊结算查询
                var queryOutpatientData = _outpatientDepartmentService.QueryOutpatientDepartmentCost(param);
                resultData.ReimbursementExpensesAmount = queryOutpatientData.ReimbursementExpensesAmount;
                resultData.SelfPayFeeAmount = queryOutpatientData.SelfPayFeeAmount;
                resultData.MedicalTreatmentTotalCost = queryOutpatientData.AllAmount;
                resultData.SettlementNo = cancelSettlementData.SettlementNo;
                resultData.SettlementType = residentData.SettlementType;
                resultData.InsuranceType = residentData.InsuranceType;
                if (!string.IsNullOrWhiteSpace(residentData.OtherInfo))
                {
                    resultData.PayMsg = CommonHelp.GetPayMsg(residentData.OtherInfo);
                }
                resultData.IsBirthHospital = residentData.IsBirthHospital;
                y.Data = resultData;

            });

        }
        /// <summary>
        ///门诊月结汇总
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData MonthlyHospitalization([FromUri]MonthlyHospitalizationUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                _outpatientDepartmentNewService.MonthlyHospitalization(param);
            });

        }
        ///// <summary>
        /////取消门诊月结汇总
        ///// </summary>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ApiJsonResultData CancelMonthlyHospitalization([FromUri]CancelMonthlyHospitalizationUiParam param)
        //{
        //    return new ApiJsonResultData(ModelState).RunWithTry(y =>
        //    {
        //        _outpatientDepartmentNewService.CancelMonthlyHospitalization(param);

        //    });

        //}
        /// <summary>
        /// 门诊生育结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientPlanBirthSettlement([FromBody]OutpatientPlanBirthSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                //var resultData = JsonConvert.DeserializeObject<WorkerHospitalizationPreSettlementDto>(param.ResultData);
                //_outpatientDepartmentService.OutpatientPlanBirthSettlement(param);

            });

        }
        /// <summary>
        /// 获取门诊电子凭参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientNationEcTransParam([FromBody]OutpatientNationEcTransUiParam  param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
              
              var data=  _outpatientDepartmentNewService.OutpatientNationEcTransParam(param);
                y.Data = data;
            });

        }

        /// <summary>
        /// 门诊电子凭支付
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientNationEcTrans([FromBody]OutpatientNationEcTransUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
              var data=_outpatientDepartmentNewService.OutpatientNationEcTrans(param);
                var msg = new List<PayMsgData>();
                msg.Add(new PayMsgData()
                {
                    Name = "账户支金额",
                    Value = data.AccountPayAmount.ToString(CultureInfo.InvariantCulture)
                });
                msg.Add(new PayMsgData()
                {
                    Name = "自付金额",
                    Value = data.SelfPayAmount.ToString(CultureInfo.InvariantCulture)
                });
                y.Data = new OutpatientCostReturnDataDto()
                {
                 
                    SelfPayFeeAmount = data.SelfPayAmount,
                    PayMsg = msg
                };
              
            });

        }
        /// <summary>
        /// 门诊明细查询
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData OutpatientDetailQuery([FromUri]OutpatientDetailQueryUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var queryData = _outpatientDepartmentNewService.OutpatientDetailQuery(param);
                var data = new
                {
                    data = queryData.PageData,
                    count = queryData.PageData.Count(),
                    NewAmount= queryData.NewTotalCost,
                    MedicalInsuranceTotalCost=  queryData.MedicalInsuranceTotalCost
                };
                y.Data = data;

            });
        }
        /// <summary>
        /// 清除门诊调差数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData DeleteOutpatientAdjustmentDifference([FromBody]UiBaseDataParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                string sql = $@"update [dbo].[OutpatientFee] set IsDelete=1 where    [PatientId] is null  and [OutpatientNo]=''
                       and IsDelete=0 and IsAdjustmentDifference=1 ";
                       _hisSqlRepository.ExecuteSql(sql);
             
                //  y.Data = data;
            });

        }
      
        /// <summary>
        /// 获取门诊居民电子凭证参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetOutpatientNationEcTransResidentParam([FromBody]OutpatientNationEcTransUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.GetOutpatientNationEcTransResidentParam(param);
                y.Data = data;
            });

        }

        /// <summary>
        /// 居民电子凭证支付
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientNationEcTransResident([FromBody]OutpatientNationEcTransUiParam param)
       {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.OutpatientNationEcTransResident(param);
               
                y.Data = new OutpatientCostReturnDataDto()
                {
                    ReimbursementExpensesAmount = data.ReimbursementAmount,
                    SelfPayFeeAmount = data.CashPaymentAmount,
                    PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(data))
                };

            });

        }
        /// <summary>
        /// 获取门诊划卡参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData WorkerOutpatientSettlementCardParam([FromBody]WorkerHospitalSettlementCardUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.WorkerOutpatientSettlementCardParam(param);
                y.Data = data;
            });

        }
        /// <summary>
        /// 门诊划卡
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData WorkerOutpatientSettlementCard([FromBody]WorkerHospitalSettlementCardUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.WorkerOutpatientSettlementCard(param);
                y.Data = new OutpatientCostReturnDataDto()
                {
                    SelfPayFeeAmount = data.CashPayment,
                    PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(data))
                };
            });

        }
        /// <summary>
        /// 获取门诊居民预结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetResidentOutpatientPreSettlementParam([FromBody]GetResidentOutpatientSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.GetResidentOutpatientPreSettlementParam(param);
                y.Data = data;
            });

        }
        /// <summary>
        /// 普通居民预结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData ResidentOutpatientPreSettlement([FromBody]GetResidentOutpatientSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.ResidentOutpatientPreSettlement(param);
                y.Data = new OutpatientCostReturnDataDto()
                {
                    SelfPayFeeAmount = data.CashPaymentAmount,
                    PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(data))
                };
            });

        }
        /// <summary>
        /// 获取门诊划卡参数 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData ResidentOutpatientSettlementCardParam([FromBody]GetResidentOutpatientSettlementCardUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.ResidentOutpatientSettlementCardParam(param);
                y.Data = data;
            });

        }
        /// <summary>
        /// 门诊居民划卡
        /// </summary>
        /// <param name="param"></param> 
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData ResidentOutpatientSettlementCard([FromBody]GetResidentOutpatientSettlementCardUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var data = _outpatientDepartmentNewService.ResidentOutpatientSettlementCard(param);
                y.Data = new OutpatientCostReturnDataDto()
                {
                    SelfPayFeeAmount = data.CashPaymentAmount,
                    PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(data))
                };
            });

        }
        /// <summary>
        /// 门诊居民确认结算
        /// </summary>
        /// <param name="param"></param> 
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientResidentConfirmSettlement([FromBody]OutpatientResidentConfirmSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
               _outpatientDepartmentNewService.OutpatientResidentConfirmSettlement(param);
            });

        }
        /// <summary>
        /// 门诊费用差值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientAdjustmentDifferenceValue([FromBody]AdjustmentDifferenceValueParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                List<string> idList = new List<string>();
                idList.Add(param.Id);
                param.UnitPrice = CommonHelp
                    .ValueToFour(Convert.ToDecimal(param.Amount) / Convert.ToDecimal(param.Quantity)).ToString(CultureInfo.InvariantCulture);
                //费用数据
                var feeDataList = _hisSqlRepository.InpatientInfoDetailQuery(new InpatientInfoDetailQueryParam()
                {
                    IdList = idList,
                    UploadMark = 0,

                });
                if (feeDataList != null && feeDataList.Any())
                {
                    var feeData = feeDataList.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(feeData.ProjectCode)) throw new Exception("当前项目未医保对码,不能调整!!!");
                    string sql = $@"update [dbo].[HospitalizationFee] set [UnitPrice]={param.UnitPrice},[Quantity]={param.Quantity},[Amount]={param.Amount},
                                 [AdjustmentDifferenceValue]={feeData.Amount} where id='{param.Id}'";
                    _hisSqlRepository.ExecuteSql(sql);
                    _systemManageRepository.AddHospitalLog(new AddHospitalLogParam()
                    {
                        BusinessId = param.BusinessId,
                        RelationId = Guid.Parse(param.Id),
                        Remark = "调整费用明细",
                        User = userBase,
                        JoinOrOldJson = JsonConvert.SerializeObject(feeData)


                    });
                }
                else
                {

                    throw new Exception("查询数据失败");
                }


                //var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                //处方上传
                //var data = _residentMedicalInsuranceNewService.GetPrescriptionUploadParam(param, userBase);
                //y.Data = data;
            });



        }

        #endregion
        #region 公共信息
        /// <summary>
        /// 获取医院信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData GetHospitalInfo([FromUri]UiBaseDataParam param)
        {
            return new ApiJsonResultData(ModelState, new QueryMedicalInsuranceDetailInfoDto()).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                //获取医院等级
                var gradeData = _systemManageRepository.QueryHospitalOrganizationGrade(userBase.OrganizationCode);
                if (gradeData.EffectiveTime != null)
                {
                    var fistDate = Convert.ToDateTime(gradeData.EffectiveTime);
                    var ts = fistDate.Subtract(DateTime.Now);
                    int days = ts.Days;
                    if (days <= 7)
                    {
                        gradeData.Msg = "授权时间还剩余"+ days+"天!!!";
                    }
                    if (days <0)
                    { //超期7天暂停服务
                        if (days < -7)
                        {
                            gradeData.IsSuspend = 1;
                        }
                        gradeData.Msg = "授权时间超期" + Math.Abs(days)  + "天,请注意超期7天会暂停服务!!!";
                    }
                   
                }

                y.Data = gradeData;
            });

        }
        /// <summary>
        /// 门诊不传医保查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData OutpatientExclusionQuery([FromUri]OutpatientExclusionQueryUiParam param)
        {
            return new ApiJsonResultData(ModelState, new QueryICD10InfoDto()).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);

                var queryData = _hisSqlRepository.OutpatientExclusionQuery(new OutpatientExclusionQueryParam()
                {
                    Page = param.Page,
                    DirectoryName = param.DirectoryName,
                    Limit = param.Limit,
                    OrganizationCode = userBase.OrganizationCode
                });
                var data = new
                {
                    data = queryData.Values.FirstOrDefault(),
                    count = queryData.Keys.FirstOrDefault()
                };
                y.Data = data;
            });
        }
        /// <summary>
        /// 添加门诊不传医保
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientExclusionAdd([FromBody]OutpatientExclusionAddUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                var data = _hisSqlRepository.OutpatientExclusionAdd(new OutpatientExclusionAddParam()
                {
                    DirectoryName = param.DirectoryName,
                    DirectoryCode = param.DirectoryCode,
                    DirectoryCategoryName= param.DirectoryCategoryName,
                    User = userBase
                });
               
            });

        }

        /// <summary>
        /// 取消门诊不传医保
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData OutpatientExclusionCancel([FromBody]OutpatientExclusionCancelUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
              
                var data = _hisSqlRepository.OutpatientExclusionCancel(new OutpatientExclusionCancelParam()
                {
                  
                    UserId = param.UserId,
                    Id = param.Id
                });

            });

        }
        #endregion
        #region 医保
        /// <summary>
        /// 获取医保入院登记参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetHospitalizationRegisterParam([FromBody]ResidentHospitalizationRegisterUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                //初始化职工参数
                var workerParam = AutoMapper.Mapper.Map<WorKerHospitalizationRegisterUiParam>(param);
                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");     
                ////职工
                if (param.InsuranceType == "310")
                { 
                  var workerDataParam =  _workerMedicalInsuranceNewService.GetWorkerHospitalizationRegisterParam(
                     workerParam);
                     var data = JsonConvert.SerializeObject(workerDataParam); 
                    
                    y.Data = data;
                }
                //居民
                if (param.InsuranceType == "342")
                {
                    var hospitalizationRegisterParam= _residentMedicalInsuranceNewService.GetResidentHospitalizationRegisterParam(param);
                    y.Data = XmlSerializeHelper.XmlSerialize(hospitalizationRegisterParam);
                } 
            });

        }
        /// <summary>
        /// 医保入院登记
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData HospitalizationRegister([FromBody]ResidentHospitalizationRegisterUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                //初始化职工参数
                var workerParam = AutoMapper.Mapper.Map<WorKerHospitalizationRegisterUiParam>(param);
                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");

                ////职工
                if (param.InsuranceType == "310") _workerMedicalInsuranceNewService.WorkerHospitalizationRegister(workerParam);
                //居民
                if (param.InsuranceType == "342") _residentMedicalInsuranceNewService.HospitalizationRegister(param);
            });

        }
        /// <summary>
        /// 获取医保入院登记修改参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetHospitalizationModifyParam([FromBody]HospitalizationModifyUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {

                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId
                });
                //职工
                if (residentData.InsuranceType == "310")
                {
                   
                    //生育
                    if (residentData.IsBirthHospital == 1)
                    {
                        var modifyParam = _residentMedicalInsuranceNewService.GetHospitalizationModifyParam(param);
                        y.Data = XmlSerializeHelper.XmlSerialize(modifyParam);
                    }
                    else 
                    {
                        var hospitalizationModifyParam = _workerMedicalInsuranceNewService.GetModifyWorkerHospitalizationParam(param);
                        y.Data = JsonConvert.SerializeObject(hospitalizationModifyParam);
                    }
                }
               
                //居民
                if (residentData.InsuranceType == "342")
                {
                   var hospitalizationModifyParam= _residentMedicalInsuranceNewService.GetHospitalizationModifyParam(param);
                    y.Data = XmlSerializeHelper.XmlSerialize(hospitalizationModifyParam);
                }
            });

        }
        /// <summary>
        /// 医保入院登记修改
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData HospitalizationModify([FromBody]HospitalizationModifyUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {

                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
               
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId
                });
                ////职工
                if (residentData.InsuranceType == "310") _workerMedicalInsuranceNewService.ModifyWorkerHospitalization(param);
                //居民
                if (residentData.InsuranceType == "342") _residentMedicalInsuranceNewService.HospitalizationModify(param);
            });

        }
        /// <summary>
        /// 获取生育住院参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetBirthHospitalizationRegisterParam([FromBody]BirthHospitalizationRegisterUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                //初始化居民参数
                var residentParam = AutoMapper.Mapper.Map<ResidentHospitalizationRegisterUiParam>(param);
                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                ////职工
                if (param.InsuranceType == "310")
                {
                    var workerDataParam = _workerMedicalInsuranceNewService.GetWorkerBirthHospitalizationRegisterParam(
                        param);
                    y.Data = XmlSerializeHelper.XmlSerialize(workerDataParam);
                }
                //居民
                if (param.InsuranceType == "342")
                {
                    var hospitalizationRegisterParam = _residentMedicalInsuranceNewService.GetResidentHospitalizationRegisterParam(residentParam);
                    y.Data = XmlSerializeHelper.XmlSerialize(hospitalizationRegisterParam);
                }


            });

        }
        /// <summary>
        /// 生育入院登记
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData BirthHospitalizationRegister([FromBody]BirthHospitalizationRegisterUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {

                //初始化居民参数
                var residentParam = AutoMapper.Mapper.Map<ResidentHospitalizationRegisterUiParam>(param);
                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                ////职工
                if (param.InsuranceType == "310")
                {
                    _workerMedicalInsuranceService.WorkerBirthHospitalizationRegister(param);
                }
                //居民
                if (param.InsuranceType == "342")
                {
                    _residentMedicalInsuranceNewService.HospitalizationRegister(residentParam);
                }


               

            });

        }
        /// <summary>
        /// 获取医保住院费用预结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetHospitalizationPreSettlementParam([FromBody]HospitalizationPreSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState, new HospitalizationPresettlementDto()).RunWithTry(y =>
            {
                var resultData = new SettlementDto();
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId
                });
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                //更新病人处方明细
                _webServiceBasicService.GetInpatientInfoDetail(userBase, param.BusinessId);
                var queryParam = new InpatientInfoDetailQueryParam()
                {
                    BusinessId = param.BusinessId
                };
                //获取病人处方明细
                var queryData = _hisSqlRepository.InpatientInfoDetailQuery(queryParam);
                if (!queryData.Any()) throw new Exception("当前病人没有处方明细,不能进行预结算!!!");
                if (queryData.Count(c => c.UploadMark == 0) > 0) throw new Exception("当前病人还有处方明细未上传至医保,不能进行预结算!!!");
                if (residentData.MedicalInsuranceState == MedicalInsuranceState.HisSettlement) throw new Exception("当前病人已医保结算,不能预结算!!!");
                //职工
                if (residentData.InsuranceType == "310")
                {
                    if (residentData.IsBirthHospital == 0)
                    {
                        var workerPreSettlementParam = _workerMedicalInsuranceNewService.GetWorkerHospitalizationPreSettlementParam(param);
                        y.Data = JsonConvert.SerializeObject(workerPreSettlementParam);
                    }//职工生育预结算
                    else if (residentData.IsBirthHospital == 1)
                    {
                        if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                        var workerBirthPreSettlementData = _workerMedicalInsuranceNewService.GetWorkerBirthPreSettlementParam(new WorkerBirthPreSettlementUiParam()
                        {
                            TransKey = param.TransKey,
                            BusinessId = param.BusinessId,
                            DiagnosisList = param.DiagnosisList,
                            UserId = param.UserId,
                            MedicalCategory = param.MedicalCategory,
                            FetusNumber = param.FetusNumber

                        });
                        y.Data = XmlSerializeHelper.XmlSerialize(workerBirthPreSettlementData);

                    }
                }
                //居民
                if (residentData.InsuranceType == "342")
                {
                    var hospitalizationPreSettlementParam = _residentMedicalInsuranceNewService.GetHospitalizationPreSettlement(param);
                    y.Data = XmlSerializeHelper.XmlSerialize(hospitalizationPreSettlementParam);
                }
            });

        }
        /// <summary>
        /// 医保住院费用预结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData HospitalizationPreSettlement([FromBody]HospitalizationPreSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState, new HospitalizationPresettlementDto()).RunWithTry(y =>
            {
                var resultData = new SettlementDto();
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId
                });
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                //更新病人处方明细
                _webServiceBasicService.GetInpatientInfoDetail(userBase, param.BusinessId);
                var queryParam = new InpatientInfoDetailQueryParam()
                {
                    BusinessId = param.BusinessId
                };
                //获取病人处方明细
                var queryData = _hisSqlRepository.InpatientInfoDetailQuery(queryParam);
                if (!queryData.Any()) throw new Exception("当前病人没有处方明细,不能进行预结算!!!");
                if (queryData.Count(c => c.UploadMark == 0) > 0) throw new Exception("当前病人还有处方明细未上传至医保,不能进行预结算!!!");
                if (residentData.MedicalInsuranceState == MedicalInsuranceState.HisSettlement) throw new Exception("当前病人已医保结算,不能预结算!!!");
                //职工
                if (residentData.InsuranceType == "310")
                {

                    if (residentData.IsBirthHospital == 0)
                    {
                        var workerSettlementData = _workerMedicalInsuranceNewService.WorkerHospitalizationPreSettlement(param);
                        resultData.PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(workerSettlementData));
                        resultData.CashPayment = workerSettlementData.CashPayment;
                        resultData.ReimbursementExpenses = workerSettlementData.ReimbursementExpenses;
                        resultData.TotalAmount = workerSettlementData.TotalAmount;

                    }//职工生育预结算
                    else if (residentData.IsBirthHospital == 1)
                    {
                        if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                        var workerSettlementData = _workerMedicalInsuranceNewService.WorkerBirthPreSettlement(new WorkerBirthPreSettlementUiParam()
                        {
                            TransKey = param.TransKey,
                            BusinessId = param.BusinessId,
                            DiagnosisList = param.DiagnosisList,
                            UserId = param.UserId,
                            MedicalCategory = param.MedicalCategory,
                            FetusNumber = param.FetusNumber,
                            SettlementJson = param.SettlementJson

                        });
                        resultData.PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(workerSettlementData));
                        resultData.CashPayment = workerSettlementData.CashPayment;
                        resultData.ReimbursementExpenses = workerSettlementData.ReimbursementExpenses;
                        resultData.TotalAmount = workerSettlementData.TotalAmount;
                    }


                }
                //居民
                if (residentData.InsuranceType == "342")
                {
                    var residentSettlementData = _residentMedicalInsuranceNewService.HospitalizationPreSettlement(param);
                    resultData.PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(residentSettlementData));
                    resultData.CashPayment = residentSettlementData.CashPayment;
                    resultData.ReimbursementExpenses = residentSettlementData.ReimbursementExpenses;
                    resultData.TotalAmount = residentSettlementData.TotalAmount;
                }

                y.Data = resultData;


            });

        }
        /// <summary>
        /// 获取医保出院费用结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetLeaveHospitalSettlementParam([FromBody]LeaveHospitalSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState, new HospitalizationPresettlementDto()).RunWithTry(y =>
            {
                var resultData = new SettlementDto();
                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                userBase.TransKey = param.TransKey;
               
                var infoData = new GetInpatientInfoParam()
                {
                    User = userBase,
                    BusinessId = param.BusinessId,
                };
                ////获取住院病人信息
                //var inpatientData = _webServiceBasicService.GetInpatientInfo(infoData);
                //获取病人结算信息
                var settlementData = _webServiceBasicService.GetHisHospitalizationSettlement(infoData);
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId
                });

                if (settlementData.AllAmount !=
                    residentData.MedicalInsuranceAllAmount)
                {
                    throw new Exception("医保总费用与基层总费用不相等,不能进行结算!!!");
                }

                //职工
                if (residentData.InsuranceType == "310")
                {
                    if (residentData.IsBirthHospital == 0)
                    {
                        var workerSettlementParam = _workerMedicalInsuranceNewService.GetWorkerHospitalizationSettlementParam(new WorkerHospitalizationSettlementUiParam()
                        {
                            DiagnosisList = param.DiagnosisList,
                            TransKey = param.TransKey,
                            BusinessId = param.BusinessId,
                            LeaveHospitalInpatientState = param.LeaveHospitalInpatientState,
                            UserId = param.UserId,
                        });
                        y.Data = JsonConvert.SerializeObject(workerSettlementParam);

                    }//职工生育结算
                    else if (residentData.IsBirthHospital == 1)
                    {
                        var workerBirthSettlementParam = _workerMedicalInsuranceNewService.GetWorkerBirthSettlementParam(
                          new WorkerBirthSettlementUiParam()
                          {
                              TransKey = param.TransKey,
                              UserId = param.UserId,
                              BusinessId = param.BusinessId,
                              AccountPayment = !string.IsNullOrWhiteSpace(param.AccountPayment) == true ? Convert.ToDecimal(param.AccountPayment) : 0,
                              DiagnosisList = param.DiagnosisList,
                              MedicalCategory = param.MedicalCategory,
                              FetusNumber = param.FetusNumber,
                              LeaveHospitalInpatientState = param.LeaveHospitalInpatientState,

                          });
                        y.Data = XmlSerializeHelper.XmlSerialize(workerBirthSettlementParam);
                    }


                }
                //居民
                if (residentData.InsuranceType == "342")
                {
                    var residentSettlementDataParam = _residentMedicalInsuranceNewService.GetHospitalizationSettlement(param);
                    y.Data = XmlSerializeHelper.XmlSerialize(residentSettlementDataParam);
                }
            });

        }
        /// <summary>
        /// 医保出院费用结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData LeaveHospitalSettlement([FromBody]LeaveHospitalSettlementUiParam param)
        {
            return new ApiJsonResultData(ModelState, new HospitalizationPresettlementDto()).RunWithTry(y =>
            {
                decimal insuranceBalance = !string.IsNullOrWhiteSpace(param.InsuranceBalance) == true
                    ? Convert.ToDecimal(param.InsuranceBalance): 0;
                var resultData = new SettlementDto();
                if (param.DiagnosisList == null) throw new Exception("诊断不能为空!!!");
                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId
                });
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                userBase.TransKey = param.TransKey;
                //var infoData = new GetInpatientInfoParam()
                //{
                //    User = userBase,
                //    BusinessId = param.BusinessId,
                //};
                ////获取病人结算信息
                //var settlementData = _webServiceBasicService.GetHisHospitalizationSettlement(infoData);
                //if (settlementData.AllAmount !=
                //   residentData.MedicalInsuranceAllAmount)
                //{
                //    throw new Exception("医保总费用与基层总费用不相等,不能进行结算!!!");
                //}
                //职工
                if (residentData.InsuranceType == "310")
                {
                    if (residentData.IsBirthHospital == 0)
                    {
                        var workerSettlementData = _workerMedicalInsuranceNewService.WorkerHospitalizationSettlement(new WorkerHospitalizationSettlementUiParam()
                        {
                            DiagnosisList = param.DiagnosisList,
                            TransKey = param.TransKey,
                            BusinessId = param.BusinessId,
                            LeaveHospitalInpatientState = param.LeaveHospitalInpatientState,
                            UserId = param.UserId,
                            SettlementJson = param.SettlementJson,
                            InsuranceBalance = insuranceBalance
                        });
                        resultData.CashPayment = workerSettlementData.CashPayment;
                        resultData.PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(workerSettlementData));
                        resultData.ReimbursementExpenses = workerSettlementData.ReimbursementExpenses;
                        resultData.TotalAmount = workerSettlementData.TotalAmount;
                    }//职工生育结算
                    else if (residentData.IsBirthHospital == 1)
                    {
                        var workerSettlementData = _workerMedicalInsuranceNewService.WorkerBirthSettlement(
                          new WorkerBirthSettlementUiParam()
                          {
                              TransKey = param.TransKey,
                              UserId = param.UserId,
                              BusinessId = param.BusinessId,
                              AccountPayment = !string.IsNullOrWhiteSpace(param.AccountPayment) == true ? Convert.ToDecimal(param.AccountPayment) : 0,
                              DiagnosisList = param.DiagnosisList,
                              MedicalCategory = param.MedicalCategory,
                              FetusNumber = param.FetusNumber,
                              LeaveHospitalInpatientState = param.LeaveHospitalInpatientState,
                              SettlementJson = param.SettlementJson,
                              InsuranceBalance = insuranceBalance

                          });
                        resultData.CashPayment = workerSettlementData.CashPayment;
                        resultData.PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(workerSettlementData));
                        resultData.ReimbursementExpenses = workerSettlementData.ReimbursementExpenses;
                        resultData.TotalAmount = workerSettlementData.TotalAmount;
                    }
                }
                //居民
                if (residentData.InsuranceType == "342")
                {
                    var residentSettlementData = _residentMedicalInsuranceNewService.LeaveHospitalSettlement(param);
                    resultData.PayMsg = CommonHelp.GetPayMsg(JsonConvert.SerializeObject(residentSettlementData));
                    resultData.CashPayment = residentSettlementData.CashPayment;
                    resultData.ReimbursementExpenses = residentSettlementData.ReimbursementExpenses;
                    resultData.TotalAmount = residentSettlementData.TotalAmount;
                }

                y.Data = resultData;

            });

        }
        /// <summary>
        /// 获取取消结算参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetLeaveHospitalSettlementCancelParam(
            [FromBody] LeaveHospitalSettlementCancelUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId,
                    OrganizationCode = userBase.OrganizationCode
                };
                userBase.TransKey = param.TransKey;
               
                //获取医保病人信息
                var residentData =
                    _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
                if (residentData == null) throw new Exception("当前病人未办理医保入院登记!!!");
                if (param.CancelLimit == "1")
                {
                    if (residentData.MedicalInsuranceState != MedicalInsuranceState.HisSettlement)
                    {
                        throw new Exception("当前病人未办理医保结算，不能取消结算!!!");
                    }
                }

               
                //居民
                if (residentData.InsuranceType == "342")
                {
                    var settlementCancelParam = new LeaveHospitalSettlementCancelParam()
                    {
                        MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                        SettlementNo = residentData.SettlementNo,
                        Operators = userBase.UserName,
                        CancelLimit = param.CancelLimit,

                    };
                    y.Data = XmlSerializeHelper.XmlSerialize(settlementCancelParam);
                }

                //职工
                if (residentData.InsuranceType == "310")
                {
                    if (residentData.IsBirthHospital == 0)
                    {
                        
                        //获取医院等级
                        var gradeData =
                            _systemManageRepository.QueryHospitalOrganizationGrade(userBase.OrganizationCode);
                        var cancelParam = new WorkerSettlementCancelParam()
                        {
                            BusinessId = param.BusinessId,
                            Id = residentData.Id,
                            User = userBase,
                            SettlementNo = residentData.SettlementNo,
                            CancelLimit = param.CancelLimit,
                            MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                            AdministrativeArea = gradeData.AdministrativeArea,
                            OrganizationCode = userBase.OrganizationCode,
                            WorkersStrokeCardNo = residentData.WorkersStrokeCardNo,
                            CancelSettlementRemarks = param.CancelSettlementRemarks,
                        };
                        y.Data = JsonConvert.SerializeObject(cancelParam);
                       
                    }
                }

                    //职工生育住院取消采用居民取消结算
                    if (residentData.IsBirthHospital == 1)
                    {
                        var settlementCancelParam = new LeaveHospitalSettlementCancelParam()
                        {
                            MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                            SettlementNo = residentData.SettlementNo,
                            Operators = userBase.UserName,
                            CancelLimit = param.CancelLimit,

                        };
                    y.Data = XmlSerializeHelper.XmlSerialize(settlementCancelParam);

                }
            });
        }
        /// <summary>
        /// 取消医保出院费用结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData LeaveHospitalSettlementCancel([FromBody]LeaveHospitalSettlementCancelUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {   //获取操作人员信息

                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId,
                    OrganizationCode = userBase.OrganizationCode
                };
                userBase.TransKey = param.TransKey;

                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
                if (residentData == null) throw new Exception("当前病人未办理医保入院登记!!!");
                //居民
                if (residentData.InsuranceType == "342")
                {
                    var settlementCancelParam = new LeaveHospitalSettlementCancelParam()
                    {
                        MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                        SettlementNo = residentData.SettlementNo,
                        Operators = CommonHelp.GuidToStr(userBase.UserId),
                        CancelLimit = param.CancelLimit,

                    };
                    var cancelParam = new LeaveHospitalSettlementCancelInfoParam()
                    {
                        BusinessId = param.BusinessId,
                        Id = residentData.Id,
                        User = userBase,
                    };
                    _residentMedicalInsuranceNewService.LeaveHospitalSettlementCancel(settlementCancelParam, cancelParam);
                }
                //职工
                if (residentData.InsuranceType == "310")
                {
                    if (residentData.IsBirthHospital == 0)
                    {
                        if (residentData.MedicalInsuranceState != MedicalInsuranceState.HisSettlement) throw new Exception("当前病人未医保结算");
                        //获取医院等级
                        var gradeData = _systemManageRepository.QueryHospitalOrganizationGrade(userBase.OrganizationCode);
                        var cancelParam = new WorkerSettlementCancelParam()
                        {
                            BusinessId = param.BusinessId,
                            Id = residentData.Id,
                            User = userBase,
                            SettlementNo = residentData.SettlementNo,
                            CancelLimit = param.CancelLimit,
                            MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                            AdministrativeArea = gradeData.AdministrativeArea,
                            OrganizationCode = userBase.OrganizationCode,
                            WorkersStrokeCardNo = residentData.WorkersStrokeCardNo,
                            CancelSettlementRemarks = param.CancelSettlementRemarks,
                        };
                        _workerMedicalInsuranceService.WorkerSettlementCancel(cancelParam);
                    }
                    //职工生育住院取消采用居民取消结算
                    if (residentData.IsBirthHospital == 1)
                    {
                        var settlementCancelParam = new LeaveHospitalSettlementCancelParam()
                        {
                            MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                            SettlementNo = residentData.SettlementNo,
                            Operators = CommonHelp.GuidToStr(userBase.UserId),
                            CancelLimit = param.CancelLimit,

                        };
                        var cancelParam = new LeaveHospitalSettlementCancelInfoParam()
                        {
                            BusinessId = param.BusinessId,
                            Id = residentData.Id,
                            User = userBase,
                        };
                        _residentMedicalInsuranceNewService.LeaveHospitalSettlementCancel(settlementCancelParam, cancelParam);
                    }


                }
            });

        }
        /// <summary>
        /// 医保入院登记取消
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData MedicalInsuranceHospitalizationRegisterCancel([FromBody]UiBaseDataParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                //获取操作人员信息
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                var queryResidentParam = new QueryMedicalInsuranceResidentInfoParam()
                {
                    BusinessId = param.BusinessId,
                    OrganizationCode = userBase.OrganizationCode
                };
                userBase.TransKey = param.TransKey;

                //获取医保病人信息
                var residentData = _medicalInsuranceSqlRepository.QueryMedicalInsuranceResidentInfo(queryResidentParam);
                if (residentData == null) throw new Exception("当前病人未办理医保入院登记,不能取消入院登记!!!");
                if (residentData.MedicalInsuranceState == MedicalInsuranceState.HisSettlement)
                {
                    throw new Exception("当前病人已办理结算,请先取消结算后,再取消入院登记!!!");
                }
                else
                {
                    CancelOperation("2");
                }
                void CancelOperation(string cancelLimit)
                {
                    //居民
                    if (residentData.InsuranceType == "342")
                    {
                        var settlementCancelParam = new LeaveHospitalSettlementCancelParam()
                        {
                            MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                            SettlementNo = residentData.SettlementNo,
                            Operators = CommonHelp.GuidToStr(userBase.UserId),
                            CancelLimit = cancelLimit,

                        };
                        var cancelParam = new LeaveHospitalSettlementCancelInfoParam()
                        {
                            BusinessId = param.BusinessId,
                            Id = residentData.Id,
                            User = userBase,
                        };
                        _residentMedicalInsuranceNewService.LeaveHospitalSettlementCancel(settlementCancelParam, cancelParam);
                    }
                    //职工
                    if (residentData.InsuranceType == "310")
                    {
                        if (residentData.IsBirthHospital == 0)
                        {

                            //获取医院等级
                            var gradeData = _systemManageRepository.QueryHospitalOrganizationGrade(userBase.OrganizationCode);
                            var cancelParam = new WorkerSettlementCancelParam()
                            {
                                BusinessId = param.BusinessId,
                                Id = residentData.Id,
                                User = userBase,
                                SettlementNo = residentData.SettlementNo,
                                CancelLimit = cancelLimit,
                                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                                AdministrativeArea = gradeData.AdministrativeArea,
                                OrganizationCode = userBase.OrganizationCode,
                                WorkersStrokeCardNo = residentData.WorkersStrokeCardNo,
                                CancelSettlementRemarks = "",
                            };
                            _workerMedicalInsuranceNewService.WorkerSettlementCancel(cancelParam);
                        }
                        //职工生育住院取消采用居民取消结算
                        if (residentData.IsBirthHospital == 1)
                        {
                            var settlementCancelParam = new LeaveHospitalSettlementCancelParam()
                            {
                                MedicalInsuranceHospitalizationNo = residentData.MedicalInsuranceHospitalizationNo,
                                SettlementNo = residentData.SettlementNo,
                                Operators = CommonHelp.GuidToStr(userBase.UserId),
                                CancelLimit = cancelLimit,

                            };
                            var cancelParam = new LeaveHospitalSettlementCancelInfoParam()
                            {
                                BusinessId = param.BusinessId,
                                Id = residentData.Id,
                                User = userBase,
                            };
                            _residentMedicalInsuranceNewService.LeaveHospitalSettlementCancel(settlementCancelParam, cancelParam);
                        }


                    }
                }
                //更新住院明细状态
                string sql = $@" update HospitalizationFee set UpdateUserId='{param.UserId}',UpdateTime=GETDATE(),UploadMark=0 
                               where HospitalizationId='{param.BusinessId}' and IsDelete=0";
                _hisSqlRepository.ExecuteSql(sql);
                //更新医保状态
                string sqlMedicalInsurance = $@"update MedicalInsurance set IsDelete=1
                               where BusinessId='{param.BusinessId}' and  MedicalInsuranceState <6   and IsDelete=0";
                _hisSqlRepository.ExecuteSql(sqlMedicalInsurance);
            });
        }
        /// <summary>
        /// 获取医保处方上传参数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetPrescriptionUploadParam([FromBody]PrescriptionUploadUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                //处方上传
                var data = _residentMedicalInsuranceNewService.GetPrescriptionUploadParam(param,userBase);
                y.Data = data;
            });

        }
        /// <summary>
        /// 调整差值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData AdjustmentDifferenceValue([FromBody]AdjustmentDifferenceValueParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                List<string> idList = new List<string>();
                idList.Add(param.Id);
                param.UnitPrice = CommonHelp
                    .ValueToFour(Convert.ToDecimal(param.Amount) / Convert.ToDecimal(param.Quantity)).ToString(CultureInfo.InvariantCulture); 
                //费用数据
              var feeDataList=_hisSqlRepository.InpatientInfoDetailQuery(new InpatientInfoDetailQueryParam()
                {
                    IdList = idList,
                    UploadMark = 0,

                });
                if (feeDataList != null && feeDataList.Any())
                {
                    var feeData = feeDataList.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(feeData.ProjectCode)) throw new Exception("当前项目未医保对码,不能调整!!!"); 
                    string sql = $@"update [dbo].[HospitalizationFee] set [UnitPrice]={param.UnitPrice},[Quantity]={param.Quantity},[Amount]={param.Amount},
                                 [AdjustmentDifferenceValue]={feeData.Amount} where id='{param.Id}'";
                    _hisSqlRepository.ExecuteSql(sql);
                    _systemManageRepository.AddHospitalLog(new AddHospitalLogParam()
                    {
                        BusinessId = param.BusinessId,
                        RelationId = Guid.Parse(param.Id),
                        Remark = "调整费用明细",
                        User = userBase,
                        JoinOrOldJson = JsonConvert.SerializeObject(feeData) 


                    });
                }
                else
                {

                    throw new Exception("查询数据失败");
                }
               
               
                //var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                //处方上传
                //var data = _residentMedicalInsuranceNewService.GetPrescriptionUploadParam(param, userBase);
                //y.Data = data;
            });

           

        }
      

        /// <summary>
        ///  删除中心库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData DeleteCenterLibraryData([FromBody]UiBaseDataParam param )
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
               
                //费用数据
                var feeDataList = _hisSqlRepository.InpatientInfoDetailQuery(new InpatientInfoDetailQueryParam()
                {
                   BusinessId = param.BusinessId,
                    UploadMark = 1,

                });
                if (feeDataList != null && feeDataList.Any())
                {
                    throw  new Exception("还有未取消上传的数据,请先取消数据上传,再清理数据");
                }
                else
                {  //更新住院明细状态
                    string sql = $@" update HospitalizationFee set DeleteUserId='{param.UserId}',DeleteTime=getDate(),IsDelete=1
                               where HospitalizationId='{param.BusinessId}' and IsDelete=0";
                    _hisSqlRepository.ExecuteSql(sql);
                }
            });

        }
        /// <summary>
        /// 医保处方上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData PrescriptionUpload([FromBody]PrescriptionUploadUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);

                var updateParam = new PrescriptionUploadUpdateDataParam()
                {
                    BusinessId = param.BusinessId,
                    User = userBase,
                    ProjectBatch = param.ProjectBatch,
                    UploadData = param.UploadData
                };
                //处方上传
                var data = _residentMedicalInsuranceNewService.PrescriptionUploadUpdateData(updateParam);
                y.Data = data;
            });

        }
        /// <summary>
        /// 更新组织机构数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData UpdateOrganizationData([FromBody]UiInIParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                var iniParam = new DatabaseParam()
                {
                    Field = "OrganizationCode",
                    Value = userBase.OrganizationCode,
                    TableName = "Inpatient"
                };
                var inpatientData = _hisSqlRepository.QueryDatabase(new InpatientEntity(), iniParam);
                if (inpatientData != null)
                {
                    if (inpatientData.Any())
                    {
                        foreach (var item in inpatientData)
                        {
                           _webServiceBasicService.GetInpatientInfoDetail(userBase, item.BusinessId);
                        }
                    }
                }


            });

        }

        /// <summary>
        /// 根据组织机构获取有未传费用的住院病人
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetOrganizationNotUploadCostInpatient([FromBody] PrescriptionUploadUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);

               var inpatientData = _hisSqlRepository.QueryAllHospitalizationPatients(new PrescriptionUploadAutomaticParam()
                {
                   OrganizationCode = userBase.OrganizationCode
               });
                if (inpatientData != null)
                {
                    y.Data = inpatientData.Select(c => c.BusinessId).ToList();
                }

            });
        }
        /// <summary>
        /// 查询组织机构病人信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiJsonResultData QueryOrganizationInpatientInfo([FromUri]QueryOrganizationInpatientInfoUiParam param)
        {
            return new ApiJsonResultData(ModelState, new QueryCatalogDto()).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                param.OrganizationCode = userBase.OrganizationCode;
                var queryData = _hisSqlRepository.QueryOrganizationInpatientInfo(param);
                var data = new
                {
                    data = queryData.Values.FirstOrDefault(),
                    count = queryData.Keys.FirstOrDefault()
                };
                y.Data = data;

            });



        }
        
        /// <summary>
        /// 获取处方取消上传参数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData GetDeletePrescriptionUploadParam([FromBody]BaseUiBusinessIdDataParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            { 
              var deleteParam=  _residentMedicalInsuranceNewService.GetDeletePrescriptionUploadParam(param);
                y.Data = XmlSerializeHelper.XmlSerialize(deleteParam);
            });

        }
        /// <summary>
        /// 不传医保操作
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData NotUploadMark([FromBody]NotUploadMarkUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                _residentMedicalInsuranceNewService.NotUploadMark(param);
              
            });

        }
        /// <summary>
        /// 基层数据审核
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData BatchExamineData([FromBody]BatchExamineDataUiParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            {
                var userBase = _webServiceBasicService.GetUserBaseInfo(param.UserId);
                _hisSqlRepository.BatchExamineData(new BatchExamineDataParam()
                {
                    DataIdList = param.DataIdList,
                    User = userBase,
                    BusinessId=param.BusinessId,
                    BatchExamineSign = param.BatchExamineSign

                } );
               
            });

        }
        //
        /// <summary>
        /// 医保删除处方数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiJsonResultData DeletePrescriptionUpload([FromBody]BaseUiBusinessIdDataParam param)
        {
            return new ApiJsonResultData(ModelState).RunWithTry(y =>
            { 
                _residentMedicalInsuranceNewService.DeletePrescriptionUpload(param);
            });

        }
       
        #endregion

    }
}
