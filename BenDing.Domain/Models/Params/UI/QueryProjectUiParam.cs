﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.Base;

namespace BenDing.Domain.Models.Params.UI
{
   public class QueryProjectUiParam: PaginationDto
    {/// <summary>
     /// 项目编码
     /// </summary>
        public string ProjectCode { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>

        public string ProjectName { get; set; }
        /// <summary>
        /// 项目编码类别
        /// </summary>
        public string ProjectCodeType { get; set; }

        /// <summary>
        /// 拼音助记码
        /// </summary>
        public string MnemonicCode { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public  string Manufacturer { get; set; }
        /// <summary>
        /// 准字号
        /// </summary>
        public  string QuasiFontSize { get; set; }
    }
}
