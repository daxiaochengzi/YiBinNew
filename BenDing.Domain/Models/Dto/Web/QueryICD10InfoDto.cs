﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BenDing.Domain.Models.Dto.Web
{
   public class QueryICD10InfoDto
    {
      /// <summary>
      /// 疾病编码
      /// </summary>
        public string DiseaseCoding { get; set; }
        /// <summary>
        /// 病种名称
        /// </summary>
        public string DiseaseName { get; set; }
        /// <summary>
        /// 助记码
        /// </summary>
      
        public string MnemonicCode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 疾病id
        /// </summary>
        public string DiseaseId { get; set; }
        /// <summary>
        /// 主键
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 医保疾病编码
        /// </summary>
        public string ProjectCode { get; set; }
        /// <summary>
        /// 医保疾病编码
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PairCodeUserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PairCodeTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        //是医保
        public  int IsMedicalInsurance { get; set; }
    }
}
