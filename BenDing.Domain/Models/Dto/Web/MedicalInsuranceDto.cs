﻿using BenDing.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BenDing.Domain.Models.Dto.Web
{/// <summary>
/// 
/// </summary>
   public class MedicalInsuranceDto
        { /// <summary>
          /// Id
          /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 基层His住院Id
        /// </summary>
        public string BusinessId { get; set;}
        /// <summary>
        /// 医保住院号
        /// </summary>
        public string MedicalInsuranceHospitalizationNo { get; set; }
        /// <summary>
        /// 医保卡号
        /// </summary>
        public string InsuranceNo { get; set; }
       

        /// <summary>
        /// 入院信息
        /// </summary>
        public string AdmissionInfoJson { get; set; }
       
        /// <summary>
        /// 是否为修改
        /// </summary>
        public  bool IsModify { get; set; }
        /// <summary>
        /// 医保类别
        /// </summary>
        public  int InsuranceType { get; set; }
        /// <summary>
        /// 医保状态
        /// </summary>
        public MedicalInsuranceState MedicalInsuranceState { get; set; }
        /// <summary>
        /// 是否生育入院登记
        /// </summary>
        public int IsBirthHospital { get; set; }
        /// <summary>
        /// 身份标识
        /// </summary>
         public  string IdentityMark { get; set; }
            
          /// <summary>
          /// 传入标志
          /// </summary>
          public  string AfferentSign { get; set; }
          /// <summary>
          /// 参保地
          /// </summary>
          public  string CommunityName { get; set; }
            /// <summary>
            /// 联系电话
            /// </summary>
            public string ContactPhone { get; set; }
            /// <summary>
            /// 联系地址
            /// </summary>
            public string ContactAddress { get; set; }


    }
}
