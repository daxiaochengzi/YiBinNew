﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.Base;

namespace BenDing.Domain.Models.Params.UI
{
   public class QueryHospitalizationFeeUiParam: PaginationDto
    {   /// <summary>
    /// 业务id
    /// </summary>
        [Display(Name = "业务id")]
        [Required(ErrorMessage = "{0}不能为空!!!")]
        public string BusinessId { get; set; }

        /// <summary>
        /// 上传状态查询
        /// </summary>

        public string UploadMark { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public  string BillTime { get; set; }
        /// <summary>
        /// 药品名称
        /// </summary>
        public  string DirectoryName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public  string TransKey { get; set; }
        /// <summary>
        /// 是否加载
        /// </summary>
        public  bool IsLoad { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public  string UserId { get; set; }
        /// <summary>
        /// 是否限制查询
        /// </summary>
        public int IsExamine { get; set; }
    }
}
