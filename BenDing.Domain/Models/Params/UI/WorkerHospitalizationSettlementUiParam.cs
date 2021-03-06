﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.Web;
using BenDing.Domain.Models.Params.Base;

namespace BenDing.Domain.Models.Params.UI
{
   public class WorkerHospitalizationSettlementUiParam: UiBaseDataParam
    { /// <summary>
        /// 出院病人状态(1康复，2转院，3死亡，4其他)
        /// </summary>
        [Display(Name = "出院病人状态")]
        [Required(ErrorMessage = "{0}不能为空!!!")]

        public string LeaveHospitalInpatientState { get; set; }
        /// <summary>
        /// 结算json
        /// </summary>
        public string SettlementJson { get; set; }
        public  decimal  InsuranceBalance { get; set; }
        /// <summary>
        /// 诊断
        /// </summary>

        public List<InpatientDiagnosisDto> DiagnosisList { get; set; } = null;
    }
}
