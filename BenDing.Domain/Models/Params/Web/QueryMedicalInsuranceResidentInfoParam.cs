﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BenDing.Domain.Models.Params.Web
{
  public  class QueryMedicalInsuranceResidentInfoParam
    {
        
        /// <summary>
        /// 数据id
        /// </summary>
        public string DataId { get; set; }
        /// <summary>
        /// 业务id
        /// </summary>
        public string BusinessId { get; set; }
        /// <summary>
        /// 组织机构
        /// </summary>
        public string OrganizationCode { get; set; }
        /// <summary>
        /// 结算单据号
        /// </summary>
        public  string SettlementNo { get; set; }
        /// <summary>
        /// 结算状态
        /// </summary>
        public  int MedicalInsuranceState { get; set; }


    }
}
